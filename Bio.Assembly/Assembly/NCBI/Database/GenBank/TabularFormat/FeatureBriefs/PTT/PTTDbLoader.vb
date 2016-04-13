Imports System.Text.RegularExpressions
Imports System.Text
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels
Imports LANS.SystemsBiology.ComponentModel
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports LANS.SystemsBiology.SequenceModel

Namespace Assembly.NCBI.GenBank.TabularFormat

    Public Class PTTDbLoader : Inherits TabularLazyLoader
        Implements IReadOnlyDictionary(Of String, GeneBrief)

        Dim _CDSPtt As PTT
        Dim _RNARnt As PTT
        Dim _GenomeFasta As FASTA.FastaToken
        Dim _lstFile As PTTEntry

        ''' <summary>
        ''' 整个基因组中的所有基因的集合，包括有蛋白质编码基因和RNA基因
        ''' </summary>
        ''' <remarks></remarks>
        Dim _GenomeList As Dictionary(Of String, GeneBrief) = New Dictionary(Of String, GeneBrief)

        Public ReadOnly Property GeneFastas As Dictionary(Of String, FastaObjects.Fasta)
        Public ReadOnly Property Proteins As Dictionary(Of String, FastaObjects.Fasta)

        ''' <summary>
        ''' 从PTT基因组注释数据之中获取COG分类数据
        ''' </summary>
        ''' <typeparam name="T_Entry"></typeparam>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ExportCOGProfiles(Of T_Entry As I_COGEntry)() As T_Entry()
            Dim LQuery = (From gene As GeneBrief
                          In Me._GenomeList.Values.AsParallel
                          Select gene.getCOGEntry(Of T_Entry)()).ToArray
            Return LQuery
        End Function

        ''' <summary>
        ''' 从其他的数据类型进行数据转换，转换数据格式为PTT格式，以方便用于后续的分析操作用途
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function CreateObject(briefs As IEnumerable(Of I_GeneBrief), SourceFasta As FASTA.FastaToken) As PTTDbLoader
            Dim BriefData = (From Protein In briefs Select GeneBrief.CreateObject(Protein)).ToArray
            Return New PTTDbLoader With {
                ._lstFile = New PTTEntry,
                ._CDSPtt = New PTT With {
                    .GeneObjects = BriefData,
                    .Size = SourceFasta.Length,
                    .Title = SourceFasta.Title
                },
                ._DIR = "NULL",
                ._GenomeFasta = SourceFasta,
                ._GenomeList = BriefData.ToDictionary(Function(gene) gene.Synonym),
                ._RptGenomeBrief = New Rpt With {
                    .Size = SourceFasta.Length,
                    .NumberOfGenes = briefs.Count
                }
            }
        End Function

        ''' <summary>
        ''' 会依照所传递进来的<paramref name="Strand">链的方向的参数</paramref>来查找与之相关的基因
        ''' </summary>
        ''' <param name="LociStart"></param>
        ''' <param name="LociEnds"></param>
        ''' <param name="Strand"></param>
        ''' <param name="ATGDistance"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetRelatedGenes(LociStart As Integer, LociEnds As Integer, Strand As Strands, Optional ATGDistance As Integer = 500) As Relationship(Of GeneBrief)()
            If Strand = Strands.Unknown Then
                Return ComponentModel.Loci.GetRelatedGenes(Of GeneBrief)(Me.Values.ToArray, LociStart, LociEnds, ATGDistance)
            End If

            Dim LQuery = (From genEntry In Me Where genEntry.Value.Location.Strand = Strand Select genEntry.Value).ToArray
            Return ComponentModel.Loci.GetRelatedGenes(Of GeneBrief)(LQuery, LociStart, LociEnds, ATGDistance)
        End Function

        ''' <summary>
        ''' This function will ignore the nucleoside direction adn founds the gene on both strand of the DNA.(将会忽略DNA链，两条链的位置上都会寻找)
        ''' </summary>
        ''' <param name="LociStart"></param>
        ''' <param name="LociEnds"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetRelatedGenes(LociStart As Integer, LociEnds As Integer, Optional ATGDistance As Integer = 500) As Relationship(Of GeneBrief)()
            Return ComponentModel.Loci.GetRelatedGenes(Of GeneBrief)(Me.Values, LociStart, LociEnds, ATGDistance)
        End Function

        Protected Sub New()
            Call MyBase.New("", DbAPI.PTTs)
        End Sub

        Sub New(entry As PTTEntry)
            Call MyBase.New(entry.DIR, DbAPI.PTTs)

            _lstFile = entry
            _RptGenomeBrief = Rpt.Load(Of Rpt)(_lstFile.rpt)

            For Each genEntry As GeneBrief In ORF_PTT().GeneObjects
                Call _GenomeList.Add(genEntry.Synonym, genEntry)
            Next
            For Each genEntry As GeneBrief In RNARnt.GeneObjects
                Call _GenomeList.Add(genEntry.Synonym, genEntry)
            Next

            Dim FastaFile As FASTA.FastaFile
            FastaFile = FASTA.FastaFile.Read(_lstFile.faa)
            Proteins = (From prot As FASTA.FastaToken
                        In FastaFile
                        Let uniqueId As String = GetLocusId(prot.Attributes(1))
                        Select FastaObjects.Fasta.CreateObject(uniqueId, prot)) _
                            .ToDictionary(Function(x As FastaObjects.Fasta) x.UniqueId)

            FastaFile = FASTA.FastaFile.Read(_lstFile.ffn)
            GeneFastas = (From genFa As FASTA.FastaToken
                          In FastaFile
                          Let UniqueId As String = GetGeneUniqueId(genFa.Attributes(4))
                          Select FastaObjects.Fasta.CreateObject(UniqueId, genFa)) _
                             .ToDictionary(Function(x As FastaObjects.Fasta) x.UniqueId)
            FastaFile = FASTA.FastaFile.Read(_lstFile.frn)
            For Each genFa As FastaObjects.Fasta In (From fa As FASTA.FastaToken
                                                     In FastaFile
                                                     Let UniqueId As String = Regex.Match(fa.Title, "locus_tag=[^]]+").Value.Split(CChar("=")).Last
                                                     Select FastaObjects.Fasta.CreateObject(UniqueId, fa))
                Call GeneFastas.Add(genFa.UniqueId, genFa)
            Next
        End Sub

        ''' <summary>
        ''' 请注意，通过这个构造函数只能够读取一个数据库的数据，假若一个文件夹里面同时包含有了基因组数据和质粒的数据，则不推荐使用这个函数进行构造加载
        ''' </summary>
        ''' <param name="DIR"></param>
        Sub New(DIR As String)
            Call Me.New(DbAPI.GetEntryList(DIR).First)
        End Sub

        ''' <summary>
        ''' 基因序列中的pid似乎是无效的，都一样的，只能通过location来获取序列的标识号了
        ''' </summary>
        ''' <param name="Location"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetGeneUniqueId(Location As String) As String
            Location = Location.Split.First
            Dim Points = (From m As Match In Regex.Matches(Location, "\d+") Let n = CInt(Val(m.Value)) Select n Order By n Ascending).ToArray
            Dim LQuery = (From genEntry In _GenomeList
                          Where genEntry.Value.Location.Left = Points.First AndAlso
                              genEntry.Value.Location.Right = Points.Last
                          Select genEntry.Key).FirstOrDefault
            Return LQuery
        End Function

        Private Function GetLocusId(Pid As String) As String
            Dim LQuery = (From genEntry In _GenomeList Where String.Equals(Pid, genEntry.Value.PID) Select genEntry.Key).FirstOrDefault
            Return LQuery
        End Function

        ''' <summary>
        ''' *.ptt
        ''' </summary>
        ''' <returns></returns>
        Public Function ORF_PTT() As PTT
            If _CDSPtt Is Nothing Then
                _CDSPtt = PTT.Load(_lstFile.ptt)
            End If
            Return _CDSPtt
        End Function

        ''' <summary>
        ''' *.rpt
        ''' </summary>
        ''' <returns></returns>
        Public Function RNARnt() As PTT
            If _RNARnt Is Nothing Then
                _RNARnt = PTT.Load(_lstFile.rnt)
            End If
            Return _RNARnt
        End Function

        Public Function CreateReader() As NucleotideModels.SegmentReader
            Return New NucleotideModels.SegmentReader(GenomeFasta)
        End Function

        ''' <summary>
        ''' (*.fna)(基因组的全长序列)
        ''' </summary>
        ''' <returns></returns>
        Public Function GenomeFasta() As FASTA.FastaToken
            If _GenomeFasta Is Nothing Then
                _GenomeFasta = FASTA.FastaToken.LoadNucleotideData(_lstFile.fna)
            End If
            Return _GenomeFasta
        End Function

        ''' <summary>
        ''' *.rpt
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property RptGenomeBrief As Rpt

