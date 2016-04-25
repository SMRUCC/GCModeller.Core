Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization

Public Class Genbank : Inherits ClassObject

    Public ReadOnly Property DIR As String

    Sub New(DIR As String)
        Me.DIR = DIR
    End Sub

    ''' <summary>
    ''' 创建索引文件
    ''' </summary>
    ''' <param name="DIR"></param>
    ''' <returns></returns>
    Public Shared Function Install(DIR As String) As Integer

    End Function
End Class

Public Class GenbankIndex
    Public Property DIR As String
    Public Property genome As String
    Public Property AccId As String

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
