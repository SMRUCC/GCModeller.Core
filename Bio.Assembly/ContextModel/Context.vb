Imports LANS.SystemsBiology.ComponentModel
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports LANS.SystemsBiology.SequenceModel.NucleotideModels
Imports Microsoft.VisualBasic.Serialization

Namespace ContextModel

    Public Structure Context

        Public ReadOnly Feature As NucleotideLocation
        Public ReadOnly Upstream As NucleotideLocation
        Public ReadOnly Downstream As NucleotideLocation
        Public ReadOnly Antisense As NucleotideLocation
        Public ReadOnly Tag As String

        Sub New(g As IGeneBrief, dist As Integer)
            Call Me.New(g.Location, dist, g.ToString)
        End Sub

        Sub New(loci As NucleotideLocation, dist As Integer, Optional tag As String = Nothing)
            Feature = loci
            tag = NotEmpty(tag, loci.ToString)

            If loci.Strand = Strands.Forward Then
                Upstream = New NucleotideLocation(loci.Left - dist, loci.Left, Strands.Forward)
                Downstream = New NucleotideLocation(loci.Right, loci.Right + dist, Strands.Forward)
                Antisense = New NucleotideLocation(loci.Left, loci.Right, Strands.Reverse)
            Else
                Upstream = New NucleotideLocation(loci.Right, loci.Right + dist, Strands.Reverse)
                Downstream = New NucleotideLocation(loci.Left - dist, loci.Left, Strands.Reverse)
                Antisense = New NucleotideLocation(loci.Left, loci.Right, Strands.Forward)
            End If
        End Sub

        Sub New(g As Contig, dist As Integer)
            Call Me.New(g.MappingLocation, dist, g.ToString)
        End Sub

        Public Function GetRelation(loci As NucleotideLocation, stranded As Boolean) As SegmentRelationships
            If stranded Then
                Return __relStranede(loci)
            Else
                Return __relUnstrand(loci)
            End If
        End Function

        Private Function __relUnstrand(loci As NucleotideLocation) As SegmentRelationships
            Dim rel As SegmentRelationships = __getRel(loci)

            If rel = SegmentRelationships.Blank Then
                If loci.Strand <> Feature.Strand Then
                    If Antisense.IsInside(loci) Then
                        Return SegmentRelationships.InnerAntiSense
                    End If
                End If
            End If

            Return rel
        End Function

        Private Function __getRel(loci As NucleotideLocation) As SegmentRelationships
            If Upstream.IsInside(loci) Then
                Return SegmentRelationships.UpStream
            ElseIf Downstream.IsInside(loci) Then
                Return SegmentRelationships.DownStream
            ElseIf Feature.Equals(loci, 1) Then
                Return SegmentRelationships.Equals
            ElseIf Feature.IsInside(loci) Then
                Return SegmentRelationships.Inside
            Else
                If Feature.IsInside(loci.Left) AndAlso Upstream.IsInside(loci.Right) Then
                    Return SegmentRelationships.UpStreamOverlap
                ElseIf Feature.IsInside(loci.Right) AndAlso Upstream.IsInside(loci.Left) Then
                    Return SegmentRelationships.UpStreamOverlap
                ElseIf Feature.IsInside(loci.Left) AndAlso Downstream.IsInside(loci.Right) Then
                    Return SegmentRelationships.DownStreamOverlap
                ElseIf Feature.IsInside(loci.Right) AndAlso Downstream.IsInside(loci.Left) Then
                    Return SegmentRelationships.DownStreamOverlap
                ElseIf loci.IsInside(Feature) Then
                    Return SegmentRelationships.Cover
                Else
                    Return SegmentRelationships.Blank
                End If
            End If
        End Function

        Private Function __relStranede(loci As NucleotideLocation) As SegmentRelationships
            If loci.Strand <> Feature.Strand Then  ' 不在同一条链之上
                If Antisense.IsInside(loci) Then
                    Return SegmentRelationships.InnerAntiSense
                Else
                    Return SegmentRelationships.Blank
                End If
            Else
                Return __getRel(loci)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return Tag
        End Function
    End Structure
End Namespace