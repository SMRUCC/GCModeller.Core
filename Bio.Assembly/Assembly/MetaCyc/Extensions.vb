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
    End Module
End Namespace