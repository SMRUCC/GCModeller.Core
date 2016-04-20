Namespace SequenceModel

    Public Interface IPatternProvider
        Function PWM() As IEnumerable(Of IPatternSite)
    End Interface

    Public Interface IPatternSite
        Default ReadOnly Property Probability(c As Char) As Double
    End Interface
End Namespace