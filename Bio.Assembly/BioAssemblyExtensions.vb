﻿Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.GBFF.Keywords.FEATURES
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports LANS.SystemsBiology.SequenceModel
Imports LANS.SystemsBiology.SequenceModel.NucleotideModels
Imports LANS.SystemsBiology.SequenceModel.FASTA
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels
Imports Microsoft.VisualBasic.Language

<PackageNamespace("Bio.Extensions", Publisher:="xie.guigang@gcmodeller.org")>
Public Module BioAssemblyExtensions

    ''' <summary>
    ''' 将COG字符串进行修剪，返回的是大写的COG符号
    ''' COG4771P -> P;   
    ''' P -> P;   
    ''' &lt;SPACE> -> -;   
    ''' - -> -;
    ''' </summary>
    ''' <param name="str"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("TrimText.COG")>
    <Extension> Public Function GetCOGCategory(str As String) As String
        If String.IsNullOrEmpty(str) OrElse String.Equals("-", str) Then
            Return "-"
        Else
            str = Regex.Replace(str, "COG\d+", "", RegexOptions.IgnoreCase)
            str = str.ToUpper
            Return str
        End If
    End Function

    ''' <summary>
    ''' Generate <see cref="FastaFile"/> from a specific fasta source collection.
    ''' </summary>
    ''' <typeparam name="TFasta"><see cref="FastaToken"/></typeparam>
    ''' <param name="data">Target fasta source collection which its elements base type is <see cref="fastaToken"/></param>
    ''' <returns></returns>
    <Extension> Public Function [DirectCast](Of TFasta As FASTA.FastaToken)(data As IEnumerable(Of TFasta)) As FASTA.FastaFile
        Dim FastaFile As New FASTA.FastaFile(From Fasta As TFasta In data Select DirectCast(Fasta, FASTA.FastaToken))
        Return FastaFile
    End Function

    ''' <summary>
    ''' Convert the nucleotide sequence strand direction enumeration as character brief code. [<see cref="ComponentModel.Loci.Strands"/> => +, -, ?]
    ''' </summary>
    ''' <param name="strand"></param>
    ''' <returns></returns>
    <Extension> Public Function GetBriefCode(strand As Strands) As String
        Select Case strand
            Case Strands.Forward : Return "+"
            Case Strands.Reverse : Return "-"
            Case Else
                Return "?"
        End Select
    End Function

    ''' <summary>
    ''' Convert the nucleotide seuqnece strand description word as character brief code.
    ''' (获取核酸链链方向的描述简要代码)
    ''' </summary>
    ''' <param name="strand"></param>
    ''' <returns></returns>
    <Extension> Public Function GetBriefStrandCode(strand As String) As String
        Dim value As Strands = GetStrand(strand)
        Return value.GetBriefCode
    End Function

    ''' <summary>
    ''' Convert the string value type nucleotide strand information description data into a strand enumerate data.
    ''' </summary>
    ''' <param name="c">从文本文件之中所读取出来关于链方向的字符串描述数据</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetStrands(c As Char) As Strands
        Return CStr(c).GetStrand
    End Function

    ''' <summary>
    ''' Is this ORF is in the reversed strand direction?(判断一段ORF核酸序列是否为反向的)
    ''' </summary>
    ''' <param name="nt">
    ''' This function parameter is only allowed nucleotide sequence.
    ''' (请注意，这个只允许核酸序列)
    ''' </param>
    ''' <returns></returns>
    <Extension> Public Function IsReversed(nt As I_PolymerSequenceModel) As Boolean
        If Not InStrAny(nt.SequenceData, "ATG", "GTG") = 1 Then
            Dim last As String = Mid(nt.SequenceData, Len(nt.SequenceData) - 3, 3)
            Return Not String.IsNullOrEmpty(last.EqualsAny("GTG", "GTA"))
        Else
            Return False ' 起始密码子不在最开始的位置，不是反向的
        End If
    End Function

    <Extension> Public Function CreatePTTObject(contigs As IEnumerable(Of SimpleSegment)) As TabularFormat.PTT
        Dim genes As GeneBrief() = contigs.ToArray(Function(gene) gene.ToPTTGene)
        Dim PTT As New TabularFormat.PTT(genes)
        Return PTT
    End Function

    ''' <summary>
    ''' 对位点进行分组操作
    ''' </summary>
    ''' <param name="contigs"></param>
    ''' <param name="offsets"></param>
    ''' <returns></returns>
    <Extension> Public Function Group(Of Contig As NucleotideModels.Contig)(
                                         contigs As IEnumerable(Of Contig),
                                         Optional offsets As Integer = 5) As Dictionary(Of Integer, Contig())

        Dim Groups As New Dictionary(Of Integer, List(Of Contig))
        Dim idx As Integer = 1

        For Each loci As NucleotideModels.Contig In contigs
            Dim equalContig As Func(Of IEnumerable(Of Contig), Contig) =
                Function(value) LinqAPI.DefaultFirst(Of Contig) <= From site As Contig
                                                                   In value
                                                                   Let siteMap As NucleotideLocation =
                                                                       site.MappingLocation
                                                                   Where siteMap.Equals(loci.MappingLocation, offsets)
                                                                   Select site
            Dim hash As Integer =
                LinqAPI.DefaultFirst(Of Integer) <= From x
                                                    In Groups.AsParallel
                                                    Let equal As Contig = equalContig(x.Value)
                                                    Where Not equal Is Nothing
                                                    Select x.Key
            If hash < 1 Then
                Call Groups.Add(idx.MoveNext, New List(Of Contig) From {loci})      ' 新的分组
            Else
                Dim lst As List(Of Contig) = Groups(hash)
                Call lst.Add(loci)
            End If
        Next

        Return Groups.ToDictionary(Function(x) x.Key, Function(x) x.Value.ToArray)
    End Function
End Module
