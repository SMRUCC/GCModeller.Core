﻿Imports System.Text.RegularExpressions
Imports System.Runtime.CompilerServices
Imports System.Text
Imports LANS.SystemsBiology.Assembly.KEGG.DBGET
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic

Namespace Assembly.KEGG.WebServices.InternalWebFormParsers

    ''' <summary>
    ''' KEGG 网页表格的数据解析方法，在Value之中可能会有重复的Key数据出现
    ''' </summary>
    ''' <remarks></remarks>
    Public Class WebForm : Implements IReadOnlyDictionary(Of String, String())
        Implements System.IDisposable

        ''' <summary>
        ''' Entry, {trim_formatted, non-process}
        ''' </summary>
        Dim _strData As SortedDictionary(Of String, KeyValuePair(Of String, String)())
        Dim _url As String

        Public ReadOnly Property WebPageTitle As String
        Public Property AllLinksWidget As AllLinksWidget

        Sub New(Url As String)
            Dim PageContent As String = Url.GET.Replace("&nbsp;", " ").Replace("&gt;", ">").Replace("&lt;", "<")
            Dim Tokens As String() = Regex.Split(PageContent, "<th class="".+?"" align="".+?""").Skip(1).ToArray
            Dim TempChunk As String() = (From strValue As String In Tokens
                                         Let value As String = Regex.Match(strValue, "<nobr>.+?</nobr>.+", RegexOptions.Singleline).Value.Trim
                                         Where Not String.IsNullOrEmpty(value)
                                         Select value).ToArray
            Me._WebPageTitle = GetNodeValue(Regex.Match(PageContent, PAGE_CONTENT_TITLE).Value)
            Me._url = Url
            Me._strData = New SortedDictionary(Of String, KeyValuePair(Of String, String)())

            Dim Fields = (From strValue As String In TempChunk
                          Let Key As String = Regex.Match(strValue, "<nobr>.+?</nobr>").Value
                          Let Value As String = RegexReplace(strValue.Replace(Key, ""), WebForm.HtmlFormatControl)
                          Select New KeyValuePair(Of String, String())(GetNodeValue(Key), {Value.TrimA, strValue})).ToArray

            For Each Key As String In (From item In Fields Let KeyValue As String = item.Key Select KeyValue Distinct).ToArray
                Call _strData.Add(Key, (From item In Fields
                                        Where String.Equals(Key, item.Key)
                                        Select New KeyValuePair(Of String, String)(item.Value(Scan0), item.Value(1))).ToArray)
            Next

            AllLinksWidget = InternalWebFormParsers.AllLinksWidget.InternalParser(PageContent)
            Call ParseRefList(PageContent)
        End Sub

        Private Sub ParseRefList(Page As String)
            Dim Tokens As String() = Strings.Split(Page, "<nobr>Reference</nobr></th>").Skip(1).ToArray
            Me.References = bGetObject.Reference.References(Tokens)
        End Sub

        Public Property References As bGetObject.Reference()

        Private Shared ReadOnly HtmlFormatControl As String() = New String() {"<td .+?>", "<div .+?>", "</th>|</div>|</td>|</tr>|<tr>|<tbody>|<div>|</tbody>|</table>|<nobr>|</nobr>", "<table .+?>"}

        Protected Friend Shared Function parseList(strValue As String, SplitRegx As String) As ComponentModel.KeyValuePair()
            If String.IsNullOrEmpty(strValue) Then
                Return New ComponentModel.KeyValuePair() {}
            End If

            Dim ComponentList As List(Of ComponentModel.KeyValuePair) = New List(Of ComponentModel.KeyValuePair)
            Dim Chunkbuffer As String() = (From m As Match In Regex.Matches(strValue, SplitRegx) Select m.Value Distinct).ToArray

            For i As Integer = 0 To Chunkbuffer.Count - 2
                Dim p1 As Integer = InStr(strValue, Chunkbuffer(i))
                Dim p2 As Integer = InStr(strValue, Chunkbuffer(i + 1))
                Dim strTemp As String = Mid(strValue, p1, p2 - p1)

                Dim ComponentEntry As String = Regex.Match(strTemp, SplitRegx).Value
                Dim ComponentDescription As String = strTemp.Replace(ComponentEntry, "").Trim

                ComponentEntry = KEGG.WebServices.InternalWebFormParsers.WebForm.GetNodeValue(ComponentEntry)
                ComponentDescription = KEGG.WebServices.InternalWebFormParsers.WebForm.RemoveHrefLink(ComponentDescription)

                Call ComponentList.Add(New ComponentModel.KeyValuePair With {.Key = ComponentEntry, .Value = ComponentDescription})
            Next

            Dim p As Integer = InStr(strValue, Chunkbuffer.Last)
            strValue = Mid(strValue, p)
            Dim LastEntry As ComponentModel.KeyValuePair = New ComponentModel.KeyValuePair
            LastEntry.Key = Regex.Match(strValue, SplitRegx).Value
            LastEntry.Value = KEGG.WebServices.InternalWebFormParsers.WebForm.RemoveHrefLink(strValue.Replace(LastEntry.Key, "").Trim)
            LastEntry.Key = KEGG.WebServices.InternalWebFormParsers.WebForm.GetNodeValue(LastEntry.Key)

            Call ComponentList.Add(LastEntry)

            Return ComponentList.ToArray
        End Function

        ''' <summary>
        ''' 将符合目标规则的字符串替换为空字符串
        ''' </summary>
        ''' <param name="strData"></param>
        ''' <param name="ExprCollection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function RegexReplace(strData As String, ExprCollection As String()) As String
            For Each strItem As String In ExprCollection
                strData = Regex.Replace(strData, strItem, "")
            Next
            Return strData
        End Function

        ''' <summary>
        ''' 假若目标字符串的格式为一个HTML节点格式的话，则可以使用本方法进行解析
        ''' </summary>
        ''' <param name="strData"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function GetNodeValue(strData As String) As String
            strData = Regex.Match(strData, ">.+?<").Value
            If String.IsNullOrEmpty(strData) Then
                Return ""
            Else
                strData = Mid(strData, 2)
                strData = Mid(strData, 1, Len(strData) - 1)
                Return strData
            End If
        End Function

        ''' <summary>
        ''' 获取某一个字段的数据
        ''' </summary>
        ''' <param name="KeyWord">网页的表格之中的最左端的字段名</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetValue(KeyWord As String) As String()
            If _strData.ContainsKey(KeyWord) Then
                Return _strData.Item(KeyWord).ToArray(Function(obj) obj.Key)
            Else
                Return New String() {""}
            End If
        End Function

        Public Function GetRaw(Keyword As String) As String()
            If _strData.ContainsKey(Keyword) Then
                Return _strData.Item(Keyword).ToArray(Function(obj) obj.Value)
            Else
                Return New String() {""}
            End If
        End Function

        Public Overrides Function ToString() As String
            Return _url
        End Function

        Const HREF As String = "<a href="".+?"">.+?</a>"

        Public Shared Function RemoveHrefLink(strValue As String) As String
            If String.IsNullOrEmpty(strValue) Then
                Return ""
            End If

            Dim Links = (From m As Match
                         In Regex.Matches(strValue, HREF)
                         Select Original = m.Value,
                             Value = GetNodeValue(m.Value)).ToArray
            Dim sBuilder As StringBuilder = New StringBuilder(strValue)

            For Each LinkItem In Links
                Call sBuilder.Replace(LinkItem.Original, LinkItem.Value)
            Next

            Return sBuilder.ToString
        End Function

