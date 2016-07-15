﻿#Region "Microsoft.VisualBasic::b22db83c89da2c535538bea8caf45118, ..\GCModeller\core\Bio.Assembly\SequenceModel\NucleicAcid\NucleicAcidStaticsProperty.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Text
Imports SMRUCC.genomics.SequenceModel.ISequenceModel
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic

Namespace SequenceModel.NucleotideModels

    <PackageNamespace("NucleicAcid.Property", Publisher:="amethyst.asuka@gcmodeller.org")>
    Public Module NucleicAcidStaticsProperty

        <ExportAPI("GC%", Info:="Calculate the GC content of the target sequence data.")>
        <Extension> Public Function GC_Content(Sequence As IEnumerable(Of DNA)) As Double
            Dim LQuery = (From nn As DNA
                          In Sequence
                          Where nn = DNA.dGMP OrElse
                              nn = DNA.dCMP
                          Select 1).ToArray
            Return LQuery.Length / Sequence.Count
        End Function

        ''' <summary>
        ''' A, T, G, C
        ''' </summary>
        ''' <param name="Sequence"></param>
        ''' <param name="pA"></param>
        ''' <param name="pT"></param>
        ''' <param name="pG"></param>
        ''' <param name="pC"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetCompositionVector(Sequence As Char(), ByRef pA As Double,
                                                                 ByRef pT As Double,
                                                                 ByRef pG As Double,
                                                                 ByRef pC As Double) As Integer()
            Dim Data As Integer() = GetCompositionVector(Sequence)

            pA = Data(0) / Sequence.Length
            pT = Data(1) / Sequence.Length
            pG = Data(2) / Sequence.Length
            pC = Data(3) / Sequence.Length

            Return Data
        End Function

        ''' <summary>
        ''' A, T, G, C
        ''' </summary>
        ''' <param name="Sequence"></param>
        ''' <returns>A, T, G, C</returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("CompositionVector")>
        Public Function GetCompositionVector(Sequence As Char()) As Integer()
            Dim A As Integer = (From ch In Sequence Where ch = "A"c Select 1).Count
            Dim T As Integer = (From ch In Sequence Where ch = "T"c Select 1).Count
            Dim G As Integer = (From ch In Sequence Where ch = "G"c Select 1).Count
            Dim C As Integer = (From ch In Sequence Where ch = "C"c Select 1).Count

            Return New Integer() {A, T, G, C}
        End Function

        ''' <summary>
        ''' Calculate the GC content of the target sequence data.
        ''' </summary>
        ''' <param name="Sequence">序列数据大小写不敏感</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("GC%", Info:="Calculate the GC content of the target sequence data.")>
        Public Function GCContent(Sequence As I_PolymerSequenceModel) As Double
            Return GCContent(Sequence.SequenceData)
        End Function

        ''' <summary>
        ''' Calculate the GC content of the target sequence data.
        ''' </summary>
        ''' <param name="NT">序列数据大小写不敏感</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("GC%", Info:="Calculate the GC content of the target sequence data.")>
        Public Function GCContent(NT As String) As Double
            Return (Count(NT, "G"c, "g"c) + Count(NT, "C"c, "c"c)) / Len(NT)
        End Function

        ''' <summary>
        ''' The melting temperature of P1 is Tm(P1), which is a reference temperature for a primer to perform annealing and known as the Wallace formula
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Tm", Info:="The melting temperature of P1 is Tm(P1), which is a reference temperature for a primer to perform annealing and known as the Wallace formula")>
        Public Function Tm(<Parameter("Primer", "Short DNA sequence which its length is less than 35nt.")> Primer As String) As Double
            Return (Count(Primer, "C"c, "c"c) + Count(Primer, "G"c, "g"c)) * 4 + (Count(Primer, "A"c, "a"c) + Count(Primer, "T"c, "t"c)) * 2
        End Function

        Public Function Count(str As String, ParamArray ch As Char()) As Integer
            If String.IsNullOrEmpty(str) OrElse ch.IsNullOrEmpty Then
                Return 0
            Else
                If ch.Length = 1 Then
                    Dim chr As Char = ch.First
                    Return str.Count(predicate:=Function(c As Char) c = chr)
                Else
                    Return str.Count(predicate:=Function(c As Char) Array.IndexOf(ch, c) > -1)
                End If
            End If
        End Function

        <ExportAPI("GC%", Info:="Calculate the GC content of the target sequence data.")>
        Public Function GCContent(SequenceModel As I_PolymerSequenceModel, SlideWindowSize As Integer, Steps As Integer, Circular As Boolean) As Double()
            Return __contentCommon(SequenceModel, SlideWindowSize, Steps, Circular, {"G", "C"})
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="SequenceModel"></param>
        ''' <param name="SlideWindowSize"></param>
        ''' <param name="Steps"></param>
        ''' <param name="Circular"></param>
        ''' <param name="base">必须是大写的字符</param>
        ''' <returns></returns>
        Private Function __contentCommon(SequenceModel As I_PolymerSequenceModel,
                                         SlideWindowSize As Integer,
                                         Steps As Integer,
                                         Circular As Boolean,
                                         base As Char()) As Double()
            If Circular Then
                Return __circular(SequenceModel, SlideWindowSize, Steps, base)
            Else
                Return __liner(SequenceModel, SlideWindowSize, Steps, base)
            End If
        End Function

        Private Function __liner(SequenceModel As I_PolymerSequenceModel,
                                 SlideWindowSize As Integer,
                                 Steps As Integer,
                                 base As Char()) As Double()
            Dim SequenceData As String = SequenceModel.SequenceData.ToUpper
            Dim ChunkBuffer As List(Of Double) = New List(Of Double)

            For i As Integer = 1 To SequenceData.Length Step Steps
                Dim Segment As String = Mid(SequenceData, i, SlideWindowSize)
                Dim n As Double = __getPercent(Segment, SlideWindowSize, base)
                Call ChunkBuffer.Add(n)
            Next

            Return ChunkBuffer.ToArray
        End Function

        Private Function __circular(SequenceModel As I_PolymerSequenceModel,
                                    SlideWindowSize As Integer,
                                    Steps As Integer,
                                    base As Char()) As Double()
            Dim SequenceData As String = SequenceModel.SequenceData.ToUpper
            Dim ChunkBuffer As List(Of Double) = New List(Of Double)
            For i As Integer = 1 To SequenceData.Length - SlideWindowSize Step Steps
                Dim Segment As String = Mid(SequenceData, i, SlideWindowSize)
                Dim n As Double = __getPercent(Segment, SlideWindowSize, base)
                Call ChunkBuffer.Add(n)
            Next
            For i As Integer = SequenceData.Length - SlideWindowSize + 1 To SequenceData.Length Step Steps
                Dim Segment As String = Mid(SequenceData, i, SlideWindowSize)
                Dim l = SlideWindowSize - Len(Segment)
                Segment &= Mid(SequenceData, 1, l)
                Dim n As Double = __getPercent(Segment, SlideWindowSize, base)
                Call ChunkBuffer.Add(n)
            Next
            Return ChunkBuffer.ToArray
        End Function

        Private Function __getPercent(segment As String, winSize As Integer, base As Char()) As Double
            Dim LQuery = (From b As Char In base Select segment.Count(b)).ToArray
            Dim n As Double = LQuery.Sum
            n = n / winSize
            Return n
        End Function

        <ExportAPI("AT%")>
        Public Function ATPercent(SequenceModel As I_PolymerSequenceModel, SlideWindowSize As Integer, Steps As Integer, Circular As Boolean) As Double()
            Return __contentCommon(SequenceModel, SlideWindowSize, Steps, Circular, {"A", "T"})
        End Function

        Public Delegate Function NtProperty(SequenceModel As I_PolymerSequenceModel, SlideWindowSize As Integer, Steps As Integer, Circular As Boolean) As Double()

        ''' <summary>
        ''' Calculation the GC skew of a specific nucleotide acid sequence.(对核酸链分子计算GC偏移量，请注意，当某一个滑窗区段内的GC是相等的话，则会出现正无穷)
        ''' </summary>
        ''' <param name="SequenceModel">Target sequence object should be a nucleotide acid sequence.(目标对象必须为核酸链分子)</param>
        ''' <param name="Circular"></param>
        ''' <returns>返回的矩阵是每一个核苷酸碱基上面的GC偏移量</returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("GCSkew", Info:="Calculation the GC skew of a specific nucleotide acid sequence.")>
        Public Function GCSkew(SequenceModel As I_PolymerSequenceModel, SlideWindowSize As Integer, Steps As Integer, Circular As Boolean) As Double()
            Dim SequenceData As String = SequenceModel.SequenceData.ToUpper
            Dim bufs As New List(Of Double)

            If Circular Then
                For i As Integer = 1 To SequenceData.Length - SlideWindowSize Step Steps
                    Dim Segment As String = Mid(SequenceData, i, SlideWindowSize)
                    Dim G = (From ch In Segment Where ch = "G"c Select 1).ToArray.Length
                    Dim C = (From ch In Segment Where ch = "C"c Select 1).ToArray.Length
                    Call bufs.Add((G - C) / (G + C))
                Next
                For i As Integer = SequenceData.Length - SlideWindowSize + 1 To SequenceData.Length Step Steps
                    Dim Segment As String = Mid(SequenceData, i, SlideWindowSize)
                    Dim l = SlideWindowSize - Len(Segment)
                    Segment &= Mid(SequenceData, 1, l)
                    Dim G = (From ch In Segment Where ch = "G"c Select 1).ToArray.Length
                    Dim C = (From ch In Segment Where ch = "C"c Select 1).ToArray.Length
                    Call bufs.Add((G - C) / (G + C))
                Next
            Else
                For i As Integer = 1 To SequenceData.Length Step Steps
                    Dim Segment As String = Mid(SequenceData, i, SlideWindowSize)
                    Dim G = (From ch In Segment Where ch = "G"c Select 1).ToArray.Length
                    Dim C = (From ch In Segment Where ch = "C"c Select 1).ToArray.Length
                    Call bufs.Add((G - C) / (G + C))
                Next
            End If

            bufs = (From n As Double In bufs Select __NAHandle(n, SlideWindowSize)).ToList '碱基之间是有顺序的，故而不适用并行化拓展
            Return bufs.ToArray
        End Function

        Private Function __NAHandle(n As Double, SlideWindowSize As Integer) As Double
            If n = Double.PositiveInfinity OrElse Double.IsNaN(n) OrElse Double.IsInfinity(n) OrElse Double.IsNegativeInfinity(n) OrElse Double.IsPositiveInfinity(n) Then
                Return 1 / SlideWindowSize
            Else
                Return n
            End If
        End Function
    End Module
End Namespace
