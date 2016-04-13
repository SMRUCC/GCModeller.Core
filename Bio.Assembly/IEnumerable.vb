Namespace Collection.Generic

    Public Class IEnumerable

        Public Interface IAccessionIdEnumerable
            Property UniqueId As String
        End Interface

        ''' <summary>
        ''' 按照UniqueId列表来筛选出目标集合
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="IdList"></param>
        ''' <param name="TargetCollection"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Takes(Of T As Assembly.Collection.Generic.IEnumerable.IAccessionIdEnumerable) _
            (IdList As System.Collections.Generic.IEnumerable(Of String), TargetCollection As System.Collections.Generic.IEnumerable(Of T)) As T()

            Dim Result As T() = New T(IdList.Count - 1) {}
            For i As Integer = 0 To IdList.Count - 1
                Dim Id As String = IdList(i)
                Dim LQuery As T() = (From obj As T
                                     In TargetCollection.AsParallel
                                     Where String.Equals(Id, obj.UniqueId)
                                     Select obj).ToArray
                If Not LQuery.Count = 0 Then
                    Result(i) = LQuery.First
                End If
            Next

            Return (From obj As T In Result Where Not obj Is Nothing Select obj Order By obj.UniqueId Ascending).ToArray
        End Function
    End Class
End Namespace