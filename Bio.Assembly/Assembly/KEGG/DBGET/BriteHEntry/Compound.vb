﻿#Region "Microsoft.VisualBasic::1e7a4ec9aeb76a66f4fb84a1ac40d7bf, ..\GCModeller\core\Bio.Assembly\Assembly\KEGG\DBGET\BriteHEntry\Compound.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
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

Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Terminal
Imports Microsoft.VisualBasic.Terminal.ProgressBar
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.genomics.Assembly.KEGG.DBGET.bGetObject

Namespace Assembly.KEGG.DBGET.BriteHEntry

    ''' <summary>
    ''' Compounds with Biological Roles.(在这里面包含有KEGG compounds的下载API)
    ''' </summary>
    ''' <remarks>
    ''' Compounds
    ''' 
    '''  br08001  Compounds with biological roles
    '''  br08002  Lipids
    '''  br08003  Phytochemical compounds
    '''  br08005  Bioactive peptides
    '''  br08006  Endocrine disrupting compounds
    '''  br08007  Pesticides
    '''  br08008  Carcinogens
    '''  br08009  Natural toxins
    '''  br08010  Target-based classification of compounds
    ''' </remarks>
    Public Class Compound

        Public Property [Class] As String
        Public Property Category As String
        Public Property SubCategory As String
        Public Property Order As String
        Public Property Entry As KeyValuePair

        Private Shared Function Build(Model As BriteHText) As Compound()
            Dim list As New List(Of Compound)

            Select Case Model.Degree

                Case "C"c
                    For Each [Class] As BriteHText In Model.CategoryItems

                        If [Class].CategoryItems.IsNullOrEmpty Then
                            Continue For
                        End If

                        For Each category As BriteHText In [Class].CategoryItems
                            If category.CategoryItems.IsNullOrEmpty Then
                                Continue For
                            End If

                            list += From htext As BriteHText
                                    In category.CategoryItems
                                    Select New Compound With {
                                        .Class = [Class].ClassLabel,
                                        .Category = category.ClassLabel,
                                        .Entry = New KeyValuePair With {
                                            .Key = htext.EntryId,
                                            .Value = htext.Description
                                        }
                                    }
                        Next
                    Next

                Case "D"c
                    For Each [Class] As BriteHText In Model.CategoryItems

                        If [Class].CategoryItems.IsNullOrEmpty Then
                            Continue For
                        End If

                        For Each category As BriteHText In [Class].CategoryItems
                            If category.CategoryItems.IsNullOrEmpty Then
                                Continue For
                            End If
                            For Each subCategory As BriteHText In category.CategoryItems
                                If subCategory.CategoryItems.IsNullOrEmpty Then
                                    Continue For
                                End If

                                list += From br As BriteHText
                                        In subCategory.CategoryItems
                                        Select New Compound With {
                                            .Class = [Class].ClassLabel,
                                            .Category = category.ClassLabel,
                                            .SubCategory = subCategory.ClassLabel,
                                            .Entry = New KeyValuePair With {
                                                .Key = br.EntryId,
                                                .Value = br.Description
                                            }
                                        }
                            Next
                        Next
                    Next

                Case "E"c
                    For Each [class] As BriteHText In Model.CategoryItems
                        If [class].CategoryItems.IsNullOrEmpty Then
                            Continue For
                        End If
                        For Each category As BriteHText In [class].CategoryItems
                            If category.CategoryItems.IsNullOrEmpty Then
                                Continue For
                            End If
                            For Each subCategory As BriteHText In category.CategoryItems
                                If subCategory.CategoryItems.IsNullOrEmpty Then
                                    Continue For
                                End If
                                For Each order As BriteHText In subCategory.CategoryItems
                                    If order.CategoryItems.IsNullOrEmpty Then
                                        Continue For
                                    End If

                                    list += From br As BriteHText
                                            In order.CategoryItems
                                            Select New Compound With {
                                                .Class = [class].ClassLabel,
                                                .Category = category.ClassLabel,
                                                .SubCategory = subCategory.ClassLabel,
                                                .Order = order.ClassLabel,
                                                .Entry = New KeyValuePair With {
                                                    .Key = br.EntryId,
                                                    .Value = br.Description
                                                }
                                            }

                                Next
                            Next
                        Next
                    Next
            End Select

            Return list.ToArray
        End Function

        ''' <summary>
        ''' KEGG BRITE contains a classification of lipids
        ''' 
        ''' > http://www.kegg.jp/kegg-bin/get_htext?br08002.keg
        ''' </summary>
        ''' <returns></returns>
        Public Function Lipids() As Compound()
            Dim satellite As New ResourcesSatellite(GetType(LICENSE))
            Return Compound.Build(BriteHText.Load(satellite.GetString(cpd_br08002)))
        End Function

        ''' <summary>
        ''' Removes all of the download failured result from the workspace
        ''' </summary>
        ''' <param name="DIR$"></param>
        Public Shared Sub WorkspaceCleanup(DIR$)
            On Error Resume Next

            For Each XML As String In ls - l - r - "*.XML" <= DIR
                Dim name$ = XML.BaseName
                Dim compound As bGetObject.Compound

                If name.First = "C"c Then
                    compound = XML.LoadXml(Of bGetObject.Compound)
                Else
                    compound = XML.LoadXml(Of bGetObject.Glycan)
                End If

                If compound.IsNullOrEmpty Then

                    ' 这个对象是空的，需要从工作区清除
                    Call FileIO.FileSystem.DeleteFile(XML)
                    Call FileIO.FileSystem.DeleteFile(XML.TrimSuffix & ".KCF")
                    Call FileIO.FileSystem.DeleteFile(XML.TrimSuffix & ".gif")

                    cat(".")
                End If
            Next
        End Sub

