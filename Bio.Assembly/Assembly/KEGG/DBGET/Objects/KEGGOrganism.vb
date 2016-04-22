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
        <ExportAPI("KEGG.spCode", Info:="Convert the species genome full name into the KEGG 3 letters briefly code.")>
        Public Function GetKEGGSpeciesCode(Name As String) As String
            Dim LQuery = (From x As Organism In __cacheList.ToArray.AsParallel ' Let lev As DistResult = StatementMatches.Match(Name, x.Species) Where Not lev Is Nothing AndAlso lev.NumMatches >= 2
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
                eulst(i) = Organism.CreateObject(Tokens(i))
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

    ''' <summary>
    ''' KEGG Organisms: Complete Genomes (http://www.genome.jp/kegg/catalog/org_list.html)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class KEGGOrganism

        Public Function ToArray() As Organism()
            Return {Me.Eukaryotes, Me.Prokaryote}.MatrixToVector
        End Function

        Public Property Eukaryotes As Organism()
            Get
                Return _Eukaryotes.Values.ToArray
            End Get
            Set(value As Organism())
                If value.IsNullOrEmpty Then
                    _Eukaryotes = New Dictionary(Of Organism)
                Else
                    _Eukaryotes = value.ToDictionary
                End If
            End Set
        End Property
        Public Property Prokaryote As Prokaryote()
            Get
                Return _Prokaryote.Values.ToArray
            End Get
            Set(value As Prokaryote())
                If value.IsNullOrEmpty Then
                    _Prokaryote = New Dictionary(Of Prokaryote)
                Else
                    _Prokaryote = value.ToDictionary
                End If
            End Set
        End Property

        Dim _Eukaryotes As Dictionary(Of Organism)
        Dim _Prokaryote As Dictionary(Of Prokaryote)

        Default Public ReadOnly Property GetOrganismData(SpeciesCode As String) As Organism
            Get
                If _Eukaryotes.ContainsKey(SpeciesCode) Then
                    Return _Eukaryotes(SpeciesCode)
                End If

                If _Prokaryote.ContainsKey(SpeciesCode) Then
                    Return _Prokaryote(SpeciesCode)
                End If

                Return Nothing
            End Get
        End Property
    End Class

    Public Class Organism : Implements sIdEnumerable

        <XmlAttribute> Public Property Kingdom As String
        <XmlAttribute> Public Property Phylum As String
        <XmlAttribute> Public Property [Class] As String
        ''' <summary>
        ''' 物种全称
        ''' </summary>
        ''' <returns></returns>
        Public Property Species As String

        ''' <summary>
        ''' KEGG里面的物种的简称代码
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property KEGGId As String Implements sIdEnumerable.Identifier
        ''' <summary>
        ''' FTP url on NCBI
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property RefSeq As String

        Public Const Field As String = "<td rowspan=\d+  align=[a-z]+>.+</td>"
        Public Const ClassType As String = "<td rowspan=\d+  align=[a+z]+><a href='.+'>.+</a></td>"
        Public Const CELL_ITEM As String = "<td align=[a-z]+><a href='.+'>[a-z0-9]+</a>"

        Protected Friend Shared __setValues As System.Action(Of Organism, String)() =
            New System.Action(Of Organism, String)() {
 _
                Sub(objOrganism As Organism, value As String) objOrganism.Kingdom = value,
                Sub(objOrganism As Organism, value As String) objOrganism.Phylum = value,
                Sub(objOrganism As Organism, value As String) objOrganism.Class = value,
                Sub(objOrganism As Organism, value As String) objOrganism.KEGGId = value,
                Sub(objOrganism As Organism, value As String) objOrganism.Species = value,
                Sub(objOrganism As Organism, value As String) objOrganism.RefSeq = value
           }

        Friend Overridable Function Trim() As Organism
            Phylum = GetValue(Phylum)
            [Class] = GetValue([Class])
            Species = GetValue(Species)
            KEGGId = GetValue(KEGGId)
            RefSeq = Regex.Match(RefSeq, """.+""").Value
            RefSeq = Mid(RefSeq, 2, Len(RefSeq) - 2)

            Return Me
        End Function

        Protected Friend Shared Function GetValue(str As String) As String
            If String.IsNullOrEmpty(str) Then
                Return ""
            End If
            str = Regex.Match(str, ">.+?<", RegexOptions.Singleline).Value
            str = Mid(str, 2, Len(str) - 2)
            Return str
        End Function

        Friend Shared Function CreateObject(srcText As String) As Organism
            Dim Tokens As String() = (From m As Match
                                      In Regex.Matches(srcText, "<a href=.+?</a>", RegexOptions.Singleline + RegexOptions.IgnoreCase)
                                      Select m.Value).ToArray
            If Tokens.IsNullOrEmpty Then Return Nothing
            Dim Organism As Organism = New Organism
            Dim p As Integer = Organism.__setValues.Length - 1

            For i As Integer = Tokens.Length - 1 To 0 Step -1
                Call Organism.__setValues(p)(Organism, Tokens(i))
                p -= 1
            Next
            Return Organism
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("{0}: {1}", KEGGId, Species)
        End Function
    End Class

    Public Class Prokaryote : Inherits Organism

        <XmlAttribute> Public Property Year As String

        Public Sub New()
        End Sub

        Sub New(org As Organism)
            MyBase.Class = org.Class
            MyBase.KEGGId = org.KEGGId
            MyBase.Kingdom = org.Kingdom
            MyBase.Phylum = org.Phylum
            MyBase.RefSeq = org.RefSeq
            MyBase.Species = org.Species
        End Sub

        Protected Friend Shared Shadows SetValues As System.Action(Of Prokaryote, String)() =
            New System.Action(Of Prokaryote, String)() {
            Sub(objOrganism As Prokaryote, value As String) objOrganism.Kingdom = value,
            Sub(objOrganism As Prokaryote, value As String) objOrganism.Phylum = value,
            Sub(objOrganism As Prokaryote, value As String) objOrganism.Class = value,
            Sub(objOrganism As Prokaryote, value As String) objOrganism.KEGGId = value,
            Sub(objOrganism As Prokaryote, value As String) objOrganism.Species = value,
            Sub(objOrganism As Prokaryote, value As String) objOrganism.Year = value,
            Sub(objOrganism As Prokaryote, value As String) objOrganism.RefSeq = value
        }

        Protected Friend Sub New(srcText As String)
            Dim Tokens As String() = (From m As Match
                                      In Regex.Matches(srcText, "<td.+?</td>", RegexOptions.Singleline + RegexOptions.IgnoreCase)
                                      Select m.Value).ToArray

            If Tokens.IsNullOrEmpty Then
            Else
                Dim p As Integer = Prokaryote.SetValues.Length - 1

                For i As Integer = Tokens.Length - 1 To 0 Step -1
                    Call Prokaryote.SetValues(p)(Me, Tokens(i))
                    p -= 1
                Next
            End If
        End Sub

        Friend Overrides Function Trim() As Organism
            Phylum = GetValue(Phylum)
            [Class] = GetValue([Class])
            Species = GetValue(Species)
            KEGGId = GetValue(KEGGId)
            Year = GetValue(Year)

            If Not String.IsNullOrEmpty(RefSeq) Then
                RefSeq = Regex.Match(RefSeq, "<a href="".+?"">").Value
            End If
            If Not String.IsNullOrEmpty(RefSeq) Then
                RefSeq = Mid(RefSeq, 10, Len(RefSeq) - 11)
            End If

            Kingdom = GetValue(Kingdom)

            Return Me
        End Function

        Protected Friend Overloads Shared Function GetValue(str As String) As String
            If String.IsNullOrEmpty(str) Then
                Return ""
            End If
            Dim m = Regex.Match(str, "<a href="".+?"">.+?</a>")
            If m.Success Then
                str = Organism.GetValue(m.Value)
            Else
                str = Organism.GetValue(str)
            End If

            Return str
        End Function
    End Class
End Namespace