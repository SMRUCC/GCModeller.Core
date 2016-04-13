Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Reflection

Namespace Assembly.MetaCyc.File.DataFiles

    ''' <summary>
    ''' 该细胞系统中的所有的小分子化合物的集合，本集合取决于代谢网络的结构以及控制物质跨膜运输的蛋白质
    ''' </summary>
    ''' <remarks>
    ''' 对于Compounds表而言，由于其包含的对象仅为小分子的代谢物，故而大分子的蛋白质分子不会出现在此列表之中
    ''' </remarks>
    Public Class Compounds : Inherits DataFile(Of MetaCyc.File.DataFiles.Slots.Compound)

        Public Shared Shadows ReadOnly AttributeList As String() = {
            "UNIQUE-ID", "TYPES", "COMMON-NAME", "ABBREV-NAME", "ANTICODON",
            "ATOM-CHARGES", "CATALYZES", "CFG-ICON-COLOR", "CHEMICAL-FORMULA",
            "CITATIONS", "CODONS", "COFACTORS-OF", "COFACTORS-OR-PROSTHETIC-GROUPS-OF",
            "COMMENT", "COMMENT-INTERNAL", "COMPONENT-COEFFICIENTS",
            "COMPONENT-OF", "COMPONENTS", "CONSENSUS-SEQUENCE", "CREDITS",
            "DATA-SOURCE", "DBLINKS", "DNA-FOOTPRINT-SIZE", "DOCUMENTATION",
            "ENZYME-NOT-USED-IN", "GENE", "GO-TERMS", "GROUP-COORDS-2D",
            "GROUP-INTERNALS", "HAS-NO-STRUCTURE?", "HIDE-SLOT?",
            "IN-MIXTURE", "INCHI", "INTERNALS-OF-GROUP", "ISOZYME-SEQUENCE-SIMILARITY",
            "LEFT-END-POSITION", "LOCATIONS", "MEMBER-SORT-FN", "MODIFIED-FORM",
            "MOLECULAR-WEIGHT", "MOLECULAR-WEIGHT-EXP", "MOLECULAR-WEIGHT-KD",
            "MOLECULAR-WEIGHT-SEQ", "MONOISOTOPIC-MW", "N+1-NAME", "N-1-NAME",
            "N-NAME", "NEIDHARDT-SPOT-NUMBER", "NON-STANDARD-INCHI", "PI",
            "PKA1", "PKA2", "PKA3", "PROSTHETIC-GROUPS-OF", "RADICAL-ATOMS",
            "REGULATED-BY", "REGULATES", "RIGHT-END-POSITION", "SMILES", "SPECIES",
            "SPLICE-FORM-INTRONS", "STRUCTURE-GROUPS", "STRUCTURE-LINKS", "SUPERATOMS",
            "SYMMETRY", "SYNONYMS", "SYSTEMATIC-NAME", "TAUTOMERS", "TEMPLATE-FILE",
            "UNMODIFIED-FORM"}

        ''' <summary>
        ''' Get an object instance in the compounds table using its common name or synonymous name.
        ''' </summary>
        ''' <param name="CommonName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetObject(CommonName As String) As Slots.Compound
            Dim LQuery = (From compound As Slots.Compound In MyBase.FrameObjects.AsParallel
                          Let condition = Function() As Boolean
                                              If String.Equals(CommonName, compound.CommonName, StringComparison.OrdinalIgnoreCase) Then
                                                  Return True
                                              End If
                                              If String.Equals(CommonName, compound.AbbrevName, StringComparison.OrdinalIgnoreCase) Then
                                                  Return True
                                              End If
                                              Dim NameQuery = (From strName As String In compound.Synonyms
                                                               Where String.Equals(strName, CommonName, StringComparison.OrdinalIgnoreCase)
                                                               Select 1).ToArray.Sum
                                              Return NameQuery > 0
                                          End Function _
                        Where True = condition()
                        Select compound).ToArray

            If LQuery.IsNullOrEmpty Then
                Return Nothing
            Else
                Return LQuery.First
            End If
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In Compounds.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        'Public Shared Shadows Widening Operator CType(e As LANS.SystemsBiology.Assembly.MetaCyc.File.AttributeValue) As Compounds
        '    Dim NewFile As New Compounds
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.Compound) =
        '        From c As MetaCyc.File.AttributeValue.Object
        '        In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.Compound)

        '    NewFile.DbProperty = e.DbProperty
        '    NewFile.FrameObjects = Query.ToList

        '    Return NewFile
        'End Operator

        'Public Shared Shadows Widening Operator CType(spath As String) As Compounds
        '    Dim File As MetaCyc.File.AttributeValue = spath
        '    Return CType(File, Compounds)
        'End Operator
    End Class
End Namespace
