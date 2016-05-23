Imports System.Runtime.CompilerServices
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels
Imports LANS.SystemsBiology.ComponentModel
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic

Namespace ContextModel

    Public Module LocationDescriptions

        ''' <summary>
        ''' 判断本对象是否是由<see cref="BlankSegment"></see>方法所生成的空白片段
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function IsBlankSegment(Gene As I_GeneBrief) As Boolean
            If Gene Is Nothing Then
                Return True
            End If
            Return String.Equals(Gene.Identifier, BLANK_VALUE) OrElse
                Gene.Location.FragmentSize = 0
        End Function

        Public Const BLANK_VALUE As String = "Blank"

        <Extension> Public Function BlankSegment(Of T As I_GeneBrief)(Location As NucleotideLocation) As T
            Dim BlankData = Activator.CreateInstance(Of T)()
            BlankData.COG = BLANK_VALUE
            BlankData.Product = BLANK_VALUE
            BlankData.Identifier = BLANK_VALUE
            BlankData.Length = Location.FragmentSize
            BlankData.Location = Location
            Return BlankData
        End Function

        Private Function __getLocationFunction(Of T_Gene As I_GeneBrief)(
                                                  GeneSegment As T_Gene,
                                                  SegmentLocation As NucleotideLocation) As SegmentRelationships

            Dim r = GeneSegment.Location.GetRelationship(SegmentLocation)

            If r = SegmentRelationships.DownStream AndAlso
                GeneSegment.Location.Strand = Strands.Reverse Then
                Return SegmentRelationships.UpStream  '反向的基因需要被特别注意，当目标片段处于下游的时候，该下游片段可能为该基因的启动子区

            ElseIf r = SegmentRelationships.UpStream AndAlso
                GeneSegment.Location.Strand = Strands.Reverse Then
                Return SegmentRelationships.DownStream

            ElseIf r = SegmentRelationships.UpStreamOverlap AndAlso
                GeneSegment.Location.Strand = Strands.Reverse Then
                Return SegmentRelationships.DownStreamOverlap

            ElseIf r = SegmentRelationships.DownStreamOverlap AndAlso
                GeneSegment.Location.Strand = Strands.Reverse Then
                Return SegmentRelationships.UpStreamOverlap

            Else
                Return r
            End If
        End Function

        ''' <summary>
        ''' 获取某一个指定的位点在基因组之中的内部反向的基因的集合
        ''' </summary>
        ''' <typeparam name="T_Gene"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="LociStart"></param>
        ''' <param name="LociEnds"></param>
        ''' <param name="Strand"></param>
        ''' <returns></returns>
        Public Function GetInnerAntisense(Of T_Gene As I_GeneBrief)(source As IEnumerable(Of T_Gene),
                                                                    LociStart As Integer,
                                                                    LociEnds As Integer,
                                                                    Strand As Strands) As T_Gene()
            Dim Raw As Relationship(Of T_Gene)() = source.GetRelatedGenes(LociStart, LociEnds, 0)
            Dim LQuery = (From obj In Raw
                          Where obj.Relation = SegmentRelationships.Inside AndAlso '只需要在内部并且和指定的链的方向反向的对象就可以了
                              (Strand <> obj.Gene.Location.Strand AndAlso
                              obj.Gene.Location.Strand <> Strands.Unknown)
                          Select obj.Gene).ToArray
            Return LQuery
        End Function

        ''' <summary>
        ''' Gets the related genes on a specific loci site location.(函数获取某一个给定的位点附近的所有的有关联的基因对象。
        ''' 请注意，这个函数仅仅是依靠于两个位点之间的相互位置关系来判断的，
        ''' 并没有判断链的方向，假若需要判断链的方向，请在调用本函数之前就将参数<paramref name="datasource"/>按照链的方向筛选出来)
        ''' </summary>
        ''' <param name="DataSource"></param>
        ''' <param name="LociStart"></param>
        ''' <param name="LociEnds"></param>
        ''' <param name="ATGDistance"></param>
        ''' <returns>请注意，函数所返回的列表之中包含有不同的关系！</returns>
        ''' <remarks></remarks>
        <Extension> Public Function GetRelatedGenes(DataSource As Generic.IEnumerable(Of GeneBrief),
                                                    LociStart As Integer,
                                                    LociEnds As Integer,
                                                    Optional ATGDistance As Integer = 500) As Relationship(Of GeneBrief)()
            Return GetRelatedGenes(Of GeneBrief)(DataSource, LociStart, LociEnds, ATGDistance)
        End Function

        ''' <summary>
        ''' Gets the related genes on a specific loci site location.(函数获取某一个给定的位点附近的所有的有关联的基因对象。
        ''' 请注意，这个函数仅仅是依靠于两个位点之间的相互位置关系来判断的，
        ''' 并没有判断链的方向，假若需要判断链的方向，请在调用本函数之前就将参数<paramref name="source"/>按照链的方向筛选出来)
        ''' </summary>
        ''' <typeparam name="T_Gene"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="LociStart"></param>
        ''' <param name="LociEnds"></param>
        ''' <param name="ATGDistance"></param>
        ''' <returns>请注意，函数所返回的列表之中包含有不同的关系！</returns>
        ''' <remarks></remarks>
        <Extension> Public Function GetRelatedGenes(Of T_Gene As I_GeneBrief)(
                                    source As Generic.IEnumerable(Of T_Gene),
                                    LociStart As Integer,
                                    LociEnds As Integer,
                                    Optional ATGDistance As Integer = 500) As Relationship(Of T_Gene)()
            Dim ntSite As New ComponentModel.Loci.NucleotideLocation(
                LociStart,
                LociEnds,
                Strand:=Strands.Unknown)
            Return GetRelatedGenes(source, ntSite, ATGDistance)
        End Function

        Private Structure __getRelationDelegate(Of T_Gene As I_GeneBrief)
            Dim DataSource As Generic.IEnumerable(Of T_Gene)
            Dim Loci As NucleotideLocation

            Public Function GetRelation(RelationType As SegmentRelationships) As T_Gene()
                Dim Loci As NucleotideLocation = Me.Loci

                Return (From GeneSegment As T_Gene
                        In DataSource
                        Let Relation = __getLocationFunction(GeneSegment, Loci)
                        Where Relation = RelationType
                        Select GeneSegment).ToArray
            End Function

            Public Function GetRelation(RelationType As SegmentRelationships, ATGDistance As Integer) As T_Gene()
                Dim Genes = GetRelation(RelationType)
                Dim Loci As ComponentModel.Loci.NucleotideLocation = Me.Loci
                Dim LQuery = (From GeneObject As T_Gene
                              In Genes
                              Where Math.Abs(GetATGDistance(Loci, GeneObject)) <= ATGDistance
                              Select GeneObject).ToArray '获取ATG距离小于阈值的所有基因
                Return LQuery
            End Function
        End Structure

        ''' <summary>
        ''' <see cref="SegmentRelationships.UpStreamOverlap"/> and 
        ''' <see cref="SegmentRelationships.UpStream"/>
        ''' </summary>
        ''' <typeparam name="T_Gene"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="Loci"></param>
        ''' <param name="ATGDistance"></param>
        ''' <returns></returns>
        <Extension> Public Function GetRelatedUpstream(Of T_Gene As I_GeneBrief)(source As IEnumerable(Of T_Gene),
                                                                                 Loci As NucleotideLocation,
                                                                                 Optional ATGDistance As Integer = 2000) As Relationship(Of T_Gene)()
            Dim LociDelegate = New __getRelationDelegate(Of T_Gene)() With {
                .DataSource = source,
                .Loci = Loci.Normalization
            }
            Dim UpStreams As New KeyValuePair(Of SegmentRelationships, T_Gene())(
               SegmentRelationships.UpStream,
                LociDelegate.GetRelation(SegmentRelationships.UpStream, ATGDistance))
            Dim UpStreamOverlaps As New KeyValuePair(Of SegmentRelationships, T_Gene())(
               SegmentRelationships.UpStreamOverlap,
                LociDelegate.GetRelation(SegmentRelationships.UpStreamOverlap, ATGDistance))
            Dim array = {UpStreams, UpStreamOverlaps}
            Dim data0 = array.ToArray(Function(x) x.Value.ToArray(Function(g) New Relationship(Of T_Gene)(g, x.Key))).MatrixToList
            Return data0.ToArray
        End Function

        Public Function GetRelatedGenes(DataSource As IEnumerable(Of GeneBrief), Loci As NucleotideLocation, relation As SegmentRelationships) As GeneBrief()
            Dim LociDelegate = New __getRelationDelegate(Of GeneBrief)() With {
                .DataSource = DataSource,
                .Loci = Loci
            }
            Return LociDelegate.GetRelation(relation)
        End Function

        ''' <summary>
        ''' Gets the related genes on a specific loci site location.(函数获取某一个给定的位点附近的所有的有关联的基因对象。
        ''' 请注意，这个函数仅仅是依靠于两个位点之间的相互位置关系来判断的，
        ''' 并没有判断链的方向，假若需要判断链的方向，请在调用本函数之前就将参数<paramref name="source"/>按照链的方向筛选出来)
        ''' </summary>
        ''' <typeparam name="T_Gene"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="ATGDistance"></param>
        ''' <returns>请注意，函数所返回的列表之中包含有不同的关系！</returns>
        ''' <remarks></remarks>
        <Extension> Public Function GetRelatedGenes(Of T_Gene As I_GeneBrief)(source As IEnumerable(Of T_Gene), Loci As NucleotideLocation, Optional ATGDistance As Integer = 500) As Relationship(Of T_Gene)()
            Dim foundTEMP As T_Gene()
            Dim lstRelated As New List(Of Relationship(Of T_Gene))
            Dim LociDelegate As New __getRelationDelegate(Of T_Gene)() With {
                .DataSource = source,
                .Loci = Loci.Normalization
            }

            foundTEMP = LociDelegate.GetRelation(SegmentRelationships.UpStream)
            foundTEMP = (From GeneObject As T_Gene
                         In foundTEMP
                         Where Math.Abs(GetATGDistance(Loci, GeneObject)) <= ATGDistance
                         Select GeneObject).ToArray '获取ATG距离小于阈值的所有基因

            If Not foundTEMP.IsNullOrEmpty Then
                Dim lstBuff = (From Gene As T_Gene
                               In foundTEMP.AsParallel
                               Select New Relationship(Of T_Gene)(Gene, SegmentRelationships.UpStream)).ToArray
                Call lstRelated.AddRange(lstBuff)
            End If

            For Each RelationShip In New ComponentModel.Loci.SegmentRelationships() {
 _
               SegmentRelationships.Equals,
               SegmentRelationships.Inside,
               SegmentRelationships.DownStreamOverlap,
               SegmentRelationships.UpStreamOverlap,
               SegmentRelationships.Cover
            }

                foundTEMP = LociDelegate.GetRelation(RelationShip)

                If Not foundTEMP.IsNullOrEmpty Then
                    Dim lstBuff = (From Gene As T_Gene
                                   In foundTEMP.AsParallel
                                   Select New Relationship(Of T_Gene)(Gene, RelationShip)).ToArray
                    Call lstRelated.AddRange(lstBuff)
                End If
            Next

            Dim DownStreamGenes = LociDelegate.GetRelation(SegmentRelationships.DownStream)
            Dim Dwsrt As T_Gene() = (From Gene As T_Gene
                                     In DownStreamGenes
                                     Let Distance As Integer = __atgDistance(Gene, Loci)
                                     Where Distance <= ATGDistance
                                     Select Gene).ToArray

            If Not Dwsrt.IsNullOrEmpty Then
                Dim lstBuff = (From Gene As T_Gene
                               In foundTEMP.AsParallel
                               Select New Relationship(Of T_Gene)(Gene, SegmentRelationships.DownStream)).ToArray
                Call lstRelated.AddRange(lstBuff)
            End If

            Return lstRelated.ToArray  '只返回下游的第一个基因
        End Function

        Private Function __atgDistance(Of T_Gene As I_GeneBrief)(Gene As T_Gene, LociLocation As NucleotideLocation) As Integer
            Call LociLocation.Normalization()
            Call Gene.Location.Normalization()

            If Gene.Location.Strand = Strands.Forward Then
                Return Math.Abs(Gene.Location.Right - LociLocation.Left)
            Else
                Return Math.Abs(Gene.Location.Left - LociLocation.Right)
            End If
        End Function

        ''' <summary>
        ''' Calculates the ATG distance between the target gene and a loci segment on.(计算位点相对于某一个基因的ATG距离)
        ''' </summary>
        ''' <param name="loci"></param>
        ''' <param name="Gene"></param>
        ''' <returns>总是计算最大的距离</returns>
        ''' <remarks></remarks>
        <Extension> Public Function GetATGDistance(loci As ComponentModel.Loci.Location, Gene As I_GeneBrief) As Integer
            Call loci.Normalization()
            Call Gene.Location.Normalization()

            If Gene.Location.Strand = Strands.Forward Then '直接和左边相减
                Return loci.Right - Gene.Location.Left
            ElseIf Gene.Location.Strand = Strands.Reverse Then  '互补链方向的基因，则应该减去右边
                Return Gene.Location.Right - loci.Left
            Else
                Return loci.Left - Gene.Location.Left
            End If
        End Function

        ''' <summary>
        ''' Gets the loci location description data.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="posi"></param>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function LocationDescription(Of T As I_GeneBrief)(posi As SegmentRelationships, data As T) As String
            If IsBlankSegment(data) Then
                Return "Intergenic region"

            ElseIf posi = SegmentRelationships.DownStream Then
                Return String.Format("In the downstream of [{0}] gene ORF.", data.Identifier)
            ElseIf posi = SegmentRelationships.Equals Then
                Return String.Format("Is [{0}] gene ORF.", data.Identifier)
            ElseIf posi = SegmentRelationships.Inside Then
                Return String.Format("Inside the [{0}] gene ORF.", data.Identifier)
            ElseIf posi = SegmentRelationships.DownStreamOverlap Then
                Return String.Format("Overlap on down_stream with [{0}] gene ORF.", data.Identifier)
            ElseIf posi = SegmentRelationships.UpStreamOverlap Then
                Return String.Format("Overlap on up_stream with [{0}] gene ORF.", data.Identifier)
            Else
                Return String.Format("In the promoter region of [{0}] gene ORF.", data.Identifier)
            End If
        End Function
    End Module
End Namespace