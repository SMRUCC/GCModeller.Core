Imports System.Text.RegularExpressions
Imports System.Runtime.CompilerServices
Imports System.Text

Namespace Assembly.KEGG.WebServices.InternalWebFormParsers

    Public Class AllLinksWidget

        Public Property Links As ComponentModel.KeyValuePair()

        'http://www.genome.jp/dbget-bin/get_linkdb?-t+8+path:map00010

        Default Public ReadOnly Property Url(ItemKey As String) As String
            Get
                Dim LQuery = (From lnkValue As ComponentModel.KeyValuePair
                              In Links
                              Where String.Equals(lnkValue.Key, ItemKey)
                              Select lnkValue.Value).ToArray
                Return LQuery.FirstOrDefault
            End Get
        End Property

        Public Shared Function InternalParser(PageContent As String) As AllLinksWidget
            Dim Links As AllLinksWidget = New AllLinksWidget
            PageContent = Regex.Match(PageContent, "All links.+</pre>", RegexOptions.Singleline).Value
            Dim TempChunk As String() = (From m As Match In Regex.Matches(PageContent, "<a href="".+?"">.+?</a>") Select m.Value).ToArray

            Links.Links = (From s As String In TempChunk
                           Let url As String = "http://www.genome.jp" & s.Get_href
                           Let Key As String = s.GetValue
                           Select New ComponentModel.KeyValuePair With {
                               .Key = Regex.Replace(Key, "\(.+?\)", "").Trim,
                               .Value = url}).ToArray
            Return Links
        End Function

        Public Overrides Function ToString() As String
            Return String.Join("; ", (From m As ComponentModel.KeyValuePair
                                      In Links
                                      Select ss = m.ToString).ToArray)
        End Function
    End Class
End Namespace