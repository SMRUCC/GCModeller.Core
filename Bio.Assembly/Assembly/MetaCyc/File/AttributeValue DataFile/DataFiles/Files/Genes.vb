Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Reflection
Imports Microsoft.VisualBasic

Namespace Assembly.MetaCyc.File.DataFiles
    ''' <summary>
    ''' Each frame in the class Genes describes a single gene, meaning a region of DNA that defines a 
    ''' coding region for one or more gene products. Multiple gene products may be produced because 
    ''' of modification of an RNA or protein.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Genes : Inherits DataFile(Of MetaCyc.File.DataFiles.Slots.Gene)

        Public Shared Shadows ReadOnly AttributeList As String() = {
 _
            "UNIQUE-ID", "TYPES", "COMMON-NAME", "ACCESSION-1", "ACCESSION-2", "CENTISOME-POSITION", "CITATIONS", "COMMENT", "COMMENT-INTERNAL",
            "COMPONENT-OF", "CREDITS", "DATA-SOURCE", "DBLINKS", "DOCUMENTATION", "HIDE-SLOT?", "IN-PARALOGOUS-GENE-GROUP", "INTERRUPTED?",
            "KNOCKOUT-GROWTH-OBSERVATIONS", "LAST-UPDATE", "LEFT-END-POSITION", "MEMBER-SORT-FN", "PRODUCT", "REGULATED-BY", "RIGHT-END-POSITION",
            "SYNC-W-ORTHOLOG", "SYNONYMS", "TEMPLATE-FILE", "TRANSCRIPTION-DIRECTION"}

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In Genes.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        ''' <summary>
        ''' 获取所有基因对象的UniqueId所组成的集合
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetAllGeneIds() As String()
            Dim Query = From Gene In Me.AsParallel Select Gene.Identifier '
            Return Query.ToArray
        End Function

        ''' <summary>
        ''' 尝试着取出所有的基因对象的启动子序列，结果不是很准确，请慎用！！！
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function TryParsePromoters(Genome As SequenceModel.FASTA.FastaToken) As SequenceModel.FASTA.FastaFile
            Dim pList As List(Of SequenceModel.FASTA.FastaToken) = New List(Of SequenceModel.FASTA.FastaToken)
            For Each Gene In Me
                Dim Seq As SequenceModel.FASTA.FastaToken = New SequenceModel.FASTA.FastaToken With {.Attributes = New String() {Gene.Identifier, Gene.Identifier & "_PM", "Di: " & Gene.TranscriptionDirection, Gene.CommonName}}
                If String.Equals(Gene.TranscriptionDirection, "+") Then
                    Seq.SequenceData = Mid(Genome.SequenceData, Val(Gene.LeftEndPosition) - 300, 300)
                ElseIf String.Equals(Gene.TranscriptionDirection, "-") Then
                    Seq.SequenceData = Mid(Genome.SequenceData, Val(Gene.RightEndPosition), 300)
                    Call SequenceModel.FASTA.FastaToken.Complement(Seq)
                Else
#If DEBUG Then
                    Console.WriteLine("Unknown direaction: {0}", Gene.Identifier)
#End If
                End If
            Next

            Return pList
        End Function

        'Public Shared Shadows Widening Operator CType(e As LANS.SystemsBiology.Assembly.MetaCyc.File.AttributeValue) As Genes
        '    Dim NewFile As New Genes
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.Gene) =
        '        From c As MetaCyc.File.AttributeValue.Object
        '        In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.Gene)

        '    NewFile.DbProperty = e.DbProperty
        '    NewFile.FrameObjects = Query.ToList

        '    Return NewFile
        'End Operator

        'Public Shared Shadows Widening Operator CType(Path As String) As Genes
        '    Dim File As MetaCyc.File.AttributeValue = Path
        '    Return CType(File, Genes)
        'End Operator
    End Class
End Namespace