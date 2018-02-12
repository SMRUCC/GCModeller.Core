﻿#Region "Microsoft.VisualBasic::a4fd0e862c254be213a1fde8682e3c5e, core\Bio.Assembly\Assembly\MiST2\DocArchive\MiST2.vb"

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

    '     Class MiST2
    ' 
    '         Function: GetProfileImage, IsHisK, IsRR, ParseLine, (+2 Overloads) Save
    '                   ToString
    ' 
    '         Sub: Parse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports SMRUCC.genomics.SequenceModel
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Language

Namespace Assembly.MiST2

    ''' <summary>
    ''' MiST2对某一个基因组的注释所产生的数据库文件
    ''' </summary>
    <XmlType("MiST2", [Namespace]:="http://gcmodeller.org/mistdb.com")>
    Public Class MiST2 : Implements ISaveHandle

        <XmlElement> Public Property MajorModules As Replicon()
        ''' <summary>
        ''' * Note: Chart based on domain counts
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlElement> Public Property ProfileImageUrl As String
        <XmlAttribute> Public Property MiST2Code As String
        <XmlElement> Public Property Organism As String
        <XmlElement> Public Property Taxonomy As String
        <XmlElement> Public Property Size As String
        <XmlElement> Public Property Status As String
        <XmlElement> Public Property Replicons As String
        <XmlElement> Public Property Genes As String
        <XmlElement> Public Property Proteins As String
        <XmlElement> Public Property GC As String

        Const TABLE_LINE As String = "<tbody>.+?</tbody>"
        Const TABLE_CELL As String = "<td>.+?</td>"

        Friend Sub Parse(strText As String)
            strText = Regex.Match(strText, TABLE_LINE, RegexOptions.Singleline).Value
            Dim Lines = (From m As Match In Regex.Matches(strText, "<tr.+?</tr>", RegexOptions.Singleline + RegexOptions.IgnoreCase) Select m.Value).ToArray
            MajorModules = (From Line As String In Lines Select ParseLine(Line)).ToArray
        End Sub

        Public Function IsHisK(Id As String) As Boolean
            For Each [Module] In MajorModules
                If Not (From item In [Module].TwoComponent.HHK Where String.Equals(item.ID, Id) Select 1).ToArray.IsNullOrEmpty Then
                    Return True
                End If
                If Not (From item In [Module].TwoComponent.HisK Where String.Equals(item.ID, Id) Select 1).ToArray.IsNullOrEmpty Then
                    Return True
                End If
            Next

            Return False
        End Function

        Public Overrides Function ToString() As String
            Return Organism
        End Function

        Public Function IsRR(Id As String) As Boolean
            For Each [Module] In MajorModules
                If Not (From item In [Module].TwoComponent.HRR Where String.Equals(item.ID, Id) Select 1).ToArray.IsNullOrEmpty Then
                    Return True
                End If
                If Not (From item In [Module].TwoComponent.RR Where String.Equals(item.ID, Id) Select 1).ToArray.IsNullOrEmpty Then
                    Return True
                End If
            Next

            Return False
        End Function

        Private Shared Function ParseLine(strText As String) As Replicon
            Dim RepliconMajorModule As Replicon = New Replicon
            Dim tokens As String() = (From m As Match In Regex.Matches(strText, TABLE_CELL, RegexOptions.Singleline) Select m.Value).ToArray
            Dim mts = Regex.Matches(strText, "<td class=""no_border left"">[^<]+?</td>", RegexOptions.Singleline + RegexOptions.IgnoreCase)

            RepliconMajorModule.Accession = Regex.Match(mts(0).Value, ">.+<").Value
            RepliconMajorModule.Accession = Mid(RepliconMajorModule.Accession, 2, Len(RepliconMajorModule.Accession) - 2)
            RepliconMajorModule.Replicon = Regex.Match(mts(1).Value, ">.+<").Value
            RepliconMajorModule.Replicon = Mid(RepliconMajorModule.Replicon, 2, Len(RepliconMajorModule.Replicon) - 2)

            RepliconMajorModule.Size = Mid(tokens.First, 5, Len(tokens.First) - 9)

            Dim url As String() = New String(tokens.Count - 3) {}
            Call Array.ConstrainedCopy(tokens, 1, url, 0, 9)
            For i As Integer = 0 To url.Count - 1 '解析数据资源
                Dim strTemp As String = Regex.Match(url(i), "href="".+?""", RegexOptions.IgnoreCase).Value
                If Not String.IsNullOrEmpty(strTemp) Then
                    strTemp = Mid(strTemp, 6)
                    url(i) = "http://mistdb.com" & Mid(strTemp, 2, Len(strTemp) - 2)
                Else
                    url(i) = ""
                End If
            Next

            Dim p As int = Scan0

            RepliconMajorModule.OneComponent = WebServices.Download(url(++p))
            RepliconMajorModule.TwoComponent = New TwoComponent With {
                    .HisK = WebServices.Download(url(++p)),
                    .HHK = WebServices.Download(url(++p)),
                    .RR = WebServices.Download(url(++p)),
                    .HRR = WebServices.Download(url(++p)),
                    .Other = WebServices.Download(url(++p))}
            RepliconMajorModule.Chemotaxis = WebServices.Download(url(++p))
            RepliconMajorModule.ECF = WebServices.Download(url(++p))
            RepliconMajorModule.Other = WebServices.Download(url(++p))

            Return RepliconMajorModule
        End Function

        Public Function GetProfileImage() As System.Drawing.Image
            Dim FilePath As String = String.Format("{0}/{1}.png", Environment.GetFolderPath(Environment.SpecialFolder.InternetCache), Me.MiST2Code)
            If ProfileImageUrl.DownloadFile(save:=FilePath) Then
                Return System.Drawing.Image.FromFile(FilePath)
            Else
                Return Nothing
            End If
        End Function

        Public Shared Widening Operator CType(Path As String) As MiST2
            Return Path.LoadXml(Of Assembly.MiST2.MiST2)()
        End Operator

        Public Function Save(Optional Path As String = "", Optional encoding As Text.Encoding = Nothing) As Boolean Implements ISaveHandle.Save
            Return Me.GetXml.SaveTo(Path, encoding)
        End Function

        Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(Path, encoding.CodePage)
        End Function
    End Class
End Namespace
