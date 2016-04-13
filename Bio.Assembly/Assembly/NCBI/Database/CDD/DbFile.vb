Imports Dir = System.String
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace Assembly.NCBI.CDD

    ''' <summary>
    ''' CDD database builder.(CDD数据库构建工具)
    ''' </summary>
    ''' <remarks>
    ''' ftp://ftp.ncbi.nlm.nih.gov/pub/mmdb/cdd/cdd.tar.gz
    ''' </remarks>
    ''' 
    <XmlRoot("CDD.DB_File", Namespace:="http://code.google.com/p/genome-in-code/ncbi/cdd")>
    <PackageNamespace("NCBI.CDD", Url:="ftp://ftp.ncbi.nlm.nih.gov/pub/mmdb/cdd/cdd.tar.gz",
                      Category:=APICategories.ResearchTools,
                      Description:="CDD database builder.",
                      Publisher:="xie.guigang@gmail.com")>
    <Cite(Title:="CDD: a Conserved Domain Database for protein classification", Volume:=33, Year:=2005,
          Journal:="Nucleic Acids Res",
          Keywords:="Amino Acid Sequence
Conserved Sequence
*Databases, Protein
Phylogeny
*Protein Structure, Tertiary
Proteins/*classification
Sequence Alignment
Sequence Analysis, Protein
User-Computer Interface",
          Authors:="Marchler-Bauer, A.
Anderson, J. B.
Cherukuri, P. F.
DeWeese-Scott, C.
Geer, L. Y.
Gwadz, M.
He, S.
Hurwitz, D. I.
Jackson, J. D.
Ke, Z.
Lanczycki, C. J.
Liebert, C. A.
Liu, C.
Lu, F.
Marchler, G. H.
Mullokandov, M.
Shoemaker, B. A.
Simonyan, V.
Song, J. S.
Thiessen, P. A.
Yamashita, R. A.
Yin, J. J.
Zhang, D.
Bryant, S. H.",
          DOI:="10.1093/nar/gki069",
          ISSN:="1362-4962 (Electronic)
0305-1048 (Linking)",
          Abstract:="The Conserved Domain Database (CDD) is the protein classification component of NCBI's Entrez query and retrieval system. CDD is linked to other Entrez databases such as Proteins, Taxonomy and PubMed, and can be accessed at http://www.ncbi.nlm.nih.gov/entrez/query.fcgi?db=cdd. 
CD-Search, which is available at http://www.ncbi.nlm.nih.gov/Structure/cdd/wrpsb.cgi, is a fast, interactive tool to identify conserved domains in new protein sequences. 
CD-Search results for protein sequences in Entrez are pre-computed to provide links between proteins and domain models, and computational annotation visible upon request. 
Protein-protein queries submitted to NCBI's BLAST search service at http://www.ncbi.nlm.nih.gov/BLAST are scanned for the presence of conserved domains by default. 
While CDD started out as essentially a mirror of publicly available domain alignment collections, such as SMART, Pfam and COG, we have continued an effort to update, and in some cases replace these models with domain hierarchies curated at the NCBI. 
Here, we report on the progress of the curation effort and associated improvements in the functionality of the CDD information retrieval system.",
          AuthorAddress:="National Center for Biotechnology Information, National Library of Medicine, National Institutes of Health, Building 38 A, Room 8N805, 8600 Rockville Pike, Bethesda, MD 20894, USA. bauer@ncbi.nlm.nih.gov",
          Issue:="Database issue",
          Pages:="D192-6",
          PubMed:=15608175)>
    Public Class DbFile : Inherits Microsoft.VisualBasic.ComponentModel.ITextFile

        Dim _innerDict As Dictionary(Of String, CDD.SmpFile)

        <XmlElement> Public Property SmpData As CDD.SmpFile()
            Get
                If Not _innerDict Is Nothing Then
                    Return _innerDict.Values.ToArray
                Else
                    Return New SmpFile() {}
                End If
            End Get
            Set(value As CDD.SmpFile())
                If Not value.IsNullOrEmpty Then
                    _innerDict = value.ToDictionary(Function(o As CDD.SmpFile) o.Identifier)
                Else
                    Call $"Null database entries!".__DEBUG_ECHO
                    _innerDict = New Dictionary(Of Dir, SmpFile)
                End If
            End Set
        End Property

        <XmlAttribute> Public Property Id As String
        <XmlAttribute> Public Property BuildTime As String

        Public Function FindByTabId(strTagId As String) As CDD.SmpFile
            Dim TagId As Integer = Val(strTagId)
            Dim LQuery = (From smp As SmpFile
                          In SmpData
                          Where TagId = smp.Id
                          Select smp).FirstOrDefault
            Return LQuery
        End Function

        ''' <summary>
        ''' 不存在则返回空值
        ''' </summary>
        ''' <param name="Id"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property Smp(Id As String) As CDD.SmpFile
            Get
                Return _innerDict.TryGetValue(Id)
            End Get
        End Property

        Public ReadOnly Property FastaUrl As String
            Get
                Return FilePath.Replace(".xml", ".fasta")
            End Get
        End Property

        ''' <summary>
        ''' 非并行版本的<see cref="CDD.SmpFile.Identifier">AccessionId</see>, <see cref="CDD.SmpFile.Id">TagId</see>, <see cref="CDD.SmpFile.CommonName">CommonName</see>
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ContainsId(id As String) As CDD.SmpFile
            If _innerDict.ContainsKey(id) Then
                Return _innerDict(id)
            End If

            Return (From item In _innerDict.Values
                    Where String.Equals(item.CommonName, id) OrElse
                        String.Equals(item.Id.ToString, id)
                    Select item).FirstOrDefault
        End Function

        ''' <summary>
        ''' 并行版本的
        ''' </summary>
        ''' <param name="Id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ContainsId_p(Id As String) As CDD.SmpFile
            If _innerDict.ContainsKey(Id) Then
                Return _innerDict(Id)
            End If

            Return (From item In _innerDict.Values.AsParallel
                    Where String.Equals(item.CommonName, Id) OrElse String.Equals(item.Id.ToString, Id)
                    Select item).FirstOrDefault
        End Function

        Private Shared Sub __buildDb(DbDir As String, Export As String)
            For Each Pn As Pn In PreLoad(DbDir)
                Call $"> Build {Pn.FilePath.ToFileURL}".__DEBUG_ECHO
                Call __buildDb(Pn, Export)
            Next
        End Sub

        Private Shared Sub __buildDb(pn As Pn, Export As String)
            Dim File As String = String.Format("{0}/{1}", Export, pn.ToString.Split(CChar("/")).Last.Replace(".pn", String.Empty))
            Dim FASTA As String = File & ".fasta"
            Dim LQuery = From FilePath As String
                         In pn.AsParallel
                         Where FileIO.FileSystem.FileExists(FilePath)
                         Let SmpFile As CDD.SmpFile = CDD.SmpFile.Load(FilePath)
                         Select SmpFile Order By SmpFile.Identifier
                         Ascending  '..AsParallel
            Dim DbFile As CDD.DbFile = New DbFile With {
                .FilePath = File & ".xml",
                .Id = pn.FilePath.Split(CChar("/")).Last,
                .SmpData = LQuery.ToArray,
                .BuildTime = Now.ToString
            }

            Call $" EXPORT fasta sequence data {FASTA}".__DEBUG_ECHO
            Call CType((From Smp As SmpFile
                        In DbFile.SmpData.AsParallel
                        Let Fsa As SequenceModel.FASTA.FastaToken = Smp.Export
                        Select Fsa).ToArray, SequenceModel.FASTA.FastaFile).Save(FASTA)
            Call DbFile.GetXml.SaveTo(DbFile.FilePath)
        End Sub

        <ExportAPI("Db.Build")>
        Public Shared Sub BuildDb(DbDir As String, Export As Dir)
            Using busy As Microsoft.VisualBasic.ConsoleDevice.Utility.CBusyIndicator =
                New Microsoft.VisualBasic.ConsoleDevice.Utility.CBusyIndicator

                Call FileIO.FileSystem.CreateDirectory(Export)
                Call busy.Start()
                Call __buildDb(DbDir, Export)
            End Using
        End Sub

        Public Overloads Function ExportFASTA() As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile
            Dim Fasta As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile = ExportFASTA(Me)
            Call Fasta.Save(Me.FastaUrl)
            Return Fasta
        End Function

        <ExportAPI("Fasta.Export")>
        Public Overloads Shared Function ExportFASTA(Db As DbFile) As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile
            Dim LQuery = From Smp As CDD.SmpFile
                         In Db.SmpData
                         Select Smp.Export '
            Dim Fasta As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile =
                CType(LQuery.ToArray, SequenceModel.FASTA.FastaFile)
            Return Fasta
        End Function

        Public Overrides Function ToString() As String
            Return FilePath
        End Function

        ''' <summary>
        ''' 根据唯一标识符的集合来获取数据库记录数据
        ''' </summary>
        ''' <param name="lstAccId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Takes(lstAccId As Generic.IEnumerable(Of String)) As DbFile
            Dim Ids As String() = lstAccId.ToArray
            Dim LQuery = From smp As SmpFile
                         In Me.SmpData.AsParallel
                         Where Array.IndexOf(Ids, smp.Identifier) > -1
                         Select smp '
            Return New DbFile With {
                .SmpData = LQuery.ToArray
            }
        End Function

        ''' <summary>
        ''' 在编译整个CDD数据库之前进行预加载
        ''' </summary>
        ''' <param name="Dir"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Db.PreLoad")>
        Public Shared Function PreLoad(Dir As Dir) As Pn()
            Dim LQuery As Generic.IEnumerable(Of Pn) = From File As String
                                                       In FileIO.FileSystem.GetFiles(Dir, FileIO.SearchOption.SearchTopLevelOnly, "*.pn")
                                                       Select CType(File, Pn) '
            Return LQuery.ToArray
        End Function

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            Return Me.GetXml.SaveTo(getPath(FilePath), getEncoding(Encoding))
        End Function
    End Class
End Namespace


