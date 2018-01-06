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

        ''' <summary>
        ''' The number of genes in this genome
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property N As Integer
            Get
                Return plus.Length + minus.Length
            End Get
        End Property

        Public ReadOnly Property AllFeatureKeys As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return featureTags.Keys.ToArray
            End Get
        End Property

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

        ''' <summary>
        ''' The number of genes between feature 1 and feature 2.
        ''' </summary>
        ''' <param name="feature1$"></param>
        ''' <param name="feature2$"></param>
        ''' <returns></returns>
        Public Function Delta(feature1$, feature2$) As Integer

        End Function

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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Absent(feature As String) As Boolean
            Return Not featureTags.ContainsKey(feature)
        End Function

        Public Overrides Function ToString() As String
            Return $"{contextName}: {plus.Length} (+), {minus.Length} (-) with {featureTags.Count} features."
        End Function
    End Class
End Namespace