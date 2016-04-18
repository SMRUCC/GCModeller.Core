Imports LANS.SystemsBiology.Assembly.MetaCyc.File.DataFiles
Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Metabolism

Namespace Assembly.MetaCyc

    Public Module Extensions

        Public ReadOnly Property Directions As IReadOnlyDictionary(Of String, ReactionDirections) =
            New Dictionary(Of String, ReactionDirections) From {
 _
            {"LEFT-TO-RIGHT", ReactionDirections.LeftToRight},
            {"REVERSIBLE", ReactionDirections.Reversible},
            {"RIGHT-TO-LEFT", ReactionDirections.RightToLeft}
        }

        Public Function GetAttributeList(Of T As Slots.Object)(data As DataFile(Of T)) As String()
            Return (From s As String
                    In data.AttributeList
                    Where Not s.IsBlank
                    Select s
                    Distinct
                    Order By s.Length Descending).ToArray
        End Function
    End Module
End Namespace