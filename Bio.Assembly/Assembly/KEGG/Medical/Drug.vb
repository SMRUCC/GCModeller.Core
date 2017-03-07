Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.genomics.ComponentModel.DBLinkBuilder

Namespace Assembly.KEGG

    Public Class Drug

        Public Property Entry As String
        Public Property Names As String()
        Public Property Formula As String
        Public Property Exact_Mass As Double
        Public Property Mol_Weight As Double
        Public Property Remarks As String()
        Public Property Activity As String
        Public Property DBLinks As DBLink()

    End Class
End Namespace