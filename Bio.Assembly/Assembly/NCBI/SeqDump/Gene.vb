Imports System.Text.RegularExpressions
Imports LANS.SystemsBiology.ComponentModel.Loci

Namespace Assembly.NCBI.SequenceDump

    ''' <summary>
    ''' NCBI genbank title format fasta parser
    ''' </summary>
    Public Class Gene : Inherits SequenceModel.FASTA.FastaToken

#Region "ReadOnly properties"

        Public ReadOnly Property CommonName As String
        Public ReadOnly Property LocusTag As String
        Public ReadOnly Property Location As LANS.SystemsBiology.ComponentModel.Loci.NucleotideLocation
#End Region

        Sub New(FastaObj As LANS.SystemsBiology.SequenceModel.FASTA.FastaToken)
            Dim strTitle As String = FastaObj.Title
            Dim LocusTag As String = Regex.Match(strTitle, "locus_tag=[^]]+").Value
            Dim Location As String = Regex.Match(strTitle, "location=[^]]+").Value
            Dim CommonName As String = Regex.Match(strTitle, "gene=[^]]+").Value

            Me._LocusTag = LocusTag.Split(CChar("=")).Last
            Me._CommonName = CommonName.Split(CChar("=")).Last
            Me._Location = LociAPI.TryParse(Location)
            Me.Attributes = FastaObj.Attributes
            Me.SequenceData = FastaObj.SequenceData
        End Sub

        Public Overloads Shared Function Load(FastaFile As String) As NCBI.SequenceDump.Gene()
            Dim FASTA As SequenceModel.FASTA.FastaFile = SequenceModel.FASTA.FastaFile.Read(FastaFile)
            Dim LQuery = (From FastaObj As SequenceModel.FASTA.FastaToken
                          In FASTA
                          Select New NCBI.SequenceDump.Gene(FastaObj)).ToArray
            Return LQuery
        End Function
    End Class
End Namespace