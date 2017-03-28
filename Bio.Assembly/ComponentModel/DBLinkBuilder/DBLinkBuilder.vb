﻿#Region "Microsoft.VisualBasic::ff0fd5da62c9be6de2fa2538edf2aedd, ..\core\Bio.Assembly\ComponentModel\DBLinkBuilder\DBLinkBuilder.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports System.Text.RegularExpressions
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.DBLinkBuilder

    Public Class DBLinks : Inherits DBLinksManager(Of DBLink)

        Dim _CheBI As DBLink(), _PubChem As DBLink
        Dim __3DMET, _HMDB, _KNApSAcK, _MASSBANK, _NIKKAJI, _PDB_CCD As DBLink

        Sub New(strData As String())
            If strData.IsNullOrEmpty Then
                MyBase.DBLinkObjects = New DBLink() {}
            End If
            Call Initialize((From strValue As String In strData Select DBLink.CreateObject(strValue)).ToArray)
        End Sub

        Sub New(objects As IEnumerable(Of DBLink))
            Call Initialize(objects)
        End Sub

        Private Sub Initialize(objects As IEnumerable(Of DBLink))
            _DBLinkObjects = objects.ToList
            Call LoadData(_DBLinkObjects, New KeyValuePair(Of String, Action(Of DBLink))() {
                          New KeyValuePair(Of String, Action(Of DBLink))("3DMET", Sub(DBLink As DBLink) Me.__3DMET = DBLink),
                          New KeyValuePair(Of String, Action(Of DBLink))("HMDB", Sub(DBLink As DBLink) Me._HMDB = DBLink),
                          New KeyValuePair(Of String, Action(Of DBLink))("KNApSAcK", Sub(DBLink As DBLink) Me._KNApSAcK = DBLink),
                          New KeyValuePair(Of String, Action(Of DBLink))("MASSBANK", Sub(DBLink As DBLink) Me._MASSBANK = DBLink),
                          New KeyValuePair(Of String, Action(Of DBLink))("NIKKAJI", Sub(DBLink As DBLink) Me._NIKKAJI = DBLink),
                          New KeyValuePair(Of String, Action(Of DBLink))("PDB-CCD", Sub(DBLink As DBLink) Me._PDB_CCD = DBLink),
                          New KeyValuePair(Of String, Action(Of DBLink))("PubChem", Sub(DBLink As DBLink) Me._PubChem = DBLink)
                     })
            _CheBI = (From item As DBLink
                      In DBLinkObjects
                      Where String.Equals(item.DBName, "CheBI", StringComparison.OrdinalIgnoreCase)
                      Select item).ToArray
        End Sub

        Private Shared Sub LoadData(DBLinks As List(Of DBLink), Methods As KeyValuePair(Of String, Action(Of DBLink))())
            For Each Method In Methods
                Dim Find As DBLink() =
                    LinqAPI.Exec(Of DBLink) <= From lnk As DBLink
                                               In DBLinks
                                               Where String.Equals(
                                                   lnk.DBName,
                                                   Method.Key,
                                                   StringComparison.OrdinalIgnoreCase)
                                               Select lnk
                Dim SetValue As Action(Of DBLink) = Method.Value
                Call SetValue(If(Find.IsNullOrEmpty, Nothing, Find.First))
            Next
        End Sub

        Public ReadOnly Property CHEBI As DBLink()
            Get
                Return _CheBI
            End Get
        End Property

        Public ReadOnly Property PUBCHEM As DBLink
            Get
                Return _PubChem
            End Get
        End Property

        Public ReadOnly Property _3DMET As DBLink
            Get
                Return __3DMET
            End Get
        End Property

        Public ReadOnly Property HMDB As DBLink
            Get
                Return _HMDB
            End Get
        End Property

        Public ReadOnly Property KNApSAcK As DBLink
            Get
                Return _KNApSAcK
            End Get
        End Property

        Public ReadOnly Property MASSBANK As DBLink
            Get
                Return _MASSBANK
            End Get
        End Property

        Public ReadOnly Property NIKKAJI As DBLink
            Get
                Return _NIKKAJI
            End Get
        End Property

        Public ReadOnly Property PDB_CCD As DBLink
            Get
                Return _PDB_CCD
            End Get
        End Property

        Public Overrides ReadOnly Property IsEmpty As Boolean
            Get
                Return _CheBI.IsNullOrEmpty AndAlso
                    _PubChem Is Nothing AndAlso
                    __3DMET Is Nothing AndAlso
                    _HMDB Is Nothing AndAlso
                    _KNApSAcK Is Nothing AndAlso
                    _MASSBANK Is Nothing AndAlso
                    _NIKKAJI Is Nothing AndAlso
                    _PDB_CCD Is Nothing
            End Get
        End Property
    End Class
End Namespace
