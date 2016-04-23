Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.GBFF.Keywords.FEATURES.Nodes
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports LANS.SystemsBiology.SequenceModel
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic
Imports LANS.SystemsBiology.SequenceModel.NucleotideModels
Imports LANS.SystemsBiology.SequenceModel.FASTA

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

    <Extension> Public Function [DirectCast](Of TFasta As FASTA.FastaToken)(data As IEnumerable(Of TFasta)) As FASTA.FastaFile
        Dim LQuery = (From Fasta As TFasta In data Select CType(Fasta, FASTA.FastaToken)).ToArray
        Dim FastaFile = CType(LQuery, FASTA.FastaFile)
        Return FastaFile
    End Function

    ''' <summary>
    ''' 尝试从一段给定的核酸序列之中寻找出可能的最长的ORF阅读框
    ''' </summary>
    ''' <param name="Nt">请注意这个函数总是从左往右进行计算的，所以请确保这个参数是正义链的或者反义链的已经反向互补了</param>
    ''' <param name="ATG"></param>
    ''' <param name="TGA"></param>
    ''' <returns></returns>
    Public Function Putative_mRNA(Nt As String, ByRef ATG As Integer, ByRef TGA As Integer, Optional ByRef ORF As String = "") As Boolean
        ATG = InStr(Nt, "ATG")

        If Not ATG > 0 Then
[NOTHING]:
            ATG = -1
            TGA = -1 '找不到ATG
            Return False
        End If

        Dim TGAList As New List(Of Integer)  '从最后段开始往前数

        For i As Integer = 1 To Len(Nt)
            Dim p As Integer = InStr(i, Nt, "TGA")
            If p > 0 Then
                Call TGAList.Add(p)
                i = p
            Else
                Exit For      '已经再也找不到位点了
            End If
        Next

        If TGAList.IsNullOrEmpty Then
            GoTo [NOTHING]     '找不到任何TGA位点
        Else
            Call TGAList.Reverse()
        End If

        For i As Integer = ATG To Len(Nt)

            ATG = InStr(i, Nt, "ATG")

            If Not ATG > 0 Then
                GoTo [NOTHING]
            End If

            For Each Point In TGAList  '从最大的开始进行匹配，要满足长度为3的整数倍这个条件，一旦满足则开始进行成分测试，假若通过则认为找到了一个可能的阅读框
                TGA = Point

                If TGA - ATG < 20 Then
                    Continue For
                End If

                '并且需要两个位点的位置的长度为3的整数倍
                Dim ORF_Length As Integer = TGA - ATG + 1
                Dim n As Integer = ORF_Length Mod 3

                If n = 0 Then  '长度为3的整数倍，  
                    '取出序列和相邻的序列进行GC比较，差异较明显则认为是
                    ORF = Mid(Nt, ATG, ORF_Length + 2)

                    If ORF_Length < 30 Then
                        Continue For
                    End If

                    Dim Adjacent As String = Mid(Nt, 1, ATG)
                    Dim a As Double = LANS.SystemsBiology.SequenceModel.NucleotideModels.GCContent(ORF)
                    Dim b As Double = LANS.SystemsBiology.SequenceModel.NucleotideModels.GCContent(Adjacent)
                    Dim d As Double = Math.Abs(a - b)
                    Dim accept As Boolean = d > 0.1
#Const DEBUG = 0
#If DEBUG Then
                    Call Console.WriteLine($"[DEBUG {Now.ToString}] ORF({ATG},{TGA})     {NameOf(Nt)}({a}) -->  {NameOf(Adjacent)}({b})   =====> d_gc%={d};   accept? {accept }")
#End If
                    If accept Then
                        Return True
                    End If

                End If

            Next

        Next

        GoTo [NOTHING]
    End Function

    ''' <summary>
    ''' <see cref="ComponentModel.Loci.Strands"/> => +, -, ?
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
    ''' 获取核酸链链方向的描述简要代码
    ''' </summary>
    ''' <param name="strand"></param>
    ''' <returns></returns>
    <Extension> Public Function GetBriefStrandCode(strand As String) As String
        Dim strandValue As Strands = GetStrand(strand)
        Return strandValue.GetBriefCode
    End Function

    ''' <summary>
    ''' Convert the string value type nucleotide strand information description data into a strand enumerate data.
    ''' </summary>
    ''' <param name="str">从文本文件之中所读取出来关于链方向的字符串描述数据</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function GetStrands(c As Char) As Strands
        Return CStr(c).GetStrand
    End Function

    ''' <summary>
    ''' 判断一段ORF核酸序列是否为反向的
    ''' </summary>
    ''' <param name="nt">请注意，这个只允许核酸序列</param>
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
        Dim Genes = contigs.ToArray(Function(gene) gene.ToPTTGene)
        Dim PTT As New TabularFormat.PTT(Genes)
        Return PTT
    End Function

    <ExportAPI("Features.Dump")>
    Public Function FeatureDumps(gb As GBFF.File, Optional features As String() = Nothing, Optional dumpAll As Boolean = False) As GeneDumpInfo()
        If dumpAll Then
            Dim fs = (From x In gb.Features.ToArray Where x.ContainsKey("gene") Select x).ToArray
            Return __dumpCDS(fs)
        End If

        If features Is Nothing Then features = {"5'UTR", "CDS", "regulatory", "misc_feature", "3'UTR"}

        Dim List As New List(Of GeneDumpInfo)
        For Each feature As String In features
            Dim fs = gb.Features.ListFeatures(feature)
            Call List.Add(_dumpMethods(feature)(fs))
        Next

        Return List.ToArray
    End Function

#Region "Dump Methods"

    Dim _dumpMethods As Dictionary(Of String, Func(Of Feature(), GeneDumpInfo())) =
        New Dictionary(Of String, Func(Of Feature(), GeneDumpInfo())) From {
            {"5'UTR", AddressOf __dump5UTRs},
            {"3'UTR", AddressOf __dump3UTRs},
            {"CDS", AddressOf __dumpCDS},
            {"regulatory", AddressOf __dumpRegulatory},
            {"misc_feature", AddressOf __dumpMiscFeature}
    }

    Private Function __dumpMiscFeature(features As Feature()) As LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports.GeneDumpInfo()
        Dim dump = features.ToArray(
            Function(feature) _
                New LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports.GeneDumpInfo With {
                  .COG = "misc_feature",
                  .Function = feature("note"),
                  .CommonName = feature("note"),
                  .Location = feature.Location.ContiguousRegion,
                  .LocusID = feature("locus_tag"),
                  .GeneName = feature("gene") & "_mics_feature",
                  .Translation = feature("translation"),
                  .ProteinId = feature("protein_id"),
                  .CDS = feature.SequenceData
              })
        Return dump
    End Function

    Private Function __dumpRegulatory(features As Feature()) As LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports.GeneDumpInfo()
        Dim dump = features.ToArray(
            Function(feature) _
                New LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports.GeneDumpInfo With {
                  .COG = "regulatory",
                  .Function = feature("regulatory_class"),
                  .CommonName = feature("note"),
                  .Location = feature.Location.ContiguousRegion,
                  .LocusID = feature("locus_tag"),
                  .GeneName = feature("gene") & "_regulatory",
                  .Translation = feature("translation"),
                  .ProteinId = feature("protein_id"),
                  .CDS = feature.SequenceData
              })
        Return dump
    End Function

    Private Function __dumpCDS(features As Feature()) As LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports.GeneDumpInfo()
        Dim dump = features.ToArray(
          Function(feature) _
              New LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports.GeneDumpInfo With {
                  .COG = "CDS",
                  .Function = feature("function"),
                  .CommonName = feature("note"),
                  .Location = feature.Location.ContiguousRegion,
                  .LocusID = feature("locus_tag"),
                  .GeneName = feature("gene"),
                  .Translation = feature("translation"),
                  .ProteinId = feature("protein_id"),
                  .CDS = feature.SequenceData
              })
        Return dump
    End Function

    <Extension> Private Function __dump5UTRs(features As Feature()) As LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports.GeneDumpInfo()
        Dim dump = features.ToArray(
            Function(feature) _
                New LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports.GeneDumpInfo With {
                    .COG = "5'UTR",
                    .Function = feature("function"),
                    .CommonName = feature("note"),
                    .Location = feature.Location.ContiguousRegion,
                    .LocusID = $"5'UTR_{feature.Location.ContiguousRegion.Left}..{feature.Location.ContiguousRegion.Right}",
                    .GeneName = $"5'UTR_{feature.Location.ContiguousRegion.Left}..{feature.Location.ContiguousRegion.Right}",
                    .CDS = feature.SequenceData
                })
        Return dump
    End Function

    <Extension> Private Function __dump3UTRs(features As Feature()) As LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports.GeneDumpInfo()
        Dim dump = features.ToArray(
            Function(feature) _
                New LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports.GeneDumpInfo With {
                    .COG = "3'UTR",
                    .Function = feature("function"),
                    .CommonName = feature("note"),
                    .Location = feature.Location.ContiguousRegion,
                    .LocusID = $"3'UTR_{feature.Location.ContiguousRegion.Left}..{feature.Location.ContiguousRegion.Right}",
                    .GeneName = $"3'UTR_{feature.Location.ContiguousRegion.Left}..{feature.Location.ContiguousRegion.Right}",
                    .CDS = feature.SequenceData
                })
        Return dump
    End Function
#End Region

    ''' <summary>
    ''' 对位点进行分组操作
    ''' </summary>
    ''' <param name="contigs"></param>
    ''' <param name="offsets"></param>
    ''' <returns></returns>
    <Extension> Public Function Group(Of Contig As NucleotideModels.Contig)(contigs As IEnumerable(Of Contig), Optional offsets As Integer = 5) As Dictionary(Of Integer, Contig())
        Dim Groups As New Dictionary(Of Integer, List(Of Contig))
        Dim idx As Integer = 1

        For Each loci As SequenceModel.NucleotideModels.Contig In contigs
            Dim hash As Integer = (From x In Groups.AsParallel
                                   Let equal = (From site As Contig In x.Value
                                                Where site.MappingLocation.Equals(loci.MappingLocation, offsets)
                                                Select site).FirstOrDefault
                                   Where Not equal Is Nothing
                                   Select x.Key).FirstOrDefault
            If hash < 1 Then
                Call Groups.Add(idx.MoveNext, New List(Of Contig) From {loci})      ' 新的分组
            Else
                Dim lst = Groups(hash)
                Call lst.Add(loci)
            End If
        Next

        Return Groups.ToDictionary(Function(x) x.Key, Function(x) x.Value.ToArray)
    End Function
End Module
