﻿#Region "Microsoft.VisualBasic::8c0f537bd1d4bd14ee114cd51594a6d0, Bio.Assembly\Assembly\KEGG\Web\Map\MapIndex.vb"

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

    '     Class MapIndex
    ' 
    '         Properties: compoundIndex, index, KeyVector, KOIndex
    ' 
    '         Function: ToString
    ' 
    '     Class MapRepository
    ' 
    '         Properties: Maps
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: BuildRepository, CreateIndex, Exists, GenericEnumerator, GetAll
    '                   GetByKey, GetEnumerator, GetWhere, QueryMapsByMembers
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Assembly.KEGG.WebServices

    Public Class MapIndex : Inherits Map
        Implements INamedValue

        <XmlElement("keys")>
        Public Property KeyVector As TermsVector
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return New TermsVector With {
                    .terms = index.Objects
                }
            End Get
            Set(value As TermsVector)
                _index = New Index(Of String)(value.terms)
                _KOIndex = value _
                    .terms _
                    .Where(Function(id)
                               Return id.IsPattern("K\d+", RegexICSng)
                           End Function) _
                    .Indexing
                _compoundIndex = value _
                    .terms _
                    .Where(Function(id)
                               Return id.IsPattern("C\d+", RegexICSng)
                           End Function) _
                    .Indexing
            End Set
        End Property

        ''' <summary>
        ''' KO, compoundID, reactionID, etc.
        ''' </summary>
        ''' <returns></returns>
        <XmlIgnore> Public ReadOnly Property index As Index(Of String)
        <XmlIgnore> Public ReadOnly Property KOIndex As Index(Of String)
        <XmlIgnore> Public ReadOnly Property compoundIndex As Index(Of String)

        Public Overrides Function ToString() As String
            Return ID
        End Function
    End Class

    ''' <summary>
    ''' The repository xml data of kegg <see cref="Map"/>
    ''' </summary>
    Public Class MapRepository : Inherits XmlDataModel
        Implements IRepositoryRead(Of String, MapIndex)
        Implements Enumeration(Of Map)

        <XmlElement("maps")> Public Property Maps As MapIndex()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return table.Values.ToArray
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As MapIndex())
                table = value.ToDictionary(Function(map) map.ID)
            End Set
        End Property

        ''' <summary>
        ''' Get by ID
        ''' </summary>
        Dim table As Dictionary(Of String, MapIndex)

        <XmlNamespaceDeclarations()>
        Public xmlns As XmlSerializerNamespaces

        Sub New()
            xmlns = New XmlSerializerNamespaces
            xmlns.Add("map", Map.XmlNamespace)
        End Sub

        Public Iterator Function QueryMapsByMembers(entity As IEnumerable(Of String)) As IEnumerable(Of MapIndex)
            For Each key As String In entity
                For Each map As MapIndex In table.Values
                    If map.index.IndexOf(key) > -1 Then
                        Yield map
                    End If
                Next
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Exists(key As String) As Boolean Implements IRepositoryRead(Of String, MapIndex).Exists
            Return table.ContainsKey(key)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetByKey(key As String) As MapIndex Implements IRepositoryRead(Of String, MapIndex).GetByKey
            Return table(key)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetWhere(clause As Func(Of MapIndex, Boolean)) As IReadOnlyDictionary(Of String, MapIndex) Implements IRepositoryRead(Of String, MapIndex).GetWhere
            Return table.Values.Where(clause).ToDictionary
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetAll() As IReadOnlyDictionary(Of String, MapIndex) Implements IRepositoryRead(Of String, MapIndex).GetAll
            Return New Dictionary(Of String, MapIndex)(table)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="directory">The reference map download directory</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function BuildRepository(directory As String) As MapRepository
            Return New MapRepository With {
                .Maps = directory _
                    .DoCall(AddressOf ScanMaps) _
                    .Select(AddressOf CreateIndex) _
                    .ToArray
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ScanMaps(directory As String) As IEnumerable(Of Map)
            Return (ls - l - r - "*.XML" <= directory).Select(AddressOf LoadXml(Of Map))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function CreateIndex(map As Map) As MapIndex
            Call map.Name.__DEBUG_ECHO

            Return New MapIndex With {
                .ID = map.ID,
                .KeyVector = New TermsVector With {
                    .terms = map _
                        .shapes _
                        .Select(Function(a) a.IDVector) _
                        .IteratesALL _
                        .Distinct _
                        .OrderBy(Function(s) s) _
                        .ToArray
                },
                .shapes = map.shapes,
                .Name = map.Name,
                .PathwayImage = map.PathwayImage,
                .URL = map.URL
            }
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of Map) Implements Enumeration(Of Map).GenericEnumerator
            For Each index In Maps
                Yield index
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of Map).GetEnumerator
            Yield GenericEnumerator()
        End Function
    End Class
End Namespace
