Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Reflection

Namespace Assembly.MetaCyc.File.DataFiles

    ''' <summary>
    ''' Frames in class Pathways encode metabolic and signaling pathways.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Pathways : Inherits DataFile(Of Slots.Pathway)

        Public Shared Shadows ReadOnly AttributeList As String() = {
            "UNIQUE-ID", "TYPES", "COMMON-NAME", "CITATIONS", "CLASS-INSTANCE-LINKS",
            "COMMENT", "CREDITS", "DBLINKS", "ENZYME-USE", "HYPOTHETICAL-REACTIONS",
            "IN-PATHWAY", "NET-REACTION-EQUATION", "PATHWAY-INTERACTIONS", "PATHWAY-LINKS",
            "POLYMERIZATION-LINKS", "PREDECESSORS", "PRIMARIES", "REACTION-LAYOUT",
            "REACTION-LIST", "SPECIES", "SUB-PATHWAYS", "SUPER-PATHWAYS", "SYNONYMS"}

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In Pathways.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        'Public Shared Shadows Widening Operator CType(e As MetaCyc.File.AttributeValue) As Pathways
        '    Dim NewFile As New Pathways
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.Pathway) =
        '        From c As MetaCyc.File.AttributeValue.Object
        '        In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.Pathway)

        '    NewFile.DbProperty = e.DbProperty
        '    NewFile.FrameObjects = Query.ToList

        '    Return NewFile
        'End Operator

        'Public Shared Shadows Widening Operator CType(Path As String) As Pathways
        '    Dim Pathways As Pathways = New Pathways
        '    Call MetaCyc.File.DataFiles.Reflection.FileStream.Read(Of  _
        '        Slots.Pathway, Pathways)(File:=Path, Stream:=Pathways)
        '    Return Pathways
        'End Operator
    End Class
End Namespace