#Region " Implements IReadOnlyDictionary(Of String, PTT.GeneBrief)"

        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, TabularFormat.ComponentModels.GeneBrief)) Implements IEnumerable(Of KeyValuePair(Of String, TabularFormat.ComponentModels.GeneBrief)).GetEnumerator
            For Each Item As KeyValuePair(Of String, TabularFormat.ComponentModels.GeneBrief) In _GenomeList
                Yield Item
            Next
        End Function

        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of KeyValuePair(Of String, TabularFormat.ComponentModels.GeneBrief)).Count
            Get
                Return _GenomeList.Count
            End Get
        End Property

        Public Function ContainsKey(key As String) As Boolean Implements IReadOnlyDictionary(Of String, TabularFormat.ComponentModels.GeneBrief).ContainsKey
            Return _GenomeList.ContainsKey(key)
        End Function

        Default Public ReadOnly Property Item(key As String) As TabularFormat.ComponentModels.GeneBrief Implements IReadOnlyDictionary(Of String, TabularFormat.ComponentModels.GeneBrief).Item
            Get
                If _GenomeList.ContainsKey(key) Then
                    Return _GenomeList(key)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Public ReadOnly Property Keys As IEnumerable(Of String) Implements IReadOnlyDictionary(Of String, TabularFormat.ComponentModels.GeneBrief).Keys
            Get
                Return _GenomeList.Keys
            End Get
        End Property

        Public Function TryGetValue(key As String, ByRef value As TabularFormat.ComponentModels.GeneBrief) As Boolean Implements IReadOnlyDictionary(Of String, TabularFormat.ComponentModels.GeneBrief).TryGetValue
            Return _GenomeList.TryGetValue(key, value)
        End Function

        Public ReadOnly Property Values As IEnumerable(Of TabularFormat.ComponentModels.GeneBrief) Implements IReadOnlyDictionary(Of String, TabularFormat.ComponentModels.GeneBrief).Values
            Get
                Return _GenomeList.Values
            End Get
        End Property

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

#End Region

        Public Shared Narrowing Operator CType(PTTDb As PTTDbLoader) As GeneBrief()
            Return PTTDb.Values.ToArray
        End Operator
    End Class
End Namespace