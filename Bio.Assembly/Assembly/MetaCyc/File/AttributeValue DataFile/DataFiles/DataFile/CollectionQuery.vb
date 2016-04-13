Imports System.Reflection
Imports LANS.SystemsBiology.Assembly.MetaCyc.File.DataFiles.Reflection

Namespace Assembly.MetaCyc.File.DataFiles

    Partial Class DataFile(Of T As MetaCyc.File.DataFiles.Slots.Object)

        ''' <summary>
        ''' 使用Index对象进行对象实例目标的查找操作
        ''' </summary>
        ''' <param name="UniqueId"></param>
        ''' <returns></returns>
        ''' <remarks>请务必要确保Index的顺序和FrameObjects的顺序一致</remarks>
        Public Function [Select2](UniqueId As String) As T
            Dim i As Integer = Array.IndexOf(Index, UniqueId)
            If i > -1 Then
                Return FrameObjects(i)
            Else 'object not found!
                Dim ErrorMessage As String = String.Format("The object that have the specific id '{0}' was not found in current list!", UniqueId)
                Throw New KeyNotFoundException(ErrorMessage)
            End If
        End Function

        ''' <summary>
        ''' 按照指定的UniqueID查找目标对象，当没有找到对象的时候，本函数不会像Select和Select2函数抛出错误，而是会返回一个空引用对象
        ''' </summary>
        ''' <param name="UniqueId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function [Get](UniqueId As String) As T
            Dim LQuery As Generic.IEnumerable(Of T) =
                From ItemObject As T In FrameObjects.AsParallel
                Where String.Equals(UniqueId, ItemObject.Identifier)
                Select ItemObject '
            Return LQuery.FirstOrDefault
        End Function

        ''' <summary>
        ''' Get a object from current list object using its <see cref="MetaCyc.File.DataFiles.Slots.[Object].Identifier">unique-id</see> property.(根据一个对象的Unique-Id字段的值来获取该目标对象，查询失败则返回空值)
        ''' </summary>
        ''' <param name="UniqueId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overridable Function [Select](UniqueId As String) As T
            If _InnerDictionary.ContainsKey(UniqueId) Then
                Return _InnerDictionary(UniqueId)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Takes a sub list of the elements that were pointed by the unique-id collection.
        ''' (获取一个UniqueId集合所指向的对象元素列表，会自动过滤掉不存在的UniqueId值)
        ''' </summary>
        ''' <param name="UniqueIdCollection">
        ''' The unique-id collection of the objects that wants to take from the list obejct.
        ''' (将要从本列表对象获取的对象的唯一标识符的集合)
        ''' </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Takes(UniqueIdCollection As Generic.IEnumerable(Of String)) As T()
            Dim LQuery As Generic.IEnumerable(Of T) = From Id As String In UniqueIdCollection Where Array.IndexOf(Index, Id) > -1 Select [Select](Id) '
            Return LQuery.ToArray
        End Function

        ''' <summary>
        ''' 使用值比较来查询出目标对象列表
        ''' </summary>
        ''' <param name="Object"></param>
        ''' <param name="Fields"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function [Select]([Object] As T, ParamArray Fields As String()) As T()
            Dim LQuery = From obj As T In Me.AsParallel
                         Where LANS.SystemsBiology.Assembly.MetaCyc.File.DataFiles.Reflection.FileStream.Equals(Of T)(obj, [Object], Fields)
                         Select obj  '
            Return LQuery.ToArray
        End Function

        ''' <summary>
        ''' 本函数较[Select]([Object] As T, ParamArray Fields As String())函数有着更高的查询性能
        ''' </summary>
        ''' <param name="Object"></param>
        ''' <param name="ItemProperties">所缓存的类型信息</param>
        ''' <param name="FieldAttributes">所缓存的类型信息</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function [Select]([Object] As T, ItemProperties As PropertyInfo(), FieldAttributes As MetaCycField(), Optional AllowEmpty As Boolean = False) As T()
            Dim LQuery As System.Linq.ParallelQuery(Of T)

            If AllowEmpty Then
                LQuery = From obj As T In Me.AsParallel Where LANS.SystemsBiology.Assembly.MetaCyc.File.DataFiles.Reflection.FileStream.Equals(Of T) _
                            (obj, [Object], ItemProperties, FieldAttributes)
                         Select obj  '
            Else
                LQuery = From obj As T In Me.AsParallel Where LANS.SystemsBiology.Assembly.MetaCyc.File.DataFiles.Reflection.FileStream.Equals(Of T) _
                            (obj, [Object], ItemProperties, FieldAttributes, True)
                         Select obj  '
            End If

            Return LQuery.ToArray
        End Function
    End Class
End Namespace