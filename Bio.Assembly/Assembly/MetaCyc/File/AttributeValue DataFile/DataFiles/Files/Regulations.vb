Namespace Assembly.MetaCyc.File.DataFiles

    ''' <summary>
    ''' This class describes most forms of protein, RNA or activity regulation.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Regulations : Inherits DataFile(Of MetaCyc.File.DataFiles.Slots.Regulation)

        Public Shared Shadows ReadOnly AttributeList As String() = {
            "UNIQUE-ID", "TYPES", "COMMON-NAME", "ACCESSORY-PROTEINS", "ANTI-ANTITERM-END-POS",
            "ANTI-ANTITERM-START-POS", "ANTITERMINATOR-END-POS", "ANTITERMINATOR-START-POS",
            "ASSOCIATED-BINDING-SITE", "ASSOCIATED-RNASE", "COMMENT", "COMMENT-INTERNAL",
            "CREDITS", "DATA-SOURCE", "DOCUMENTATION", "DOWNSTREAM-GENES-ONLY?", "GROWTH-CONDITIONS",
            "HIDE-SLOT?", "INSTANCE-NAME-TEMPLATE", "KI", "MECHANISM", "MEMBER-SORT-FN", "MODE",
            "PAUSE-END-POS", "PAUSE-START-POS", "PHYSIOLOGICALLY-RELEVANT?", "REGULATED-ENTITY",
            "REGULATOR", "SYNONYMS", "CITATIONS"}

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In Regulations.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        ''' <summary>
        ''' 查找出某一个指定UniqueId编号值的Regulation集合
        ''' </summary>
        ''' <param name="Regulator">Regulator的UniqueID属性值</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetRegulationsByRegulator(Regulator As String) As Slots.Regulation()
            Dim LQuery = From Regulation In Me.AsParallel Where String.Equals(Regulation.Regulator, Regulator) Select Regulation '
            Return LQuery.ToArray
        End Function

        'Public Shared Shadows Widening Operator CType(e As LANS.SystemsBiology.Assembly.MetaCyc.File.AttributeValue) As Regulations
        '    Dim NewFile As New Regulations
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.Regulation) =
        '        From c As MetaCyc.File.AttributeValue.Object
        '        In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.Regulation)

        '    NewFile.FrameObjects = Query.ToList
        '    NewFile.DbProperty = e.DbProperty

        '    Return NewFile
        'End Operator

        Public Function GetMechanism() As String()
            Dim TlQuery = From e In Me.FrameObjects Let s = e.Mechanism Where Len(s) > 0 Select s Distinct Order By s Ascending '
            Dim sx = TlQuery.ToArray
            Return TlQuery.ToArray
        End Function

        'Public Shared Shadows Widening Operator CType(Path As String) As Regulations
        '    Dim File As MetaCyc.File.AttributeValue = Path
        '    Return CType(File, Regulations)
        'End Operator

        Public Shared Shadows Narrowing Operator CType(e As Regulations) As MetaCyc.File.DataFiles.Slots.Regulation()
            Return e.FrameObjects.ToArray
        End Operator
    End Class
End Namespace