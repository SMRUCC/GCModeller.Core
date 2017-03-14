Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace Assembly.EBI.ChEBI.Database.IO.StreamProviders.Tsv

    Public Class Compound

        Public Property ID As String
        Public Property STATUS As String
        Public Property CHEBI_ACCESSION As String
        Public Property SOURCE As String
        Public Property PARENT_ID As String
        Public Property NAME As String
        Public Property DEFINITION As String
        Public Property MODIFIED_ON As String
        Public Property CREATED_BY As String
        Public Property STAR As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Function Load(path$) As Compound()
            Dim index As IndexOf(Of String) = path.TsvHeaders
            Dim out As New List(Of Compound)

            For Each line As String In path.IterateAllLines.Skip(1)
                Dim t$() = line.Split(ASCII.TAB)
                out += New Compound With {
                    .CHEBI_ACCESSION = t(index(NameOf(.CHEBI_ACCESSION))),
                    .CREATED_BY = t(index(NameOf(.CREATED_BY))),
                    .DEFINITION = t(index(NameOf(.DEFINITION))),
                    .ID = t(index(NameOf(.ID))),
                    .MODIFIED_ON = t(index(NameOf(.MODIFIED_ON))),
                    .NAME = t(index(NameOf(.NAME))),
                    .PARENT_ID = t(index(NameOf(.PARENT_ID))),
                    .SOURCE = t(index(NameOf(.SOURCE))),
                    .STAR = t(index(NameOf(.STAR))),
                    .STATUS = t(index(NameOf(.STATUS)))
                }
            Next

            Return out
        End Function
    End Class
End Namespace