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

        End Function

        Private Function __relStranede(loci As NucleotideLocation) As SegmentRelationships
            If loci.Strand <> Feature.Location.Strand Then

            End If
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace