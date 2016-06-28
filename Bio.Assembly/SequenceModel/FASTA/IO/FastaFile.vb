﻿#Region "Microsoft.VisualBasic::a94e03ded161f54cbbda7e082245932b, ..\Bio.Assembly\SequenceModel\FASTA\IO\FastaFile.vb"

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

Imports Path = System.String
Imports LANS.SystemsBiology.SequenceModel.FASTA.Reflection
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic
Imports System.IO
Imports Microsoft.VisualBasic.Language

Namespace SequenceModel.FASTA

    ''' <summary>
    ''' A FASTA file that contains multiple sequence data.
    ''' (一个包含有多条序列数据的FASTA文件)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class FastaFile : Inherits ITextFile
        Implements IDisposable
        Implements IEnumerable(Of FastaToken)
        Implements IList(Of FastaToken)

        Public Overloads Shared Widening Operator CType(File As Path) As FastaFile
            Return FastaFile.Read(File)
        End Operator

        Public Overloads Shared Widening Operator CType(lstFiles As String()) As FastaFile
            Return lstFiles.Merge(True, False)
        End Operator

        Sub New()
        End Sub

        Sub New(source As List(Of FastaToken))
            _innerList = source
        End Sub

        Sub New(source As IEnumerable(Of FastaToken), path As String)
            Call Me.New(source.ToList)
            FilePath = path
        End Sub

        ''' <summary>
        ''' 构造函数会自动移除空值对象
        ''' </summary>
        ''' <param name="data"></param>
        Sub New(data As IEnumerable(Of FastaToken))
            _innerList =
                LinqAPI.MakeList(Of FastaToken) <= From fa As FastaToken
                                                   In data
                                                   Where Not fa Is Nothing
                                                   Select fa
        End Sub

        Sub New(fa As FASTA.FastaToken)
            Call Me.New({fa})
        End Sub

        Sub New(fa As Generic.IEnumerable(Of FASTA.IAbstractFastaToken))
            Call Me.New(fa.ToArray(Function(x) New FastaToken(x)))
        End Sub

        Sub New(fa As Generic.IEnumerable(Of FASTA.I_FastaProvider))
            Call Me.New(fa.ToArray(Function(x) New FastaToken(x)))
        End Sub

        Sub New(path As String)
            FilePath = path
            _innerList = DocParser(FileIO.FileSystem.ReadAllText(path))
        End Sub

        Protected Friend Overridable Property _innerList As List(Of FastaToken) = New List(Of FastaToken)

        Public Iterator Function AsKSource() As IEnumerable(Of KSeq)
            For Each fa As FastaToken In _innerList
                Yield New KSeq With {
                    .Name = fa.Title,
                    .Seq = fa.SequenceData.ToCharArray
                }
            Next
        End Function

        ''' <summary>
        ''' 本FASTA数据文件对象的文件位置
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property FilePath As String
            Get
                Return MyBase.FilePath
            End Get
            Set(value As String)
                MyBase.FilePath = value
            End Set
        End Property

        Public Function AddRange(FastaCol As IEnumerable(Of FastaToken)) As Long
            Call __innerList.AddRange(FastaCol)
            Return __innerList.LongCount
        End Function

        ''' <summary>
        ''' Get a new fasta2 object which is been clear the duplicated records in the collection.
        ''' (获取去除集合中的重复的记录新列表，原有列表中数据未被改变)
        ''' </summary>
        ''' <remarks></remarks>
        Public Function Distinct() As FastaFile
            Dim List = CType((From fasta As FastaToken
                              In Me.__innerList
                              Select fasta
                              Order By fasta.Title Ascending).ToList, List(Of FastaToken))
            For i As Integer = 1 To List.Count - 1
                If String.Equals(List(i).Title, List(i - 1).Title) Then
                    Call List.RemoveAt(i)
                End If

                If i = List.Count Then
                    Exit For
                End If
            Next

            Return List
        End Function

        Public Function ToUpper() As FastaFile
            For i As Integer = 0 To Me._innerList.Count - 1
                _innerList(i).SequenceData = _innerList(i).SequenceData.ToUpper
            Next
            Return New FastaFile(_innerList)
        End Function

        Public Function ToLower() As FastaFile
            For i As Integer = 0 To Me._innerList.Count - 1
                _innerList(i).SequenceData = _innerList(i).SequenceData.ToLower
            Next
            Return New FastaFile(_innerList)
        End Function

        ''' <summary>
        ''' Load the fasta file from the local filesystem.
        ''' </summary>
        ''' <param name="File"></param>
        ''' <param name="Explicit">当参数为真的时候，目标文件不存在则会抛出错误，反之则会返回一个空文件</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Read(File As Path, Optional Explicit As Boolean = True) As FastaFile
            If Not File.FileExists Then
                If Explicit Then
                    Throw New Exception($"File ""{File.ToFileURL}"" is not exists on the file system!")
                Else
                    Return Nothing
                End If
            End If

            Dim FastaReader As New FastaFile(DocParser(IO.File.ReadAllLines(File))) With {
                .FilePath = FileIO.FileSystem.GetFileInfo(File).FullName
            }
            Return FastaReader
        End Function

        ''' <summary>
        ''' 加载一个FASTA序列文件并检查其中是否全部为核酸序列数据，假若不是，则会将该序列移除
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function LoadNucleotideData(path As String, Optional Explicit As Boolean = False) As FastaFile
            Dim Data As FastaFile = FastaFile.Read(path)

            If Data.IsNullOrEmpty Then
NULL_DATA:      Call $"""{path.ToFileURL}"" fasta data isnull or empty!".__DEBUG_ECHO
                Return Nothing
            End If

            For Each Sequence As FastaToken In Data._innerList
                Sequence.SequenceData = Sequence.SequenceData.ToUpper.Replace("N", "-")
            Next

            Dim LQuery = (From fa As FastaToken
                          In Data._innerList
                          Where Not fa.IsProtSource
                          Select fa).ToArray
            If Explicit Then
                LQuery = (From fa As FastaToken
                          In LQuery
                          Where Not fa.HaveGaps
                          Select fa).ToArray
            End If

            If LQuery.IsNullOrEmpty Then
                GoTo NULL_DATA
            End If

            Data = New FastaFile(LQuery, path)

            Return Data
        End Function

        Public Shared Function DocParser(TokenLines As String()) As List(Of FastaToken)
            Dim faToken As List(Of String) = New List(Of String)
            Dim faList As New List(Of FastaToken)

            For Each Line As String In TokenLines
                If String.IsNullOrEmpty(Line) Then
                    Continue For
                ElseIf Line.Chars(Scan0) = ">"c Then  'New FASTA Object
                    Call faList.Add(FastaToken.ParseFromStream(faToken))
                    Call faToken.Clear()
                End If

                Call faToken.Add(Line)
            Next

            If faList.Count > 0 Then
                Call faList.RemoveAt(Scan0)
            End If

            Call faList.Add(FastaToken.ParseFromStream(faToken))

            Return faList
        End Function

        Public Shared Function DocParser(doc As String) As List(Of FastaToken)
            Dim TokenLines As String() = doc.lTokens
            Return DocParser(TokenLines)
        End Function

        ''' <summary>
        ''' Try parsing a fasta file object from the text file content <paramref name="doc"></paramref>
        ''' </summary>
        ''' <param name="doc">The file data content in the fasta file, not the path of the fasta file!</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function ParseDocument(doc As String) As FastaFile
            Dim Fasta As New FastaFile(DocParser(doc))
            Return Fasta
        End Function

        Public Sub Split(saveDIR As Path)
            Call FileIO.FileSystem.CreateDirectory(saveDIR)

            Dim Index As Integer

            For Each FASTA As SequenceModel.FASTA.FastaToken In __innerList
                Index += 1
                FASTA.SaveTo(String.Format("{0}/{1}.fasta", saveDIR, Index))
            Next
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="KeyWord">A key string that to search in this fasta file.</param>
        ''' <param name="CaseSensitive">For text compaired method that not case sensitive, otherwise in the method od binary than case sensitive.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function QueryAny(KeyWord As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As FastaFile
            Dim LQuery As IEnumerable(Of FastaToken) = From Fasta As FastaToken
                                                       In __innerList.AsParallel
                                                       Where Find(Fasta.Attributes, KeyWord, CaseSensitive)
                                                       Select Fasta '
            Return New FastaFile With {
                .__innerList = LQuery.ToList
            }
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="KeyWord">A key string that to search in this fasta file.</param>
        ''' <param name="CaseSensitive">For text compaired method that not case sensitive, otherwise in the method od binary than case sensitive.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Query(KeyWord As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As FastaToken
            Dim LQuery As IEnumerable(Of FastaToken) = From FASTA As FastaToken
                                                       In __innerList.AsParallel
                                                       Where Find(FASTA.Attributes, KeyWord, CaseSensitive)
                                                       Select FASTA '
            Return LQuery.FirstOrDefault
        End Function

        Public Function Query(Keyword As String, Index As Integer, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As FastaToken()
            Dim list As IEnumerable(Of FastaToken) =
                From fsa As FastaToken
                In __innerList
                Where fsa.Attributes.Count - 1 >= Index
                Select fsa
            Dim LQuery As FastaToken() =
                LinqAPI.Exec(Of FastaToken) <= From fsa As FastaToken
                                               In list
                                               Where InStr(fsa.Attributes(Index), Keyword, CaseSensitive) > 0
                                               Select fsa '
            Return LQuery
        End Function

        Private Shared Function Find(AttributeList As String(), KeyWord As String, CaseSensitive As CompareMethod) As Boolean
            For i As Integer = 0 To AttributeList.Length - 1
                If InStr(AttributeList(i), KeyWord, CaseSensitive) Then
                    Return True
                End If
            Next

            Return False
        End Function

        Public Function Take(KeyWordList As List(Of String), Optional CaseSensitive As CompareMethod = CompareMethod.Text) As FastaFile
            Dim result As New List(Of FastaToken)

            For Each keyWord As String In KeyWordList
                result += From FASTA As FastaToken
                          In __innerList
                          Where InStr(FASTA.Title, keyWord, CaseSensitive)
                          Select FASTA '
            Next

            Return New FastaFile With {
                .__innerList = result
            }
        End Function

        Public Function Takes(prediction As System.Func(Of FastaToken, Boolean)) As FastaFile
            Dim LQuery As IEnumerable(Of FastaToken) = From fa As FastaToken
                                                       In Me._innerList
                                                       Where True = prediction(fa)
                                                       Select fa
            Return New FastaFile(LQuery)
        End Function

        ''' <summary>
        ''' Save the fasta file into the local filesystem.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding">不同的程序会对这个由要求，例如meme程序在linux系统之中要求序列文件为unicode编码格式而windows版本的meme程序则要求ascii格式</param>
        ''' <remarks></remarks>
        Public Overrides Function Save(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean
            Try
                Return Save(60, Path, encoding)
            Catch ex As Exception
                Throw New Exception(Path, ex)
            End Try
        End Function

        ''' <summary>
        ''' Save the fasta file into the local filesystem.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding">不同的程序会对这个由要求，例如meme程序在linux系统之中要求序列文件为unicode编码格式而windows版本的meme程序则要求ascii格式</param>
        ''' <remarks></remarks>
        Public Overloads Function Save(LineBreak As Integer, Optional Path As String = "", Optional encoding As Encodings = Encodings.ASCII) As Boolean
            Try
                Return Save(LineBreak, Path, encoding.GetEncodings)
            Catch ex As Exception
                Throw New Exception(Path, ex)
            End Try
        End Function

        ''' <summary>
        ''' Save the fasta file into the local filesystem.
        ''' </summary>
        ''' <param name="Path"></param>
        ''' <param name="encoding">不同的程序会对这个由要求，例如meme程序在linux系统之中要求序列文件为unicode编码格式而windows版本的meme程序则要求ascii格式</param>
        ''' <remarks></remarks>
        Public Overloads Function Save(LineBreak As Integer, Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean
            If encoding Is Nothing Then
                encoding = Encoding.ASCII
            End If

            Path = getPath(Path)

            Dim sBuilder As StringBuilder = New StringBuilder(10 * 1024)
            Dim DocumentNodes = (From FastaObject As FastaToken
                                 In _innerList.AsParallel
                                 Let NodeText As String = FastaObject.GenerateDocument(LineBreak:=LineBreak)
                                 Select NodeText,
                                     Len = NodeText.Length).ToArray
            Dim MaxSize As Double = (From n In DocumentNodes Select CDbl(n.Len)).Sum

            If MaxSize > sBuilder.MaxCapacity Then
                Return __saveUltraLargeSize((From node In DocumentNodes.AsParallel Select node.NodeText), Path, encoding, MaxSize)
            Else
                For Each nodeText In DocumentNodes
                    sBuilder.AppendLine(nodeText.NodeText)
                Next
#If debug Then
                Call Console.WriteLine($"[DEBUG  {Now.ToString}] The saved fasta file text encoding is {encoding.ToString}.")
#End If
                Call sBuilder.Replace(vbCr, "")
                Call FileIO.FileSystem.CreateDirectory(FileIO.FileSystem.GetParentPath(Path))
                Call FileIO.FileSystem.WriteAllText(Path, sBuilder.ToString, append:=False, encoding:=encoding)

                Return True
            End If
        End Function

        Private Function __saveUltraLargeSize(DocumentNodes As IEnumerable(Of String), Path As String, encoding As Encoding, MaxSize As Double) As Boolean
            Try
                Call FileIO.FileSystem.CreateDirectory(FileIO.FileSystem.GetParentPath(Path))
                Call FileIO.FileSystem.DeleteFile(Path, FileIO.UIOption.OnlyErrorDialogs, FileIO.RecycleOption.SendToRecycleBin)
            Catch ex As Exception
                ex = New Exception(Path, ex)
                Call App.LogException(ex)
            End Try

            Dim file As StreamWriter = New StreamWriter(New FileStream(Path, FileMode.OpenOrCreate), encoding)
            Dim MAT As String()() = TextPartition(DocumentNodes)

            Call $"UltralargeSize fasta file partition count is {MAT.Length}".__DEBUG_ECHO

            For Each NodeBuilder As String In (From VEC In MAT.AsParallel Select __createNode(VEC))
                Call NodeBuilder.Replace(vbCr, "")
                Call file.Write(NodeBuilder & vbCrLf)
            Next

            Return True
        End Function

        Private Shared Function __createNode(Nodes As String()) As String
            Dim sBuilder As StringBuilder = New StringBuilder(100 * 1024)

            For Each Node As String In Nodes
                Call sBuilder.AppendLine(Node)
            Next

            Return sBuilder.ToString
        End Function

        ''' <summary>
        ''' Clear the sequence data in this fasta file.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub FlushData()
            Call _innerList.Clear()
        End Sub

        ''' <summary>
        ''' 在写入数据到文本文件之前需要将目标对象转换为文本字符串
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Generate() As String
            Dim sb As New StringBuilder(10 * 1024)
            Dim s As String

            For Each fa As FastaToken In __innerList
                s = fa.GenerateDocument(LineBreak:=60)
                Call sb.AppendLine(s)
            Next
            Return sb.ToString
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="file">
        ''' The target FASTA file that to append this FASTA sequences.(将要拓展这些FASTA序列的目标FASTA文件)
        ''' </param>
        ''' <remarks></remarks>
        Public Sub AppendToFile(file As Path)
            Call FileIO.FileSystem.WriteAllText(file, Generate, append:=True)
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("{0}; [{1} records]", FilePath, Count)
        End Function

        Public Shared Shadows Widening Operator CType(source As FastaToken()) As FastaFile
            Return New FastaFile With {
                .__innerList = source.ToList
            }
        End Operator

        Public Shared Shadows Widening Operator CType(source As List(Of FastaToken)) As FastaFile
            Return New FastaFile With {
                .__innerList = source
            }
        End Operator

        Public Shared Shadows Widening Operator CType(fa As FastaToken) As FastaFile
            Return New FastaFile With {
                .__innerList = New List(Of FastaToken) From {fa}
            }
        End Operator

        Public Shadows Iterator Function GetEnumerator() As IEnumerator(Of FastaToken) Implements IEnumerable(Of FastaToken).GetEnumerator
            For i As Integer = 0 To __innerList.Count - 1
                Yield __innerList(i)
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        ''' <summary>
        ''' 使用正则表达式来搜索在序列中含有特定模式的序列对象
        ''' </summary>
        ''' <param name="regxText"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Match(regxText As String, Optional options As RegexOptions = RegexOptions.Singleline) As FastaToken()
            Dim LQuery As FastaToken() =
                LinqAPI.Exec(Of FastaToken) <= From fasta As FastaToken
                                               In Me.__innerList
                                               Where Regex.Match(fasta.SequenceData.ToUpper, regxText, options).Success
                                               Select fasta
            Return LQuery
        End Function

#Region "Implementation of IList(Of SequenceModel.FASTA.FASTA) interface."
        Public Sub Add(item As FastaToken) Implements ICollection(Of FastaToken).Add
            Call __innerList.Add(item)
        End Sub

        Public Sub Clear() Implements ICollection(Of FastaToken).Clear
            Call _innerList.Clear()
        End Sub

        Public Function Contains(item As FastaToken) As Boolean Implements ICollection(Of FastaToken).Contains
            Return _innerList.Contains(item)
        End Function

        Public Overloads Sub CopyTo(array() As FastaToken, arrayIndex As Integer) Implements ICollection(Of FastaToken).CopyTo
            Call _innerList.CopyTo(array, arrayIndex)
        End Sub

        Public ReadOnly Property NumberOfFasta As Integer Implements ICollection(Of FastaToken).Count
            Get
                Return _innerList.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of FastaToken).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As FastaToken) As Boolean Implements ICollection(Of FastaToken).Remove
            Return _innerList.Remove(item)
        End Function

        Public Function IndexOf(item As FastaToken) As Integer Implements IList(Of FastaToken).IndexOf
            Return _innerList.IndexOf(item)
        End Function

        Public Sub Insert(index As Integer, item As FastaToken) Implements IList(Of FastaToken).Insert
            Call _innerList.Insert(index, item)
        End Sub

        Default Public Property Item(index As Integer) As FastaToken Implements IList(Of FastaToken).Item
            Get
                Return _innerList(index)
            End Get
            Set(value As FastaToken)
                _innerList(index) = value
            End Set
        End Property

        Public Sub RemoveAt(index As Integer) Implements IList(Of FastaToken).RemoveAt
            Call _innerList.RemoveAt(index)
        End Sub
