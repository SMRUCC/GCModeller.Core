Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic

Namespace Assembly.KEGG.DBGET.bGetObject

    ''' <summary>
    ''' The kegg pathway annotation data.
    ''' </summary>
    ''' <remarks></remarks>
    <XmlRoot("KEGG.PathwayBrief", Namespace:="http://www.genome.jp/kegg/pathway.html")>
    Public Class Pathway : Inherits ComponentModel.PathwayBrief

        ''' <summary>
        ''' The name value of this pathway object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Name As String

        ''' <summary>
        ''' The module entry collection data in this pathway.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Modules As ComponentModel.KeyValuePair()
        ''' <summary>
        ''' The kegg compound entry collection data in this pathway.
        ''' (可以通过这个代谢物的列表得到可以出现在当前的这个代谢途径之中的所有的非酶促反应过程，
        ''' 将整个基因组里面的化合物合并起来则可以得到整个细胞内可能存在的非酶促反应过程) 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Compound As ComponentModel.KeyValuePair()

        Public Property PathwayMap As ComponentModel.KeyValuePair

        Public Property Genes As ComponentModel.KeyValuePair()
            Get
                Return _lstGene
            End Get
            Set(value As ComponentModel.KeyValuePair())
                _lstGene = value
                If Not value.IsNullOrEmpty Then _
                    _GeneDict = value.ToDictionary(Function(item As ComponentModel.KeyValuePair) item.Key) Else _
                    _GeneDict = New Dictionary(Of String, ComponentModel.KeyValuePair)
            End Set
        End Property

        Public Overrides ReadOnly Property BriteId As String
            Get
                Dim id As String = Regex.Match(EntryId, "\d+").Value
                If String.IsNullOrEmpty(id) Then
                    Return EntryId
                Else
                    Return id
                End If
            End Get
        End Property

        Public Property Disease As ComponentModel.KeyValuePair()

        Dim _lstGene As ComponentModel.KeyValuePair()
        Dim _GeneDict As Dictionary(Of String, ComponentModel.KeyValuePair) = New Dictionary(Of String, ComponentModel.KeyValuePair)

        Const SEARCH_URL As String = "http://www.kegg.jp/kegg-bin/search_pathway_text?map={0}&keyword=&mode=1&viewImage=false"
        Const PATHWAY_DBGET As String = "http://www.genome.jp/dbget-bin/www_bget?pathway:{0}{1}"

        Public Function IsContainsCompound(KEGGCompound As String) As Boolean
            If Compound.IsNullOrEmpty Then
                Return False
            End If

            Dim LQuery = (From comp In Compound Where String.Equals(comp.Key, KEGGCompound) Select comp).ToArray
            Return Not LQuery.IsNullOrEmpty
        End Function

        Public Function IsContainsGeneObject(GeneId As String) As Boolean
            Return _GeneDict.ContainsKey(GeneId)
        End Function

        ''' <summary>
        ''' Is current pathway object contains the specific module information?(当前的代谢途径对象是否包含有目标模块信息.)
        ''' </summary>
        ''' <param name="ModuleId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IsContainsModule(ModuleId As String) As Boolean
            If Modules.IsNullOrEmpty Then
                Return False
            End If
            Dim LQuery = (From [mod] In Modules
                          Where String.Equals([mod].Key, ModuleId)
                          Select [mod]).FirstOrDefault
            Return Not LQuery Is Nothing
        End Function

        ''' <summary>
        ''' Downloads all of the available pathway information for the target species genome.(下载目标基因组对象之中的所有可用的代谢途径信息)
        ''' </summary>
        ''' <param name="SpeciesCode">The brief code of the target genome species in KEGG database.(目标基因组在KEGG数据库之中的简写编号.)</param>
        ''' <param name="Export"></param>
        ''' <returns>返回成功下载的代谢途径的数目</returns>
        ''' <remarks></remarks>
        Public Shared Function Download(SpeciesCode As String, Export As String, Optional BriefFile As String = "") As Integer
            Dim BriefEntries As KEGG.DBGET.BriteHEntry.Pathway() =
                If(String.IsNullOrEmpty(BriefFile),
                   KEGG.DBGET.BriteHEntry.Pathway.LoadFromResource,
                   KEGG.DBGET.BriteHEntry.Pathway.LoadData(BriefFile))

            For Each Entry As KEGG.DBGET.BriteHEntry.Pathway In BriefEntries
                Dim EntryId As String = String.Format("{0}{1}", SpeciesCode, Entry.Entry.Key)
                Dim SaveToDir As String = BriteHEntry.Pathway.CombineDIR(Entry, Export)

                Dim XmlFile As String = String.Format("{0}/{1}.xml", SaveToDir, EntryId)
                Dim PngFile As String = String.Format("{0}/{1}.png", SaveToDir, EntryId)

                If XmlFile.FileExists AndAlso PngFile.FileExists Then
                    Continue For
                End If

                Dim Pathway As Pathway = DownloadPage(SpeciesCode, Entry.Entry.Key)

                If Pathway Is Nothing Then
                    Call $"[{SpeciesCode}] {Entry.ToString} is not exists in the KEGG!".__DEBUG_ECHO
                    Continue For
                End If

                Call DownloadPathwayMap(SpeciesCode, Entry.Entry.Key, SaveLocationDir:=SaveToDir)
                Call Pathway.GetXml.SaveTo(XmlFile)
            Next

            Return 0
        End Function

        Public Shared Function DownloadAll(Export As String, Optional BriefFile As String = "", Optional DirectoryOrganized As Boolean = True) As Integer
            Dim BriefEntries As KEGG.DBGET.BriteHEntry.Pathway() =
                If(String.IsNullOrEmpty(BriefFile),
                   KEGG.DBGET.BriteHEntry.Pathway.LoadFromResource,
                   KEGG.DBGET.BriteHEntry.Pathway.LoadData(BriefFile))

            For Each Entry As KEGG.DBGET.BriteHEntry.Pathway In BriefEntries
                Dim EntryId As String = Entry.Entry.Key
                Dim SaveToDir As String = If(DirectoryOrganized, BriteHEntry.Pathway.CombineDIR(Entry, Export), Export)

                Dim XmlFile As String = $"{SaveToDir}/map{EntryId}.xml"
                Dim PngFile As String = $"{SaveToDir}/map{EntryId}.png"

                If FileIO.FileSystem.FileExists(XmlFile) AndAlso FileIO.FileSystem.FileExists(PngFile) Then
                    If FileIO.FileSystem.GetFileInfo(XmlFile).Length > 0 AndAlso FileIO.FileSystem.GetFileInfo(PngFile).Length > 0 Then
                        Continue For
                    End If
                End If

                Dim Pathway As Pathway = DownloadPage("map", EntryId)

                If Pathway Is Nothing Then
                    Call $"{Entry.ToString} is not exists in the kegg!".__DEBUG_ECHO
                    Continue For
                End If

                Call DownloadPathwayMap("map", EntryId, SaveLocationDir:=SaveToDir)
                Call Pathway.SaveAsXml(XmlFile)
            Next

            Return 0
        End Function

        Public Shared Function DownloadPathwayMap(SpeciesCode As String, Entry As String, SaveLocationDir As String) As Boolean
            Dim Url As String = String.Format("http://www.genome.jp/kegg/pathway/{0}/{0}{1}.png", SpeciesCode, Entry)
            Return Url.DownloadFile(String.Format("{0}/{1}{2}.png", SaveLocationDir, SpeciesCode, Entry))
        End Function

        Public Shared Function DownloadPage(SpeciesCode As String, Entry As String) As Pathway
            Return DownloadPage(url:=String.Format(PATHWAY_DBGET, SpeciesCode, Entry))
        End Function

        Public Shared Function DownloadPage(url As String) As Pathway
            Dim WebForm As KEGG.WebServices.InternalWebFormParsers.WebForm =
                New WebServices.InternalWebFormParsers.WebForm(url)

            If WebForm.Count = 0 Then
                Return Nothing
            End If

            Dim Pathway As Pathway = New Pathway
            Dim SpeciesCode As String = WebForm.GetValue("Organism").FirstOrDefault

            SpeciesCode = WebServices.InternalWebFormParsers.WebForm.GetNodeValue(Regex.Match(SpeciesCode, "\[GN:<a href="".+?"">.+?</a>]").Value)
            Pathway.EntryId = Regex.Match(WebForm.GetValue("Entry").FirstOrDefault, "[a-z]+\d+", RegexOptions.IgnoreCase).Value
            Pathway.Name = WebForm.GetValue("Name").FirstOrDefault
            Pathway.Disease = __parseHTML_ModuleList(WebForm.GetValue("Disease").FirstOrDefault, LIST_TYPES.Disease)
            Pathway.PathwayMap = __parseHTML_ModuleList(WebForm.GetValue("Pathway map").FirstOrDefault, LIST_TYPES.Pathway).FirstOrDefault
            Pathway.Description = KEGG.WebServices.InternalWebFormParsers.WebForm.RemoveHrefLink(WebForm.GetValue("Description").FirstOrDefault)
            Pathway.Modules = __parseHTML_ModuleList(WebForm.GetValue("Module").FirstOrDefault, LIST_TYPES.Module)
            Pathway.Genes = KEGG.WebServices.InternalWebFormParsers.WebForm.parseList(WebForm.GetValue("Gene").FirstOrDefault, String.Format(GENE_SPLIT, SpeciesCode))
            Pathway.Compound = KEGG.WebServices.InternalWebFormParsers.WebForm.parseList(WebForm.GetValue("Compound").FirstOrDefault, COMPOUND_SPLIT)

            Return Pathway
        End Function

        Const PATHWAY_SPLIT As String = "<a href=""/kegg-bin/show_pathway.+?"">.+?"
        Const MODULE_SPLIT As String = "<a href=""/kegg-bin/show_module.+?"">.+?"
        Const GENE_SPLIT As String = "<a href=""/dbget-bin/www_bget\?{0}:.+?"">.+?</a>"
        Const COMPOUND_SPLIT As String = "\<a href\=""/dbget-bin/www_bget\?cpd:.+?""\>.+?\</a\>.+?"

        Public Shared Function GetCompoundCollection(Collection As Generic.IEnumerable(Of Pathway)) As String()
            Dim CompoundList As List(Of String) = New List(Of String)
            For Each Pathway In Collection
                If Pathway.Compound.IsNullOrEmpty Then
                    Continue For
                End If

                Call CompoundList.AddRange((From item In Pathway.Compound Select item.Key).ToArray)
            Next

            Return (From strid As String In CompoundList Where Not String.IsNullOrEmpty(strid) Select strid Distinct Order By strid Ascending).ToArray
        End Function

        Public Shared Function GetCompoundCollection(ImportsDir As String) As String()
            Dim LQuery = (From file As String
                          In FileIO.FileSystem.GetFiles(ImportsDir, FileIO.SearchOption.SearchAllSubDirectories, "*.xml")
                          Select file.LoadXml(Of Pathway)()).ToArray
            Return GetCompoundCollection(LQuery)
        End Function

        ''' <summary>
        ''' Pathway和Module的格式都是一样的，所以在这里通过<paramref name="type"/>参数来控制对象的类型
        ''' </summary>
        ''' <param name="s_Value"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Friend Shared Function __parseHTML_ModuleList(s_Value As String, type As LIST_TYPES) As ComponentModel.KeyValuePair()
            If String.IsNullOrEmpty(s_Value) Then
                Return Nothing
            End If

            Dim SplitRegex As String = ""

            Select Case type
                Case LIST_TYPES.Disease
                    SplitRegex = "<a href=""/dbget-bin/www_bget\?ds:H.+?"">.+?"
                Case LIST_TYPES.Module
                    SplitRegex = MODULE_SPLIT
                Case LIST_TYPES.Pathway
                    SplitRegex = PATHWAY_SPLIT
            End Select

            Dim Chunkbuffer As String() = (From m As Match In Regex.Matches(s_Value, SplitRegex) Select m.Value).ToArray
            Dim ModuleList As List(Of ComponentModel.KeyValuePair) = New List(Of ComponentModel.KeyValuePair)

            Select Case type
                Case LIST_TYPES.Disease
                    SplitRegex = "<a href=""/dbget-bin/www_bget\?ds:H.+?"">.+?</a>"
                Case LIST_TYPES.Module
                    SplitRegex = "<a href=""/kegg-bin/show_module.+?"">.+?</a>"
                Case LIST_TYPES.Pathway
                    SplitRegex = "<a href=""/kegg-bin/show_pathway.+?"">.+?</a>"
            End Select

            For i As Integer = 0 To Chunkbuffer.Count - 2
                Dim p1 As Integer = InStr(s_Value, Chunkbuffer(i))
                Dim p2 As Integer = InStr(s_Value, Chunkbuffer(i + 1))
                Dim len As Integer = p2 - p1
                Dim strTemp As String = Mid(s_Value, p1, len)

                Dim ModuleEntry As String = Regex.Match(strTemp, SplitRegex).Value
                Dim ModuleFunction As String = strTemp.Replace(ModuleEntry, "").Trim

                ModuleEntry = KEGG.WebServices.InternalWebFormParsers.WebForm.GetNodeValue(ModuleEntry)
                ModuleFunction = KEGG.WebServices.InternalWebFormParsers.WebForm.RemoveHrefLink(ModuleFunction)

                Call ModuleList.Add(New ComponentModel.KeyValuePair With {.Key = ModuleEntry, .Value = ModuleFunction})
            Next

            Dim p As Integer = InStr(s_Value, Chunkbuffer.Last)
            s_Value = Mid(s_Value, p)
            Dim LastEntry As ComponentModel.KeyValuePair = New ComponentModel.KeyValuePair
            LastEntry.Key = Regex.Match(s_Value, SplitRegex).Value
            LastEntry.Value = KEGG.WebServices.InternalWebFormParsers.WebForm.RemoveHrefLink(s_Value.Replace(LastEntry.Key, "").Trim)
            LastEntry.Key = KEGG.WebServices.InternalWebFormParsers.WebForm.GetNodeValue(LastEntry.Key)

            Call ModuleList.Add(LastEntry)

            Return ModuleList.ToArray
        End Function

        Friend Enum LIST_TYPES As Integer
            [Module]
            [Pathway]
            [Disease]
        End Enum

        ''' <summary>
        ''' 获取这个代谢途径之中的所有的基因。这个是安全的函数，当出现空值的基因集合的时候函数会返回一个空集合而非空值
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function GetPathwayGenes() As String()
            If Genes.IsNullOrEmpty Then
                Call $"{EntryId} Gene set is Null!".__DEBUG_ECHO
                Return New String() {}
            End If

            Dim LQuery As String() = (From gEntry As ComponentModel.KeyValuePair
                                      In Genes
                                      Select gEntry.Key).ToArray
            Return LQuery
        End Function
    End Class

    Public Class PathwayEntry

        Public Property Entry As String
        Public Property Name As String
        Public Property Description As String
        Public Property [Object] As String
        Public Property Legend As String
        Public Property Url As String

        Public Const ENTRY_ITEM As String = "<a target="".+?"" href=.+?</tr>"

        Public Overrides Function ToString() As String
            Return String.Format("{0}:  {1}", Entry, Description)
        End Function

        Public Shared Function TryParseWebPage(url As String) As PathwayEntry()
            Dim PageContent As String = url.GET
            Dim ChunkBuffer As String() = (From m As Match In Regex.Matches(PageContent, ENTRY_ITEM, RegexOptions.Singleline) Select m.Value).ToArray
            Dim LQuery = (From strLine As String In ChunkBuffer Select GenerateObject(strLine)).ToArray

            Return LQuery
        End Function

        Private Shared Function GenerateObject(strValue As String) As PathwayEntry
            Dim EntryItem As PathwayEntry = New PathwayEntry
            Dim ChunkBuffer As String() = Strings.Split(strValue, vbLf)
            EntryItem.Entry = KEGG.WebServices.InternalWebFormParsers.WebForm.GetNodeValue(ChunkBuffer.First)
            EntryItem.Url = ChunkBuffer.First.Get_href
            ChunkBuffer = ChunkBuffer.Skip(3).ToArray
            Dim p As Integer = 0
            EntryItem.Name = KEGG.WebServices.InternalWebFormParsers.WebForm.GetNodeValue(ChunkBuffer(p.MoveNext))
            EntryItem.Description = KEGG.WebServices.InternalWebFormParsers.WebForm.GetNodeValue(ChunkBuffer(p.MoveNext))
            EntryItem.Object = KEGG.WebServices.InternalWebFormParsers.WebForm.GetNodeValue(ChunkBuffer(p.MoveNext))
            EntryItem.Legend = KEGG.WebServices.InternalWebFormParsers.WebForm.GetNodeValue(ChunkBuffer(p.MoveNext))

            Return EntryItem
        End Function
    End Class
End Namespace