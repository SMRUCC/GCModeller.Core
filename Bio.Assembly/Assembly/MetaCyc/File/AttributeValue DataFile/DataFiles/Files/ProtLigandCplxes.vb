Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Reflection

Namespace Assembly.MetaCyc.File.DataFiles

    ''' <summary>
    ''' The file lists all the complexes of proteins with small-molecule ligands in the PGDB.
    ''' (在本文件中列出了本菌种内的所有与小分子配基所形成的蛋白质复合物)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ProtLigandCplxes : Inherits DataFile(Of MetaCyc.File.DataFiles.Slots.ProtLigandCplxe)

        Public Shared Shadows ReadOnly AttributeList As String() = {
 _
            "UNIQUE-ID", "TYPES", "COMMON-NAME", "ABBREV-NAME", "AROMATIC-RINGS", "ATOM-CHARGES", "CATALYZES", "CITATIONS", "COFACTORS-OF",
            "COFACTORS-OR-PROSTHETIC-GROUPS-OF", "COMMENT", "COMMENT-INTERNAL", "COMPONENT-COEFFICIENTS", "COMPONENT-OF", "COMPONENTS",
            "CONSENSUS-SEQUENCE", "CREDITS", "DATA-SOURCE", "DBLINKS", "DNA-FOOTPRINT-SIZE", "DOCUMENTATION", "ENZYME-NOT-USED-IN", "GO-TERMS",
            "HAS-NO-STRUCTURE?", "HIDE-SLOT?", "IN-MIXTURE", "ISOZYME-SEQUENCE-SIMILARITY", "LOCATIONS", "MEMBER-SORT-FN", "MODIFIED-FORM",
            "MOLECULAR-WEIGHT", "MOLECULAR-WEIGHT-EXP", "MOLECULAR-WEIGHT-KD", "MOLECULAR-WEIGHT-SEQ", "N+1-NAME", "N-1-NAME", "N-NAME",
            "NEIDHARDT-SPOT-NUMBER", "PI", "PROSTHETIC-GROUPS-OF", "RADICAL-ATOMS", "REGULATED-BY", "REGULATES", "SPECIES", "STRUCTURE-BONDS",
            "SUPERATOMS", "SYMMETRY", "SYNONYMS", "TEMPLATE-FILE"}

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In ProtLigandCplxes.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        'Public Shared Shadows Widening Operator CType(e As LANS.SystemsBiology.Assembly.MetaCyc.File.AttributeValue) As ProtLigandCplxes
        '    Dim NewFile As New ProtLigandCplxes
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.ProtLigandCplxe) =
        '        From c As MetaCyc.File.AttributeValue.Object In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.ProtLigandCplxe)

        '    NewFile.DbProperty = e.DbProperty
        '    NewFile.FrameObjects = Query.ToList

        '    Return NewFile
        'End Operator

        'Public Shared Shadows Widening Operator CType(Path As String) As ProtLigandCplxes
        '    Dim ProtLigandCplxes As ProtLigandCplxes = New ProtLigandCplxes
        '    Call MetaCyc.File.DataFiles.Reflection.FileStream.Read(Of  _
        '        Slots.ProtLigandCplxe, ProtLigandCplxes)(Path, ProtLigandCplxes)
        '    Return ProtLigandCplxes
        'End Operator
    End Class
End Namespace