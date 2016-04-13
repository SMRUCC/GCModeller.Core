Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel

Namespace Assembly.NCBI.GenBank.TabularFormat

    Public Class Rpt : Inherits ITextFile

        Public Property Accession As String
        Public Property GI As String
        ''' <summary>
        ''' The chromesome DNA total length in bp
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Size As Integer
        Public Property Taxname As String
        Public Property Taxid As String
        Public Property GeneticCode As String
        Public Property Publications As String()
        Public Property ProteinCount As Integer
        Public Property CDSCount As Integer
        Public Property PseudoCDSCount As Integer
        Public Property RNACount As Integer
        Public Property NumberOfGenes As Integer
        Public Property PseudoGeneCount As Integer
        Public Property Others As Integer
        Public Property Total As Integer

        Public Overrides Function ToString() As String
            Return Taxname
        End Function

        ''' <summary>
        ''' 从一个*.rpt文件之中加载一个基因组的摘要信息
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="FilePath"></param>
        ''' <returns></returns>
        Public Shared Function Load(Of T As Rpt)(FilePath As String) As T
            Dim Rpt As T = Activator.CreateInstance(Of T)()
            Rpt.FilePath = FilePath
            Rpt.__innerBuffer = FilePath.ReadAllLines

            Rpt.Accession = GetValue(Rpt.__innerBuffer, "Accession: ")
            Rpt.GI = GetValue(Rpt.__innerBuffer, "GI: ")
            Rpt.Size = Val(GetValue(Rpt.__innerBuffer, "DNA  length = "))
            Rpt.Taxname = GetValue(Rpt.__innerBuffer, "Taxname: ")
            Rpt.Taxid = GetValue(Rpt.__innerBuffer, "Taxid: ")
            Rpt.GeneticCode = GetValue(Rpt.__innerBuffer, "Genetic Code: ")
            Rpt.Publications = GetValue(Rpt.__innerBuffer, "Publications: ").Split(CChar("; "))
            Rpt.ProteinCount = GetValue(Rpt.__innerBuffer, "Protein count: ")
            Rpt.CDSCount = GetValue(Rpt.__innerBuffer, "CDS count: ")
            Rpt.PseudoCDSCount = GetValue(Rpt.__innerBuffer, "Pseudo CDS count: ")
            Rpt.RNACount = GetValue(Rpt.__innerBuffer, "RNA count: ")
            Rpt.NumberOfGenes = GetValue(Rpt.__innerBuffer, "Gene count: ")
            Rpt.PseudoGeneCount = GetValue(Rpt.__innerBuffer, "Pseudo gene count: ")
            Rpt.Others = GetValue(Rpt.__innerBuffer, "Others: ")
            Rpt.Total = GetValue(Rpt.__innerBuffer, "Total: ")

            Return Rpt
        End Function

        Public Overloads Function CopyTo(Of T As Rpt)() As T
            Dim obj As T = Activator.CreateInstance(Of T)()

            obj.FilePath = FilePath
            obj.__innerBuffer = If(FileIO.FileSystem.FileExists(FilePath), FilePath.ReadAllLines, New String() {})
            obj.Accession = Accession
            obj.GI = GI
            obj.Size = Size
            obj.Taxname = Taxname
            obj.Taxid = Taxid
            obj.GeneticCode = GeneticCode
            obj.Publications = Publications
            obj.ProteinCount = ProteinCount
            obj.CDSCount = CDSCount
            obj.PseudoCDSCount = PseudoCDSCount
            obj.RNACount = RNACount
            obj.NumberOfGenes = NumberOfGenes
            obj.PseudoGeneCount = PseudoGeneCount
            obj.Others = Others
            obj.Total = Total

            Return obj
        End Function

        Private Shared Function GetValue(data As String(), Key As String) As String
            Dim LQuery = (From sValue As String In data
                          Let strKey As String = Mid(sValue, 1, Len(Key))
                          Where String.Equals(strKey, Key, StringComparison.OrdinalIgnoreCase)
                          Select sValue.Replace(Key, "")).FirstOrDefault
            Return Trim(LQuery)
        End Function

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace