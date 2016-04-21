﻿Imports System.Text
Imports System.Xml.Serialization
Imports LANS.SystemsBiology.Assembly.MetaCyc.File.DataFiles
Imports LANS.SystemsBiology.Assembly.MetaCyc.File.FileSystem
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic

Namespace Assembly.MetaCyc.Schema.PathwayBrief

    <XmlType("pwy")> Public Class Pathway : Implements sIdEnumerable
        Public Property MetaCycBaseType As Slots.Pathway
        ''' <summary> 
        ''' 本代谢途径是否为一个超途径
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property SuperPathway As Boolean
        Public Property ReactionList As Key_strArrayValuePair()
        ''' <summary>
        ''' 本代谢途径所包含的的亚途径
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ContiansSubPathway As Pathway()

        Public ReadOnly Property AssociatedGenes As String()
            Get
                Dim List As List(Of String) = New List(Of String)
                For Each rxn In ReactionList
                    Call List.AddRange(rxn.Value)
                Next
                If SuperPathway Then
                    For Each pwy In ContiansSubPathway
                        Call List.AddRange(pwy.AssociatedGenes)
                    Next
                End If
                Return (From s As String In List Select s Distinct Order By s Ascending).ToArray
            End Get
        End Property

        Public Overrides Function ToString() As String
            If SuperPathway Then
                Return String.Format("{0}, {1} reactions and {2} sub-pathways", Identifier, ReactionList.Length, ContiansSubPathway.Length)
            Else
                Return String.Format("{0}, {1} reactions.", Identifier, ReactionList.Length)
            End If
        End Function

        Public Property Identifier As String Implements sIdEnumerable.Identifier
    End Class

    Public Class PathwayBrief : Inherits ComponentModel.PathwayBrief
        Implements IKeyValuePairObject(Of String, String())

        Public Property Is_Super_Pathway As Boolean

        Public ReadOnly Property NumOfGenes As Integer
            Get
                If PathwayGenes.IsNullOrEmpty Then
                    Return 0
                End If
                Return PathwayGenes.Length
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("{0}  [{1}]", Me.EntryId, String.Join("; ", PathwayGenes))
        End Function

        Public Overrides Function GetPathwayGenes() As String()
            Return PathwayGenes
        End Function

        Public Overrides Property EntryId As String Implements IKeyValuePairObject(Of String, String()).Identifier
            Get
                Return MyBase.EntryId
            End Get
            Set(value As String)
                MyBase.EntryId = value
            End Set
        End Property

        Public Property PathwayGenes As String() Implements IKeyValuePairObject(Of String, String()).Value
    End Class
End Namespace