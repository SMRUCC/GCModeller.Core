﻿Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic

Namespace Assembly.KEGG.DBGET.BriteHEntry

    ''' <summary>
    ''' KEGG里面的模块的入口点的定义数据
    ''' </summary>
    Public Class [Module] : Implements IReadOnlyId

        ''' <summary>
        ''' C
        ''' </summary>
        ''' <returns></returns>
        Public Property SubCategory As String
        ''' <summary>
        ''' A
        ''' </summary>
        ''' <returns></returns>
        Public Property [Class] As String
        ''' <summary>
        ''' B
        ''' </summary>
        ''' <returns></returns>
        Public Property Category As String
        ''' <summary>
        ''' KO
        ''' </summary>
        ''' <returns></returns>
        Public Property Entry As ComponentModel.KeyValuePair

        Public ReadOnly Property EntryId As String Implements IReadOnlyId.Identity
            Get
                Return Entry.Key
            End Get
        End Property

        ''' <summary>
        ''' 从资源文件之中加载模块的入口点的定义数据
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function LoadFromResource() As [Module]()
            Dim Model = BriteHText.Load_ko00002
            Return Build(Model)
        End Function

        ''' <summary>
        ''' 从内部资源之中加载数据然后生成字典返回
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetDictionary() As Dictionary(Of String, [Module])
            Dim res As [Module]() = LoadFromResource()
            Return GetDictionary(res)
        End Function

        Public Shared Function GetDictionary(res As String) As Dictionary(Of String, [Module])
            Return GetDictionary(LoadFile(res))
        End Function

        Public Shared Function GetDictionary(res As IEnumerable(Of [Module])) As Dictionary(Of String, [Module])
            Dim dul = (From x In res Select x Group x By x.Entry.Key Into Group).ToArray
            Dim orders = (From x In dul
                          Select x.Key,
                              ls = x.Group.ToArray
                          Order By ls.Length Descending).ToArray
            Dim dict As Dictionary(Of String, [Module]) =
                orders.ToDictionary(Function(x) x.Key,
                                    Function(x) x.ls.First)
            Return dict
        End Function

        Private Shared Function Build(Model As BriteHText) As [Module]()
            Dim ModuleList As List(Of [Module]) = New List(Of [Module])

            For Each item In Model.CategoryItems
                For Each CategoryItem In item.CategoryItems
                    For Each SubCategory As BriteHText In CategoryItem.CategoryItems
                        Dim mods As [Module]() = (From ModuleEntry As BriteHText
                                                  In SubCategory.CategoryItems
                                                  Let Entry = New ComponentModel.KeyValuePair With {
                                                      .Key = ModuleEntry.EntryId,
                                                      .Value = ModuleEntry.Description
                                                  }
                                                  Select New [Module] With {
                                                      .Entry = Entry,
                                                      .Class = item.ClassLabel,
                                                      .Category = CategoryItem.ClassLabel,
                                                      .SubCategory = SubCategory.ClassLabel}).ToArray
                        Call ModuleList.AddRange(mods)
                    Next
                Next
            Next

            Return ModuleList.ToArray
        End Function

        Public Shared Function LoadFile(path As String) As [Module]()
            Return Build(Model:=BriteHText.Load(strData:=FileIO.FileSystem.ReadAllText(path)))
        End Function

        ''' <summary>
        ''' 会按照分类来组织文件夹结构
        ''' </summary>
        ''' <param name="SpeciesCode"></param>
        ''' <param name="Export"></param>
        ''' <returns>返回成功下载的代谢途径的数目</returns>
        ''' <remarks></remarks>
        Public Shared Function DownloadModules(SpeciesCode As String, Export As String, Optional BriefFile As String = "") As Integer
            Dim BriefEntries As [Module]() =
                If(String.IsNullOrEmpty(BriefFile),
                LoadFromResource(),
                LoadFile(BriefFile))

            For Each Entry As [Module] In BriefEntries
                Call __downloadModules(SpeciesCode, Export, Entry)
            Next

            Return 0
        End Function

        Private Shared Sub __downloadModules(sp As String, EXPORT As String, Entry As [Module])
            Dim EntryId As String = String.Format("{0}{1}", sp, Entry.Entry.Key)
            Dim SaveToDir As String = $"{EXPORT}/{TrimPath(Entry.Class)}/{TrimPath(Entry.Category)}/{TrimPath(Entry.SubCategory)}"
            Dim XmlFile As String = String.Format("{0}/{1}.xml", SaveToDir, EntryId)

            If FileIO.FileSystem.FileExists(XmlFile) Then
                If FileIO.FileSystem.GetFileInfo(XmlFile).Length > 0 Then
                    Return
                End If
            End If

            Dim url As String = $"http://www.genome.jp/dbget-bin/www_bget?md:{sp}_{Entry.Entry.Key}"
            Dim [Module] As bGetObject.Module = KEGG.DBGET.bGetObject.Module.Download(url)

            If [Module] Is Nothing Then
                Call $"[{sp}] {Entry.ToString} is not exists in KEGG!".__DEBUG_ECHO
            Else
                Call [Module].GetXml.SaveTo(XmlFile)
            End If
        End Sub

        ''' <summary>
        ''' 防止文件夹的名称过长而出错
        ''' </summary>
        ''' <param name="pathToken"></param>
        ''' <returns></returns>
        Public Shared Function TrimPath(pathToken As String) As String
            If Len(pathToken) > 56 Then
                pathToken = Mid(pathToken, 1, 50) & "~"
            End If
            Return pathToken.NormalizePathString(False)
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("[{0}]  {1}", String.Join("/", [Class], Category, SubCategory), Entry.ToString)
        End Function
    End Class
End Namespace