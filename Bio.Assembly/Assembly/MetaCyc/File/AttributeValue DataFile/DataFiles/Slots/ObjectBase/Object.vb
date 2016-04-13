Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports LANS.SystemsBiology.Assembly.MetaCyc.File.DataFiles.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic

Namespace Assembly.MetaCyc.File.DataFiles.Slots

    ''' <summary>
    ''' The object type is the base type of the objects definition both in the namespace PGDB.DataFile and PGDB.Schemas 
    ''' </summary>
    ''' <remarks></remarks>
    <XmlType("MetaCyc-Slot-Object")>
    Public Class [Object] : Implements sIdEnumerable

        <MetaCycField()> <XmlAttribute> Public Overridable Property Identifier As String Implements sIdEnumerable.Identifier

        ''' <summary>
        ''' (Common-Name) This slot defines the primary name by which an object is known 
        ''' to scientists -- a widely used and familiar name (in some cases arbitrary 
        ''' choices must be made). This field can have only one value; that value must 
        ''' be a string.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MetaCycField()> <XmlAttribute> Public Overridable Property CommonName As String
        ''' <summary>
        ''' (Abbrev-Name) This slot stores an abbreviated name for an object. It is used in 
        ''' some displays.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MetaCycField()> <XmlAttribute> Public Overridable Property AbbrevName As String

        ''' <summary>
        ''' (Synonyms) This field defines one or more secondary names for an object -- names 
        ''' that a scientist might attempt to use to retrieve the object. These names may be 
        ''' out of date or ambiguous, but are used to facilitate retrieval -- the Synonyms 
        ''' should include any name that you might use to try to retrieve an object. In a 
        ''' sense, the name "Synonyms" is misleading because the names listed in this slot may 
        ''' not be exactly synonymous with the preferred name of the object.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MetaCycField(Type:=MetaCycField.Types.TStr)> <XmlElement> Public Overridable Property Synonyms As String()
        <XmlElement> Public Overridable Property Names As List(Of String)
        ''' <summary>
        ''' (Comment) The Comment slot stores a general comment about the object that contains 
        ''' the slot. The comment should always be enclosed in double quotes.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MetaCycField()> <XmlElement> Public Overridable Property Comment As String
        ''' <summary>
        ''' (Citations) This slot lists general citations pertaining to the object containing 
        ''' the slot. Citations may or may not have evidence codes attached to them. Each value 
        ''' of the slot is a string of the form 
        ''' [reference-ID] or 
        ''' [reference-id:evidence-code:timestamp:curator:probability:with]
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MetaCycField(Type:=MetaCycField.Types.TStr)> <XmlElement> Public Overridable Property Citations As List(Of String)

        ''' <summary>
        ''' The TYPES enumerate values in each object.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <MetaCycField(Type:=MetaCycField.Types.TStr)> <XmlArray> Public Overridable Property Types As List(Of String)
        <MetaCycField(Type:=MetaCycField.Types.TStr, Name:="DBLINKS")> Public Overridable Property DBLinks As String()
            Get
                Return _DBLinks.DBLinks
            End Get
            Set(value As String())
                _DBLinks = Schema.DBLinkManager.CreateFromMetaCycFormat(value)
            End Set
        End Property

        <XmlIgnore> Protected Friend [_1ObjectArray] As List(Of KeyValuePair(Of String, String))
        Protected Friend _DBLinks As MetaCyc.Schema.DBLinkManager

        ''' <summary>
        ''' (解析Unique-Id字段的值所需要的正则表达式)
        ''' </summary>
        ''' <remarks></remarks>
        Public Const UNIQUE_ID_REGX As String = "[0-9A-Z]+([-][0-9A-Z]*)*"

        ''' <summary>
        ''' 当前的对象所属的表对象
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlIgnore> Public Overridable ReadOnly Property Table As [Object].Tables
            Get
                Return Tables.classes
            End Get
        End Property

        Public Function GetDBLinks() As Schema.DBLinkManager
            Return _DBLinks
        End Function

        Public Enum Tables
            bindrxns = 0
            classes = -1
            compounds = 1
            dnabindsites
            enzrxns
            genes
            pathways
            promoters
            proteinfeatures
            proteins
            protligandcplxes
            pubs
            reactions
            regulation
            regulons
            species
            terminators
            transunits
            trna
        End Enum

        ''' <summary>
        ''' 使用关键词查询<see cref="[Object]._1ObjectArray"></see>字典对象
        ''' </summary>
        ''' <param name="Key"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function StringQuery(Key As String) As List(Of String)
            Dim QueryString As String = Key.ToUpper
            Dim LQuery = (From item In Me.[_1ObjectArray] Where String.Equals(item.Key, QueryString) Select item.Value).ToList
            Return LQuery
        End Function

        Public Sub CopyTo(Of T As [Object])(ByRef e As T)
            With e
                .AbbrevName = AbbrevName
                .Citations = Citations
                .Comment = Comment
                .CommonName = CommonName
                .Names = Names
                .[_1ObjectArray] = [_1ObjectArray]
                .Synonyms = Synonyms
                .Identifier = Identifier
                .Types = Types
            End With
        End Sub

        ''' <summary>
        ''' 查询某一个键名是否存在于这个对象之中
        ''' </summary>
        ''' <param name="KeyName">键名</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Exists(KeyName As String) As Boolean
            KeyName = KeyName.ToUpper
            Dim LQuery = (From item In Me.[_1ObjectArray] Where String.Equals(item.Key, KeyName) Select 1).ToArray.Count
            Return LQuery > 0
        End Function

        Public ReadOnly Property Item(Name As String) As String()
            Get
                Return StringQuery(Name).ToArray
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Identifier
        End Function

        Public Shared Widening Operator CType(lst As List(Of KeyValuePair(Of String, String))) As [Object]
            Dim [Object] As New [Object] With {
                .[_1ObjectArray] = lst
            }
            'Dim strTemp As List(Of String)

            '[Object].UniqueId = [Object].StringQuery("UNIQUE-ID").First

            'strTemp = [Object].StringQuery("COMMON-NAME"): If strTemp .Count > 0 then [Object] .
            'If [Object].Exists("") Then [Object].CommonName = e("COMMON-NAME") Else [Object].CommonName = [Object].UniqueId
            'If [Object].Exists("ABBREV-NAME") Then [Object].AbbrevName = e("ABBREV-NAME")
            'If [Object].Exists("COMMENT") Then [Object].Comment = e("COMMENT")

            '[Object].Synonyms = [Object].StringQuery("SYNONYMS( \d+)?")
            '[Object].Types = [Object].StringQuery("TYPES( \d+)?")
            '[Object].Citations = [Object].StringQuery("CITATIONS( \d+)?")

            Return [Object]
        End Operator

        ''' <summary>
        ''' 基类至派生类的转换
        ''' </summary>
        ''' <param name="e">数据源，基类</param>
        ''' <param name="ToType">转换至的目标类型</param> 
        ''' <typeparam name="T">类型约束</typeparam> 
        ''' <remarks></remarks>
        Public Shared Sub [TypeCast](Of T As [Object])(e As [Object], ByRef ToType As T)
            With ToType
                .AbbrevName = e.AbbrevName
                .Citations = e.Citations
                .Comment = e.Comment
                .CommonName = e.CommonName
                .Names = e.Names
                .[_1ObjectArray] = e.[_1ObjectArray]
                .Synonyms = e.Synonyms
                .Identifier = e.Identifier
                .Types = e.Types
            End With
        End Sub
    End Class
End Namespace