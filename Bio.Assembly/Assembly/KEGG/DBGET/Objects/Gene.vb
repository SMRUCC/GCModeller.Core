Imports System.Text.RegularExpressions
Imports LANS.SystemsBiology.Assembly.KEGG.DBGET.WebParser
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Assembly.KEGG.DBGET.bGetObject

    ''' <summary>
    ''' http://www.genome.jp/dbget-bin/www_bget?
    ''' </summary>
    Public Class Gene : Implements IKeyValuePairObject(Of String, String)

        Public Const URL_MODULE_GENES As String = "http://www.genome.jp/dbget-bin/get_linkdb?-t+genes+md:{0}"
        Public Const URL_PATHWAY_GENES As String = "http://www.genome.jp/dbget-bin/get_linkdb?-t+genes+path:{0}"

        Public Property Identifier As String Implements IKeyValuePairObject(Of String, String).Identifier
        Public Property Description As String Implements IKeyValuePairObject(Of String, String).Value

        Public Overrides Function ToString() As String
            Return Identifier
        End Function

        Public Shared Function Download(url As String) As Gene()
            Dim html As String = Strings.Split(url.GET, modParser.SEPERATOR).Last
            Dim Entries = (From Match As Match
                           In Regex.Matches(html, "<a href="".+?"">.+?</a>.+?$", RegexOptions.Multiline + RegexOptions.IgnoreCase)
                           Select Match.Value).ToArray
            Entries = Entries.Take(Entries.Count - 1).ToArray

            Dim Genes As Gene() = New Gene(Entries.Count - 1) {}
            For i As Integer = 0 To Entries.Count - 1
                Dim Item As String = Entries(i)
                Dim Entry As String = Regex.Match(Item, ">.+?</a>").Value
                Entry = Mid(Entry, 2, Len(Entry) - 5)
                Dim Description As String = Strings.Split(Item, "</a>").Last.Trim

                Genes(i) = New Gene With {
                    .Identifier = Entry,
                    .Description = Description
                }
            Next

            Return Genes
        End Function
    End Class
End Namespace