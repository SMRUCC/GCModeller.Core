Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels
Imports LANS.SystemsBiology.ComponentModel.Loci

Namespace SequenceModel.NucleotideModels

    ''' <summary>
    ''' 没有更多的复杂的继承或者接口实现，只是最简单序列片段对象
    ''' </summary>
    Public Class SimpleSegment : Inherits Contig
        Implements I_PolymerSequenceModel

        Public Property ID As String
        Public Property Strand As String
        Public Property Start As Integer
        Public Property Ends As Integer
        Public Property SequenceData As String Implements I_PolymerSequenceModel.SequenceData
        Public Property Complement As String

        Sub New()

        End Sub

        Sub New(loci As SimpleSegment)
            ID = loci.ID
            Strand = loci.Strand
            Start = loci.Start
            Ends = loci.Ends
            SequenceData = loci.SequenceData
            Complement = loci.Complement
        End Sub

        Sub New(loci As SimpleSegment, sId As String)
            Call Me.New(loci)
            ID = sId
        End Sub

        Protected Overrides Function __getMappingLoci() As NucleotideLocation
            Return New NucleotideLocation(Start, Ends, Strand).Normalization
        End Function

        Public Function ToPTTGene() As GeneBrief
            Return New GeneBrief With {
                .Code = "-",
                .COG = "-",
                .Gene = ID,
                .IsORF = True,
                .Length = MappingLocation.FragmentSize,
                .Location = MappingLocation,
                .PID = "-",
                .Product = "-",
                .Synonym = ID
            }
        End Function
    End Class
End Namespace