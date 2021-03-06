﻿#Region "Microsoft.VisualBasic::5ba126ea4d1181b5e10c8170a918dc05, Bio.Annotation\PTF\PtfFile.vb"

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

    '     Class PtfFile
    ' 
    '         Properties: attributes, proteins
    ' 
    '         Function: Load, ReadAnnotations, (+2 Overloads) Save, (+2 Overloads) ToString
    ' 
    '         Sub: WriteStream
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.genomics.Annotation.Ptf.Document

Namespace Ptf

    ''' <summary>
    ''' the GCModeller protein annotation tabular format file.
    ''' </summary>
    Public Class PtfFile : Implements ISaveHandle

        ' # version: ...
        ' # program: ...
        ' # time: ...
        ' # ...

        Public Property attributes As Dictionary(Of String, String)
        Public Property proteins As ProteinAnnotation()

        Public Overrides Function ToString() As String
            Return attributes.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Function ToString(protein As ProteinAnnotation) As String
            Return Document.asLineText(protein)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Load(file As String) As PtfFile
            Return Document.ParseDocument(file)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ReadAnnotations(file As Stream) As IEnumerable(Of ProteinAnnotation)
            Return Document.IterateAnnotations(file)
        End Function

        Public Shared Sub WriteStream(annotation As IEnumerable(Of ProteinAnnotation), file As TextWriter, Optional attributes As Dictionary(Of String, String) = Nothing)
            If Not attributes Is Nothing Then
                For Each key As String In attributes.Keys
                    Call file.WriteLine($"# {key}: {attributes(key)}")
                Next

                Call file.WriteLine()
            End If

            For Each protein As ProteinAnnotation In annotation.SafeQuery
                Call file.WriteLine(protein.asLineText)
            Next
        End Sub

        Public Function Save(path As String, encoding As Encoding) As Boolean Implements ISaveHandle.Save
            Using output As New StreamWriter(path.Open(doClear:=True), encoding) With {
                .NewLine = ASCII.LF
            }
                Call Document.writeTabular(Me, output)
            End Using

            Return True
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Save(path As String, Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(path, encoding.CodePage)
        End Function
    End Class
End Namespace
