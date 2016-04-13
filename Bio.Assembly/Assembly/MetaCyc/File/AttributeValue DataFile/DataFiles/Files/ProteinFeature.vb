Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Reflection

Namespace Assembly.MetaCyc.File.DataFiles

    ''' <summary>
    ''' Protein features (for example, active sites), This file lists all the protein 
    ''' features (such as active sites) in the PGDB. /* protein-features.dat */
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ProteinFeatures : Inherits DataFile(Of MetaCyc.File.DataFiles.Slots.ProteinFeature)

        Public Shared Shadows ReadOnly AttributeList As String() = {
            "UNIQUE-ID", "TYPES", "COMMON-NAME", "ALTERNATE-SEQUENCE",
            "ATTACHED-GROUP", "CATALYTIC-ACTIVITY", "CITATIONS",
            "COMMENT", "COMMENT-INTERNAL", "COMPONENT-OF", "CREDITS",
            "DATA-SOURCE", "DBLINKS", "DOCUMENTATION", "FEATURE-OF",
            "HIDE-SLOT?", "HOMOLOGY-MOTIF", "INSTANCE-NAME-TEMPLATE",
            "LEFT-END-POSITION", "LINKAGE-TYPE", "MEMBER-SORT-FN",
            "POSSIBLE-FEATURE-STATES", "RESIDUE-NUMBER", "RESIDUE-TYPE",
            "RIGHT-END-POSITION", "SYNONYMS", "TEMPLATE-FILE"}

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In ProteinFeatures.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        'Public Shared Shadows Widening Operator CType(e As LANS.SystemsBiology.Assembly.MetaCyc.File.AttributeValue) As ProteinFeatures
        '    Dim NewFile As New ProteinFeatures
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.ProteinFeature) =
        '        From c As MetaCyc.File.AttributeValue.Object
        '        In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.ProteinFeature)  'LINQ query define

        '    NewFile.DbProperty = e.DbProperty
        '    NewFile.FrameObjects = Query.ToList

        '    Return NewFile
        'End Operator

        'Public Shared Shadows Widening Operator CType(spath As String) As ProteinFeatures
        '    Dim File As MetaCyc.File.AttributeValue = spath
        '    Return CType(File, ProteinFeatures)
        'End Operator
    End Class
End Namespace