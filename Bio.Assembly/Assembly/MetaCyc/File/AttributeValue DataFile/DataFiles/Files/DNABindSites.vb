Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Reflection

Namespace Assembly.MetaCyc.File.DataFiles

    ''' <summary>
    ''' This class describes DNA regions that are binding sites for transcription factors.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DNABindSites : Inherits DataFile(Of MetaCyc.File.DataFiles.Slots.DNABindSite)

        Public Shared Shadows ReadOnly AttributeList As String() = {
 _
            "UNIQUE-ID", "TYPES", "COMMON-NAME", "ABS-CENTER-POS", "CITATIONS",
            "COMMENT", "COMMENT-INTERNAL", "COMPONENT-OF", "CREDITS", "DATA-SOURCE",
            "DBLINKS", "DOCUMENTATION", "HIDE-SLOT?", "INSTANCE-NAME-TEMPLATE",
            "INVOLVED-IN-REGULATION", "LEFT-END-POSITION", "MEMBER-SORT-FN",
            "RIGHT-END-POSITION", "SITE-LENGTH", "SYNONYMS", "TEMPLATE-FILE"}

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In DNABindSites.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        'Public Shared Shadows Widening Operator CType(e As LANS.SystemsBiology.Assembly.MetaCyc.File.AttributeValue) As DNABindSites
        '    Dim NewFile As New DNABindSites
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.DNABindSite) =
        '        From c As MetaCyc.File.DataFiles.Slots.DNABindSite In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.DNABindSite)

        '    NewFile.DbProperty = e.DbProperty
        '    NewFile.FrameObjects = Query.ToList

        '    Return NewFile
        'End Operator

        'Public Shared Shadows Widening Operator CType(spath As String) As DNABindSites
        '    Dim File As MetaCyc.File.AttributeValue = spath
        '    Return CType(File, DNABindSites)
        'End Operator
    End Class
End Namespace