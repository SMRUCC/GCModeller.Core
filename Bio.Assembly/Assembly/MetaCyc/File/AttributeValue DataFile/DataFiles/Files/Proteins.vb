﻿Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Reflection
Imports Microsoft.VisualBasic

Namespace Assembly.MetaCyc.File.DataFiles

    ''' <summary>
    ''' 本文件中列举出了所有的蛋白质复合物(CPLX)以及单体蛋白(MONOMER)
    ''' </summary>
    ''' <remarks>
    ''' 对于某一种蛋白质而言，其以单体形式存在的时候，没有催化能力，但是在形成了蛋白质复合物之后，具备了催化能力
    ''' </remarks>
    Public Class Proteins : Inherits DataFile(Of Slots.Protein)

        Public Overrides ReadOnly Property AttributeList As String()
            Get
                Return {
                    "UNIQUE-ID", "TYPES", "COMMON-NAME", "ABBREV-NAME", "AROMATIC-RINGS", "ATOM-CHARGES",
                    "CATALYZES", "CHEMICAL-FORMULA", "CITATIONS", "COFACTORS-OF",
                    "COFACTORS-OR-PROSTHETIC-GROUPS-OF", "COMMENT", "COMMENT-INTERNAL", "COMPONENT-COEFFICIENTS",
                    "COMPONENT-OF", "COMPONENTS", "CONSENSUS-SEQUENCE", "CREDITS", "DATA-SOURCE", "DBLINKS",
                    "DNA-FOOTPRINT-SIZE", "DOCUMENTATION", "ENZYME-NOT-USED-IN", "FEATURES",
                    "FUNCTIONAL-ASSIGNMENT-COMMENT", "FUNCTIONAL-ASSIGNMENT-STATUS", "GENE", "GO-TERMS",
                    "HAS-NO-STRUCTURE?", "HIDE-SLOT?", "IN-MIXTURE", "INCHI", "INSTANCE-NAME-TEMPLATE",
                    "INTERNALS-OF-GROUP", "ISOZYME-SEQUENCE-SIMILARITY", "LOCATIONS", "MEMBER-SORT-FN",
                    "MODIFIED-FORM", "MOLECULAR-WEIGHT", "MOLECULAR-WEIGHT-EXP", "MOLECULAR-WEIGHT-KD",
                    "MOLECULAR-WEIGHT-SEQ", "MONOISOTOPIC-MW", "N+1-NAME", "N-1-NAME", "N-NAME",
                    "NEIDHARDT-SPOT-NUMBER", "NON-STANDARD-INCHI", "PI", "PKA1", "PKA2", "PKA3", "PROMOTER-BOX-NAME-1",
                    "PROMOTER-BOX-NAME-2", "PROSTHETIC-GROUPS-OF", "RADICAL-ATOMS", "RECOGNIZED-PROMOTERS",
                    "REGULATED-BY", "REGULATES", "SMILES", "SPECIES", "SPLICE-FORM-INTRONS", "STRUCTURE-BONDS",
                    "SUPERATOMS", "SYMMETRY", "SYNONYMS", "SYSTEMATIC-NAME", "TAUTOMERS", "TEMPLATE-FILE",
                    "UNMODIFIED-FORM"
                }
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        ''' <summary>
        ''' 递归的使用一个组分对象的UniqueId属性值查询出包含其所有的蛋白质复合物
        ''' </summary>
        ''' <param name="Component">Component列表中的一个元素</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetProteinComplexByComponent(Component As String) As Slots.Protein()
            Dim LQuery = (From Protein As Slots.Protein In Me.AsParallel Where Protein.Components.IndexOf(Component) Select Protein).ToArray  '先查找出包含有当前Component对象的Protein列表
            Dim List As List(Of Slots.Protein) = LQuery.ToList '生成返回数据的集合
            For Each Protein In LQuery
                Call List.AddRange(GetProteinComplexByComponent(Protein.Identifier)) '以查找出来的数据进行递归查找
            Next
            Return List.ToArray
        End Function

        Public Shared Shadows Widening Operator CType(Path As String) As Proteins
            Dim Proteins As Proteins = New Proteins
            Call MetaCyc.File.DataFiles.Reflection.FileStream.Read(Of Slots.Protein, Proteins)(Path, Proteins)
            Return Proteins
        End Operator
    End Class
End Namespace