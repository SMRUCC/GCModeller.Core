Imports System.Runtime.CompilerServices
Imports SMRUCC.genomics.ComponentModel.Loci

Namespace ContextModel.Operon

    ''' <summary>
    ''' To evaluate the contribution of selected features in operon prediction, we have calculated 
    ''' the numerical values of the features, And then used these values individually And in combination 
    ''' to train a classifier. The features used in our study are
    ''' 
    ''' + (i)   the intergenic distance, 
    ''' + (ii)  the conserved gene neighborhood, 
    ''' + (iii) distances between adjacent genes' phylogenetic profiles, 
    ''' + (iv)  the ratio between the lengths of two adjacent genes And 
    ''' + (v)   frequencies Of specific DNA motifs in the intergenic regions.
    ''' </summary>
    Public Module FeatureScores

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function IntergenicDistance(upstream As NucleotideLocation, downstream As NucleotideLocation) As Integer
            If upstream.Strand <> downstream.Strand Then
                Throw New Exception("Invalid strand data!")
            Else
                If upstream.Strand = Strands.Forward Then
                    Return (downstream.Left - (upstream.Right + 1))
                Else
                    Return (upstream.Left - (downstream.Right + 1))
                End If
            End If
        End Function

        ''' <summary>
        ''' 这个函数定义了基因i和基因j在某一个基因组之中是相邻的概率高低
        ''' </summary>
        ''' <returns></returns>
        Public Function NeighborhoodConservation()

        End Function
    End Module
End Namespace