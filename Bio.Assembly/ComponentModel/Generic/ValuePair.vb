Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic

Namespace ComponentModel

    ''' <summary>
    ''' String type key value pair.(两个字符串构成的键值对)
    ''' </summary>
    ''' <remarks></remarks>
    '''
    <XmlType("DictionaryEntry", Namespace:="http://code.google.com/p/genome-in-code/ComponentModel/DictionaryEntry")>
    Public Class KeyValuePair

        Implements IKeyValuePairObject(Of String, String)
        Implements sIdEnumerable

#Region "ComponentModel.Collection.Generic.KeyValuePairObject(Of String, String) property overrides"
        <XmlAttribute>
        Public Property Key As String Implements sIdEnumerable.Identifier, IKeyValuePairObject(Of String, String).Identifier

        <XmlAttribute>
        Public Property Value As String Implements IKeyValuePairObject(Of String, String).Value
#End Region

        Sub New()

        End Sub

        Sub New(key As String, value As String)
            Me.Key = key
            Me.Value = value
        End Sub

        Public Shared Function Format_Prints(data As IEnumerable(Of KeyValuePair)) As String
            If data.IsNullOrEmpty Then
                Return ""
            End If

            Dim sBuilder As StringBuilder = New StringBuilder(New String("-"c, 120) & vbCrLf, capacity:=2048)
            Dim max As Integer = (From item In data Select Len(item.Key)).ToArray.Max

            For Each item In data
                Dim s = String.Format("  {0} {1} ==> {2}", item.Key, New String(" "c, max - Len(item.Key) + 2), item.Value)
                Call sBuilder.AppendLine(s)
            Next

            Return sBuilder.ToString
        End Function

        Public Shared Widening Operator CType(obj As KeyValuePair(Of String, String)) As KeyValuePair
            Return New KeyValuePair With {
                .Key = obj.Key,
                .Value = obj.Value
            }
        End Operator

        Public Overrides Function ToString() As String
            Return String.Format("{0} -> {1}", Key, Value)
        End Function

        Public Overloads Shared Function CreateObject(key As String, value As String) As KeyValuePair
            Return New KeyValuePair With {
                .Key = key,
                .Value = value
            }
        End Function

        Public Shared Function ToDictionary(lstData As IEnumerable(Of KeyValuePair)) As Dictionary(Of String, String)
            Dim Dictionary As Dictionary(Of String, String) = New Dictionary(Of String, String)
            For Each item In lstData
                Call Dictionary.Add(item.Key, item.Value)
            Next

            Return Dictionary
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If TypeOf obj Is KeyValuePair Then
                Dim KeyValuePair As KeyValuePair = DirectCast(obj, KeyValuePair)
                Return String.Equals(KeyValuePair.Key, Key) AndAlso
                    String.Equals(KeyValuePair.Value, Value)
            Else
                Return False
            End If
        End Function

        Public Overloads Function Equals(obj As KeyValuePair, Optional strict As Boolean = True) As Boolean
            If strict Then
                Return String.Equals(obj.Key, Key) AndAlso String.Equals(obj.Value, Value)
            Else
                Return String.Equals(obj.Key, Key, StringComparison.OrdinalIgnoreCase) AndAlso
                       String.Equals(obj.Value, Value, StringComparison.OrdinalIgnoreCase)
            End If
        End Function

        Public Shared Function Distinct(Collection As KeyValuePair()) As KeyValuePair()
            Dim lst = (From obj As KeyValuePair
                       In Collection
                       Select obj
                       Order By obj.Key Ascending).ToList
            For i As Integer = 0 To lst.Count - 1
                If i >= lst.Count Then
                    Exit For
                End If
                Dim item = lst(i)

                For j As Integer = i + 1 To lst.Count - 1
                    If j >= lst.Count Then
                        Exit For
                    End If
                    If item.Equals(lst(j)) Then
                        Call lst.RemoveAt(j)
                        j -= 1
                    End If
                Next
            Next

            Return lst.ToArray
        End Function

#Region "Linq lambda"

        Public Shared Function GetKey(k As KeyValuePair) As String
            Return k.Key
        End Function

        Public Shared Function GetValue(k As KeyValuePair) As String
            Return k.Value
        End Function
#End Region
    End Class
End Namespace
