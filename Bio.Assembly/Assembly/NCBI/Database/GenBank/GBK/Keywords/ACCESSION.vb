﻿Imports System.Text

Namespace Assembly.NCBI.GenBank.GBFF.Keywords

    Public Class ACCESSION : Inherits KeyWord

        Public Property AccessionId As String
        Public Property Alternative As String

        Public Overrides Function ToString() As String
            Return AccessionId
        End Function

        Friend Shared Function CreateObject(str As String(), DefaultID As String) As ACCESSION
            If str.IsNullOrEmpty Then '可能是新的数据，还没有提交到数据库之中，所以这里的数据库编号可能是空的
RETURN_EMPTY:   Return New ACCESSION With {
                    .AccessionId = DefaultID,
                    .Alternative = DefaultID
                }
            Else
                str = Mid(str.First, 12).Trim.Split
                If str.IsNullOrEmpty OrElse (str.Length = 1 AndAlso String.IsNullOrEmpty(str.First)) Then
                    GoTo RETURN_EMPTY
                Else
                    Return New ACCESSION With {
                        .AccessionId = str.First,
                        .Alternative = str.Last
                    }
                End If
            End If
        End Function
    End Class

    Public Class LOCUS : Inherits KeyWord

        Public Property AccessionID As String
        Public Property Length As Integer
        Public Property Type As String
        Public Property UpdateTime As String
        Public Property Molecular As String

        Public Overrides Function ToString() As String
            Return AccessionID
        End Function

        Public Shared Function InternalParser(s As String) As LOCUS
            Dim ChunkBuffer As String() = (From ss As String In s.Split.Skip(1)
                                           Where Not String.IsNullOrEmpty(ss)
                                           Select ss).ToArray
            Dim loc As LOCUS = New LOCUS

            loc.AccessionID = ChunkBuffer(0)
            loc.Length = ChunkBuffer(1)
            loc.Type = ChunkBuffer(3)
            loc.Molecular = ChunkBuffer(4)
            loc.UpdateTime = ChunkBuffer.Last

            Return loc
        End Function
    End Class

    Public Class DEFINITION : Inherits KeyWord

        Public Property Value As String

        Sub New()
        End Sub

        Sub New(def As String)
            Value = def
        End Sub

        Public Overrides Function ToString() As String
            Return Value
        End Function

        Public Shared Widening Operator CType(str As String()) As DEFINITION
            str(0) = Mid(str(0), 11)
            Return New DEFINITION With {
                .Value = String.Join(" ", (From s As String In str Select s.Trim).ToArray)
            }
        End Operator

        Public Shared Narrowing Operator CType(obj As DEFINITION) As String
            Return obj.Value
        End Operator

        Friend Shared ReadOnly WGSKeywords As String() = New String() {"whole genome shotgun", "WGS"}
    End Class

    Public Class VERSION : Inherits KeyWord

        Public Property Ver As String
        Public Property AccessionID As String
        Public Property GI As String

        Public ReadOnly Property IsEmpty As Boolean
            Get
                Return String.IsNullOrEmpty(Ver) AndAlso
                    String.IsNullOrEmpty(AccessionID) AndAlso
                    String.IsNullOrEmpty(GI)
            End Get
        End Property

        Sub New()
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="v">version string</param>
        Sub New(v As String)
            Ver = v
        End Sub

        Public ReadOnly Property VersionString As String
            Get
                Return String.Format("{0}.{1}", AccessionID, Ver)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return VersionString
        End Function

        Public Shared Widening Operator CType(str As String()) As VERSION
            If str.IsNullOrEmpty Then
                Return New VERSION
            Else
                str = Strings.Split(Mid(str.First, 11).Trim)
            End If

            Dim Ver As VERSION = New VERSION With {
                .AccessionID = str.First,
                .GI = str.Last.Replace("GI:", "")
            }
            str = Ver.AccessionID.Split(CChar("."))
            Ver.AccessionID = str.First
            Ver.Ver = str.Last
            Return Ver
        End Operator
    End Class
End Namespace