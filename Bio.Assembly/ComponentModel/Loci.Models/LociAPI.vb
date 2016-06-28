﻿#Region "Microsoft.VisualBasic::57d0abe2721225f39c7dd0ec6f2f346d, ..\Bio.Assembly\ComponentModel\Loci.Models\LociAPI.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace ComponentModel.Loci

    <PackageNamespace("Loci.API", Description:="Methods for some nucleotide utility.")>
    Public Module LociAPI

        ''' <summary>
        ''' 直接合并相邻的一个位点集合到一个新的更加长的位点
        ''' </summary>
        ''' <typeparam name="TLocation"></typeparam>
        ''' <param name="groupedData"></param>
        ''' <returns></returns>
        Public Function Merge(Of TLocation As Location)(groupedData As IEnumerable(Of TLocation)) As TLocation
            Dim RightAligned As TLocation = (From objLoci As TLocation
                                             In groupedData
                                             Select objLoci
                                             Order By objLoci.Right Descending).First
            Dim clSite As TLocation = groupedData.First.Clone
            clSite.Left = groupedData.First.Left
            clSite.Right = RightAligned.Right
            Return clSite
        End Function

        ''' <summary>
        ''' Gets the location relationship of two loci segments.(判断获取两个位点片段之间的位置关系，请注意，这个函数只依靠左右位置来判断关系，假若对核酸链的方向有要求在调用本函数之前请确保二者在同一条链之上)
        ''' </summary>
        ''' <param name="lcl">在计算之前请先调用<see cref="Location.Normalization()"/>方法来修正</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Get.Relationship")>
        <Extension> Public Function GetRelationship(x As NucleotideLocation, lcl As NucleotideLocation) As SegmentRelationships
            Call x.Normalization()
            Call lcl.Normalization()

            If lcl.Left = x.Left() AndAlso lcl.Right = x.Right() Then
                Return SegmentRelationships.Equals
            End If
            If lcl.Right < x.Left() Then
                Return SegmentRelationships.UpStream
            End If
            If lcl.Left > x.Right() Then
                Return SegmentRelationships.DownStream
            End If
            If lcl.Left > x.Left() AndAlso lcl.Right < x.Right() Then
                Return SegmentRelationships.Inside
            End If
            If x.Left() > lcl.Left AndAlso x.Right() < lcl.Right Then
                Return SegmentRelationships.Cover
            End If

            If lcl.Left <= x.Left() AndAlso lcl.Right <= x.Right() AndAlso lcl.Right > x.Left() Then
                Return SegmentRelationships.UpStreamOverlap
            End If

            If lcl.Left >= x.Left() AndAlso lcl.Right >= x.Right() AndAlso lcl.Left < x.Right() Then
                Return SegmentRelationships.DownStreamOverlap
            End If

            Return SegmentRelationships.Blank
        End Function

        ''' <summary>
        ''' Convert the string value type nucleotide strand information description data into a strand enumerate data.
        ''' </summary>
        ''' <param name="str">从文本文件之中所读取出来关于链方向的字符串描述数据</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Get.Strands"), Extension> Public Function GetStrand(str As String) As Strands
            If String.IsNullOrEmpty(str) Then
                Return Strands.Unknown
            End If

            If str.First = "("c AndAlso str.Last = ")"c Then
                str = Mid(str, 2, Len(str) - 2)
            End If

            Select Case str.ToLower
                Case "+", "forward", "plus", "#forward", "direct", "#+"
                    Return Strands.Forward
                Case "-", "reverse", "minus", "complement", "#reverse", "#-"
                    Return Strands.Reverse
                Case Else
                    Return Strands.Unknown
            End Select
        End Function

        ''' <summary>
        ''' Try parse NCBI sequence dump location/<see cref="NucleotideLocation.ToString()"/> dump location.
        ''' </summary>
        ''' <param name="sLoci"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        '''
        <ExportAPI("Loci.Parser")>
        Public Function TryParse(sLoci As String) As NucleotideLocation
            If InStr(sLoci, " ==> ") > 0 Then
                Return __tryParse(sLoci)
            End If

            Dim nuclLoci As New NucleotideLocation

            If Regex.Match(sLoci, "complement\([^)]+\)").Success Then
                nuclLoci.Strand = Strands.Reverse
            Else
                nuclLoci.Strand = Strands.Forward
            End If

            Dim pos As Long() =
                LinqAPI.Exec(Of Long) <= From match As Match
                                         In Regex.Matches(sLoci, "\d+")
                                         Let n As Long = CType(Val(match.Value), Long)
                                         Select n
                                         Order By n Ascending
            nuclLoci.Left = pos(0)
            nuclLoci.Right = pos(1)

            Return nuclLoci
        End Function

        ''' <summary>
        ''' 388739 ==> 389772 #Forward
        ''' </summary>
        ''' <param name="s_Loci"></param>
        ''' <returns></returns>
        Private Function __tryParse(s_Loci As String) As NucleotideLocation
            Dim Tokens As String() = s_Loci.Split
            Dim Left As Integer = CInt(Val(Tokens(0)))
            Dim Right As Integer = CInt(Val(Tokens(2)))
            Dim Strand As Strands = GetStrand(Tokens(3))
            Return New NucleotideLocation(Left, Right, Strand).Normalization.As(Of NucleotideLocation)
        End Function

        <ExportAPI("Loci.Equals")>
        Public Function Equals(x As NucleotideLocation, loci As NucleotideLocation, Optional AllowedOffset As Integer = 10) As Boolean
            If loci.Strand <> x.Strand Then  ' 链的方向不一样，不能相等
                Return False
            Else
                Return Equals(DirectCast(x, Location), DirectCast(loci, Location), AllowedOffset)
            End If
        End Function

        <ExportAPI("Loci.Equals")>
        Public Function Equals(x As Location, loci As Location, Optional AllowedOffset As Integer = 10) As Boolean
            Dim Loci1 As Integer() = {x.Left(), x.Right()}
            Dim Loci2 As Integer() = {loci.Left, loci.Right}

            If AllowedOffset = 0 Then
                Return Loci1.Min = Loci2.Min AndAlso
                    Loci1.Max = Loci2.Max
            Else
                Return Math.Abs(Loci1.Min - Loci2.Min) <= AllowedOffset AndAlso
                    Math.Abs(Loci1.Max - Loci2.Max) <= AllowedOffset
            End If
        End Function
    End Module
End Namespace
