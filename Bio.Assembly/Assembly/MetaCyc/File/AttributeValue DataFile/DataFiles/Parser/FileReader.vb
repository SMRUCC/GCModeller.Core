﻿Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq

Namespace Assembly.MetaCyc.File

    ''' <summary>
    ''' Database file reader of the metacyc database.
    ''' (MetaCyc数据库中的数据库文件的读取模块)
    ''' </summary>
    ''' <remarks></remarks>
    Public Module FileReader

        ''' <summary>
        ''' Try parse the data file.
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="prop"></param>
        ''' <param name="objs"></param>
        ''' <returns>Returns error message</returns>
        Public Function TryParse(path As String, ByRef prop As [Property], ByRef objs As ObjectModel()) As String
            If Not path.FileExists Then
                prop = New [Property]
                objs = New ObjectModel() {}
                Return $"{path.ToFileURL} is not found on your file system!"
            End If

            Dim lines As String() = path.ReadAllLines

            prop = GetDbProperty(lines)
            objs = GetData(lines).ToArray(Function(array) ObjectModel.ModelParser(array))

            Return ""
        End Function

        Public Function TabularParser(path As String, ByRef prop As [Property], ByRef lines As String(), ByRef first As String) As String
            If Not path.FileExists Then
                prop = New [Property]
                first = ""
                lines = New String() {}
                Return $"{path.ToFileURL} is not found on your file system!"
            End If

            lines = path.ReadAllLines
            first = lines.First
            prop = GetDbProperty(lines)
            lines = GetData(lines).ToArray.MatrixToVector

            Return ""
        End Function

        ''' <summary>
        ''' 从数据库文件中的注释行获取属性值
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetDbProperty(buffer As String()) As [Property]
            Dim Comments As String() = (From Line As String In buffer
                                        Where Not String.IsNullOrEmpty(Line) AndAlso Line.Chars(Scan0) = "#"c
                                        Select Line).ToArray   '获取文件头部说明性的数据库文件属性
            Return [Property].CreateFrom(Comments)
        End Function

        ''' <summary>
        ''' Get the data text line
        ''' (获取非注释的文本行)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Iterator Function GetData(buffer As String()) As IEnumerable(Of String())
            Dim LQuery As String() = (From Line As String In buffer
                                      Where Not String.IsNullOrEmpty(Line) AndAlso Line.Chars(Scan0) <> "#"c
                                      Select Line).ToArray 'Select the text line that not is a comment line
            Yield LQuery.Split("//")
        End Function
    End Module
End Namespace