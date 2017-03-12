Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

Namespace Assembly.KEGG.WebServices.InternalWebFormParsers

    Public Module Extensions

        Public Const DBGET$ = "DBGET integrated database retrieval system"

        <Extension>
        Public Function DivInternals(html$) As String()
            Dim ms$() = Regex.Matches(html, "<div.+?</div>", RegexICSng).ToArray
            Return ms
        End Function

        <Extension>
        Public Function Strip_NOBR(html$) As String
            Dim m$ = Regex.Match(html, "<nobr>.+?</nobr>", RegexICSng).Value
            If Not m.Length = 0 Then
                html = html.Replace(m, "")
            End If
            Return html
        End Function
    End Module
End Namespace