#Region "Internal resource ID"

        ''' <summary>
        ''' ``br08001``  Compounds with biological roles
        ''' </summary>
        Const cpd_br08001 = "br08001"
        ''' <summary>
        ''' ``br08002``  Lipids
        ''' </summary>
        Const cpd_br08002 = "br08002"
        ''' <summary>
        ''' ``br08003``  Phytochemical compounds
        ''' </summary>
        Const cpd_br08003 = "br08003"
        ''' <summary>
        ''' ``br08005``  Bioactive peptides
        ''' </summary>
        Const cpd_br08005 = "br08005"
        ''' <summary>
        ''' ``br08006``  Endocrine disrupting compounds
        ''' </summary>
        Const cpd_br08006 = "br08006"
        ''' <summary>
        ''' ``br08007``  Pesticides
        ''' </summary>
        Const cpd_br08007 = "br08007"
        ''' <summary>
        ''' ``br08008``  Carcinogens
        ''' </summary>
        Const cpd_br08008 = "br08008"
        ''' <summary>
        ''' ``br08009``  Natural toxins
        ''' </summary>
        Const cpd_br08009 = "br08009"
        ''' <summary>
        ''' ``br08010``  Target-based classification of compounds
        ''' </summary>
        Const cpd_br08010 = "br08010"

#End Region

        ''' <summary>
        ''' 请注意，这个函数只能够下载包含有分类信息的化合物，假若代谢物还没有分类信息的话，则无法利用这个函数进行下载
        ''' 
        ''' + ``br08001``  Compounds with biological roles
        ''' + ``br08002``  Lipids
        ''' + ``br08003``  Phytochemical compounds
        ''' + ``br08005``  Bioactive peptides
        ''' + ``br08006``  Endocrine disrupting compounds
        ''' + ``br08007``  Pesticides
        ''' + ``br08008``  Carcinogens
        ''' + ``br08009``  Natural toxins
        ''' + ``br08010``  Target-based classification of compounds
        ''' </summary>
        ''' <param name="EXPORT"></param>
        ''' <param name="DirectoryOrganized"></param>
        ''' <param name="forceUpdate">是否需要API对已经存在的数据进行强制更新？</param>
        ''' <param name="structInfo">是否同时也下载分子结构信息？</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function DownloadFromResource(EXPORT$,
                                                    Optional DirectoryOrganized As Boolean = True,
                                                    Optional forceUpdate As Boolean = False,
                                                    Optional structInfo As Boolean = False,
                                                    Optional maxID% = 25000) As String()

            Dim satellite As New ResourcesSatellite(GetType(LICENSE))
            Dim resource = {
                New NamedValue(Of Compound())("Compounds with biological roles", Build(BriteHText.Load(satellite.GetString(cpd_br08001)))),
                New NamedValue(Of Compound())("Lipids", Build(BriteHText.Load(satellite.GetString(cpd_br08002)))),
                New NamedValue(Of Compound())("Phytochemical compounds", Build(BriteHText.Load(satellite.GetString(cpd_br08003)))),
                New NamedValue(Of Compound())("Bioactive peptides", Build(BriteHText.Load(satellite.GetString(cpd_br08005)))),
                New NamedValue(Of Compound())("Endocrine disrupting compounds", Build(BriteHText.Load(satellite.GetString(cpd_br08006)))),
                New NamedValue(Of Compound())("Pesticides", Build(BriteHText.Load(satellite.GetString(cpd_br08007)))),
                New NamedValue(Of Compound())("Carcinogens", Build(BriteHText.Load(satellite.GetString(cpd_br08008)))),
                New NamedValue(Of Compound())("Natural toxins", Build(BriteHText.Load(satellite.GetString(cpd_br08009)))),
                New NamedValue(Of Compound())("Target-based classification of compounds", Build(BriteHText.Load(satellite.GetString(cpd_br08010))))
            }
            Dim failures As New List(Of String)

            For Each briteEntry As NamedValue(Of Compound()) In resource
                With briteEntry
                    Call __downloadsInternal(
                        .Name, .Value,
                        failures,
                        EXPORT,
                        DirectoryOrganized,
                        forceUpdate,
                        structInfo)
                End With
            Next

            Dim success As Index(Of String) = (ls - l - r - "*.XML" <= EXPORT) _
                .Select(AddressOf BaseName) _
                .Indexing

            Using progress As New ProgressBar("Downloads others", 1, CLS:=True)
                Dim tick As New ProgressProvider(maxID)
                Dim saveDIR = EXPORT & "/OtherUnknowns/"
                Dim skip As Boolean = False

                For i As Integer = 1 To maxID
                    Dim id = "C" & i.FormatZero("00000")

                    If success(id) = -1 Then
                        skip = False
                        Call Download(id, saveDIR, forceUpdate, structInfo, skip)
                    Else
                        skip = True
                    End If

                    Dim ETA$ = $"ETA={tick.ETA(progress.ElapsedMilliseconds)}"

                    If Not skip Then
                        Call Thread.Sleep(thread_sleep)
                    End If
                    Call progress.SetProgress(tick.StepProgress, details:=id & "   " & ETA)
                Next
            End Using

            Return failures
        End Function

        ''' <summary>
        ''' 将指定编号的代谢物数据下载下来然后保存在指定的文件夹之中
        ''' </summary>
        ''' <param name="entryID$"></param>
        ''' <param name="saveDIR$"></param>
        ''' <param name="forceUpdate"></param>
        ''' <param name="structInfo"></param>
        ''' <param name="skip"></param>
        ''' <returns></returns>
        Private Shared Function Download(entryID$, saveDIR$, forceUpdate As Boolean, structInfo As Boolean, ByRef skip As Boolean) As Boolean
            Dim xml$ = $"{saveDIR}/{entryID}.xml"

            If Not forceUpdate AndAlso xml.FileExists(True) Then
                skip = True
                Return True
            End If

            If entryID.First = "G"c Then
                Dim gl As Glycan = Glycan.Download(entryID)

                If gl.IsNullOrEmpty Then
                    Call $"[{entryID}] is not exists in the kegg!".Warning
                    Return False
                Else
                    Call gl.GetXml.SaveTo(xml)
                End If
            Else
                Dim cpd As bGetObject.Compound = MetabolitesDBGet.DownloadCompound(entryID)

                If cpd.IsNullOrEmpty Then
                    Call $"[{entryID}] is not exists in the kegg!".Warning
                    Return False
                Else
                    Call cpd.GetXml.SaveTo(xml)

                    If structInfo Then
                        Call cpd.DownloadKCF(xml.TrimSuffix & ".KCF")
                        Call cpd.DownloadStructureImage(xml.TrimSuffix & ".gif")
                    End If
                End If
            End If

            Return True
        End Function

        Shared ReadOnly thread_sleep% = 2000

        Shared Sub New()
            With App.GetVariable("sleep")
                If Not .StringEmpty AndAlso Val(.ref) > 1 Then
                    thread_sleep = Val(.ref)
                Else
                    thread_sleep = 2000
                End If
            End With
        End Sub

        Private Shared Sub __downloadsInternal(key$,
                                               briteEntry As Compound(),
                                               ByRef failures As List(Of String),
                                               EXPORT$,
                                               DirectoryOrganized As Boolean,
                                               forceUpdate As Boolean,
                                               structInfo As Boolean)
            ' 2017-3-12
            ' 有些entry的编号是空值？？？
            Dim keys As Compound() = briteEntry _
                .Where(Function(ID)
                           Return (Not ID Is Nothing) AndAlso
                                (Not ID.Entry Is Nothing) AndAlso
                                (Not ID.Entry.Key.StringEmpty)
                       End Function) _
                .ToArray

            Using progress As New ProgressBar("Downloads " & key, 1, CLS:=True)
                Dim tick As New ProgressProvider(keys.Length)
                Dim skip As Boolean = False

                For Each entry As Compound In keys
                    Dim EntryId As String = entry.Entry.Key
                    Dim saveDIR As String = entry.BuildPath(EXPORT, DirectoryOrganized, [class]:=key)

                    skip = False

                    If Not Download(EntryId, saveDIR, forceUpdate, structInfo, skip) Then
                        failures += EntryId
                    End If

                    Dim ETA$ = $"ETA={tick.ETA(progress.ElapsedMilliseconds)}"

                    If Not skip Then
                        Call Thread.Sleep(thread_sleep)
                    End If
                    Call progress.SetProgress(tick.StepProgress, details:=ETA)
                Next
            End Using
        End Sub

        Public Function BuildPath(EXPORT$, directoryOrganized As Boolean, Optional class$ = "") As String
            With Me
                If directoryOrganized Then
                    Dim t As New List(Of String) From {
                        EXPORT,
                        BriteHText.NormalizePath(.Class),
                        BriteHText.NormalizePath(.Category),
                        BriteHText.NormalizePath(.SubCategory)
                    }

                    If Not [class].StringEmpty Then
                        Call t.Insert(index:=1, item:=[class])
                    End If

                    Return String.Join("/", t)
                Else
                    Return EXPORT
                End If
            End With
        End Function

        ''' <summary>
        ''' 函数返回失败的编号列表
        ''' </summary>
        ''' <param name="EXPORT"></param>
        ''' <param name="BriefFile"></param>
        ''' <param name="DirectoryOrganized"></param>
        ''' <returns></returns>
        Public Shared Function DownloadCompounds(EXPORT$, briefFile$,
                                                 Optional DirectoryOrganized As Boolean = True,
                                                 Optional forceUpdate As Boolean = False) As String()

            Dim BriefEntries As Compound() = LoadFile(briefFile)
            Dim failures As New List(Of String)

            For Each entry As Compound In BriefEntries
                Dim EntryId As String = entry.Entry.Key
                Dim saveDIR As String = entry.BuildPath(EXPORT, DirectoryOrganized)
                Dim xml As String = String.Format("{0}/{1}.xml", saveDIR, EntryId)

                If Not forceUpdate AndAlso xml.FileExists(True) Then
                    Continue For
                End If

                Dim cpd As bGetObject.Compound = MetabolitesDBGet.DownloadCompound(EntryId)

                If cpd Is Nothing Then
                    Call $"[{entry.ToString}] is not exists in the kegg!".Warning
                    failures += EntryId
                Else
                    Call cpd.GetXml.SaveTo(xml)
                End If
            Next

            Return failures
        End Function

        Public Shared Function LoadFile(path As String) As Compound()
            Dim Model = BriteHText.Load(FileIO.FileSystem.ReadAllText(path))
            Return Build(Model)
        End Function
    End Class
End Namespace