#Region "Implements IReadOnlyDictionary(Of String, String)"

        Public Iterator Function GetEnumerator() As IEnumerator(Of KeyValuePair(Of String, String())) Implements IEnumerable(Of KeyValuePair(Of String, String())).GetEnumerator
            For Each Item As KeyValuePair(Of String, KeyValuePair(Of String, String)()) In Me._strData
                Yield New KeyValuePair(Of String, String())(Item.Key, Item.Value.ToArray(Function(obj) obj.Key))
            Next
        End Function

        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of KeyValuePair(Of String, String())).Count
            Get
                Return _strData.Count
            End Get
        End Property

        Public Function ContainsKey(key As String) As Boolean Implements IReadOnlyDictionary(Of String, String()).ContainsKey
            Return _strData.ContainsKey(key)
        End Function

        Default Public ReadOnly Property Item(key As String) As String() Implements IReadOnlyDictionary(Of String, String()).Item
            Get
                Return GetValue(key)
            End Get
        End Property

        Public ReadOnly Property Keys As IEnumerable(Of String) Implements IReadOnlyDictionary(Of String, String()).Keys
            Get
                Return _strData.Keys.ToArray
            End Get
        End Property

        Public Function TryGetValue(key As String, ByRef value As String()) As Boolean Implements IReadOnlyDictionary(Of String, String()).TryGetValue
            Dim raw As KeyValuePair(Of String, String)() = Nothing
            Dim f As Boolean = _strData.TryGetValue(key, raw)
            value = raw.ToArray(Function(obj) obj.Key)
            Return f
        End Function

        Public ReadOnly Property Values As IEnumerable(Of String()) Implements IReadOnlyDictionary(Of String, String()).Values
            Get
                Return _strData.Values.ToArray
            End Get
        End Property

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose( disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose( disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace