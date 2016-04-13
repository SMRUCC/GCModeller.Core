Imports LANS.SystemsBiology.Assembly.KEGG.Archives.Xml
Imports LANS.SystemsBiology.Assembly.KEGG.DBGET
Imports Microsoft.VisualBasic.Linq

Module DEBUG_MAIN

    Sub Main()


        Dim rxn = bGetObject.Reaction.DownloadFrom("http://www.genome.jp/dbget-bin/www_bget?rn:R00086")
        Dim modelssss = rxn.ReactionModel

        Call rxn.SaveAsXml("x:\safsdsdfsd____rxn.xml")


        Dim model = CompilerAPI.Compile("F:\1.13.RegPrecise_network\Cellular Phenotypes\KEGG_Pathways", "F:\1.13.RegPrecise_network\Cellular Phenotypes\KEGG_Modules", "F:\GCModeller\KEGG\Reactions", "xcb")
        Call model.SaveAsXml("x:\dfsasdfsdf.kegg.xml")

        Dim rxns = FileIO.FileSystem.GetFiles("F:\GCModeller\KEGG\Reactions", FileIO.SearchOption.SearchAllSubDirectories, "*.xml").ToArray(Function(x) x.LoadXml(Of bGetObject.Reaction))



        'Dim gff = LANS.SystemsBiology.Assembly.NCBI.GenBank.PttGenomeBrief.GenomeFeature.GFF.LoadDocument("E:\xcb_vcell\Xanthomonas_campestris_8004_uid15\CP000050.gff3")

        'Dim gff33333 = LANS.SystemsBiology.Assembly.NCBI.GenBank.PttGenomeBrief.GenomeFeature.GFF.LoadDocument("E:\Desktop\DESeq\Xcc8004.gff")

        'Dim ptt = LANS.SystemsBiology.Assembly.NCBI.GenBank.PttGenomeBrief.PTT.Load("E:\xcb_vcell\Xanthomonas_campestris_8004_uid15\CP000050.ptt")

        ''修改之前的数据为 Inside the [XC_2906] gene ORF.
        'Dim r As LANS.SystemsBiology.ComponentModel.Loci.NucleotideLocation.SegmentRelationships
        'Dim dataffff = LANS.SystemsBiology.Assembly.NCBI.GenBank.PttGenomeBrief.ComponentModels.GetRelatedGenes(Of LANS.SystemsBiology.Assembly.NCBI.GenBank.PttGenomeBrief.ComponentModels.GeneBrief)(ptt, 3491357, 3491377, r)
        'dataffff = LANS.SystemsBiology.Assembly.NCBI.GenBank.PttGenomeBrief.ComponentModels.GetRelatedGenes(Of LANS.SystemsBiology.Assembly.NCBI.GenBank.PttGenomeBrief.ComponentModels.GeneBrief)(ptt, 2874066, 874095, r, 1000)

        'Call LANS.SystemsBiology.Assembly.Uniprot.UniprotFasta.LoadFasta("E:\BLAST\db\uniprot_sprot.fasta")

        'Dim nfff = LANS.SystemsBiology.Assembly.KEGG.DBGET.ReferenceMap.ReferenceMapData.Download("map00010")


        'Dim gbk = LANS.SystemsBiology.Assembly.NCBI.GenBank.File.Read("E:\Desktop\xoc_vcell\plasmid\ncbi\FP340277.1.gbk")

        'Call LANS.SystemsBiology.Assembly.NCBI.GenBank.InvokeExport(gbk, Nothing)

        'Dim query = New LANS.SystemsBiology.Assembly.NCBI.Entrez.QueryHandler("Xanthomonas")
        'Dim list = query.DownloadCurrentPage
        'Dim n = list.First.DownloadGBK("x:\")
    End Sub
End Module
