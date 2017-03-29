﻿#Region "Microsoft.VisualBasic::d0731f62dc8596bdb9282c8a3bee9b9c, ..\core\Bio.InteractionModel\Interaction.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Abstract
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' 默认的的相互作用的方向为从A到B
''' </summary>
''' <remarks></remarks>
Public Class Interaction(Of T As INamedValue)
    Implements IInteraction

    Public Property A As T
    Public Property Interaction As String
    Public Property B As T

    ''' <summary>
    ''' Is part of the interator is nothing?
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Broken As Boolean
        Get
            Return B Is Nothing OrElse A Is Nothing
        End Get
    End Property

    Public Property source As String Implements IInteraction.source
        Get
            Return A.Key
        End Get
        Set(value As String)
            A.Key = value
        End Set
    End Property

    Public Property target As String Implements IInteraction.target
        Get
            Return B.Key
        End Get
        Set(value As String)
            B.Key = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        If Broken Then
            If A Is Nothing AndAlso B Is Nothing Then
                Return ""
            ElseIf A Is Nothing Then
                Return B.Key
            Else
                Return A.Key
            End If
        End If

        Return {source, Interaction, target}.JoinBy(vbTab)
    End Function

    ''' <summary>
    ''' 判断两个Interaction对象是否相同
    ''' </summary>
    ''' <param name="objA"></param>
    ''' <param name="objB"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Function Equals(objA As T, objB As T, Optional directed? As Boolean = False) As Boolean
        If Not directed Then
            Return (String.Equals(objA.Key, A.Key) AndAlso
                String.Equals(objB.Key, B.Key)) OrElse
                (String.Equals(objB.Key, A.Key) AndAlso
                String.Equals(objA.Key, B.Key))
        Else
            Return String.Equals(objA.Key, A.Key) AndAlso
                String.Equals(objB.Key, B.Key)
        End If
    End Function

    ''' <summary>
    ''' 判断两个Interaction对象是否相同
    ''' </summary>
    ''' <param name="objA"></param>
    ''' <param name="objB"></param>
    ''' <param name="directed">是否是具备有方向的？</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overloads Function Equals(objA As String, objB As String, Optional directed? As Boolean = False) As Boolean
        If Not directed Then
            Return (String.Equals(objA, A.Key) AndAlso String.Equals(objB, B.Key)) OrElse
                       (String.Equals(objB, A.Key) AndAlso String.Equals(objA, B.Key))
        Else
            Return String.Equals(objA, A.Key) AndAlso String.Equals(objB, B.Key)
        End If
    End Function

    Public Shared Function Generate(data As IEnumerable(Of Interaction(Of T))) As String()
        Dim result$() = data.ToArray(Function(iter) iter.ToString)
        Return result
    End Function
End Class
