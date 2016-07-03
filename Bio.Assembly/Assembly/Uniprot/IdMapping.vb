﻿#Region "Microsoft.VisualBasic::db39888535065ed18e18a34277b12471, ..\GCModeller\core\Bio.Assembly\Assembly\Uniprot\IdMapping.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports Microsoft.VisualBasic.Linq

Namespace Assembly.Uniprot

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
End Namespace
