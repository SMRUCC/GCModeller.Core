Imports System.Text.RegularExpressions

Namespace Assembly.MetaCyc.Schema.Metabolism

    ''' <summary>
    ''' 使用一个汇总的MetaCyc数据库，根据目标物种的基因组以及蛋白质信息进行MetaCyc数据库的重建工作
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class PathwayMappingTool

        Dim MetaCyc As LANS.SystemsBiology.Assembly.MetaCyc.File.FileSystem.DatabaseLoadder

        ''' <summary>
        ''' 用于进行参考的MetaCyc数据库
        ''' </summary>
        ''' <param name="MetaCyc"></param>
        ''' <remarks></remarks>
        Sub New(MetaCyc As LANS.SystemsBiology.Assembly.MetaCyc.File.FileSystem.DatabaseLoadder)
            Me.MetaCyc = MetaCyc
        End Sub

        Public Sub Initlaize()
            If Me.MetaCyc.Database.FASTAFiles.protseq.IsNullOrEmpty Then
                Dim LQuery = (From Protein In Me.MetaCyc.GetProteins Let Id As String = Assembly.MetaCyc.Schema.DBLinkManager.DBLink.GetUniprotId(Protein._DBLinks) Where Not String.IsNullOrEmpty(Id) Select Assembly.Uniprot.DownloadProtein(Id)).ToArray
                Call CType(LQuery, SequenceModel.FASTA.FastaFile).Save(Me.MetaCyc.Database.FASTAFiles.ProteinSourceFile)
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Proteins"></param>
        ''' <param name="SavedFile"></param>
        ''' <returns>返回序列下载结果，当所有的序列结果都下载完成的时候，返回0，当出现没有被下载的序列的情况时，返回未被下载的序列数</returns>
        ''' <remarks></remarks>
        Public Shared Function DownloadFromUniprot(Proteins As MetaCyc.File.DataFiles.Proteins, SavedFile As String) As Integer
            Dim NotDownloads As Integer = 0

            For Each Protein In Proteins
                Dim Id As String = Assembly.MetaCyc.Schema.DBLinkManager.DBLink.GetUniprotId(Protein._DBLinks)
                If String.IsNullOrEmpty(Id) Then '未被下载的序列对象
                    Dim Err As String = String.Format("[FASTA_OBJECT_NOT_DOWNLOAD] {0}", Protein.Identifier)
                    NotDownloads += 1
                    Call Console.WriteLine(Err)
                    Call FileIO.FileSystem.WriteAllText(System.Windows.Forms.Application.StartupPath & "/Err.log", Err & vbCrLf, append:=True)
                Else
                    Dim Fsa = Uniprot.DownloadProtein(UniprotId:=Id)
                    If Len(Fsa.SequenceData) = 0 Then
                        Dim Err As String = String.Format("[FASTA_OBJECT_NOT_DOWNLOAD] {0}", Protein.Identifier)
                        NotDownloads += 1
                        Call Console.WriteLine(Err)
                        Call FileIO.FileSystem.WriteAllText(System.Windows.Forms.Application.StartupPath & "/Err.log", Err & vbCrLf, append:=True)
                    Else
                        Fsa.Attributes = (New String() {"gnl", Id, String.Format("{0} {1} 0..0 Unknown", Protein.Identifier, Regex.Match(Fsa.Attributes.Last, "GN=\S+").Value.Split(CChar("=")).Last)})
                        Call FileIO.FileSystem.WriteAllText(SavedFile, Fsa.GenerateDocument(LineBreak:=60), append:=True, encoding:=System.Text.Encoding.ASCII)
                    End If
                End If
            Next

            Return NotDownloads
        End Function

        Public Overrides Function ToString() As String
            Return MetaCyc.ToString
        End Function
    End Class
End Namespace