﻿#Region "Microsoft.VisualBasic::3ad00707703e17d4acb3088834f11aee, ..\core\Bio.Assembly\Assembly\Uniprot\XML\Model\UniprotXML.vb"

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

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml

Namespace Assembly.Uniprot.XML

    <XmlType("uniprot")> Public Class UniprotXML

        Const ns$ = "xmlns=""http://uniprot.org/uniprot"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://uniprot.org/uniprot http://www.uniprot.org/support/docs/uniprot.xsd"""
        ' <uniparc xmlns="http://uniprot.org/uniparc" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://uniprot.org/uniparc http://www.uniprot.org/docs/uniparc.xsd" version="2017_03">
        Const uniparc_ns$ = "xmlns=""http://uniprot.org/uniparc"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://uniprot.org/uniparc http://www.uniprot.org/docs/uniparc.xsd"""

        ''' <summary>
        ''' <see cref="entry.accession"/>可以用作为字典的键名
        ''' </summary>
        ''' <returns></returns>
        <XmlElement("entry")>
        Public Property entries As entry()
        <XmlElement>
        Public Property copyright As String
        <XmlAttribute>
        Public Property version As String

        Public Shared Function Load(path$) As UniprotXML
            Dim xml As String = path.ReadAllText

            If InStr(xml, "<uniparc xmlns=", CompareMethod.Text) > 0 Then
                xml = xml.Replace(UniprotXML.uniparc_ns, Xmlns.DefaultXmlns)
                xml = xml.Replace("<uniparc xmlns", "<uniprot xmlns")
                xml = xml.Replace("</uniparc>", "</uniprot>")
            Else
                xml = xml.Replace(UniprotXML.ns, Xmlns.DefaultXmlns)
            End If

            Dim model As UniprotXML = xml.LoadFromXml(Of UniprotXML)
            Return model
        End Function

        ''' <summary>
        ''' 因为可能会存在一个蛋白质entry对应多个accession的情况，
        ''' 所以这个函数会自动将这些重复的<see cref="entry.accessions"/>进行展开，
        ''' 则取出唯一的accessionID只需要使用表达式
        ''' 
        ''' ```vbnet
        ''' DirectCast(entry, <see cref="InamedValue"/>).Key
        ''' ```
        ''' </summary>
        ''' <param name="handle$">file or directory</param>
        ''' <returns></returns>
        Public Shared Function LoadDictionary(handle$) As Dictionary(Of entry)
            Dim source As entry()

            If handle.FileExists(True) Then
                source = Load(handle).entries
            Else
                source =
                    (ls - l - r - "*.xml" <= handle) _
                    .Select(AddressOf Load) _
                    .Select(Function(xml) xml.entries) _
                    .IteratesALL _
                    .ToArray
            End If

            Dim groups = From protein As entry
                         In source _
                             .Select(Function(o) o.ShadowCopy) _
                             .IteratesALL
                         Select protein
                         Group protein By DirectCast(protein, INamedValue).Key Into Group
            Dim out As Dictionary(Of entry) = groups _
                .Select(Function(g) g.Group.First) _
                .ToDictionary

            Return out
        End Function

        Public Overrides Function ToString() As String
            Return GetXml
        End Function
    End Class
End Namespace