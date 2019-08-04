﻿#Region "Microsoft.VisualBasic::2289473da99bee6615f96a7251aefc86, Bio.Assembly\Assembly\NCBI\Database\GenBank\ExportServices\GBFFFeatureDumps.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Module GBFFFeatureDumps
    ' 
    '         Function: __dump3UTRs, __dump5UTRs, __dumpCDS, __dumpMiscFeature, __dumpRegulatory
    '                   FeatureDumps, GbffToPTT
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.genomics.Assembly.NCBI.GenBank.GBFF.Keywords.FEATURES
Imports SMRUCC.genomics.Assembly.NCBI.GenBank.TabularFormat
Imports SMRUCC.genomics.Assembly.NCBI.GenBank.TabularFormat.ComponentModels
Imports SMRUCC.genomics.ComponentModel.Annotation

Namespace Assembly.NCBI.GenBank

    Public Module GBFFFeatureDumps

        ''' <summary>
        ''' Dump feature sites information data into a tabular dataframe.
        ''' </summary>
        ''' <param name="gb"></param>
        ''' <param name="features"></param>
        ''' <param name="dumpAll"></param>
        ''' <returns></returns>
        <ExportAPI("Features.Dump")>
        <Extension>
        Public Function FeatureDumps(gb As GBFF.File,
                                     Optional features As String() = Nothing,
                                     Optional dumpAll As Boolean = False) As GeneTable()
            If dumpAll Then
                Dim fs As Feature() =
                    LinqAPI.Exec(Of Feature) <= From x As Feature
                                                In gb.Features
                                                Where x.ContainsKey("gene")
                                                Select x
                Return fs.__dumpCDS
            End If

            If features Is Nothing Then features = {"5'UTR", "CDS", "regulatory", "misc_feature", "3'UTR"}

            Dim result As New List(Of GeneTable)

            For Each feature As String In features
                Dim fs As Feature() = gb.Features.ListFeatures(feature)
                result += _dumpMethods(feature)(fs)
            Next

            Return result.ToArray
        End Function

#Region "Dump Methods"

        Dim _dumpMethods As Dictionary(Of String, Func(Of Feature(), GeneTable())) =
            New Dictionary(Of String, Func(Of Feature(), GeneTable())) From {
                {"5'UTR", AddressOf __dump5UTRs},
                {"3'UTR", AddressOf __dump3UTRs},
                {"CDS", AddressOf __dumpCDS},
                {"regulatory", AddressOf __dumpRegulatory},
                {"misc_feature", AddressOf __dumpMiscFeature}
        }

        Private Function __dumpMiscFeature(features As Feature()) As GeneTable()
            Dim dump As GeneTable() =
                LinqAPI.Exec(Of Feature, GeneTable)(features) <=
                    Function(feature As Feature) New GeneTable With {
                        .COG = "misc_feature",
                        .Function = feature("note"),
                        .CommonName = feature("note"),
                        .Location = feature.Location.ContiguousRegion,
                        .LocusID = feature("locus_tag"),
                        .GeneName = feature("gene") & "_mics_feature",
                        .Translation = feature("translation"),
                        .ProteinId = feature("protein_id"),
                        .CDS = feature.SequenceData
                    }
            Return dump
        End Function

        Private Function __dumpRegulatory(features As Feature()) As GeneTable()
            Dim dump As GeneTable() = features.Select(
                Function(feature) New GeneTable With {
                    .COG = "regulatory",
                    .Function = feature("regulatory_class"),
                    .CommonName = feature("note"),
                    .Location = feature.Location.ContiguousRegion,
                    .LocusID = feature("locus_tag"),
                    .GeneName = feature("gene") & "_regulatory",
                    .Translation = feature("translation"),
                    .ProteinId = feature("protein_id"),
                    .CDS = feature.SequenceData
               }).ToArray
            Return dump
        End Function

        <Extension>
        Private Function __dumpCDS(features As Feature()) As GeneTable()
            Dim dump As GeneTable() = features.Select(
                Function(feature) New GeneTable With {
                    .COG = "CDS",
                    .Function = feature("function"),
                    .CommonName = feature("note"),
                    .Location = feature.Location.ContiguousRegion,
                    .LocusID = feature("locus_tag"),
                    .GeneName = feature("gene"),
                    .Translation = feature("translation"),
                    .ProteinId = feature("protein_id"),
                    .CDS = feature.SequenceData
               }).ToArray
            Return dump
        End Function

        <Extension> Private Function __dump5UTRs(features As Feature()) As GeneTable()
            Dim dump As GeneTable() = features.Select(
                Function(feature) New GeneTable With {
                    .COG = "5'UTR",
                    .Function = feature("function"),
                    .CommonName = feature("note"),
                    .Location = feature.Location.ContiguousRegion,
                    .LocusID = $"5'UTR_{feature.Location.ContiguousRegion.Left}..{feature.Location.ContiguousRegion.Right}",
                    .GeneName = $"5'UTR_{feature.Location.ContiguousRegion.Left}..{feature.Location.ContiguousRegion.Right}",
                    .CDS = feature.SequenceData
                }).ToArray
            Return dump
        End Function

        <Extension> Private Function __dump3UTRs(features As Feature()) As GeneTable()
            Dim dump As GeneTable() = features.Select(
                Function(feature) New GeneTable With {
                    .COG = "3'UTR",
                    .Function = feature("function"),
                    .CommonName = feature("note"),
                    .Location = feature.Location.ContiguousRegion,
                    .LocusID = $"3'UTR_{feature.Location.ContiguousRegion.Left}..{feature.Location.ContiguousRegion.Right}",
                    .GeneName = $"3'UTR_{feature.Location.ContiguousRegion.Left}..{feature.Location.ContiguousRegion.Right}",
                    .CDS = feature.SequenceData
                }).ToArray
            Return dump
        End Function
#End Region

        <Extension>
        Public Function GbffToPTT(contextInfo As IEnumerable(Of GeneTable), size%, Optional title$ = "Unknown") As PTT
            Dim genes As GeneBrief() = contextInfo _
                .Select(Function(context) GeneBrief.CreateObject(g:=context)) _
                .ToArray
            Dim description As New PTT With {
                .GeneObjects = genes,
                .Size = size,
                .Title = title
            }

            Return description
        End Function
    End Module
End Namespace
