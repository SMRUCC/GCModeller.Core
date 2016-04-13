
Namespace Assembly.MetaCyc.File.DataFiles

    ''' <summary>
    ''' Frames in this class encode transcription units, which are defined as a set of genes and
    ''' associated control regions that produce a single transcript. Thus, there is a one-to-one
    ''' correspondence between transcription start sites and transcription units. If a set of genes
    ''' is controlled by multiple transcription start sites, then a PGDB should define multiple
    ''' transcription-unit frames, one for each transcription start site.
    ''' (在本类型中所定义的对象编码一个转录单元，一个转录单元定义了一个基因及与其相关联的转录调控DNA片段
    ''' 的集合，故而，在本对象中有一个与转录单元相一一对应的转录起始位点。假若一个基因簇是由多个转录起始
    ''' 位点所控制的，那么将会在MetaCyc数据库之中分别定义与这些转录起始位点相对应的转录单元【即，每一个
    ''' 本类型的对象的属性之中，仅有一个转录起始位点属性】)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class TransUnits : Inherits DataFile(Of Slots.TransUnit)

        ''' <summary>
        ''' 定义在转录单元类型之中的所有的属性的列表
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared Shadows ReadOnly AttributeList As String() = {
 _
            "UNIQUE-ID", "TYPES", "COMMON-NAME", "CITATIONS", "COMMENT", "COMMENT-INTERNAL",
            "COMPONENT-OF", "COMPONENTS", "CREDITS", "DATA-SOURCE", "DBLINKS", "DOCUMENTATION",
            "EXTENT-UNKNOWN?", "HIDE-SLOT?", "INSTANCE-NAME-TEMPLATE", "LEFT-END-POSITION",
            "MEMBER-SORT-FN", "REGULATED-BY", "RIGHT-END-POSITION", "SYNONYMS", "TEMPLATE-FILE"}

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In TransUnits.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        'Public Shared Shadows Widening Operator CType(e As LANS.SystemsBiology.Assembly.MetaCyc.File.AttributeValue) As TransUnits
        '    Dim TransUnits As TransUnits = New TransUnits
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.TransUnit) =
        '        From c As MetaCyc.File.AttributeValue.Object
        '        In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.TransUnit)

        '    TransUnits.DbProperty = e.DbProperty
        '    TransUnits.FrameObjects = Query.ToList

        '    Return TransUnits
        'End Operator

        'Public Shared Shadows Widening Operator CType(spath As String) As TransUnits
        '    Dim File As MetaCyc.File.AttributeValue = spath
        '    Return CType(File, TransUnits)
        'End Operator
    End Class
End Namespace