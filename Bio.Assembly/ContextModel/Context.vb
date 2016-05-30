Imports LANS.SystemsBiology.ComponentModel
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports Microsoft.VisualBasic.Serialization

Namespace ContextModel

    Public Structure Context(Of T As IGeneBrief)

        Public ReadOnly Feature As T
        Public ReadOnly Upstream As NucleotideLocation
        Public ReadOnly Downstream As NucleotideLocation
        Public ReadOnly Antisense As NucleotideLocation

        Sub New(g As T, dist As Integer)
            Feature = g

            If g.Location.Strand = Strands.Forward Then
                Upstream = New NucleotideLocation(g.Location.Left - dist, g.Location.Left, Strands.Forward)
                Downstream = New NucleotideLocation(g.Location.Right, g.Location.Right + dist, Strands.Forward)
                Antisense = New NucleotideLocation(g.Location.Left, g.Location.Right, Strands.Reverse)
            Else
                Upstream = New NucleotideLocation(g.Location.Right, g.Location.Right + dist, Strands.Reverse)
                Downstream = New NucleotideLocation(g.Location.Left - dist, g.Location.Left, Strands.Reverse)
                Antisense = New NucleotideLocation(g.Location.Left, g.Location.Right, Strands.Forward)
            End If
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
                If loci.Strand <> Feature.Location.Strand Then
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
            ElseIf Feature.Location.Equals(loci, 1) Then
                Return SegmentRelationships.Equals
            ElseIf Feature.Location.IsInside(loci) Then
                Return SegmentRelationships.Inside
            Else
                If Feature.Location.IsInside(loci.Left) AndAlso Upstream.IsInside(loci.Right) Then
                    Return SegmentRelationships.UpStreamOverlap
                ElseIf Feature.Location.IsInside(loci.Right) AndAlso Upstream.IsInside(loci.Left) Then
                    Return SegmentRelationships.UpStreamOverlap
                ElseIf Feature.Location.IsInside(loci.Left) AndAlso Downstream.IsInside(loci.Right) Then
                    Return SegmentRelationships.DownStreamOverlap
                ElseIf Feature.Location.IsInside(loci.Right) AndAlso Downstream.IsInside(loci.Left) Then
                    Return SegmentRelationships.DownStreamOverlap
                ElseIf loci.IsInside(Feature.Location) Then
                    Return SegmentRelationships.Cover
                Else
                    Return SegmentRelationships.Blank
                End If
            End If
        End Function

        Private Function __relStranede(loci As NucleotideLocation) As SegmentRelationships
            If loci.Strand <> Feature.Location.Strand Then  ' 不在同一条链之上
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
            Return Me.GetJson
        End Function
    End Structure
End Namespace