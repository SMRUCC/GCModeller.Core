Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.genomics.ComponentModel
Imports SMRUCC.genomics.ComponentModel.Loci

Namespace ContextModel

    Public Class GenomeContext(Of T As IGeneBrief)

        Dim plus As T()
        Dim minus As T()
        Dim featureTags As Dictionary(Of String, T())
        ''' <summary>
        ''' The name of this genome
        ''' </summary>
        Dim contextName$

        Sub New(genome As IEnumerable(Of T), Optional name$ = "unnamed")
            featureTags = genome _
                .GroupBy(Function(g)
                             If g.Feature.IsNullOrEmpty Then
                                 Return "-"
                             Else
                                 Return g.Feature
                             End If
                         End Function) _
                .ToDictionary(Function(g) g.Key,
                              Function(g) g.ToArray)

            plus = selectByStrand(Strands.Forward)
            minus = selectByStrand(Strands.Reverse)
            contextName = name
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function selectByStrand(strand As Strands) As T()
            Return featureTags _
                .Values _
                .IteratesALL _
                .Where(Function(g) g.Location.Strand = strand) _
                .ToArray
        End Function

        Public Function GetByFeature(feature As String) As T()
            If featureTags.ContainsKey(feature) Then
                Return featureTags(feature)
            Else
                Return {}
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{contextName}: {plus.Length} (+), {minus.Length} (-) with {featureTags.Count} features."
        End Function
    End Class
End Namespace