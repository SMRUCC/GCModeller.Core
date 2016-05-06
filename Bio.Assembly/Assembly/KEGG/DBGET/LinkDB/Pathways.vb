Imports System.Net
Imports System.Text.RegularExpressions
Imports LANS.SystemsBiology.Assembly.KEGG.DBGET.bGetObject
Imports LANS.SystemsBiology.Assembly.KEGG.WebServices
Imports LANS.SystemsBiology.Assembly.KEGG.WebServices.InternalWebFormParsers
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

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

        Public Iterator Function AllEntries(sp As String) As IEnumerable(Of ListEntry)
            Dim html As String = Strings.Split(URLProvider(sp).GET, modParser.SEPERATOR).Last
            Dim Entries As String() = Regex.Matches(html, "<a href="".+?"">.+?</a>.+?$", RegexOptions.IgnoreCase Or RegexOptions.Multiline).ToArray

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

        Public Iterator Function Download2(speciesId As String) As IEnumerable(Of Pathway)
            Dim Downloader As New WebClient()

            Call FileIO.FileSystem.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) & "/Pathways/")

            For Each entry As ListEntry In AllEntries(speciesId)
                Dim Url As String = String.Format(Gene.URL_PATHWAY_GENES, entry.EntryID)
                Dim ImageUrl = String.Format("http://www.genome.jp/kegg/pathway/{0}/{1}.png", speciesId, entry.EntryID)
                Dim ObjUrl = "http://www.genome.jp/dbget-bin/www_bget?pathway+" & entry.EntryID
                Dim path As String = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) & "/pathways/webpages/" & entry.EntryID & ".html"

                Call ObjUrl.GET.SaveTo(path)
                Call Downloader.DownloadFile(ImageUrl, Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) & "/Pathways/" & entry.EntryID & ".png")

                Yield Pathway.DownloadPage(path)
            Next
        End Function
    End Module
End Namespace