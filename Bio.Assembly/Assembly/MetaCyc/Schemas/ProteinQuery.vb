Imports Microsoft.VisualBasic

Namespace Assembly.MetaCyc.Schema

    Public Class ProteinQuery
        Dim MetaCyc As MetaCyc.File.FileSystem.DatabaseLoadder

        Sub New(MetaCyc As MetaCyc.File.FileSystem.DatabaseLoadder)
            Me.MetaCyc = MetaCyc
        End Sub

        ''' <summary>
        ''' 递归的获取某一个指定的蛋白质的所有Component对象
        ''' </summary>
        ''' <param name="ProteinId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetAllComponentList(ProteinId As String) As MetaCyc.File.DataFiles.Slots.Object()
            Dim protein = MetaCyc.GetProteins.Select(ProteinId)

            If protein Is Nothing Then '目标对象则可能是Compound对象
                Dim Compound = MetaCyc.GetCompounds.Select(ProteinId)
                If Not Compound Is Nothing Then
                    Return New MetaCyc.File.DataFiles.Slots.Object() {Compound}
                End If
            Else
                If protein.Components.IsNullOrEmpty Then '单体蛋白质
                    Return New MetaCyc.File.DataFiles.Slots.Object() {protein}
                Else '蛋白质复合物，则必须要进行递归查找了
                    Dim objList As List(Of MetaCyc.File.DataFiles.Slots.Object) = New List(Of MetaCyc.File.DataFiles.Slots.Object)
                    For Each ComponentId As String In protein.Components
                        Call objList.AddRange(GetAllComponentList(ComponentId))
                    Next

                    Return objList.ToArray
                End If
            End If

            Return New MetaCyc.File.DataFiles.Slots.Object() {}
        End Function
    End Class
End Namespace