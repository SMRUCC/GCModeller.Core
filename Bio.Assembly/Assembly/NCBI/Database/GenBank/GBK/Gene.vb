﻿Imports System.Runtime.CompilerServices
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.GBFF.Keywords.FEATURES.Nodes
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic

Namespace Assembly.NCBI.GenBank.GBFF

    Public Class GeneObject
        Implements sIdEnumerable
        Implements IKeyValuePairObject(Of String, Feature())
        Implements ITripleKeyValuesPair(Of String, String, Feature())

        Public Property Gene As String Implements ITripleKeyValuesPair(Of String, String, Feature()).Value2
        Public Property LocusTag As String Implements sIdEnumerable.Identifier, IKeyValuePairObject(Of String, Feature()).Identifier,
            ITripleKeyValuesPair(Of String, String, Feature()).Identifier
        Public Property Features As Feature() Implements IKeyValuePairObject(Of String, Feature()).Value,
            ITripleKeyValuesPair(Of String, String, Feature()).Value3

        Public Overrides Function ToString() As String
            Return LocusTag
        End Function
    End Class

    <PackageNamespace("Genbank.Export_Genes")>
    Public Module ExportGenes

        <ExportAPI("GET.Genes")>
        <Extension> Public Function GetGenes(gb As File) As GeneObject()
            Dim GeneList = gb.GeneList
            Dim GQuery As Generic.IEnumerable(Of GeneObject) =
                From Gene In GeneList
                Let Query = (
                    From e In gb.Features._innerList Where String.Equals(e.Query("locus_tag"), Gene.Key)
                    Select e).ToArray
                Select New GeneObject With {
                    .LocusTag = Gene.Key,
                    .Gene = Gene.Value,
                    .Features = Query
                }

            Return GQuery.ToArray
        End Function

        <ExportAPI("GET.Gene")>
        <Extension> Public Function GetGene(gb As File, LocusTag As String) As GeneObject
            Dim GQuery =
                From Feature In gb.Features._innerList
                Where String.Equals(Feature.Query("locus_tag"), LocusTag)
                Select Feature '
            Dim List = GQuery.ToList
            Dim Gene = (From e In List Where String.Equals(e.KeyName, "gene") Select e).First

            Call List.Remove(Gene)

            Return New GeneObject With {
                .LocusTag = LocusTag,
                .Gene = Gene.Query("gene"),
                .Features = List.ToArray
            }
        End Function
    End Module
End Namespace