Imports LANS.SystemsBiology.Assembly.NCBI.GenBank
Imports LANS.SystemsBiology.Repository
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
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
        Dim index As String = $"{DIR}/{Genbank.IndexJournal}"

        Call "".SaveTo(index)  ' 清除原始文件的所有数据，重新创建索引文件

        Using DbWriter As New DocumentStream.Linq.WriteStream(Of GenbankIndex)(index)  ' 打开数据库的文件句柄

            For Each table As String In ls - l - lsDIR - r <= DIR
                For Each gbk As String In ls - l - r - wildcards("*.gbk", "*.gb") <= table
                    For Each gb As GBFF.File In GBFF.File.LoadDatabase(gbk)  ' 使用迭代器读取数据库文件

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

''' <summary>
''' NCBI genbank repository system
''' </summary>
Public Class Genbank : Inherits ClassObject
    Implements IRepository(Of String, GenbankIndex)

    Public Const IndexJournal As String = "ncbi_genbank.csv"

    Public ReadOnly Property DIR As String

    ReadOnly __indexHash As Dictionary(Of GenbankIndex)

    Sub New(DIR As String)
        Dim index As String = $"{DIR}/{IndexJournal}"

        Me.DIR = DIR
        Me.__indexHash = index.LoadCsv(Of GenbankIndex).ToDictionary
    End Sub

#Region "Implements IRepository(Of String, GenbankIndex)"

    Public Function Exists(key As String) As Boolean Implements IRepositoryRead(Of String, GenbankIndex).Exists
        Return __indexHash.ContainsKey(key)
    End Function

    Public Function GetByKey(key As String) As GenbankIndex Implements IRepositoryRead(Of String, GenbankIndex).GetByKey
        If __indexHash.ContainsKey(key) Then
            Return __indexHash(key)
        Else
            Return Nothing
        End If
    End Function

    Public Function GetWhere(clause As Func(Of GenbankIndex, Boolean)) As IReadOnlyDictionary(Of String, GenbankIndex) Implements IRepositoryRead(Of String, GenbankIndex).GetWhere
        Dim LQuery As IEnumerable(Of GenbankIndex) =
            From x As GenbankIndex
            In __indexHash.Values
            Where True = clause(x)
            Select x
        Return LQuery.ToDictionary(Function(x) x.AccId)
    End Function

    Public Function GetAll() As IReadOnlyDictionary(Of String, GenbankIndex) Implements IRepositoryRead(Of String, GenbankIndex).GetAll
        Return New Dictionary(Of String, GenbankIndex)(__indexHash)
    End Function

    Public Sub Delete(key As String) Implements IRepositoryWrite(Of String, GenbankIndex).Delete
        Throw New ReadOnlyException(ReadOnlyRepository)
    End Sub

    Public Sub AddOrUpdate(entity As GenbankIndex, key As String) Implements IRepositoryWrite(Of String, GenbankIndex).AddOrUpdate
        Throw New ReadOnlyException(ReadOnlyRepository)
    End Sub

    Const ReadOnlyRepository As String = "This repository is readonly, please updates from method: Installer.Install"

    Public Function AddNew(entity As GenbankIndex) As String Implements IRepositoryWrite(Of String, GenbankIndex).AddNew
        Throw New ReadOnlyException(ReadOnlyRepository)
    End Function
#End Region
End Class

Public Class GenbankIndex : Implements IKeyedEntity(Of String), sIdEnumerable

    ''' <summary>
    ''' DIR name
    ''' </summary>
    ''' <returns></returns>
    Public Property DIR As String
    Public Property genome As String
    ''' <summary>
    ''' locus_tag, 索引文件的表主键
    ''' </summary>
    ''' <returns></returns>
    Public Property AccId As String Implements IKeyedEntity(Of String).Key, sIdEnumerable.Identifier
    Public Property definition As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
