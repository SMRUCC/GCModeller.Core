﻿Imports Microsoft.VisualBasic

Namespace Assembly.KEGG.DBGET.BriteHEntry

    Public Class EnzymaticReaction
        Public Property EC As String
        Public Property [Class] As String
        Public Property Category As String
        Public Property SubCategory As String
        Public Property Entry As ComponentModel.KeyValuePair

        Public Shared Function LoadFromResource() As EnzymaticReaction()
            Dim Model = BriteHText.Load(My.Resources.br08201)
            Return Build(Model)
        End Function

        Private Shared Function Build(Model As BriteHText) As EnzymaticReaction()
            Dim ReactionList As List(Of EnzymaticReaction) = New List(Of EnzymaticReaction)

            For Each ClassItem As BriteHText In Model.CategoryItems
                For Each Category As BriteHText In ClassItem.CategoryItems
                    For Each SubCategory As BriteHText In Category.CategoryItems

                        If SubCategory.CategoryItems.IsNullOrEmpty Then
                            Continue For
                        End If

                        For Each EC As BriteHText In SubCategory.CategoryItems
                            If Not EC.CategoryItems.IsNullOrEmpty Then
                                Call ReactionList.AddRange(__rxns(EC, ClassItem, Category, SubCategory))
                            End If
                        Next
                    Next
                Next
            Next

            Return ReactionList.ToArray
        End Function

        Private Shared Function __rxns(EC As BriteHText, [class] As BriteHText, category As BriteHText, subCat As BriteHText) As EnzymaticReaction()
            Dim LQuery = (From rxn As BriteHText In EC.CategoryItems
                          Let erxn As EnzymaticReaction = New EnzymaticReaction With {
                              .EC = EC.ClassLabel,
                              .Category = category.ClassLabel,
                              .Class = [class].ClassLabel,
                              .SubCategory = subCat.ClassLabel,
                              .Entry = New ComponentModel.KeyValuePair With {
                                    .Key = rxn.EntryId,
                                    .Value = rxn.Description
                              }
                          }
                          Select erxn).ToArray
            Return LQuery
        End Function

        Public Shared Function LoadFile(path As String) As EnzymaticReaction()
            Return Build(BriteHText.Load(FileIO.FileSystem.ReadAllText(path)))
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("[{0}: {1}] {2}", String.Join("/", [Class], Category, SubCategory), EC, Entry.ToString)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Export"></param>
        ''' <returns>返回成功下载的代谢途径的数目</returns>
        ''' <remarks></remarks>
        Public Shared Function DownloadReactions(Export As String, Optional BriefFile As String = "", Optional DirectoryOrganized As Boolean = True) As Integer
            Dim BriefEntries = If(String.IsNullOrEmpty(BriefFile), LoadFromResource(), LoadFile(BriefFile))

            For Each Entry As EnzymaticReaction In BriefEntries
                Dim EntryId As String = Entry.Entry.Key
                Dim SaveToDir As String = If(DirectoryOrganized, __getDIR(Export, Entry), Export)
                Dim XmlFile As String = String.Format("{0}/{1}.xml", SaveToDir, EntryId)

                If FileIO.FileSystem.FileExists(XmlFile) Then
                    If FileIO.FileSystem.GetFileInfo(XmlFile).Length > 0 Then
                        Continue For
                    End If
                End If

                Dim rMod As bGetObject.Reaction = bGetObject.Reaction.Download(EntryId)

                If rMod Is Nothing Then
                    Call $"[{Entry.ToString}] is not exists in the kegg!".__DEBUG_ECHO
                    Call FileIO.FileSystem.WriteAllText(App.HOME & "/DownloadsFailures.log", Entry.ToString & vbCrLf, append:=True)
                    Continue For
                End If

                Call rMod.GetXml.SaveTo(XmlFile)
            Next

            Return 0
        End Function

        Private Shared Function __getDIR(outDIR As String, entry As EnzymaticReaction) As String
            Dim [class] As String = __trimInner(entry.Class)
            Dim cat As String = __trimInner(entry.Category)
            Dim subCat As String = __trimInner(entry.SubCategory)

            Return String.Join("/", outDIR, [class], cat, subCat)
        End Function

        Private Shared Function __trimInner(s As String) As String
            Return If(s.Length > 56, Mid(s, 1, 56) & "~", s)
        End Function
    End Class
End Namespace