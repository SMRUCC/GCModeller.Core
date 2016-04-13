Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Assembly.Uniprot

    <PackageNamespace("Uniprot.WebServices")>
    Public Module WebServices

        Const UNIPROT_QUERY As String = "http://www.uniprot.org/uniprot/?query=name%3A{0}+AND+taxonomy%3A{1}&sort=score"

        ''' <summary>
        ''' Create a protein query url. 
        ''' </summary>
        ''' <param name="geneId"></param>
        ''' <param name="taxonomy"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Query.Create", Info:="Create a protein query url. ")>
        Public Function CreateQuery(<Parameter("Gene.ID")> geneId As String, taxonomy As String) As String
            Return String.Format(UNIPROT_QUERY, geneId, taxonomy)
        End Function

        Const UNIPROT_FASTA_DOWNLOAD_URL As String = "http://www.uniprot.org/uniprot/{0}.fasta"

        ''' <summary>
        ''' Download a protein sequence fasta data from http://www.uniprot.org/ using a specific <paramref name="UniprotId"></paramref>. （从http://www.uniprot.org/网站之上下载一条蛋白质序列）
        ''' </summary>
        ''' <param name="UniprotId">The uniprot id of a protein sequence.(蛋白质在Uniprot数据库之中的编号)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Protein.Download", Info:="Download a protein sequence fasta data from http://www.uniprot.org/ using a specific UniprotId.")>
        Public Function DownloadProtein(UniprotId As String) As LANS.SystemsBiology.SequenceModel.FASTA.FastaToken
            Dim pageContent As String = String.Format(UNIPROT_FASTA_DOWNLOAD_URL, UniprotId).GET
            Return LANS.SystemsBiology.SequenceModel.FASTA.FastaToken.TryParse(pageContent)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="url">CreateQuery(geneId, taxonomy)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("ListEntries")>
        Public Function GetEntries(url As String) As Entry
            Dim pageContent As String = url.GET
            Return Nothing
        End Function

        Public Class Entry
            Public Property Entry As ComponentModel.KeyValuePair
            Public Property EntryName As String
            Public Property StatusReviewed As Boolean
            Public Property ProteinNames As String
            Public Property GeneNames As String
            Public Property Organism As String
            Public Property Length As String

            Public Overrides Function ToString() As String
                Return Entry.ToString
            End Function
        End Class
    End Module

    ''' <summary>
    ''' idmapping_selected.tab
    ''' We also provide this tab-delimited table which includes
    ''' the following mappings delimited by tab
    ''' 
    ''' 1. UniProtKB-AC
    ''' 2. UniProtKB-ID
    ''' 3. GeneID (EntrezGene)
    ''' 4. RefSeq
    ''' 5. GI
    ''' 6. PDB
    ''' 7. GO
    ''' 8. UniRef100
    ''' 9. UniRef90
    ''' 10. UniRef50
    ''' 11. UniParc
    ''' 12. PIR
    ''' 13. NCBI-taxon
    ''' 14. MIM
    ''' 15. UniGene
    ''' 16. PubMed
    ''' 17. EMBL
    ''' 18. EMBL-CDS
    ''' 19. Ensembl
    ''' 20. Ensembl_TRS
    ''' 21. Ensembl_PRO
    ''' 22. Additional PubMed
    ''' </summary>
    Public Class IdMapping

        Public Property UniProtKB_AC As String
        Public Property UniProtKB_ID As String
        Public Property GeneID_EntrezGene As String
        Public Property RefSeq As String
        Public Property GI As String
        Public Property PDB As String
        Public Property GO As String
        Public Property UniRef100 As String
        Public Property UniRef90 As String
        Public Property UniRef50 As String
        Public Property UniParc As String
        Public Property PIR As String
        Public Property NCBI_Taxon As String
        Public Property MIM As String
        Public Property UniGene As String
        Public Property PubMed As String
        Public Property EMBL As String
        Public Property EMBL_CDS As String
        Public Property Ensembl As String
        Public Property Ensembl_TRS As String
        Public Property Ensembl_PRO As String
        Public Property Additional_PubMed As String

        Public Shared Function LoadDoc(path As String) As LinkedList(Of IdMapping)
            Dim Reader As New Microsoft.VisualBasic.PartitionedStream(path, 1024)
            Dim list As New LinkedList(Of IdMapping)

            Do While Not Reader.EOF
                Dim lines As String() = Reader.ReadPartition
                If lines.IsNullOrEmpty Then
                    Continue Do
                End If
                Dim data As IdMapping() = lines.ToArray(Function(line) __createObject(line))
                Call list.AddRange(data)
            Loop

            Return list
        End Function

        Private Shared Function __createObject(line As String) As IdMapping
            Dim Tokens As String() = Strings.Split(line, vbTab)
            Dim p As Integer = 0
            Dim Maps As New IdMapping

            With Maps
                .UniProtKB_AC = Tokens.Get(p.MoveNext)
                .UniProtKB_ID = Tokens.Get(p.MoveNext)
                .GeneID_EntrezGene = Tokens.Get(p.MoveNext)
                .RefSeq = Tokens.Get(p.MoveNext)
                .GI = Tokens.Get(p.MoveNext)
                .PDB = Tokens.Get(p.MoveNext)
                .GO = Tokens.Get(p.MoveNext)
                .UniRef100 = Tokens.Get(p.MoveNext)
                .UniRef90 = Tokens.Get(p.MoveNext)
                .UniRef50 = Tokens.Get(p.MoveNext)
                .UniParc = Tokens.Get(p.MoveNext)
                .PIR = Tokens.Get(p.MoveNext)
                .NCBI_Taxon = Tokens.Get(p.MoveNext)
                .MIM = Tokens.Get(p.MoveNext)
                .UniGene = Tokens.Get(p.MoveNext)
                .PubMed = Tokens.Get(p.MoveNext)
                .EMBL = Tokens.Get(p.MoveNext)
                .EMBL_CDS = Tokens.Get(p.MoveNext)
                .Ensembl = Tokens.Get(p.MoveNext)
                .Ensembl_TRS = Tokens.Get(p.MoveNext)
                .Ensembl_PRO = Tokens.Get(p.MoveNext)
                .Additional_PubMed = Tokens.Get(p.MoveNext)
            End With

            Return Maps
        End Function
    End Class

    ''' <summary>
    ''' A fasta object which is specific for the uniprot fasta title parsing.(专门用于解析Uniprot蛋白质序列记录的Fasta对象)
    ''' 
    ''' The following is a description of FASTA headers for UniProtKB (including alternative isoforms), UniRef, UniParc and archived UniProtKB versions. 
    ''' NCBI's program formatdb (in particular its -o option) is compatible with the UniProtKB fasta headers.
    ''' 
    ''' UniProtKB
    ''' >db|UniqueIdentifier|EntryName ProteinName OS=OrganismName[ GN=GeneName]PE=ProteinExistence SV=SequenceVersion
    ''' 
    ''' Where:
    ''' db Is 'sp' for UniProtKB/Swiss-Prot and 'tr' for UniProtKB/TrEMBL.
    ''' UniqueIdentifier Is the primary accession number of the UniProtKB entry.
    ''' EntryName Is the entry name of the UniProtKB entry.
    ''' ProteinName Is the recommended name of the UniProtKB entry as annotated in the RecName field. For UniProtKB/TrEMBL entries without a RecName field, the SubName field Is used. 
    ''' In case of multiple SubNames, the first one Is used. The 'precursor' attribute is excluded, 'Fragment' is included with the name if applicable.
    ''' OrganismName Is the scientific name of the organism of the UniProtKB entry.
    ''' GeneName Is the first gene name of the UniProtKB entry. If there Is no gene name, OrderedLocusName Or ORFname, the GN field Is Not listed.
    ''' ProteinExistence Is the numerical value describing the evidence for the existence of the protein.
    ''' SequenceVersion Is the version number of the sequence.
    ''' </summary>
    ''' <remarks>http://www.uniprot.org/help/fasta-headers</remarks>
    Public Class UniprotFasta : Inherits LANS.SystemsBiology.SequenceModel.FASTA.FastaToken

        Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable

        ''' <summary>
        ''' UniqueIdentifier Is the primary accession number of the UniProtKB entry.
        ''' </summary>
        ''' <returns></returns>
        Public Property UniprotID As String Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable.Identifier
        ''' <summary>
        ''' EntryName Is the entry name of the UniProtKB entry.
        ''' </summary>
        ''' <returns></returns>
        Public Property EntryName As String
        ''' <summary>
        ''' OrganismName Is the scientific name of the organism of the UniProtKB entry.
        ''' </summary>
        ''' <returns></returns>
        Public Property OrgnsmSpName As String
        ''' <summary>
        ''' GeneName Is the first gene name of the UniProtKB entry. If there Is no gene name, OrderedLocusName Or ORFname, the GN field Is Not listed. GeneName(基因名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property GN As String
        ''' <summary>
        ''' ProteinExistence Is the numerical value describing the evidence for the existence of the protein.
        ''' </summary>
        ''' <returns></returns>
        Public Property PE As String
        ''' <summary>
        ''' SequenceVersion Is the version number of the sequence.
        ''' </summary>
        ''' <returns></returns>
        Public Property SV As String
        ''' <summary>
        ''' ProteinName Is the recommended name of the UniProtKB entry as annotated in the RecName field. For UniProtKB/TrEMBL entries without a RecName field, the SubName field Is used. 
        ''' In case of multiple SubNames, the first one Is used. The 'precursor' attribute is excluded, 'Fragment' is included with the name if applicable.
        ''' </summary>
        ''' <returns></returns>
        Public Property ProtName As String

        Public Overrides Function ToString() As String
            Return String.Format("sp|{0}|{1} {2} OS={3} GN={4} PE={5} SV={6}", EntryName, UniprotID, ProtName, OrgnsmSpName, GN, PE, SV)
        End Function

        ''' <summary>
        ''' 从读取的文件数据之中创建一个Uniprot序列对象
        ''' </summary>
        ''' <param name="FastaRaw"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' >sp|Q197F8|002R_IIV3 Uncharacterized protein 002R OS=Invertebrate iridescent virus 3 GN=IIV3-002R PE=4 SV=1
        ''' </remarks>
        Public Shared Function CreateObject(FastaRaw As LANS.SystemsBiology.SequenceModel.FASTA.FastaToken) As UniprotFasta
            Dim UniprotFasta As UniprotFasta = FastaRaw.Copy(Of UniprotFasta)()
            Dim s As String = UniprotFasta.Attributes(2)

            UniprotFasta.EntryName = s.Split.First
            UniprotFasta.UniprotID = UniprotFasta.Attributes(1)
            UniprotFasta.OrgnsmSpName = Regex.Match(s, "OS=[^=]+GN\s*=").Value
            UniprotFasta.OrgnsmSpName = Regex.Replace(UniprotFasta.OrgnsmSpName, "\s*GN\s*=", "")
            UniprotFasta.GN = Regex.Replace(Regex.Match(s, "GN=[^=]+PE\s*=").Value, "\s*PE\s*=", "").Trim
            UniprotFasta.PE = Regex.Match(s, "PE=\d+").Value
            UniprotFasta.SV = Regex.Match(s, "SV=\d+").Value

            Try
                If Not String.IsNullOrEmpty(UniprotFasta.OrgnsmSpName) Then s = s.Replace(UniprotFasta.OrgnsmSpName, "")
                If Not String.IsNullOrEmpty(UniprotFasta.PE) Then s = s.Replace(UniprotFasta.PE, "")
                If Not String.IsNullOrEmpty(UniprotFasta.GN) Then s = s.Replace(UniprotFasta.GN, "")
                If Not String.IsNullOrEmpty(UniprotFasta.SV) Then s = s.Replace(UniprotFasta.SV, "")
                If Not String.IsNullOrEmpty(UniprotFasta.EntryName) Then s = s.Replace(UniprotFasta.EntryName, "").Trim
                UniprotFasta.ProtName = s

                UniprotFasta.OrgnsmSpName = Mid(UniprotFasta.OrgnsmSpName, 4).Trim
                UniprotFasta.GN = Mid(UniprotFasta.GN, 4)
                UniprotFasta.PE = Mid(UniprotFasta.PE, 4)
                UniprotFasta.SV = Mid(UniprotFasta.SV, 4)

                Return UniprotFasta
            Catch ex As Exception
                Throw New SyntaxErrorException(String.Format("Header parsing error at  ------> ""{0}""" & vbCrLf & vbCrLf & ex.ToString, FastaRaw.Title))
            End Try
        End Function

        ''' <summary>
        ''' Load the uniprot fasta sequence file. 
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function LoadFasta(path As String) As UniprotFasta()
            Call Console.WriteLine("[DEBUG] start loading the fasta sequence from file ""{0}""...", path)
            Dim sw As Stopwatch = Stopwatch.StartNew
            Dim LQuery = (From item As LANS.SystemsBiology.SequenceModel.FASTA.FastaToken
                          In LANS.SystemsBiology.SequenceModel.FASTA.FastaFile.Read(path).AsParallel
                          Select UniprotFasta.CreateObject(item)).ToArray
            Call Console.WriteLine("[DEBUG] Uniprot fasta data load done!   {0}ms", sw.ElapsedMilliseconds)
            Return LQuery
        End Function
    End Class
End Namespace