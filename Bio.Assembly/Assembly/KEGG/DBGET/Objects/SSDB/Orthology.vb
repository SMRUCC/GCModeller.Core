﻿Imports System.Text.RegularExpressions
Imports LANS.SystemsBiology.Assembly.KEGG.WebServices
Imports LANS.SystemsBiology.Assembly.KEGG.WebServices.WebRequest
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Assembly.KEGG.DBGET.bGetObject.SSDB

    ''' <summary>
    ''' KEGG KO分类
    ''' </summary>
    Public Class Orthology : Inherits LANS.SystemsBiology.Assembly.KEGG.DBGET.bGetObject.bGetObject

        Public Overrides ReadOnly Property Code As String
            Get
                Return "ko"
            End Get
        End Property

        Public Property [Module] As ComponentModel.KeyValuePair()
        Public Property Pathway As ComponentModel.KeyValuePair()
        Public Property Disease As ComponentModel.KeyValuePair()
        Public Property Genes As QueryEntry()
        Public Property References As Reference()
        Public Property xRefEntry As Microsoft.VisualBasic.ComponentModel.TripleKeyValuesPair()
            Get
                Return _xRef
            End Get
            Set(value As Microsoft.VisualBasic.ComponentModel.TripleKeyValuesPair())
                _xRef = value
                If value.IsNullOrEmpty Then
                    _xRefDict = New Dictionary(Of String, Microsoft.VisualBasic.ComponentModel.TripleKeyValuesPair())
                Else
                    _xRefDict = (From x As Microsoft.VisualBasic.ComponentModel.TripleKeyValuesPair
                                 In value
                                 Select x
                                 Group x By x.Key Into Group) _
                                    .ToDictionary(Function(x) x.Key, elementSelector:=Function(x) x.Group.ToArray)
                End If
            End Set
        End Property
        Public Property EC As String

        Dim _xRef As Microsoft.VisualBasic.ComponentModel.TripleKeyValuesPair()
        Dim _xRefDict As Dictionary(Of String, Microsoft.VisualBasic.ComponentModel.TripleKeyValuesPair())

        Public Function GetXRef(Db As String) As Microsoft.VisualBasic.ComponentModel.TripleKeyValuesPair()
            If _xRefDict.ContainsKey(Db) Then
                Return _xRefDict(Db)
            Else
                Return Nothing
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Me.Entry}]  {Name}: {Me.Definition}"
        End Function
    End Class
End Namespace