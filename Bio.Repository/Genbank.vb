Imports LANS.SystemsBiology.Assembly.NCBI.GenBank
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels
Imports LANS.SystemsBiology.Repository
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic
Imports LANS.SystemsBiology.Assembly

Public Module Installer

    ''' <summary>
    ''' 这个函数主要是进行创建数据库的索引文件
    ''' </summary>
    ''' <param name="DIR">Extract location of file: all.gbk.tar.gz from NCBI FTP website.</param>
    ''' <returns></returns>
    Public Function Install(DIR As String, refresh As Boolean) As Boolean
        Dim index As String = $"{DIR}/.genbank/{Genbank.IndexJournal}"

        If refresh Then
            Call "".SaveTo(index)  ' 清除原始文件的所有数据，重新创建索引文件
        End If

        Using DbWriter As New DocumentStream.Linq.WriteStream(Of GenbankIndex)(index)  ' 打开数据库的文件句柄

            For Each table As String In ls - l - lsDIR - r <= DIR   ' 一个物种的文件夹
                Dim path As String = $"{DIR}/.genbank/meta/{table.BaseName}.csv"

                If path.FileLength > 0 Then
                    If Not refresh Then
                        Continue For
                    End If
                End If

                Dim genes As New List(Of GeneInfo)

                For Each gbk As String In ls - l - r - wildcards("*.gbk", "*.gb") <= table
                    For Each gb As GBFF.File In GBFF.File.LoadDatabase(gbk)  ' 使用迭代器读取数据库文件

                        Dim idx As New GenbankIndex With {
                            .AccId = gb.Locus.AccessionID,
                            .definition = gb.Definition.Value,
                            .DIR = table.BaseName,
                            .genome = gb.Source.SpeciesName
                        }
                        Call DbWriter.Flush(idx)   ' 将对象写入内存缓存，进入队列等待回写入文件系统

                        genes += gb.GbffToORF_PTT.GeneObjects.ToArray(Function(g) New GeneInfo(g, idx.AccId))
                    Next
                Next

                Call genes.SaveTo(path)
            Next
        End Using

        Return True
    End Function
End Module

''' <summary>
''' NCBI genbank repository system.(请注意，这个对象里面的所有的Repository实体都是使用genbank编号来作为Key的)
''' </summary>
Public Class Genbank : Inherits ClassObject
    Implements IRepository(Of String, GenbankIndex)

    Public Const IndexJournal As String = "ncbi_genbank.csv"

    Public ReadOnly Property DIR As String

    ReadOnly __indexHash As Dictionary(Of GenbankIndex)

    Sub New(DIR As String)
        Dim index As String = $"{DIR}/.genbank/{IndexJournal}"

        Me.DIR = DIR
        Me.__indexHash = index.LoadCsv(Of GenbankIndex).ToDictionary
    End Sub

