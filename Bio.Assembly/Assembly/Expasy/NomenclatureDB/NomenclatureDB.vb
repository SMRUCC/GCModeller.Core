﻿Imports System.Text.RegularExpressions
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic
Imports System.Xml.Serialization

Namespace Assembly.Expasy.Database

    ''' <summary>
    ''' ENZYME nomenclature database.(Expasy数据库之中的enzyme.dat注释文件)
    ''' </summary>
    ''' <remarks></remarks>
    <XmlRoot("ENZYME nomenclature database", Namespace:="http://enzyme.expasy.org/")>
    Public Class NomenclatureDB : Inherits Microsoft.VisualBasic.ComponentModel.ITextFile

        Dim __defHash As Dictionary(Of String, Enzyme)

        <XmlElement("Enzyme", Namespace:="http://code.google.com/p/genome-in-code/expasy")>
        Public Property Enzymes As Enzyme()

        <XmlAttribute("Release", Namespace:="EXPASY/RELEASE_VERSION")>
        Public Property Release As Date

        <XmlText>
        Public Property Copyright As String

        ''' <summary>
        ''' 当目标编号不存在于Expasy数据库之中的时候会返回空值
        ''' </summary>
        ''' <param name="EC"></param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Default Public ReadOnly Property DataItem(EC As String) As Enzyme
            Get
                If Not __defHash.ContainsKey(EC) Then
                    Return Nothing
                End If
                Return __defHash(EC)
            End Get
        End Property

        ''' <summary>
        ''' 从Expasy数据库之中导出某一种酶分类编号的所有的Uniprot数据库之中的蛋白质编号
        ''' </summary>
        ''' <param name="ECNumber"></param>
        ''' <param name="Strict">是否严格匹配酶分类编号，假若严格匹配的话，则必须要字符串完全相等才会有输出结果，假若不严格匹配，理论上假若所输入的酶分类标号有通配符，即相连着的两个".."符号存在的话，则所有该大分类之下的所有的蛋白酶分子的编号都会被输出</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSwissProtEntries(ECNumber As String, Optional Strict As Boolean = False) As String()
            If (From ch As Char In ECNumber Where ch = "."c Select 1).Count = 2 Then
                If Strict Then
                    Return New String() {}
                End If

                Dim LQuery = (From item In Enzymes Where InStr(item.Identification, ECNumber) = 1 Select item.SwissProt).ToArray
                Return LQuery.MatrixToVector.Distinct.ToArray
            Else
                Return (From id As String In Enzymes.GetItem(ECNumber).SwissProt Where Not String.IsNullOrEmpty(id) Select id Distinct).ToArray
            End If
        End Function

        Public Function TryExportUniprotFasta(data As Generic.IEnumerable(Of LANS.SystemsBiology.Assembly.Uniprot.UniprotFasta)) As LANS.SystemsBiology.SequenceModel.FASTA.FastaFile
            Dim UniprotIDs As String() = (From item In Me.Enzymes Select item.SwissProt).ToArray.MatrixToList.Distinct.ToArray
            Dim LQuery As LANS.SystemsBiology.SequenceModel.FASTA.FastaToken() = (From item In data.AsParallel Where Array.IndexOf(UniprotIDs, item.UniprotID) Select item).ToArray
            Return CType(LQuery, LANS.SystemsBiology.SequenceModel.FASTA.FastaFile)
        End Function

        ''' <summary>
        ''' Load the expasy database from the text file.(从文本文件之中加载expasy数据库)
        ''' </summary>
        ''' <param name="File">File path of the expasy database file.(exapsy数据库文件的文件路径)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function CreateObject(File As String) As NomenclatureDB
            Dim strData As String() = Regex.Split(FileIO.FileSystem.ReadAllText(File).Replace("-!-", ""), "^//$", RegexOptions.Multiline)
            Dim DBDescription As String = strData.First
            Dim Enzymes As Enzyme() = (From strLine As String
                                       In strData.Skip(1).AsParallel
                                       Let strLines As String() = (From strItem As String
                                                                   In Strings.Split(strLine, vbLf)
                                                                   Let strL As String = Trim(strItem)
                                                                   Where Not String.IsNullOrEmpty(strL)
                                                                   Select strL).ToArray
                                       Where Not strLines.IsNullOrEmpty
                                       Let Enzyme = Enzyme.__createObjectFromText(strData:=strLines)
                                       Select Enzyme
                                       Order By Enzyme.Identification Ascending).ToArray
            Return New NomenclatureDB With {
                .FilePath = File,
                .Enzymes = Enzymes
            }
        End Function

        Public Sub Export(ByRef Classes As CsvExport.Enzyme(), ByRef SwissProt As CsvExport.SwissProt())
            Classes = (From item In Enzymes Select CsvExport.Enzyme.CreateObject(item)).ToArray
            Dim SwissProtList As List(Of CsvExport.SwissProt) = New List(Of CsvExport.SwissProt)
            For Each item In Enzymes
                Call SwissProtList.AddRange(CsvExport.SwissProt.CreateObjects(item))
            Next
            SwissProt = SwissProtList.ToArray
        End Sub

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace