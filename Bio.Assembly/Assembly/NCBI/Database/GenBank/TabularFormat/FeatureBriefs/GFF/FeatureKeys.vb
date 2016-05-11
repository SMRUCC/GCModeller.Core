Imports System.Text.RegularExpressions
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels
Imports System.Text
Imports LANS.SystemsBiology.ComponentModel.Loci

Namespace Assembly.NCBI.GenBank.TabularFormat

    Public Module FeatureKeys

        Public Const tRNA As String = "tRNA"
        Public Const CDS As String = "CDS"
        Public Const exon As String = "exon"
        Public Const gene As String = "gene"
        Public Const tmRNA As String = "tmRNA"
        Public Const rRNA As String = "rRNA"
        Public Const region As String = "region"

        Public Enum Features As Integer
            CDS = -1
            gene = 0
            tRNA
            exon
            tmRNA
            rRNA
            region
        End Enum

        Public ReadOnly Property FeaturesHash As IReadOnlyDictionary(Of String, Features) =
            New Dictionary(Of String, Features) From {
 _
            {FeatureKeys.CDS, Features.CDS},
            {FeatureKeys.exon, Features.exon},
            {FeatureKeys.gene, Features.gene},
            {FeatureKeys.region, Features.region},
            {FeatureKeys.rRNA, Features.rRNA},
            {FeatureKeys.tmRNA, Features.tmRNA},
            {FeatureKeys.tRNA, Features.tRNA}
        }
    End Module
End Namespace