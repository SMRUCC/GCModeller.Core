Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.genomics.Metagenomics

Namespace Assembly.KEGG.DBGET.BriteHEntry

    ''' <summary>
    ''' br08601
    ''' </summary>
    Public Module Organism

        <Extension> Public Function Fill(organisms As htext) As Taxonomy()
            Dim out As New List(Of Taxonomy)
            Dim A$ = ""
            Dim B$ = ""
            Dim C$ = ""
            Dim D$ = ""
            Dim E$ = ""

            For Each htext In organisms.Hierarchical.EnumerateEntries

            Next

            Return out
        End Function
    End Module
End Namespace