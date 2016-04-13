Imports LANS.SystemsBiology.ComponentModel.Loci
Imports LANS.SystemsBiology.SequenceModel.FASTA

Namespace Assembly.Bac_sRNA.org

    Public Class Sequence : Inherits FastaToken

        Public ReadOnly Property UniqueId As String
            Get
                Return MyBase.Attributes(0)
            End Get
        End Property

        Public ReadOnly Property Specie As String
            Get
                Return MyBase.Attributes(1)
            End Get
        End Property

        Public ReadOnly Property Name As String
            Get
                Return MyBase.Attributes(2)
            End Get
        End Property

        Dim _Location As NucleotideLocation

        Public ReadOnly Property Location As NucleotideLocation
            Get
                If _Location Is Nothing Then
                    _Location = New NucleotideLocation With {
                        .Left = MyBase.Attributes(3),
                        .Right = MyBase.Attributes(4),
                        .Strand = LociAPI.GetStrand(MyBase.Attributes(5))
                    }
                End If
                Return _Location
            End Get
        End Property

        Public Shared Function [CType](fa As FastaToken) As Sequence
            Return New Sequence With {
                .Attributes = fa.Attributes,
                .SequenceData = fa.SequenceData
            }
        End Function
    End Class
End Namespace