#End Region

        ''' <summary>
        ''' 可以使用这个方法来作为通用的继承与fasta对象的集合的序列的数据保存工作
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data"></param>
        ''' <param name="path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        Public Shared Function SaveData(Of T As IAbstractFastaToken)(data As IEnumerable(Of T), path As String, Optional encoding As Encoding = Nothing) As Boolean
            Dim LQuery = (From fa As T
                          In data
                          Select FastaToken.GenerateDocumentText(fa) & vbCrLf).ToArray

            If encoding Is Nothing Then
                encoding = Encoding.ASCII
            End If

            Return LQuery.FlushAllLines(path, encoding)
        End Function

        Public Overloads Shared Operator +(FASTA As FastaFile, fa As FastaToken) As FastaFile
            Call FASTA.Add(fa)
            Return FASTA
        End Operator

        Public Overloads Shared Operator +(FASTA As FastaFile, source As IEnumerable(Of FastaToken)) As FastaFile
            Call FASTA.AddRange(source)
            Return FASTA
        End Operator

        Public Overloads Shared Operator +(FASTA As FastaFile, source As IEnumerable(Of IEnumerable(Of FastaToken))) As FastaFile
            Return FASTA + source.MatrixAsIterator
        End Operator

        Public Shared Operator >(source As FastaFile, path As String) As Boolean
            Return source.Save(path, Encoding.ASCII)
        End Operator

        Public Shared Operator <(source As FastaFile, path As String) As Boolean
            Throw New NotSupportedException
        End Operator
    End Class
End Namespace
