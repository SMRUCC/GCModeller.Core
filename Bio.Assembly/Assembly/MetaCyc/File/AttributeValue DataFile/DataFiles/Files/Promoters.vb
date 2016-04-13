Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Reflection
Imports LANS.SystemsBiology.SequenceModel
Imports LANS.SystemsBiology.SequenceModel.FASTA.Reflection
Imports Microsoft.VisualBasic

Namespace Assembly.MetaCyc.File.DataFiles

    ''' <summary>
    ''' Frames in this class define transcription start sites.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Promoters : Inherits DataFile(Of MetaCyc.File.DataFiles.Slots.Promoter)

        Public Shared Shadows ReadOnly AttributeList As String() = {
 _
            "UNIQUE-ID", "TYPES", "COMMON-NAME", "ABSOLUTE-PLUS-1-POS",
            "BINDS-SIGMA-FACTOR", "CITATIONS", "COMMENT", "COMMENT-INTERNAL",
            "COMPONENT-OF", "CREDITS", "DATA-SOURCE", "DBLINKS",
            "DOCUMENTATION", "HIDE-SLOT?", "INSTANCE-NAME-TEMPLATE",
            "LEFT-END-POSITION", "MEMBER-SORT-FN", "MINUS-10-LEFT",
            "MINUS-10-RIGHT", "MINUS-35-LEFT", "MINUS-35-RIGHT",
            "RIGHT-END-POSITION", "SYNONYMS", "TEMPLATE-FILE"}

        Public Overrides Function ToString() As String
            Return String.Format("{0}  {1} frame object records.", DbProperty.ToString, FrameObjects.Count)
        End Function

        Friend Overrides Function GetAttributeList() As String()
            Return (From s As String In Promoters.AttributeList Select s Order By Len(s) Descending).ToArray
        End Function

        'Public Shared Shadows Widening Operator CType(e As LANS.SystemsBiology.Assembly.MetaCyc.File.AttributeValue) As Promoters
        '    Dim Promoters As Promoters = New Promoters
        '    Dim Query As Generic.IEnumerable(Of MetaCyc.File.DataFiles.Slots.Promoter) =
        '        From c As MetaCyc.File.AttributeValue.Object In e.Objects.AsParallel
        '        Select CType(c, MetaCyc.File.DataFiles.Slots.Promoter)

        '    Promoters.DbProperty = e.DbProperty
        '    Promoters.FrameObjects = Query.ToList

        '    Return Promoters
        'End Operator

        Public Shared Shadows Widening Operator CType(Collection As List(Of Slots.Promoter)) As Promoters
            Return New Promoters With {.FrameObjects = Collection}
        End Operator

        ''' <summary>
        ''' 获取所有的启动子的序列的集合
        ''' </summary>
        ''' <param name="Genome"></param>
        ''' <returns></returns>
        ''' <remarks>启动子的序列长度取250bp</remarks>
        Public Function GetPromoters(Genome As FASTA.FastaToken) As FASTA.FastaFile
            Dim pList As List(Of FASTA.FastaToken) = New List(Of FASTA.FastaToken)
            For Each Promoter In Me
                Dim Seq As FASTA.FastaToken = New FASTA.FastaToken With {
                    .Attributes = New String() {Promoter.Identifier}
                }
                Dim d As Integer = Promoter.Direction
                If d = 1 Then
                    Seq.SequenceData = Mid(Genome.SequenceData, Val(Promoter.AbsolutePlus1Pos) - 250, 250)
                ElseIf d = -1 Then
                    Seq.SequenceData = Mid(Genome.SequenceData, Val(Promoter.AbsolutePlus1Pos), 250)
                    Call FASTA.FastaToken.Complement(Seq)
                Else
                    Continue For
                End If

                Call pList.Add(Seq)
            Next

            Return pList
        End Function

        'Public Shared Shadows Widening Operator CType(spath As String) As Promoters
        '    Dim File As MetaCyc.File.AttributeValue = spath
        '    Return CType(File, Promoters)
        'End Operator
    End Class
End Namespace