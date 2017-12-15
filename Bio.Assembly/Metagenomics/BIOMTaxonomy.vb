﻿#Region "Microsoft.VisualBasic::21c653407fefa1c4fdffb96a564b8248, ..\GCModeller\core\Bio.Assembly\Metagenomics\BIOMTaxonomy.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.genomics.Assembly.NCBI.Taxonomy

Namespace Metagenomics

    Public Module BIOMTaxonomy

        ''' <summary>
        ''' ``k__{x.superkingdom};p__{x.phylum};c__{x.class};o__{x.order};f__{x.family};g__{x.genus};s__{x.species}``
        ''' </summary>
        Public ReadOnly Property BIOMPrefix As String() = {"k__", "p__", "c__", "o__", "f__", "g__", "s__"}

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' Example:
        ''' 
        ''' ```
        ''' no rank__cellular organisms;superkingdom__Eukaryota;no rank__Opisthokonta;kingdom__Fungi;subkingdom__Dikarya;phylum__Ascomycota;no rank__saccharomyceta;subphylum__Pezizomycotina;no rank__leotiomyceta;no rank__dothideomyceta;class__Dothideomycetes;no rank__Dothideomycetes incertae sedis;order__Botryosphaeriales;family__Botryosphaeriaceae;genus__Macrophomina;species__Macrophomina phaseolina;no rank__Macrophomina phaseolina MS6
        ''' ```
        ''' </remarks>
        Public ReadOnly Property BIOMPrefixAlt As Index(Of String) = {
            "superkingdom__", "phylum__", "class__", "order__", "family__", "genus__", "species__"
        }

        Public ReadOnly Property BriefParser As DefaultValue(Of TaxonomyLineageParser) = New TaxonomyLineageParser(AddressOf TaxonomyParser)
        Public ReadOnly Property CompleteParser As DefaultValue(Of TaxonomyLineageParser) = New TaxonomyLineageParser(AddressOf TaxonomyParserAlt)

        Public Delegate Function TaxonomyLineageParser(taxonomy As String) As Dictionary(Of String, String)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TaxonomyString(lineage As String()) As String
            Return lineage _
                .SeqIterator _
                .Select(Function(l) BIOMPrefix(l.i) & l.value) _
                .JoinBy(";")
        End Function

        ReadOnly descRanks$() = NcbiTaxonomyTree.stdranks.Reverse.ToArray

        ''' <summary>
        ''' 这个函数不像<see cref="TaxonomyParser"/>和<see cref="TaxonomyParserAlt"/>这两个是专门针对
        ''' 具有分类信息前缀的字符串的解析。这个函数是主要针对于没有分类层次信息前缀的字符串的解析函数，
        ''' 由于没有分类信息的前缀字符串，故而这个函数也主要是依靠tokens之间的前后顺序来赋值分类的rank信息
        ''' </summary>
        ''' <param name="lineage">从大到小排序的</param>
        ''' <returns></returns>
        <Extension>
        Public Function TaxonomyFromString(lineage$, Optional deli As Char = ";"c) As Taxonomy
            ' 有些分类字符串里面会存在[....]这种形式的格式，不清楚是什么意思
            ' 在这里去除掉[和]
            Dim tokens$() = lineage _
                .Replace("[", "") _
                .Replace("]", "") _
                .Split(deli) _
                .Select(AddressOf Trim) _
                .ToArray
            Dim taxonomyRanks = tokens _
                .SeqIterator _
                .ToDictionary(Function(i) descRanks(i),
                              Function(rank) rank.value)

            Return New Taxonomy(taxonomyRanks)
        End Function

#Region "Parsing BIOM style taxonomy string"

        ''' <summary>
        ''' For <see cref="BIOMPrefix"/>
        ''' </summary>
        ''' <param name="taxonomy$"></param>
        ''' <returns></returns>
        Public Function TaxonomyParser(taxonomy$) As Dictionary(Of String, String)
            Dim tokens$() = taxonomy.Split(";"c) _
                .Select(AddressOf Strings.Trim) _
                .ToArray
            Dim catalogs As NamedValue(Of String)() = tokens _
                .Select(Function(t) t.GetTagValue("__")) _
                .ToArray
            Dim out As New Dictionary(Of String, String)

            For Each x As NamedValue(Of String) In catalogs
                Dim name$ = x.Value

                Select Case x.Name
                    Case "k" : Call out.Add(NcbiTaxonomyTree.superkingdom, name)
                    Case "p" : Call out.Add(NcbiTaxonomyTree.phylum, name)
                    Case "c" : Call out.Add(NcbiTaxonomyTree.class, name)
                    Case "o" : Call out.Add(NcbiTaxonomyTree.order, name)
                    Case "f" : Call out.Add(NcbiTaxonomyTree.family, name)
                    Case "g" : Call out.Add(NcbiTaxonomyTree.genus, name)
                    Case "s" : Call out.Add(NcbiTaxonomyTree.species, name)
                    Case Else
                End Select
            Next

            Return out
        End Function

        ''' <summary>
        ''' For <see cref="BIOMPrefixAlt"/>
        ''' </summary>
        ''' <param name="taxonomy$"></param>
        ''' <returns></returns>
        Public Function TaxonomyParserAlt(taxonomy$) As Dictionary(Of String, String)
            Dim tokens$() = taxonomy.Split(";"c)
            Dim catalogs As NamedValue(Of String)() = tokens _
                .Select(Function(t) t.GetTagValue("__")) _
                .ToArray
            Dim out As New Dictionary(Of String, String)

            For Each x As NamedValue(Of String) In catalogs
                If _BIOMPrefixAlt.IndexOf(x.Name) > -1 Then
                    Call out.Add(x.Name, x.Value)
                End If
            Next

            Return out
        End Function
#End Region

        <Extension>
        Public Function FillLineageEmpty(lineage As Dictionary(Of String, String)) As Dictionary(Of String, String)
            For Each level As String In NcbiTaxonomyTree.stdranks
                If Not lineage.ContainsKey(level) Then
                    Call lineage.Add(level, "NA")
                End If
            Next

            Return lineage
        End Function
    End Module
End Namespace
