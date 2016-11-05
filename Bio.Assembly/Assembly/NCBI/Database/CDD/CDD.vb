﻿#Region "Microsoft.VisualBasic::6e3d1c9e65e789855b59593c9664ccbb, ..\GCModeller\core\Bio.Assembly\Assembly\NCBI\Database\CDD\CDD.vb"

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

Imports DIR = System.String
Imports System.Text

Namespace Assembly.NCBI.CDD

    ''' <summary>
    ''' The query interface of the local CDD database.(CDD数据库的查询接口)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DomainInfo

        Dim Dir As DIR, CDDInfoLoader As CDDLoader

        Friend ReadOnly Property DataLoadMethods As Dictionary(Of String, System.Func(Of CDD.DbFile)) =
            New Dictionary(Of String, Func(Of CDD.DbFile)) From {
                {"Cdd", Function() CDDInfoLoader.GetCdd},
                {"Cog", Function() CDDInfoLoader.GetCog},
                {"Kog", Function() CDDInfoLoader.GetKog},
                {"Pfam", Function() CDDInfoLoader.GetPfam},
                {"Prk", Function() CDDInfoLoader.GetPrk},
                {"Smart", Function() CDDInfoLoader.GetSmart},
                {"Tigr", Function() CDDInfoLoader.GetTigr}}

        Default Public ReadOnly Property Db(Name As String) As CDD.DbFile
            Get
                Return _DataLoadMethods(Name)()
            End Get
        End Property

        Public ReadOnly Property Cdd As CDD.DbFile
            Get
                Return CDDInfoLoader.GetCdd
            End Get
        End Property

        Public ReadOnly Property Cog As CDD.DbFile
            Get
                Return CDDInfoLoader.GetCog
            End Get
        End Property

        Public ReadOnly Property Kog As CDD.DbFile
            Get
                Return CDDInfoLoader.GetKog
            End Get
        End Property

        Public ReadOnly Property Pfam As CDD.DbFile
            Get
                Return CDDInfoLoader.GetPfam
            End Get
        End Property

        Public ReadOnly Property Prk As CDD.DbFile
            Get
                Return CDDInfoLoader.GetPrk
            End Get
        End Property

        Public ReadOnly Property Smart As CDD.DbFile
            Get
                Return CDDInfoLoader.GetSmart
            End Get
        End Property

        Public ReadOnly Property Tigr As CDD.DbFile
            Get
                Return CDDInfoLoader.GetTigr
            End Get
        End Property

        ''' <summary>
        ''' 没有查询到的时候会返回一个空值
        ''' </summary>
        ''' <param name="Id"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Query(Id As String) As CDD.SmpFile
            Dim LQuery As Generic.IEnumerable(Of CDD.SmpFile) = From entry As SmpFile
                                                                In CDDInfoLoader.Cdd.SmpData
                                                                Where String.Equals(entry.Identifier, Id)
                                                                Select entry '
            Return LQuery.FirstOrDefault
        End Function

        Public Function Query(Id As String, Db As String) As CDD.SmpFile
            Return Query(Id, _DataLoadMethods(Db)())
        End Function

        Public Shared Function Query(Id As String, Db As CDD.DbFile) As SmpFile
            Dim LQuery = From Domain As SmpFile
                         In Db.SmpData
                         Where String.Equals(Domain.Identifier, Id)
                         Select Domain '
            Return LQuery.FirstOrDefault
        End Function

        Public Overrides Function ToString() As String
            Return Dir
        End Function

        Public Shared Function PreLoad(Dir As Dir) As DomainInfo
            Return New DomainInfo With {
                .Dir = Dir, .CDDInfoLoader = New CDDLoader(Dir)
            }
        End Function

        Public Shared Widening Operator CType(Dir As Dir) As DomainInfo
            Return DomainInfo.PreLoad(Dir)
        End Operator
    End Class
End Namespace
