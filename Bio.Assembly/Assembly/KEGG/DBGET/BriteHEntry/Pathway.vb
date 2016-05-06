﻿Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic

Namespace Assembly.KEGG.DBGET.BriteHEntry

    ''' <summary>
    ''' The brief entry information for the pathway objects in the KEGG database.(KEGG数据库之中的代谢途径对象的入口点信息) 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Pathway : Implements IReadOnlyId

        ''' <summary>
        ''' A
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property [Class] As String
        ''' <summary>
        ''' B
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Category As String

        ''' <summary>
        ''' C
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Entry As ComponentModel.KeyValuePair

        Public Overrides Function ToString() As String
            Return String.Format("[{0}]{1}   {2}", [Class], Category, Entry.ToString)
        End Function

        ''' <summary>
        ''' 从程序的自身的资源文件之中加载数据
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function LoadFromResource() As Pathway()
            Dim TempFile As String = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) & "/KEGG_PATHWAYS.txt"
            Call IO.File.WriteAllText(TempFile, My.Resources.br08901, encoding:=System.Text.Encoding.ASCII)
            Return LoadData(TempFile)
        End Function

        Public Shared Function LoadDictionary() As Dictionary(Of String, Pathway)
            Dim data = LoadFromResource()
            Return LoadDictionary(data)
        End Function

        Public Shared Function LoadDictionary(res As IEnumerable(Of Pathway)) As Dictionary(Of String, Pathway)
            Dim dict = res.ToDictionary(Function(x) x.EntryId)
            Return dict
        End Function

        Public Shared Function LoadDictionary(res As String) As Dictionary(Of String, Pathway)
            Dim data As Pathway() = LoadData(res)
            Return LoadDictionary(data)
        End Function

        ''' <summary>
        ''' 从文件之中加载数据
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function LoadData(path As String) As Pathway()
            Dim Chunkbuffer As String() = (From strLine As String In IO.File.ReadAllLines(path)
                                           Where Not String.IsNullOrEmpty(strLine) AndAlso
                                               (strLine.First = "A"c OrElse strLine.First = "B"c OrElse strLine.First = "C"c)
                                           Select strLine).ToArray
            Dim [Class] As String = "", Category As String = ""
            Dim ItemList As List(Of Pathway) = New List(Of Pathway)

            For i As Integer = 0 To Chunkbuffer.Length - 1
                Dim strLine As String = Chunkbuffer(i)
                Dim Id As Char = strLine.First

                strLine = Mid(strLine, 2).Trim

                If Id = "A"c Then
                    [Class] = BriteHText.NormalizePath(strLine.GetValue)
                ElseIf Id = "B"c Then
                    Category = BriteHText.NormalizePath(strLine)
                ElseIf Id = "C"c Then
                    Dim IdNum As String = Regex.Match(strLine, "\d{5}").Value
                    strLine = strLine.Replace(IdNum, "").Trim
                    ItemList += New Pathway With {
                        .Category = Category,
                        .Class = [Class],
                        .Entry = New ComponentModel.KeyValuePair With {
                            .Key = IdNum,
                            .Value = strLine
                        }
                    }
                End If
            Next

            Return ItemList.ToArray
        End Function

        Public Shared Function CombineDIR(entry As Pathway, ParentDIR As String) As String
            Return String.Join("/", ParentDIR, [Module].TrimPath(entry.Class), [Module].TrimPath(entry.Category))
        End Function

        Public ReadOnly Property EntryId As String Implements IReadOnlyId.locusId
            Get
                Return Entry.Key
            End Get
        End Property

        Public Shared Function GetClass(EntryID As String, data As Pathway()) As Pathway
            Dim MatchID As String = (From m As Match
                                     In Regex.Matches(EntryID, "\d{5}")
                                     Select m.Value).Last
            Dim LQuery As Pathway = (From pwy As Pathway
                                     In data
                                     Where String.Equals(MatchID, pwy.Entry.Key)
                                     Select pwy).FirstOrDefault
            Return LQuery
        End Function
    End Class
End Namespace