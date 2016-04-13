Imports System.Data.Linq.Mapping
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic

Namespace Assembly.KEGG.Archives.Csv

    ''' <summary>
    ''' CSV data model for storage the kegg pathway brief information.(用于向Csv文件保存数据的简单格式的代谢途径数据存储对象)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Pathway : Inherits ComponentModel.PathwayBrief
        Implements IKeyValuePairObject(Of String, String())

        Public Overrides Function GetPathwayGenes() As String()
            Return PathwayGenes
        End Function

        ''' <summary>
        ''' Pathway object KEGG database entry id.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property EntryId As String Implements IKeyValuePairObject(Of String, String()).Identifier
            Get
                Return MyBase.EntryId
            End Get
            Set(value As String)
                MyBase.EntryId = value
            End Set
        End Property

        ''' <summary>
        ''' Phenotype Class
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Column(Name:="br.Class")> Public Property [Class] As String

        ''' <summary>
        ''' Phenotype Category
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Column(Name:="br.Category")> Public Property Category As String

        ''' <summary>
        ''' The enzyme gene which was involved in this pathway catalysts.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property PathwayGenes As String() Implements IKeyValuePairObject(Of String, String()).Value

        Public Overrides ReadOnly Property BriteId As String
            Get
                Return Regex.Match(EntryId, "\d+").Value
            End Get
        End Property

        Public Shared Function GenerateObject(XmlModel As KEGG.DBGET.bGetObject.Pathway) As Pathway
            Return New Pathway With {
                .EntryId = XmlModel.EntryId,
                .Description = XmlModel.Description,
                .PathwayGenes = XmlModel.GetPathwayGenes
            }
        End Function

        ''' <summary>
        ''' 将所下载的代谢途径数据转换为CSV文档保存
        ''' </summary>
        ''' <param name="Dir"></param>
        ''' <param name="spCode">物种名称的三字母简写，例如xcb</param>
        ''' <returns></returns>
        Public Shared Function LoadData(Dir As String, spCode As String) As Pathway()
            Dim XMLFiles As KEGG.DBGET.bGetObject.Pathway() = (From File As String
                                                               In FileIO.FileSystem.GetFiles(Dir, FileIO.SearchOption.SearchAllSubDirectories, "*.xml")
                                                               Select File.LoadXml(Of KEGG.DBGET.bGetObject.Pathway)()).ToArray
            Return CreateObjects(XMLFiles, spCode)
        End Function

        Public Shared Function CreateObjects(Data As KEGG.DBGET.bGetObject.Pathway(), spCode As String) As Pathway()
            Dim ClassDictionary As SortedDictionary(Of String, KEGG.DBGET.BriteHEntry.Pathway) =
                New SortedDictionary(Of String, DBGET.BriteHEntry.Pathway)
            For Each item In KEGG.DBGET.BriteHEntry.Pathway.LoadFromResource
                Call ClassDictionary.Add($"{spCode}{item.Entry.Key}", item)
            Next

            Dim PathwayList As List(Of Pathway) = New List(Of Pathway)
            For Each item In Data
                Dim PathwayObject = Pathway.GenerateObject(item)
                Dim BriteEntry = ClassDictionary(PathwayObject.EntryId)

                PathwayObject.Class = BriteEntry.Class
                PathwayObject.Category = BriteEntry.Category
                Call PathwayList.Add(PathwayObject)
            Next

            Return PathwayList.ToArray
        End Function

        Public Shared Function CreateObjects(Model As KEGG.Archives.Xml.XmlModel) As Pathway()
            Dim Pathways = Model.GetAllPathways
            Return CreateObjects(Pathways, Model.spCode)
        End Function
    End Class
End Namespace