Imports System.ComponentModel.Composition
Imports System.Resources

<Export(GetType(ResourceManager))>
Public Module Resources

    <Export(GetType(ResourceManager))>
    Public ReadOnly Property Resources As ResourceManager
        Get
            Return My.Resources.ResourceManager
        End Get
    End Property
End Module
