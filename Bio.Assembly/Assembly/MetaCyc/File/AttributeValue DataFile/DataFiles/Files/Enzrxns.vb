Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Reflection

Namespace Assembly.MetaCyc.File.DataFiles

    ''' <summary>
    ''' Frames in the class Enzymatic-Reactions describe attributes of an enzyme with respect 
    ''' to a particular reaction. For reactions that are catalyzed by more than one enzyme, 
    ''' or for enzymes that catalyze more than one reaction, multiple Enzymatic-Reactions 
    ''' frames are created, one for each enzyme/reaction pair. For example, Enzymatic-Reactions 
    ''' frames can represent the fact that two enzymes that catalyze the same reaction may be 
    ''' controlled by different activators and inhibitors.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Enzrxns : Inherits DataFile(Of MetaCyc.File.DataFiles.Slots.Enzrxn)

        Public Shared Shadows ReadOnly AttributeList As String() =
            {
                "UNIQUE-ID", "TYPES", "COMMON-NAME", "ALTERNATIVE-COFACTORS", "ALTERNATIVE-SUBSTRATES",
                "BASIS-FOR-ASSIGNMENT", "CITATIONS", "COFACTOR-BINDING-COMMENT", "COFACTORS",
                "COFACTORS-OR-PROSTHETIC-GROUPS", "COMMENT", "COMMENT-INTERNAL", "CREDITS", "DATA-SOURCE",
                "DBLINKS", "DOCUMENTATION", "ENZYME", "HIDE-SLOT?", "INSTANCE-NAME-TEMPLATE", "KCAT",
                "KM", "MEMBER-SORT-FN", "PH-OPT", "PHYSIOLOGICALLY-RELEVANT?", "PROSTHETIC-GROUPS",
                "REACTION", "REACTION-DIRECTION", "REGULATED-BY", "REQUIRED-PROTEIN-COMPLEX",
                "SPECIFIC-ACTIVITY", "SYNONYMS", "TEMPERATURE-OPT", "TEMPLATE-FILE", "VMAX"}

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In Enzrxns.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        'Public Shared Shadows Widening Operator CType(e As LANS.SystemsBiology.Assembly.MetaCyc.File.AttributeValue) As Enzrxns
        '    Dim NewFile As New Enzrxns
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.Enzrxn) =
        '        From c As MetaCyc.File.AttributeValue.Object
        '        In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.Enzrxn)

        '    NewFile.DbProperty = e.DbProperty
        '    NewFile.FrameObjects = Query.ToList

        '    Return NewFile
        'End Operator

        'Public Shared Shadows Widening Operator CType(Path As String) As Enzrxns
        '    Dim Enzrxns As Enzrxns = New Enzrxns
        '    Call MetaCyc.File.DataFiles.Reflection.FileStream.Read(Of Slots.Enzrxn, Enzrxns)(Path, Enzrxns)
        '    Return Enzrxns
        'End Operator
    End Class
End Namespace