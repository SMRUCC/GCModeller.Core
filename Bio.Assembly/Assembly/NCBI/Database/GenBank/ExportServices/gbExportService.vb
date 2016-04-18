Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.GBFF.Keywords
Imports System.Text.RegularExpressions
Imports System.Text
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.CsvExports
Imports System.Runtime.CompilerServices
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.GBFF.Keywords.FEATURES.Nodes
Imports Microsoft.VisualBasic
Imports LANS.SystemsBiology.SequenceModel
Imports Microsoft.VisualBasic.Serialization
Imports LANS.SystemsBiology.ComponentModel.Loci

Namespace Assembly.NCBI.GenBank

    ''' <summary>
    ''' Genbank export methods collection.
    ''' </summary>
    ''' <remarks></remarks>
    Public Module gbExportService

        ''' <summary>
        ''' 尝试去除重复的记录
        ''' </summary>
        ''' <param name="source">原始的Genbank数据库文件所存放的文件夹</param>
        ''' <returns></returns>
        ''' <param name="Trim">是否去除没有序列数据的或者格式已经损坏的数据库文件，并且将原始文件删除？默认都去除</param>
        ''' <remarks></remarks>
        Public Function Distinct(source As String, Optional Trim As Boolean = True) As CsvExports.Plasmid()
            Dim LQuery = (From Path As String
                          In FileIO.FileSystem.GetFiles(source, FileIO.SearchOption.SearchAllSubDirectories, "*.gb", "*.gbk").AsParallel
                          Let Genbank = NCBI.GenBank.GBFF.File.Load(Path)
                          Select Path, Genbank).ToArray
            If Trim Then
                For Each PathEntry As String In (From nn In LQuery Where nn.Genbank Is Nothing OrElse Not nn.Genbank.HasSequenceData Select nn.Path).ToArray
                    Call FileIO.FileSystem.DeleteFile(PathEntry)
                Next

                LQuery = (From item In LQuery Where Not item.Genbank Is Nothing OrElse item.Genbank.HasSequenceData Select item).ToArray
            Else
                LQuery = (From item In LQuery Where Not item.Genbank Is Nothing Select item).ToArray
            End If

            '生成摘要数据
            Dim Brief = (From item In LQuery.AsParallel
                         Let BriefInfo = CsvExports.gbEntryBrief.ConvertObject(Of CsvExports.Plasmid)(item.Genbank)
                         Let Signature As String = BriefInfo.GC_Content.ToString & BriefInfo.Length.ToString & BriefInfo.Organism.ToLower
                         Select item.Path, BriefInfo, Signature, SubmitDate = BriefInfo.GetSubmitDate
                         Group By Signature Into Group).ToArray

            '删除原始数据
            Dim Distincted As New List(Of CsvExports.Plasmid)
            For Each Entry In Brief
                If Entry.Group.Count = 1 Then
                    Call Distincted.Add(Entry.Group.First.BriefInfo)
                Else
                    '按照日期排序，只取出最新提交的数据
                    Dim OrderedLQuery = (From item In Entry.Group Select item Order By item.SubmitDate Descending).ToArray
                    Call Distincted.Add(OrderedLQuery.First.BriefInfo)

                    For Each Duplicated In OrderedLQuery.Skip(1)
                        Call FileIO.FileSystem.DeleteFile(Duplicated.Path)
                    Next
                End If
            Next

            Return Distincted.ToArray
        End Function

        ''' <summary>
        ''' 返回去除掉重复的数据之后的AccessionId编号
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Distinct(data As Generic.IEnumerable(Of CsvExports.Plasmid)) As CsvExports.Plasmid()
            '生成摘要数据
            Dim Brief = (From BriefInfo In data.AsParallel
                         Let Signature As String = BriefInfo.GC_Content.ToString & BriefInfo.Length.ToString & BriefInfo.Organism.ToLower
                         Select BriefInfo, Signature, SubmitDate = BriefInfo.GetSubmitDate
                         Group By Signature Into Group).ToArray

            '删除原始数据
            Dim Distincted As New List(Of CsvExports.Plasmid)
            For Each Entry In Brief
                If Entry.Group.Count = 1 Then
                    Call Distincted.Add(Entry.Group.First.BriefInfo)
                Else
                    '按照日期排序，只取出最新提交的数据
                    Dim OrderedLQuery = (From item In Entry.Group Select item Order By item.SubmitDate Descending).ToArray
                    Call Distincted.Add(OrderedLQuery.First.BriefInfo)
                End If
            Next

            Return Distincted.ToArray
        End Function

        ''' <summary>
        ''' 将PTT文件夹之中的基因组序列数据复制到目标文件夹之中
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CopyGenomeSequence(source As String, copyTo As String) As Integer
            Dim LQuery = (From Path In source.LoadSourceEntryList("*.fna").AsParallel
                          Let Fasta = New TabularFormat.FastaObjects.GenomeSequence(SequenceModel.FASTA.FastaToken.Load(Path.Value))
                          Select Fasta.SaveBriefData(copyTo & "/" & Path.Key & ".fasta")).ToArray
            Return LQuery.Count
        End Function

        ''' <summary>
        ''' 假若目标GBK是使用本模块之中的方法保存或者导出来的，则可以使用本方法生成Entry列表；（在返回的结果之中，KEY为文件名，没有拓展名，VALUE为文件的路径）
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="ext">文件类型的拓展名称</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function LoadGbkSource(source As String, ParamArray ext As String()) As Dictionary(Of String, String)
            Dim LQuery = (From path As String
                          In FileIO.FileSystem.GetFiles(source, FileIO.SearchOption.SearchAllSubDirectories, ext)
                          Select ID = TryParseGBKID(path),
                              path
                          Group By ID Into Group).ToArray
            Dim Dict = LQuery.ToDictionary(keySelector:=Function(item) item.ID, elementSelector:=Function(item) item.Group.First.path)
            Return Dict
        End Function

        ''' <summary>
        ''' 将GBK文件之中的基因的位置数据导出为PTT格式的数据
        ''' </summary>
        ''' <param name="Genbank">导出gene和RNA部分的数据</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function GbkffExportToPTT(Genbank As NCBI.GenBank.GBFF.File) As TabularFormat.PTT
            Dim Genes As Feature() = (From Feature As Feature
                                                                 In Genbank.Features._innerList
                                      Where String.Equals(Feature.KeyName, "gene", StringComparison.OrdinalIgnoreCase) OrElse
                                                                     InStr(Feature.KeyName, "RNA", CompareMethod.Text) > 0
                                      Select Feature).ToArray
            Return Genes.__toGenes(Genbank.Origin.SequenceData.Length, Genbank.Definition.Value)
        End Function

        <Extension> Private Function __toGenes(genes As Feature(), size As Integer, def As String) As TabularFormat.PTT
            Dim PTTGenes = (From GeneObject As Feature
                            In genes
                            Select Gene = __featureToPTT(GeneObject)
                            Group Gene By Gene.Synonym Into Group).ToArray
            Dim LQuery = (From GeneGr In PTTGenes Where GeneGr.Group.Count > 1 Select GeneGr).ToArray
            For Each DulGeneObject In LQuery
                Call $"""{DulGeneObject.Synonym}"" data was duplicated!".__DEBUG_ECHO
            Next
            Return New NCBI.GenBank.TabularFormat.PTT With {
                .GeneObjects = (From GeneObject In PTTGenes Select GeneObject.Group.First).ToArray,
                .Size = size,
                .Title = def
            }
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="gb">只导出CDS部分的数据</param>
        ''' <returns></returns>
        <Extension> Public Function GbffToORF_PTT(gb As GBFF.File) As TabularFormat.PTT
            Dim Genes As Feature() = (From Feature As Feature
                                      In gb.Features._innerList
                                      Where String.Equals(Feature.KeyName, "CDS", StringComparison.OrdinalIgnoreCase)
                                      Select Feature).ToArray
            Return Genes.__toGenes(gb.Origin.SequenceData.Length, gb.Definition.Value)
        End Function

        Private Function __featureToPTT(featureSite As Feature) As TabularFormat.ComponentModels.GeneBrief
            Dim loci As New ComponentModel.Loci.NucleotideLocation(
                featureSite.Location.Locations.First.Left,
                featureSite.Location.Locations.Last.Right,
                featureSite.Location.Complement)
            Dim locusId As String = featureSite.Query(FeatureQualifiers.locus_tag)
            Dim GB As New TabularFormat.ComponentModels.GeneBrief With {
                .Synonym = locusId,
                .PID = featureSite.Query(FeatureQualifiers.protein_id),
                .Product = featureSite.Query(FeatureQualifiers.product),
                .Gene = featureSite.Query(FeatureQualifiers.gene),
                .Location = loci
            }
            GB.Length = GB.Location.FragmentSize

            If String.IsNullOrEmpty(GB.Synonym) Then
                GB.Synonym = featureSite.Query(FeatureQualifiers.gene)
            End If
            If String.IsNullOrEmpty(GB.Synonym) Then
                GB.Synonym = featureSite.Location.UniqueId
            End If

            Return GB
        End Function

        <Extension> Public Function InvokeExport(gbk As NCBI.GenBank.GBFF.File, ByRef GeneList As GeneDumpInfo()) As KeyValuePair(Of gbEntryBrief, String)
            Dim LQuery = (From FeatureData As Feature
                          In gbk.Features._innerList.AsParallel
                          Where String.Equals(FeatureData.KeyName, "CDS", StringComparison.OrdinalIgnoreCase)
                          Select GeneDumpInfo.DumpEXPORT(New CDS(FeatureData))).ToArray
            GeneList = LQuery
            Return New KeyValuePair(Of gbEntryBrief, String)(gbEntryBrief.ConvertObject(Of gbEntryBrief)(gbk), gbk.Origin.SequenceData)
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="GeneList"></param>
        ''' <param name="GBK"></param>
        ''' <param name="FastaExport">Fasta序列文件的导出文件夹</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function BatchExport(list As IEnumerable(Of GBFF.File),
                                    ByRef GeneList As GeneDumpInfo(),
                                    ByRef GBK As gbEntryBrief(),
                                    FastaExport As String,
                                    Optional FastaWithAnnotation As Boolean = False) As Integer

            Dim ExportList As Dictionary(Of CsvExports.gbEntryBrief, String) = New Dictionary(Of gbEntryBrief, String)
            Dim GeneChunkList As List(Of GeneDumpInfo) = New List(Of GeneDumpInfo)
            Dim FastaFile As FASTA.FastaFile = New FASTA.FastaFile
            Dim PlasmidList As FASTA.FastaFile = New FASTA.FastaFile
            Dim GeneSequenceList As FASTA.FastaFile = New FASTA.FastaFile

            Call "Flushed memory....".__DEBUG_ECHO
            Call FlushMemory()
            Call $"There is ""{list.Count}"" genome source will be export...".__DEBUG_ECHO

            Dim ExportLQuery = (From GBKFF As GBFF.File
                                In list.AsParallel
                                Let GenesTempChunk As GeneDumpInfo() = (From FeatureData As Feature
                                                                        In GBKFF.Features._innerList.AsParallel
                                                                        Where String.Equals(FeatureData.KeyName, "CDS", StringComparison.OrdinalIgnoreCase)
                                                                        Select GeneDumpInfo.DumpEXPORT(New CDS(FeatureData))).ToArray
                                Let Entry = gbEntryBrief.ConvertObject(Of gbEntryBrief)(GBKFF)
                                Let FastaDump As FASTA.FastaFile =
                                    If(FastaWithAnnotation, __exportWithAnnotation(GenesTempChunk), __exportNoAnnotation(GenesTempChunk))
                                Let Plasmid As FASTA.FastaToken =
                                    New FASTA.FastaToken With {
                                        .Attributes = New String() {Entry.AccessionID},
                                        .SequenceData = GBKFF.Origin.SequenceData.ToUpper
                                    }
                                Let Reader = New SequenceModel.NucleotideModels.SegmentReader(GBKFF.Origin.SequenceData, False)
                                Let GeneFastaDump = CType((From GeneObject In GBKFF.Features._innerList.AsParallel
                                                           Where String.Equals(GeneObject.KeyName, "gene", StringComparison.OrdinalIgnoreCase)
                                                           Let loc = GeneObject.Location.ContiguousRegion
                                                           Let Sequence As String = Reader.GetSegmentSequence(loc.Left, loc.Right)
                                                           Select New LANS.SystemsBiology.SequenceModel.FASTA.FastaToken With {
                                                               .Attributes = New String() {GeneObject.Query("locus_tag"), GeneObject.Location.ToString},
                                                               .SequenceData = If(GeneObject.Location.Complement, SequenceModel.NucleotideModels.NucleicAcid.Complement(Sequence), Sequence)
                                                           }).ToArray, LANS.SystemsBiology.SequenceModel.FASTA.FastaFile)
                                Select GBKFF,
                                    GenesTempChunk,
                                    Entry,
                                    FastaDump,
                                    Plasmid,
                                    Reader,
                                    GeneFastaDump).ToArray

            For Each item In ExportLQuery
                Call GeneChunkList.AddRange(item.GenesTempChunk)
                Call ExportList.Add(item.Entry, item.Entry.AccessionID)
                Call item.FastaDump.Save(FastaExport & "/Orf/" & item.Entry.AccessionID & ".fasta")
                Call item.Plasmid.SaveTo(FastaExport & "/Genomes/" & item.Entry.AccessionID & ".fasta")
                Call item.GeneFastaDump.Save(FastaExport & "/Genes/" & item.Entry.AccessionID & ".fasta")

                Call FastaFile.AddRange(item.FastaDump)
                Call PlasmidList.Add(item.Plasmid)
                Call GeneSequenceList.AddRange(item.GeneFastaDump)
            Next

            GeneList = GeneChunkList.ToArray
            GBK = (From entryInfo In ExportList Select entryInfo.Key).ToArray

            Try
                Call FastaFile.Save(FastaExport & "/CDS.Gene.fasta")
                Call PlasmidList.Save(FastaExport & "/Genbank.ORIGINS.fasta")
                Call GeneSequenceList.Save(FastaExport & "/Gene.Nt.fasta")
            Catch ex As Exception
                Call App.LogException(ex)
            End Try

            Return ExportList.Count
        End Function

        <Extension> Public Function ExportGeneAnno(gbk As LANS.SystemsBiology.Assembly.NCBI.GenBank.GBFF.File) As GeneDumpInfo()
            Dim GenesTempChunk As GeneDumpInfo() = (From FeatureData As Feature
                                                    In gbk.Features._innerList.AsParallel
                                                    Where String.Equals(FeatureData.KeyName, "CDS", StringComparison.OrdinalIgnoreCase)
                                                    Select GeneDumpInfo.DumpEXPORT(New CDS(FeatureData))).ToArray
            Return GenesTempChunk
        End Function

        <Extension> Public Function ExportPTTAsDump(PTT As NCBI.GenBank.TabularFormat.PTT) As GeneDumpInfo()
            Dim LQuery = (From GeneObject In PTT.GeneObjects.AsParallel
                          Select New GeneDumpInfo With {
                              .CDS = "",
                              .COG = GeneObject.COG,
                              .CommonName = GeneObject.Gene,
                              .EC_Number = "-",
                              .Function = GeneObject.Product,
                              .GC_Content = 0,
                              .GeneName = GeneObject.Gene,
                              .GI = "-",
                              .GO = "-",
                              .InterPro = {},
                              .Left = GeneObject.Location.Left,
                              .Length = GeneObject.Location.FragmentSize,
                              .Location = GeneObject.Location,
                              .LocusID = GeneObject.Synonym,
                              .ProteinId = GeneObject.Synonym,
                              .Right = GeneObject.Location.Right,
                              .Species = "",
                              .SpeciesAccessionID = "",
                              .Strand = GeneObject.Location.Strand.ToString,
                              .Translation = "",
                              .Transl_Table = "",
                              .UniprotSwissProt = "",
                              .UniprotTrEMBL = ""}).ToArray
            Return LQuery
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="list"></param>
        ''' <param name="GeneList"></param>
        ''' <param name="GBK"></param>
        ''' <param name="FastaExport"></param>
        ''' <param name="FastaWithAnnotation">是否将序列的注释信息一同导出来，<see cref="vbTrue"></see>会将功能注释信息和菌株信息一同导出，<see cref="vbFalse"></see>则仅仅会导出基因号，假若没有基因号，则会导出蛋白质编号</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function BatchExportPlasmid(list As Generic.IEnumerable(Of NCBI.GenBank.GBFF.File), ByRef GeneList As GeneDumpInfo(), ByRef GBK As Plasmid(), FastaExport As String, Optional FastaWithAnnotation As Boolean = False) As Integer
            Dim ExportList As Dictionary(Of Plasmid, String) = New Dictionary(Of Plasmid, String)
            Dim GeneChunkList As List(Of GeneDumpInfo) = New List(Of GeneDumpInfo)
            Dim FastaFile As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile = New SequenceModel.FASTA.FastaFile
            Dim PlasmidList As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile = New SequenceModel.FASTA.FastaFile
            Dim GeneSequenceList As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile = New SequenceModel.FASTA.FastaFile

            Dim Source = (From item In list.AsParallel Where item.IsPlasmidSource Select item).ToArray

            Call Console.WriteLine("[DEBUG] Flushed memory....")
            list = Nothing
            Call FlushMemory()
            Call Console.WriteLine("[DEBUG] There is ""{0}"" plasmid source will be export...", Source.Count)

            For Each GBKFF As LANS.SystemsBiology.Assembly.NCBI.GenBank.GBFF.File In Source
                Dim GenesTempChunk As GeneDumpInfo() = (From FeatureData As Feature
                                                        In GBKFF.Features._innerList.AsParallel
                                                        Where String.Equals(FeatureData.KeyName, "CDS", StringComparison.OrdinalIgnoreCase)
                                                        Select GeneDumpInfo.DumpEXPORT(New CDS(FeatureData))).ToArray
                Dim Entry = NCBI.GenBank.CsvExports.Plasmid.Build(GBKFF)

                Call ExportList.Add(Entry, GBKFF.Origin.SequenceData)
                Call GeneChunkList.AddRange(GenesTempChunk)

                '导出Fasta序列
                Dim FastaDump As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile =
                    If(FastaWithAnnotation, __exportWithAnnotation(GenesTempChunk), __exportNoAnnotation(GenesTempChunk))

                If FastaDump.Count > 0 Then
                    Call FastaDump.Save(String.Format("{0}/plasmid_cds/{1}.fasta", FastaExport, GBKFF.Accession.AccessionId))
                    Call FastaFile.AddRange(FastaDump)
                End If

                Dim Plasmid As LANS.SystemsBiology.SequenceModel.FASTA.FastaToken =
                    New LANS.SystemsBiology.SequenceModel.FASTA.FastaToken With {
                        .Attributes = New String() {Entry.AccessionID & "_" & Entry.PlasmidID.Replace("-", "_")},
                        .SequenceData = GBKFF.Origin.SequenceData.ToUpper
                }

                Call PlasmidList.Add(Plasmid)
                Call Plasmid.SaveTo(String.Format("{0}/plasmids/{1}.fasta", FastaExport, GBKFF.Accession.AccessionId))

                Dim Reader As New SequenceModel.NucleotideModels.SegmentReader(GBKFF.Origin.SequenceData, False)
                Dim GeneFastaDump = CType((From GeneObject In GBKFF.Features._innerList.AsParallel
                                           Where String.Equals(GeneObject.KeyName, "gene", StringComparison.OrdinalIgnoreCase)
                                           Let loc = GeneObject.Location.ContiguousRegion
                                           Let Sequence As String = Reader.GetSegmentSequence(loc.Left, loc.Right)
                                           Select New LANS.SystemsBiology.SequenceModel.FASTA.FastaToken With {
                                               .Attributes = New String() {GeneObject.Query("locus_tag"), GeneObject.Location.ToString},
                                               .SequenceData = If(GeneObject.Location.Complement, SequenceModel.NucleotideModels.NucleicAcid.Complement(Sequence), Sequence)
                                           }).ToArray, LANS.SystemsBiology.SequenceModel.FASTA.FastaFile)

                If GeneFastaDump.Count > 0 Then
                    Call GeneSequenceList.AddRange(GeneFastaDump.ToArray)
                    Call GeneFastaDump.Save(String.Format("{0}/plasmid_genes/{1}.fasta", FastaExport, GBKFF.Accession.AccessionId))
                    Call GeneFastaDump.FlushData()
                End If
            Next

            GeneList = GeneChunkList.ToArray
            GBK = (From item In ExportList Select item.Key).ToArray

            Try
                Call FastaFile.Save(FastaExport & "/CDS_GENE.fasta")
                Call PlasmidList.Save(FastaExport & "/GBKFF.ORIGINS_plasmid.fasta")
                Call GeneSequenceList.Save(FastaExport & "/GENE_SEQUENCE.fasta")
            Catch ex As Exception

            End Try

            Return ExportList.Count
        End Function

        Private Function __exportNoAnnotation(data As GeneDumpInfo()) As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile
            Dim LQuery = (From GeneObject As GeneDumpInfo
                          In data.AsParallel
                          Let fa = New LANS.SystemsBiology.SequenceModel.FASTA.FastaToken With {
                              .Attributes = New String() {GeneObject.LocusID},
                              .SequenceData = GeneObject.Translation
                          }
                          Select fa).ToArray
            Return CType(LQuery, SequenceModel.FASTA.FastaFile)
        End Function

        Private Function __exportWithAnnotation(data As GeneDumpInfo()) As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile
            Dim LQuery = (From GeneObject As GeneDumpInfo In data.AsParallel
                          Let attrs As String() = New String() {GeneObject.LocusID, GeneObject.GeneName, GeneObject.GI, GeneObject.CommonName, GeneObject.Function, GeneObject.Species}
                          Select New LANS.SystemsBiology.SequenceModel.FASTA.FastaToken With {
                              .Attributes = attrs,
                              .SequenceData = GeneObject.Translation
                          }).ToArray
            Return CType(LQuery, SequenceModel.FASTA.FastaFile)
        End Function

        <Extension> Public Function TryParseGBKID(path As String) As String
            Dim Name As String = IO.Path.GetFileNameWithoutExtension(path)
            Name = Regex.Replace(Name, "\.\d+", "")
            Return Name.ToUpper
        End Function

        <Extension>
        Public Function GeneNtFasta(gb As GBFF.File) As FASTA.FastaFile
            Dim Reader As New NucleotideModels.SegmentReader(gb.Origin.SequenceData, False)
            Dim list As New List(Of FASTA.FastaToken)
            Dim loc As NucleotideLocation = Nothing
            Dim attrs As String() = Nothing
            Dim Sequence As String
            Dim products As Dictionary(Of GeneDumpInfo) = gb.ExportGeneAnno.ToDictionary

            Try
                For Each gene As Feature In (From x As Feature
                                             In gb.Features._innerList.AsParallel
                                             Where String.Equals(x.KeyName, "gene", StringComparison.OrdinalIgnoreCase)
                                             Select x)

                    Dim locus_tag As String = gene.Query("locus_tag")

                    loc = gene.Location.ContiguousRegion
                    attrs = {locus_tag, gene.Location.ToString, products.SafeGetValue(locus_tag)?.Function}
                    Sequence = Reader.GetSegmentSequence(loc.Left, loc.Right)
                    Sequence = If(gene.Location.Complement, NucleotideModels.NucleicAcid.Complement(Sequence), Sequence)

                    list += New FASTA.FastaToken(attrs, Sequence)
                Next
            Catch ex As Exception
                ex = New Exception(gb.ToString, ex)
                ex = New Exception(attrs.GetJson, ex)
                ex = New Exception(loc.GetJson, ex)
                ex = New Exception(gb.Accession.GetJson, ex)
                Call App.LogException(ex)
                Throw ex
            End Try

            Return New FASTA.FastaFile(list)
        End Function
    End Module
End Namespace