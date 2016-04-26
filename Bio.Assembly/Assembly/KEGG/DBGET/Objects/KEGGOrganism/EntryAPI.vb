Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text.Similarity

Namespace Assembly.KEGG.DBGET.bGetObject.Organism

    ''' <summary>
    ''' 
    ''' </summary>
    <PackageNamespace("KEGG.DBGET.spEntry",
                      Publisher:="amethyst.asuka@gcmodeller.org",
                      Url:=EntryAPI.WEB_URL,
                      Category:=APICategories.UtilityTools,
                      Description:="KEGG Organisms: Complete Genomes")>
    Public Module EntryAPI

        ReadOnly __cacheList As KEGGOrganism
        ReadOnly __spHash As Dictionary(Of String, Organism)

        Sub New()
            Try
                Dim res As New SoftwareToolkits.Resources(GetType(EntryAPI).Assembly)
                __cacheList = __loadList(res.GetString("KEGG_Organism_Complete_Genomes"))
                __spHash = __cacheList.ToArray.ToDictionary(Function(x) x.KEGGId)
            Catch ex As Exception
                Call App.LogException(ex)
                Call ex.PrintException
            End Try
        End Sub

        Public Function GetValue(sp As String) As Organism
            If __spHash.ContainsKey(sp) Then
                Return __spHash(sp)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' 通过本地资源从基因组全名之中得到KEGG之中的三字母的简写代码
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("KEGG.spCode",
                   Info:="Convert the species genome full name into the KEGG 3 letters briefly code.")>
        Public Function GetKEGGSpeciesCode(Name As String) As String
            Dim LQuery As Organism = (From x As Organism
                                      In __cacheList.ToArray.AsParallel ' Let lev As DistResult = StatementMatches.Match(Name, x.Species) Where Not lev Is Nothing AndAlso lev.NumMatches >= 2
                                      Where String.Equals(Name, x.Species, StringComparison.OrdinalIgnoreCase)
                                      Select x).FirstOrDefault
            If LQuery Is Nothing Then
                Call $"Could not found any entry for ""{Name}"" from KEGG...".__DEBUG_ECHO
                Return ""
            Else
                Return LQuery.KEGGId
            End If
        End Function

        ''' <summary>
        ''' Load KEGG organism list from the internal resource.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("list.Load", Info:="Load KEGG organism list from the internal resource.")>
        Public Function GetOrganismListFromResource() As KEGGOrganism
            Dim res As New SoftwareToolkits.Resources(GetType(EntryAPI).Assembly)
            Dim html As String = res.GetString("KEGG_Organisms__Complete_Genomes")
            Return __loadList(html)
        End Function

        Public Const WEB_URL As String = "http://www.genome.jp/kegg/catalog/org_list.html"
        Public Const DELIMITER As String = "</td>"
        Public Const CELL As String = "<tr .+?</tr>"

        Private Function __loadList(html As String) As KEGGOrganism
            Dim Tokens As String() = Regex.Matches(html, CELL, RegexICSng).ToArray.Skip(1).ToArray
            Dim eulst As Organism() = New Organism(Tokens.Length - 1) {}
            Dim i As Integer
            Dim prlst As Prokaryote() = New Prokaryote(Tokens.Length - i) {}

            For i = 0 To Tokens.Length - 1
                eulst(i) = Organism.__createObject(Tokens(i))
                If eulst(i) Is Nothing Then
                    Exit For
                End If
            Next

            Dim j As Integer
            For i = i + 1 To Tokens.Length - 1
                prlst(j) = New Prokaryote(Tokens(i))
                j += 1
            Next

            Dim LQuery = (From Handle As Integer In eulst.Sequence
                          Let obj As Organism = eulst(Handle)
                          Where Not obj Is Nothing
                          Select obj.Trim).ToArray
            Dim lstProk As Prokaryote() = (From handle As Integer In prlst.Sequence
                                           Let obj As Prokaryote = prlst(handle)
                                           Where Not obj Is Nothing
                                           Select DirectCast(obj.Trim, Prokaryote)).ToArray
            Dim lstKEGGOrgsm As KEGGOrganism =
                New KEGGOrganism With {
                    .Eukaryotes = LQuery,
                    .Prokaryote = lstProk
            }

            Dim Phylum As String = ""
            Dim [Class] As String = ""
            For idx As Integer = 0 To lstKEGGOrgsm.Eukaryotes.Length - 1
                Dim Organism = lstKEGGOrgsm.Eukaryotes(idx)
                If Not String.IsNullOrEmpty(Organism.Class) Then
                    [Class] = Organism.Class
                Else
                    Organism.Class = [Class]
                End If
                If Not String.IsNullOrEmpty(Organism.Phylum) Then
                    Phylum = Organism.Phylum
                Else
                    Organism.Phylum = Phylum
                End If
            Next

            Dim Kingdom As String = ""
            Phylum = "" : [Class] = ""
            For idx As Integer = 0 To lstKEGGOrgsm.Prokaryote.Length - 1
                Dim Organism = lstKEGGOrgsm.Prokaryote(idx)
                If Not String.IsNullOrEmpty(Organism.Class) Then
                    [Class] = Organism.Class
                Else
                    Organism.Class = [Class]
                End If
                If Not String.IsNullOrEmpty(Organism.Phylum) Then
                    Phylum = Organism.Phylum
                Else
                    Organism.Phylum = Phylum
                End If
                If Not String.IsNullOrEmpty(Organism.Kingdom) Then
                    Kingdom = Organism.Kingdom
                Else
                    Organism.Kingdom = Kingdom
                End If
            Next

            Return lstKEGGOrgsm
        End Function

        ''' <summary>
        ''' Gets the latest KEGG organism list from query the KEGG database.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("list.Get", Info:="Gets the latest KEGG organism list from query the KEGG database.")>
        Public Function GetOrganismList() As KEGGOrganism
            Dim html As String = WEB_URL.GET
            Return __loadList(html)
        End Function

        Public Function FromResource(url As String) As KEGGOrganism
            Dim page As String = url.GET
            Return __loadList(page)
        End Function
    End Module
End Namespace