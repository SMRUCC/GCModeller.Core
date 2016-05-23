Imports LANS.SystemsBiology.ComponentModel
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports Microsoft.VisualBasic.ComponentModel.Ranges

Namespace ContextModel

    Public Class GenomeContextProvider(Of T As IGeneBrief)

        ReadOnly _forwards As OrderSelector(Of IntTag(Of T))
        ReadOnly _reversed As OrderSelector(Of IntTag(Of T))

        Sub New(genome As IGenomicsContextProvider(Of T))
            _forwards = IntTag(Of T).OrderSelector(
                genome.GetStrandFeatures(Strands.Forward),
                Function(x) x.Location.Left,
                True)
            _reversed = IntTag(Of T).OrderSelector(
                genome.GetStrandFeatures(Strands.Reverse),
                Function(x) x.Location.Right,
                False)
        End Sub

        Private Function __getLocationFunction(Of T As IGeneBrief)(
                                                GeneSegment As T,
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
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="LociStart"></param>
        ''' <param name="LociEnds"></param>
        ''' <param name="Strand"></param>
        ''' <returns></returns>
        Public Function GetInnerAntisense(Of T As IGeneBrief)(source As IEnumerable(Of T),
                                                                    LociStart As Integer,
                                                                    LociEnds As Integer,
                                                                    Strand As Strands) As T()
            Dim Raw As Relationship(Of T)() = source.GetRelatedGenes(LociStart, LociEnds, 0)
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
        <Extension> Public Function GetRelatedGenes(DataSource As IEnumerable(Of GeneBrief),
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
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="LociStart"></param>
        ''' <param name="LociEnds"></param>
        ''' <param name="ATGDistance"></param>
        ''' <returns>请注意，函数所返回的列表之中包含有不同的关系！</returns>
        ''' <remarks></remarks>
        <Extension> Public Function GetRelatedGenes(Of T As IGeneBrief)(
                                    source As IEnumerable(Of T),
                                    LociStart As Integer,
                                    LociEnds As Integer,
                                    Optional ATGDistance As Integer = 500) As Relationship(Of T)()
            Dim ntSite As New NucleotideLocation(
                LociStart,
                LociEnds,
                Strand:=Strands.Unknown)
            Return GetRelatedGenes(source, ntSite, ATGDistance)
        End Function

        Private Structure __getRelationDelegate(Of T As IGeneBrief)
            Dim DataSource As IEnumerable(Of T)
            Dim Loci As NucleotideLocation

            Public Function GetRelation(RelationType As SegmentRelationships) As T()
                Dim Loci As NucleotideLocation = Me.Loci

                Return (From GeneSegment As T
                        In DataSource
                        Let Relation = __getLocationFunction(GeneSegment, Loci)
                        Where Relation = RelationType
                        Select GeneSegment).ToArray
            End Function

            Public Function GetRelation(RelationType As SegmentRelationships, ATGDistance As Integer) As T()
                Dim Genes = GetRelation(RelationType)
                Dim Loci As ComponentModel.Loci.NucleotideLocation = Me.Loci
                Dim LQuery = (From GeneObject As T
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
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="Loci"></param>
        ''' <param name="ATGDistance"></param>
        ''' <returns></returns>
        <Extension> Public Function GetRelatedUpstream(Of T As IGeneBrief)(source As IEnumerable(Of T),
                                                                                 Loci As NucleotideLocation,
                                                                                 Optional ATGDistance As Integer = 2000) As Relationship(Of T)()
            Dim LociDelegate = New __getRelationDelegate(Of T)() With {
                .DataSource = source,
                .Loci = Loci.Normalization
            }
            Dim UpStreams As New KeyValuePair(Of SegmentRelationships, T())(
               SegmentRelationships.UpStream,
                LociDelegate.GetRelation(SegmentRelationships.UpStream, ATGDistance))
            Dim UpStreamOverlaps As New KeyValuePair(Of SegmentRelationships, T())(
               SegmentRelationships.UpStreamOverlap,
                LociDelegate.GetRelation(SegmentRelationships.UpStreamOverlap, ATGDistance))
            Dim array = {UpStreams, UpStreamOverlaps}
            Dim data0 = array.ToArray(Function(x) x.Value.ToArray(Function(g) New Relationship(Of T)(g, x.Key))).MatrixToList
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
        ''' <typeparam name="T"></typeparam>
        ''' <param name="source"></param>
        ''' <param name="ATGDistance"></param>
        ''' <returns>请注意，函数所返回的列表之中包含有不同的关系！</returns>
        ''' <remarks></remarks>
        <Extension> Public Function GetRelatedGenes(Of T As IGeneBrief)(source As IEnumerable(Of T), Loci As NucleotideLocation, Optional ATGDistance As Integer = 500) As Relationship(Of T)()
            Dim foundTEMP As T()
            Dim lstRelated As New List(Of Relationship(Of T))
            Dim LociDelegate As New __getRelationDelegate(Of T)() With {
                .DataSource = source,
                .Loci = Loci.Normalization
            }

            foundTEMP = LociDelegate.GetRelation(SegmentRelationships.UpStream)
            foundTEMP = (From GeneObject As T
                         In foundTEMP
                         Where Math.Abs(GetATGDistance(Loci, GeneObject)) <= ATGDistance
                         Select GeneObject).ToArray '获取ATG距离小于阈值的所有基因

            If Not foundTEMP.IsNullOrEmpty Then
                Dim lstBuff = (From Gene As T
                               In foundTEMP.AsParallel
                               Select New Relationship(Of T)(Gene, SegmentRelationships.UpStream)).ToArray
                Call lstRelated.AddRange(lstBuff)
            End If

            For Each RelationShip In New SegmentRelationships() {
 _
               SegmentRelationships.Equals,
               SegmentRelationships.Inside,
               SegmentRelationships.DownStreamOverlap,
               SegmentRelationships.UpStreamOverlap,
               SegmentRelationships.Cover
            }

                foundTEMP = LociDelegate.GetRelation(RelationShip)

                If Not foundTEMP.IsNullOrEmpty Then
                    Dim lstBuff = (From Gene As T
                                   In foundTEMP.AsParallel
                                   Select New Relationship(Of T)(Gene, RelationShip)).ToArray
                    Call lstRelated.AddRange(lstBuff)
                End If
            Next

            Dim DownStreamGenes = LociDelegate.GetRelation(SegmentRelationships.DownStream)
            Dim Dwsrt As T() = (From Gene As T
                                     In DownStreamGenes
                                Let Distance As Integer = LocationDescriptions.AtgDistance(Gene, Loci)
                                Where Distance <= ATGDistance
                                Select Gene).ToArray

            If Not Dwsrt.IsNullOrEmpty Then
                Dim lstBuff = (From Gene As T
                               In foundTEMP.AsParallel
                               Select New Relationship(Of T)(Gene, SegmentRelationships.DownStream)).ToArray
                Call lstRelated.AddRange(lstBuff)
            End If

            Return lstRelated.ToArray  '只返回下游的第一个基因
        End Function
    End Class
End Namespace