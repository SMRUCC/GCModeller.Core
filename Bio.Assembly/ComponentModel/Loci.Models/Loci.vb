Imports System.Text.RegularExpressions
Imports LANS.SystemsBiology.ComponentModel.Loci.Abstract

Namespace ComponentModel.Loci

    Public Class Loci : Implements ILocationComponent

        Public Property TagData As String
        Public Property Left As Long Implements ILocationComponent.Left
        Public Property Right As Long Implements ILocationComponent.Address

        Public Overrides Function ToString() As String
            Return TagData
        End Function
    End Class
End Namespace