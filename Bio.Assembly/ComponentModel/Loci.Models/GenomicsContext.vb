Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels

Namespace ComponentModel.Loci.Abstract

    Public Interface IGenomicsContextProvider(Of T As I_GeneBrief)

        ''' <summary>
        ''' 获取某一个位点在位置上有相关联系的基因
        ''' </summary>
        ''' <param name="loci"></param>
        ''' <param name="unstrand"></param>
        ''' <param name="ATGDist"></param>
        ''' <returns></returns>
        Function GetRelatedGenes(loci As NucleotideLocation,
                                 Optional unstrand As Boolean = False,
                                 Optional ATGDist As Integer = 500) As Relationship(Of T)()

        Function GetStrandFeatures(strand As Strands) As T()
    End Interface
End Namespace