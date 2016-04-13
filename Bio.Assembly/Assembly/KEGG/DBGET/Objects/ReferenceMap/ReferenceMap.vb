Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports LANS.SystemsBiology.Assembly.KEGG.WebServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Assembly.KEGG.DBGET.ReferenceMap

    ''' <summary>
    ''' KEGG数据库之中的参考途径
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <XmlType("KEGG-ReferenceMapData", Namespace:="http://code.google.com/p/genome-in-code/kegg/reference_map_data")>
    Public Class ReferenceMapData : Inherits ComponentModel.PathwayBrief

        ''' <summary>
        ''' 直系同源的参考基因
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlElement("ReferenceGeneData")>
        Public Property ReferenceGenes As KeyValuePairObject(Of KEGG.WebServices.ListEntry, KeyValuePairObject(Of String, SequenceModel.FASTA.FastaToken)())()
            Get
                If _refGeneOrthology.IsNullOrEmpty Then
                    Return New KeyValuePairObject(Of KEGG.WebServices.ListEntry, KeyValuePairObject(Of String, SequenceModel.FASTA.FastaToken)())() {}
                End If
                Return _refGeneOrthology.Values.ToArray
            End Get
            Set(value As KeyValuePairObject(Of KEGG.WebServices.ListEntry, KeyValuePairObject(Of String, SequenceModel.FASTA.FastaToken)())())
                If value.IsNullOrEmpty Then
                    _refGeneOrthology = New Dictionary(Of String, KeyValuePairObject(Of ListEntry, KeyValuePairObject(Of String, SequenceModel.FASTA.FastaToken)()))
                Else
                    _refGeneOrthology = value.ToDictionary(Function(obj) obj.Key.EntryID)
                End If
            End Set
        End Property

        Dim _refGeneOrthology As Dictionary(Of String, KeyValuePairObject(Of KEGG.WebServices.ListEntry, KeyValuePairObject(Of String, SequenceModel.FASTA.FastaToken)()))
        Dim _refReactions As Dictionary(Of String, ReferenceMap.ReferenceReaction) =
            New Dictionary(Of String, ReferenceReaction)

        Public Property [Class] As String
        Public Property Name As String
        Public Property [Module] As ComponentModel.KeyValuePair()
        Public Property Disease As ComponentModel.KeyValuePair()
        Public Property OtherDBs As ComponentModel.KeyValuePair()
        Public Property References As String()
        Public Property Reactions As ReferenceReaction()
            Get
                If _refReactions.IsNullOrEmpty Then
                    Return New ReferenceMap.ReferenceReaction() {}
                End If
                Return _refReactions.Values.ToArray
            End Get
            Set(value As ReferenceReaction())
                If value.IsNullOrEmpty Then
                    _refReactions = New Dictionary(Of String, ReferenceReaction)
                Else
                    _refReactions = value.ToDictionary(Function(refRxn) refRxn.Entry)
                End If
            End Set
        End Property

        Public Function GetReaction(ID As String) As ReferenceMap.ReferenceReaction
            If _refReactions.ContainsKey(ID) Then
                Return _refReactions(ID)
            Else
                Return Nothing
            End If
        End Function

        Public Overrides Function GetPathwayGenes() As String()
            Dim LQuery = (From item In Me.ReferenceGenes Select (From nn In item.Value Select nn.Key.Split(CChar(":")).Last).ToArray).ToArray.MatrixToVector
            Return LQuery
        End Function

        Const DBGET_URL As String = "http://www.genome.jp/dbget-bin/www_bget?"
        Const MODULE_PATTERN As String = "<a href=""/kegg-bin/show_module\?M\d+.+?\[PATH:.+?</a>\]"

        Public Function GetGeneOrthology(refRxn As ReferenceMap.ReferenceReaction) As KeyValuePairObject(Of KEGG.WebServices.ListEntry, KeyValuePairObject(Of String, SequenceModel.FASTA.FastaToken)())()
            Dim LQuery = (From ort In refRxn.SSDBs Where _refGeneOrthology.ContainsKey(ort.Key) Select _refGeneOrthology(ort.Key)).ToArray
            Return LQuery
        End Function

        Public Function GetGeneOrthology(KO_ID As String) As KeyValuePairObject(Of KEGG.WebServices.ListEntry, KeyValuePairObject(Of String, SequenceModel.FASTA.FastaToken)())
            If _refGeneOrthology.ContainsKey(KO_ID) Then
                Return _refGeneOrthology(KO_ID)
            Else
                Return Nothing
            End If
        End Function

        Public Shared Function Download(ID As String) As ReferenceMap.ReferenceMapData
            Dim Form As KEGG.WebServices.InternalWebFormParsers.WebForm = New WebServices.InternalWebFormParsers.WebForm(Url:=DBGET_URL & ID)
            Dim RefMap As ReferenceMap.ReferenceMapData = New ReferenceMap.ReferenceMapData With {.EntryId = ID}

            RefMap.Name = Form("Name").FirstOrDefault
            RefMap.Description = Form("Description").FirstOrDefault
            RefMap.Class = Form("Class").FirstOrDefault

            Dim sValue As String

            sValue = Form("Module").FirstOrDefault
            If Not String.IsNullOrEmpty(sValue) Then
                RefMap.Module = (From m As Match In Regex.Matches(sValue, MODULE_PATTERN)
                                 Let str As String = m.Value
                                 Let ModID As String = Regex.Match(str, "M\d+").Value
                                 Let descr As String = str.Replace(String.Format("<a href=""/kegg-bin/show_module?{0}"">{0}</a>", ModID), "").Trim
                                 Select New ComponentModel.KeyValuePair With {
                                     .Key = ModID,
                                     .Value = Regex.Replace(descr, "<a href=""/kegg-bin/show_pathway?map\d+\+M\d+"">map\d+</a>", ModID)}).ToArray
            End If

            sValue = Form("Disease").FirstOrDefault
            If Not String.IsNullOrEmpty(sValue) Then
                RefMap.Disease = (From m As Match
                                  In Regex.Matches(sValue, "<a href=""/dbget-bin/www_bget\?ds:H.+?"">H.+?</a> [^<]+")
                                  Let str As String = m.Value
                                  Select __diseaseParser(str)).ToArray
            End If

            sValue = Form("Other DBs").FirstOrDefault

            If Not String.IsNullOrEmpty(sValue) Then
                RefMap.OtherDBs = __DBLinksParser(sValue)
            End If

            Dim ReactionEntryList = KEGG.WebServices.LoadList(Form.AllLinksWidget("KEGG REACTION")) '代谢途径之中的代谢反应的集合
            Dim RefGeneEntryList = KEGG.WebServices.LoadList(Form.AllLinksWidget("Gene")) '当前的这个代谢途径之中的直系同源基因列表
            RefMap.ReferenceGenes = (From item As ListEntry In RefGeneEntryList
                                     Select New KeyValuePairObject(Of KEGG.WebServices.ListEntry, KeyValuePairObject(Of String, SequenceModel.FASTA.FastaToken)()) _
                                     With {.Key = item, .Value = Nothing}).ToArray
            RefMap.Reactions = (From Entry In ReactionEntryList Select __downloadRefRxn(Entry)).ToArray

            Return RefMap
        End Function

        Private Shared Function __downloadRefRxn(Entry As WebServices.ListEntry) As ReferenceMap.ReferenceReaction
            Dim path As String = "./Downloads/" & Entry.EntryID.NormalizePathString & ".xml"

            If FileIO.FileSystem.FileExists(path) Then
                Dim refData = path.LoadXml(Of ReferenceMap.ReferenceReaction)()
                If Not refData Is Nothing AndAlso Not String.IsNullOrEmpty(refData.Equation) Then
                    Return refData
                End If
            End If

            Dim ref = ReferenceReaction.Download(Entry)
            Call ref.GetXml.SaveTo(path)
            Return ref
        End Function

        Const DB_LINK_PATTERN As String = ".+: (<a href="".+?"">.+?</a>\s*)+"

        Private Shared Function __DBLinksParser(str As String) As ComponentModel.KeyValuePair()
            Dim ChunkBuffer As String() = (From m As Match In Regex.Matches(str, DB_LINK_PATTERN) Select m.Value).ToArray
            Dim LQuery = (From s As String In ChunkBuffer Select __parserLinks(s)).ToArray.MatrixToVector
            Return LQuery
        End Function

        Private Shared Function __parserLinks(str As String) As ComponentModel.KeyValuePair()
            Dim DBName As String = Regex.Match(str, ".+?:").Value
            Dim ID As String() = (From m As Match In Regex.Matches(str.Replace(DBName, ""), "<a href=.+?</a>") Select Regex.Match(m.Value, ">.+?</a>").Value).ToArray
            DBName = DBName.Split(CChar(":")).First
            Dim LQuery = (From sid As String In ID Select New ComponentModel.KeyValuePair With {.Key = DBName, .Value = sid.GetValue}).ToArray
            Return LQuery
        End Function

        Private Shared Function __diseaseParser(str As String) As ComponentModel.KeyValuePair
            Dim dsID As String = Regex.Match(str, "H\d+").Value
            Dim Description As String = str.Replace(String.Format("<a href=""/dbget-bin/www_bget?ds:{0}"">{0}</a>", dsID), "")
            Return New ComponentModel.KeyValuePair With {.Key = dsID, .Value = Description}
        End Function
    End Class
End Namespace