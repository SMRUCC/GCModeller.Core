Imports System.Text.RegularExpressions
Imports LANS.SystemsBiology.Assembly.KEGG.DBGET.bGetObject
Imports LANS.SystemsBiology.Assembly.KEGG.WebServices.InternalWebFormParsers
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Assembly.KEGG.DBGET.LinkDB

    Public Module Pathways

        Const URL_KEGG_PATHWAY_ENTRY_PAGE As String = "http://www.genome.jp/dbget-bin/get_linkdb?-t+pathway+genome:{0}"

        Public Function Download2(speciesId As String) As KeyValuePairObject(Of KeyValuePair(Of String, String), Gene())()
            Dim pageContent As String = Strings.Split(String.Format(URL_KEGG_PATHWAY_ENTRY_PAGE, speciesId).GET, modParser.SEPERATOR).Last
            Dim Entries = (From Match As Match
                           In Regex.Matches(pageContent, "<a href="".+?"">.+?</a>.+?$", RegexOptions.Multiline + RegexOptions.IgnoreCase)
                           Select Match.Value).ToArray
            Dim Genes As KeyValuePairObject(Of KeyValuePair(Of String, String), Gene())() =
                New KeyValuePairObject(Of KeyValuePair(Of String, String), Gene())(Entries.Length - 2) {}

            Dim Downloader As New System.Net.WebClient()

            Entries = Entries.Take(Entries.Count - 1).ToArray

            Call FileIO.FileSystem.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) & "/Pathways/")

            For i As Integer = 0 To Genes.Count - 1
                Dim Item As String = Entries(i)
                Dim Entry As String = Regex.Match(Item, ">.+?</a>").Value
                Entry = Mid(Entry, 2, Len(Entry) - 5)
                Dim Description As String = Strings.Split(Item, "</a>").Last.Trim
                Dim Url As String = String.Format(Gene.URL_PATHWAY_GENES, Entry)
                Dim ImageUrl = String.Format("http://www.genome.jp/kegg/pathway/{0}/{1}.png", speciesId, Entry)

                Dim ObjUrl = "http://www.genome.jp/dbget-bin/www_bget?pathway+" & Entry
                Call ObjUrl.GET.SaveTo(Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) & "/pathways/webpages/" & Entry & ".html")
                Call Downloader.DownloadFile(ImageUrl, Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) & "/Pathways/" & Entry & ".png")

                Genes(i) = New KeyValuePairObject(Of KeyValuePair(Of String, String), Gene()) With {
                    .Key = New KeyValuePair(Of String, String)(Entry, Description),
                    .Value = Gene.Download(Url)
                }
            Next

            Return Genes
        End Function
    End Module
End Namespace