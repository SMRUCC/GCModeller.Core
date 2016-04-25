Imports LANS.SystemsBiology.Assembly.NCBI.GenBank
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Serialization

Public Module Installer

    ''' <summary>
    ''' 这个函数主要是进行创建数据库的索引文件
    ''' </summary>
    ''' <param name="DIR">Extract location of file: all.gbk.tar.gz from NCBI FTP website.</param>
    ''' <returns></returns>
    Public Function Install(DIR As String) As Boolean
        Dim index As String = $"{DIR}/ncbi_genbank.csv"

        Call "".SaveTo(index)  ' 清除原始文件的所有数据，重新创建索引文件

        Using DbWriter As New DocumentStream.Linq.WriteStream(Of GenbankIndex)(index)

            For Each table As String In ls - l - lsDIR - r <= DIR
                For Each gbk As String In ls - l - r - wildcards("*.gbk", "*.gb") <= table
                    For Each gb As GBFF.File In GBFF.File.LoadDatabase(gbk)  ' 读取数据库文件

                        Dim idx As New GenbankIndex With {
                            .AccId = gb.Locus.AccessionID,
                            .definition = gb.Definition.Value,
                            .DIR = table.BaseName,
                            .genome = gb.Source.SpeciesName
                        }
                        Call DbWriter.Flush(idx)   ' 将对象写入内存缓存，进入队列等待回写入文件系统

                    Next
                Next
            Next
        End Using

        Return True
    End Function
End Module

Public Class Genbank : Inherits ClassObject

    Public ReadOnly Property DIR As String

    Sub New(DIR As String)
        Me.DIR = DIR
    End Sub
End Class

Public Class GenbankIndex
    Public Property DIR As String
    Public Property genome As String
    Public Property AccId As String
    Public Property definition As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
