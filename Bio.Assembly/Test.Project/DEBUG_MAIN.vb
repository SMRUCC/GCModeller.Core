Imports LANS.SystemsBiology.Assembly.KEGG.Archives.Xml
Imports LANS.SystemsBiology.Assembly.KEGG.DBGET
Imports LANS.SystemsBiology.Assembly.MetaCyc.File.DataFiles
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat
Imports Microsoft.VisualBasic.Linq

Module DEBUG_MAIN

    Sub Main()

        '    Dim gff = LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.GFF.LoadDocument("D:\Xanthomonas\Xanthomonas citri pv. citri 306\GCA_000007165.1_ASM716v1_genomic.gff")
        '   Dim all_CDS = New GFF(gff, Features.CDS)

        '       Call Language.UnixBash.LinuxRunHelper.PerlShell()

        '   Dim ddddd = LANS.SystemsBiology.Assembly.KEGG.Archives.Csv.Pathway.LoadData("F:\GCModeller.Core\Downloads\Xanthomonas_oryzae_oryzicola_BLS256_uid16740", "xor")

        '  Call ddddd.SaveTo("F:\GCModeller.Core\Downloads\Xanthomonas_oryzae_oryzicola_BLS256_uid16740/xor.Csv")

        Dim alllll = LANS.SystemsBiology.Assembly.KEGG.DBGET.LinkDB.Pathways.AllEntries("xcb").ToArray
        Dim pwys = LANS.SystemsBiology.Assembly.KEGG.DBGET.LinkDB.Pathways.Downloads("xor", "F:\GCModeller.Core\Downloads\Xanthomonas_oryzae_oryzicola_BLS256_uid16740").ToArray

        Dim s = LANS.SystemsBiology.Assembly.KEGG.DBGET.bGetObject.Organism.GetKEGGSpeciesCode("Agrobacterium tumefaciens str. C58 (Cereon)")


        Dim compound As Compounds = Compounds.LoadCompoundsData("G:\1.13.RegPrecise_network\FBA\xcam314565\19.0\data\compounds.dat")


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
