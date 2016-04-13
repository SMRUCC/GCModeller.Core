﻿Imports System.Text.RegularExpressions
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic

Namespace ComponentModel.DBLinkBuilder

    Public MustInherit Class DBLinksManager(Of TLink As IDBLink)
        Implements Generic.IReadOnlyCollection(Of TLink)

        Public Shared ReadOnly Property PrefixDB As String() = New String() {
            "ChEBI", "3DMET", "HMDB",
            "KNApSAcK", "MASSBANK",
            "NIKKAJI", "PDB-CCD",
            "PubChem", "KEGG.Compound"
        }

        Protected _DBLinkObjects As List(Of TLink)

        Default Public ReadOnly Property Item(DBName As String) As TLink()
            Get
                Dim LQuery = (From ItemDBLink As TLink
                              In Me._DBLinkObjects
                              Where String.Equals(DBName, ItemDBLink.locusId, StringComparison.OrdinalIgnoreCase)
                              Select ItemDBLink).ToArray
                Return LQuery
            End Get
        End Property

        Public Property DBLinkObjects As TLink()
            Get
                Return _DBLinkObjects.ToArray
            End Get
            Set(value As TLink())
                _DBLinkObjects = value.ToList
            End Set
        End Property

        Public ReadOnly Property DBLinks As String()
            Get
                Return (From item As TLink
                        In _DBLinkObjects
                        Select item.GetFormatValue).ToArray
            End Get
        End Property

        Public Sub AddEntry(Entry As TLink)
            Dim LQuery = (From item As TLink In DBLinkObjects
                          Where String.Equals(item.locusId, Entry.locusId, StringComparison.OrdinalIgnoreCase) AndAlso
                              String.Equals(item.Entry, Entry.Entry, StringComparison.OrdinalIgnoreCase)
                          Select item).ToArray

            If LQuery.IsNullOrEmpty Then
                Call _DBLinkObjects.Add(Entry)
            End If
        End Sub

        Public Sub Remove(Db_Name As String, Entry As String)
            Dim links As TLink() = Item(Db_Name)

            If Not links.IsNullOrEmpty Then
                For Each ll In (From n As TLink
                                In links
                                Where String.Equals(n.Entry, Entry, StringComparison.OrdinalIgnoreCase)
                                Select n).ToArray
                    Call _DBLinkObjects.Remove(ll)
                Next
            End If
        End Sub

        Public Sub AddEntry(DBName As String, Entry As String)
            Dim EntryObject As TLink = Activator.CreateInstance(Of TLink)()
            EntryObject.locusId = DBName
            EntryObject.Entry = Entry
            Call AddEntry(Entry:=EntryObject)
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of TLink) Implements IEnumerable(Of TLink).GetEnumerator
            For Each item As TLink In DBLinkObjects
                Yield item
            Next
        End Function

        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of TLink).Count
            Get
                Return DBLinkObjects.Length
            End Get
        End Property

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public MustOverride ReadOnly Property IsEmpty As Boolean
    End Class
End Namespace