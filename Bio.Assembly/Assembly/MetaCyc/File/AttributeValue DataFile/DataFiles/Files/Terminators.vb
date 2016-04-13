Namespace Assembly.MetaCyc.File.DataFiles

    Public Class Terminators : Inherits DataFile(Of MetaCyc.File.DataFiles.Slots.Terminator)

        Public Shared Shadows ReadOnly AttributeList As String() = {
            "UNIQUE-ID", "TYPES", "COMMON-NAME", "CITATIONS", "COMMENT", "COMMENT-INTERNAL",
            "COMPONENT-OF", "CREDITS", "DATA-SOURCE", "DBLINKS", "DOCUMENTATION", "HIDE-SLOT?",
            "INSTANCE-NAME-TEMPLATE", "LEFT-END-POSITION", "MEMBER-SORT-FN",
            "RIGHT-END-POSITION", "SYNONYMS", "TEMPLATE-FILE"}

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In Terminators.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        'Public Shared Shadows Widening Operator CType(e As LANS.SystemsBiology.Assembly.MetaCyc.File.AttributeValue) As Terminators
        '    Dim NewFile As New Terminators
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.Terminator) =
        '        From c As AttributeValue.Object In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.Terminator)

        '    NewFile.DbProperty = e.DbProperty
        '    NewFile.FrameObjects = Query.ToList

        '    Return NewFile
        'End Operator

        'Public Shared Shadows Widening Operator CType(spath As String) As Terminators
        '    Dim File As MetaCyc.File.AttributeValue = spath
        '    Return CType(File, Terminators)
        'End Operator
    End Class
End Namespace