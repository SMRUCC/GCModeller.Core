Imports LANS.SystemsBiology.ComponentModel.Loci

Namespace Assembly.Bac_sRNA.org

    Public Class Database

        Public Property Interactions As Interaction()
        Public Property Sequences As Sequence()

        Dim DataDir As String

        Sub New(DataDir As String)
            Me.Interactions = ImportsInteraction(DataDir & "/interaction.txt")
            Me.Sequences = (From Fsa As SequenceModel.FASTA.FastaToken
                            In SequenceModel.FASTA.FastaFile.Read(DataDir & "/seq.txt")._innerList
                            Select Sequence.CType(Fsa)).ToArray
            Me.DataDir = DataDir
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("http://bac-srna.org; file:///{0}", DataDir)
        End Function

        Public Shared Function ImportsInteraction(FilePath As String) As Interaction()
            Dim File As String() = IO.File.ReadAllLines(FilePath)
            Dim LQuery = From Line As String In File.Skip(1).ToArray.AsParallel
                         Let Tokens As String() = Strings.Split(Line, "")
                         Let Interaction = New Bac_sRNA.org.Interaction With {
                             .sRNAid = Tokens(0),
                             .Organism = Tokens(1),
                             .Name = Tokens(2),
                             .Regulation = Tokens(3),
                             .TargetName = Tokens(4),
                             .Reference = Tokens(5)
                         }
                         Select Interaction
                         Order By Interaction.sRNAid '
            Return LQuery.ToArray
        End Function
    End Class
End Namespace