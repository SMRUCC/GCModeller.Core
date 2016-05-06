Imports System.Net
Imports System.Text.RegularExpressions
Imports LANS.SystemsBiology.Assembly.KEGG.DBGET.bGetObject
Imports LANS.SystemsBiology.Assembly.KEGG.WebServices
Imports LANS.SystemsBiology.Assembly.KEGG.WebServices.InternalWebFormParsers
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic

Namespace Assembly.KEGG.DBGET.LinkDB

    ''' <summary>
    ''' LinkDB Search for KEGG pathways
    ''' </summary>
    Public Module Pathways

        Const KEGGPathwayLinkDB_URL As String = "http://www.genome.jp/dbget-bin/get_linkdb?-t+pathway+genome:{0}"

        Public Function URLProvider(sp As String) As String
            Dim url As String = String.Format(KEGGPathwayLinkDB_URL, sp)
            Return url
        End Function

        Const LinkItem As String = "<a href="".+?"">.+?</a>.+?$"

        Public Iterator Function AllEntries(sp As String) As IEnumerable(Of ListEntry)
            Dim html As String = Strings.Split(URLProvider(sp).GET, modParser.SEPERATOR).Last
            Dim Entries As String() =
                Regex.Matches(html, LinkItem, RegexICMul).ToArray

            For Each entry As String In Entries.Take(Entries.Length - 1)
                Dim key As String = Regex.Match(entry, ">.+?</a>").Value
                key = Mid(key, 2, Len(key) - 5)
                Dim Description As String = Strings.Split(entry, "</a>").Last.Trim
                Dim url As String = "http://www.genome.jp" & entry.Get_href

                Yield New ListEntry With {
                    .EntryID = key,
                    .Description = Description,
                    .Url = url
                }
            Next
        End Function

        Public Iterator Function Download2(sp As String, Optional EXPORT As String = "./LinkDB-Pathways/") As IEnumerable(Of Pathway)
            Dim Downloader As New WebClient()
            Dim entries As New List(Of ListEntry)

            For Each entry As ListEntry In AllEntries(sp)
                Dim ImageUrl = String.Format("http://www.genome.jp/kegg/pathway/{0}/{1}.png", sp, entry.EntryID)
                Dim pathwayPage = "http://www.genome.jp/dbget-bin/www_bget?pathway+" & entry.EntryID
                Dim path As String = EXPORT & "/webpages/" & entry.EntryID & ".html"
                Dim img As String = EXPORT & $"/{entry.EntryID}.png"

                Call pathwayPage.GET.SaveTo(path)
                Call Downloader.DownloadFile(ImageUrl, img)

                Dim data As Pathway = Pathway.DownloadPage(path)

                Call data.SaveAsXml(EXPORT & $"/{entry.EntryID}.Xml")

                entries += entry

                Yield data
            Next

            Call entries.GetJson.SaveTo(EXPORT & $"/{sp}.json")
        End Function
    End Module
End Namespace