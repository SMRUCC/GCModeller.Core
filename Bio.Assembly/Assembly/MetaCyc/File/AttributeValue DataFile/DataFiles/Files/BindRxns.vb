Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Reflection

Namespace Assembly.MetaCyc.File.DataFiles

    Public Class BindRxns : Inherits DataFile(Of MetaCyc.File.DataFiles.Slots.BindReaction)

        Public Shared Shadows ReadOnly AttributeList As String() = {
            "UNIQUE-ID", "TYPES", "COMMON-NAME", "ACTIVATORS",
            "BALANCE-STATE", "BASAL-TRANSCRIPTION-VALUE",
            "CITATIONS", "COMMENT", "COMPONENT-OF", "COMPONENTS",
            "CREDITS", "DBLINKS", "DELTAG0", "DEPRESSORS",
            "EC-NUMBER", "ENZYMATIC-REACTION",
            "EQUILIBRIUM-CONSTANT", "IN-PATHWAY", "INHIBITORS",
            "INSTANCE-NAME-TEMPLATE", "LEFT", "OFFICIAL-EC?",
            "ORPHAN?", "REACTANTS", "REACTION-PRESENT-IN-E-COLI?",
            "REQUIREMENTS", "RIGHT", "SIGNAL", "SPECIES",
            "SPONTANEOUS?", "STIMULATORS", "SYNONYMS"}

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In BindRxns.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        'Public Shared Shadows Widening Operator CType(e As LANS.SystemsBiology.Assembly.MetaCyc.File.AttributeValue) As BindRxns
        '    Dim NewFile As New BindRxns
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.BindReaction) =
        '        From c As MetaCyc.File.AttributeValue.Object
        '        In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.BindReaction) '.AsParallel 

        '    NewFile.DbProperty = e.DbProperty
        '    NewFile.FrameObjects = Query.ToList

        '    Return NewFile
        'End Operator

        'Public Shared Shadows Widening Operator CType(Path As String) As BindRxns
        '    Try
        '        Dim File As MetaCyc.File.AttributeValue = Path
        '        Return CType(File, BindRxns)
        '    Catch ex As Exception
        '        Return New BindRxns
        '    End Try
        'End Operator
    End Class
End Namespace