#Region "Implements IRepository(Of String, GenbankIndex)"

    ''' <summary>
    ''' 查询出gbk的文件路径，这个主要是为了RegPrecise查询使用的
    ''' </summary>
    ''' <param name="genome"></param>
    ''' <param name="locus"></param>
    ''' <returns></returns>
    Public Function Query(genome As String, locus As IEnumerable(Of String)) As GenbankIndex
        Dim LQuery = (From x As GenbankIndex
                      In __indexHash.Values.AsParallel
                      Let edits As DistResult = LevenshteinDistance.ComputeDistance(genome, x.genome)
                      Where Not edits Is Nothing
                      Select x,
                          edits
                      Order By edits.MatchSimilarity Descending).Take(10)
        For Each x In LQuery
            Dim path As String = $"{DIR}/.genbank/meta/{x.x.DIR}.Csv"
            Dim genes As IEnumerable(Of GeneInfo) = path.LoadCsv(Of GeneInfo)
            Dim gHash As Dictionary(Of GeneInfo) = (From g As GeneInfo
                                                    In genes
                                                    Where String.Equals(g.accId, x.x.AccId, StringComparison.OrdinalIgnoreCase)
                                                    Select g
                                                    Group g By g.locus_tag Into Group) _
                                                         .Select(Function(o) o.Group.First).ToDictionary
            For Each sid As String In locus
                If gHash.ContainsKey(sid) Then
                    Return x.x
                End If
            Next
        Next

        Return Nothing
    End Function

    Public Function Query(source As KEGG.WebServices.QuerySource) As GenbankIndex
        Return Query(source.genome, source.locusId)
    End Function

    Public Function Exists(key As String) As Boolean Implements IRepositoryRead(Of String, GenbankIndex).Exists
        Return __indexHash.ContainsKey(key)
    End Function

    Public Function GetByKey(key As String) As GenbankIndex Implements IRepositoryRead(Of String, GenbankIndex).GetByKey
        If __indexHash.ContainsKey(key) Then
            Return __indexHash(key)
        Else
            Return Nothing
        End If
    End Function

    Public Function GetWhere(clause As Func(Of GenbankIndex, Boolean)) As IReadOnlyDictionary(Of String, GenbankIndex) Implements IRepositoryRead(Of String, GenbankIndex).GetWhere
        Dim LQuery As IEnumerable(Of GenbankIndex) =
            From x As GenbankIndex
            In __indexHash.Values
            Where True = clause(x)
            Select x
        Return LQuery.ToDictionary(Function(x) x.AccId)
    End Function

    Public Function GetAll() As IReadOnlyDictionary(Of String, GenbankIndex) Implements IRepositoryRead(Of String, GenbankIndex).GetAll
        Return New Dictionary(Of String, GenbankIndex)(__indexHash)
    End Function

    Public Sub Delete(key As String) Implements IRepositoryWrite(Of String, GenbankIndex).Delete
        Throw New ReadOnlyException(ReadOnlyRepository)
    End Sub

    Public Sub AddOrUpdate(entity As GenbankIndex, key As String) Implements IRepositoryWrite(Of String, GenbankIndex).AddOrUpdate
        Throw New ReadOnlyException(ReadOnlyRepository)
    End Sub

    Const ReadOnlyRepository As String = "This repository is readonly, please updates from method: Installer.Install"

    Public Function AddNew(entity As GenbankIndex) As String Implements IRepositoryWrite(Of String, GenbankIndex).AddNew
        Throw New ReadOnlyException(ReadOnlyRepository)
    End Function
#End Region
End Class

Public Class GeneInfo
    Implements IKeyedEntity(Of String), sIdEnumerable

    ''' <summary>
    ''' 基因组的编号
    ''' </summary>
    ''' <returns></returns>
    Public Property accId As String
    ''' <summary>
    ''' 基因的编号
    ''' </summary>
    ''' <returns></returns>
    Public Property locus_tag As String Implements sIdEnumerable.Identifier, IKeyedEntity(Of String).Key
    Public Property [function] As String

    Sub New(g As GeneBrief, acc As String)
        locus_tag = g.Synonym
        [function] = g.Product
        accId = acc
    End Sub

    Sub New()
    End Sub

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Class GenbankIndex : Implements IKeyedEntity(Of String), sIdEnumerable

    ''' <summary>
    ''' DIR name
    ''' </summary>
    ''' <returns></returns>
    Public Property DIR As String
    Public Property genome As String
    ''' <summary>
    ''' locus_tag, 索引文件的表主键
    ''' </summary>
    ''' <returns></returns>
    Public Property AccId As String Implements IKeyedEntity(Of String).Key, sIdEnumerable.Identifier
    Public Property definition As String

    Public Function Gbk(DIR As String) As GBFF.File
        Dim path As String = $"{DIR}/{Me.DIR}/"
        Dim files As IEnumerable(Of String) = ls - l - r - wildcards("*.gb", "*.gbk") <= path

        If files.IsNullOrEmpty Then
            Return Nothing
        Else
            Dim LQuery As String = (From file As String
                                    In files
                                    Let name As String = file.BaseName
                                    Where InStr(name, AccId, CompareMethod.Text) = 1
                                    Select file).FirstOrDefault

            If String.IsNullOrEmpty(LQuery) Then
                Return Nothing
            Else
                Return GBFF.File.Load(Path:=LQuery)
            End If
        End If
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
