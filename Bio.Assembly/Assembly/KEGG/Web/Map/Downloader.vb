﻿#Region "Microsoft.VisualBasic::2d1edc6ae934c085adc8edf861ea1aac, Bio.Assembly\Assembly\KEGG\Web\Map\Downloader.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
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



' /********************************************************************************/

' Summaries:

'     Module MapDownloader
' 
'         Function: DownloadHumans, Downloads, DownloadsKGML, loadEntryAuto
' 
' 
' /********************************************************************************/

#End Region

Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Terminal
Imports Microsoft.VisualBasic.Terminal.ProgressBar
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.genomics.Assembly.KEGG.DBGET.LinkDB
Imports SMRUCC.genomics.ComponentModel
Imports PathwayEntry = SMRUCC.genomics.Assembly.KEGG.DBGET.BriteHEntry.Pathway
Imports r = System.Text.RegularExpressions.Regex

Namespace Assembly.KEGG.WebServices

    Public Class MapQuery : Inherits WebQuery(Of PathwayEntry)

        Public Sub New(briteFile$,
                       <CallerMemberName>
                       Optional cache As String = Nothing,
                       Optional interval As Integer = -1,
                       Optional offline As Boolean = False)

            Call MyBase.New(
                createUrl(briteFile),
                AddressOf getID(briteFile).Invoke,
                AddressOf ParseHtml,
                prefix:=Nothing,
                cache:=cache,
                interval:=interval,
                offline:=offline
            )
        End Sub

        Friend Shared Function getID(briteFile As String) As Func(Of PathwayEntry, String)
            Return Function(entry As PathwayEntry)
                       If briteFile.StringEmpty Then
                           Return "map" & entry.EntryId
                       Else
                           Dim s = entry.entry.text
                           s = r.Match(s, "\[PATH:.+?\]", RegexICSng).Value
                           s = s.GetStackValue("[", "]").Split(":"c).Last
                           Return s
                       End If
                   End Function
        End Function

        Private Shared Function ParseHtml(html$, schema As Type) As Object
            Return Map.ParseHTML(html)
        End Function

        Private Shared Function createUrl(briteFile As String) As Func(Of PathwayEntry, String)
            Dim getID = MapQuery.getID(briteFile)

            Return Function(entry)
                       Return $"http://www.genome.jp/kegg-bin/show_pathway?{getID(entry)}"
                   End Function
        End Function
    End Class

    ''' <summary>
    ''' KEGG reference map downloader API
    ''' </summary>
    Public Module MapDownloader

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function loadEntryAuto(file As String) As PathwayEntry()
            If file.FileExists Then
                Return PathwayEntry.LoadData(file)
            Else
                Return PathwayEntry.LoadFromResource()
            End If
        End Function

        ''' <summary>
        ''' Download the KEGG reference map data.
        ''' </summary>
        ''' <param name="EXPORT$"></param>
        ''' <param name="briefFile$"></param>
        ''' <returns></returns>
        Public Function Downloads(EXPORT$, Optional briefFile$ = Nothing) As String()
            Dim entries As PathwayEntry() = loadEntryAuto(briefFile)
            Dim web As New MapQuery(briefFile, $"{EXPORT}/.cache/")
            Dim failures As New List(Of String)
            Dim tick As New ProgressProvider(entries.Length)
            Dim msg$
            Dim getId = MapQuery.getID(briefFile)

            Using progress As New ProgressBar("Downloads KEGG pathway map data...", 1, CLS:=True)
                For Each entry As PathwayEntry In entries
                    Dim id$ = getId(entry)
                    Dim save$ = $"{EXPORT}/{entry.GetPathCategory}/{id}.XML"

                    Try
                        If id.StringEmpty Then
                            Continue For
                        Else
                            Call web.Query(Of Map)(entry, ".html").GetXml.SaveTo(save, UTF8WithoutBOM)
                        End If
                    Catch ex As Exception
                        failures += id
                        ex = New Exception(entry.GetJson, ex)

                        Call ex.PrintException
                        Call App.LogException(ex)
                    Finally
                        msg = tick.ETA(progress.ElapsedMilliseconds).FormatTime
                        msg = entry.EntryId & "  " & msg

                        progress.SetProgress(tick.StepProgress, msg)
                    End Try
                Next
            End Using

            Return failures
        End Function

        Public Function DownloadHumans(EXPORT$) As String()
            Dim url$ = "http://www.kegg.jp/kegg-bin/download_htext?htext=hsa00001.keg&format=htext&filedir="
            Dim temp = App.GetAppSysTempFile(".txt")

            If Not url.DownloadFile(temp) Then
                Throw New UnauthorizedAccessException
            End If

            Return MapDownloader.Downloads(EXPORT, briefFile:=temp)
        End Function

        ''' <summary>
        ''' 下载某一个物种所注释的代谢途径的数据
        ''' </summary>
        ''' <param name="sp"></param>
        ''' <param name="EXPORT"></param>
        ''' <returns>
        ''' 函数返回失败的编号列表
        ''' </returns>
        Public Function DownloadsKGML(sp$, Optional EXPORT$ = "./LinkDB-pathways.KGML/") As String()
            Dim entries As New List(Of ListEntry)
            Dim briteTable As Dictionary(Of String, PathwayEntry) = PathwayEntry.LoadDictionary
            Dim Downloader As New WebClient()
            Dim failures As New List(Of String)
            Dim all As ListEntry() = Pathways.AllEntries(sp).ToArray
            Dim url$
            Dim msg$
            Dim path$, bCode$
            Dim refer$

            Using progress As New ProgressBar("KEGG LinkDB Downloads KEGG Pathways KGML network data....", 1, CLS:=True)
                Dim tick As New ProgressProvider(all.Length)

                For Each entry As ListEntry In all
                    Try
                        url = KGML.pathway.ResourceURL(entry.entryId)
                        msg = entry.description & " " & tick.ETA(progress.ElapsedMilliseconds).FormatTime
                        bCode = r.Match(entry.entryId, "\d+").Value
                        path = $"{EXPORT}/{briteTable(bCode).GetPathCategory}/{entry.entryId}.Xml"
                        refer = $"http://www.kegg.jp/kegg-bin/highlight_pathway?scale=1.0&map={entry.entryId}"

                        Call url.GET(refer:=refer).SaveTo(path, TextEncodings.UTF8WithoutBOM)
                        Call progress.SetProgress(tick.StepProgress, msg)
                    Catch ex As Exception
                        Call App.LogException(New Exception(entry.GetJson, ex))
                        Call failures.Add(entry.entryId)
                    Finally
                        Call Thread.Sleep(3 * 1000)
                    End Try
                Next
            End Using

            Return failures
        End Function
    End Module
End Namespace
