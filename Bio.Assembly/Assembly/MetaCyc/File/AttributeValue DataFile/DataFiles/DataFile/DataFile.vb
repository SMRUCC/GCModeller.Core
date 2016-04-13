Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Reflection
Imports LANS.SystemsBiology.Assembly.MetaCyc.File.DataFiles.Reflection
Imports Microsoft.VisualBasic

Namespace Assembly.MetaCyc.File.DataFiles

    ''' <summary>
    ''' All of the data file object in the metacyc database will be inherits from this class object type.
    ''' (在MetaCyc数据库之中的所有元素对象都必须要继承自此对象类型)
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <remarks></remarks>
    Public Class DataFile(Of T As MetaCyc.File.DataFiles.Slots.Object)
        Implements Generic.IEnumerable(Of T)
        Implements MetaCyc.File.FileSystem.PGDB.MetaCycTable
        '     Implements IReadOnlyDictionary(Of String, T)
        Implements IReadOnlyList(Of T)

        Public Property DbProperty As [Property]

        Friend _Index As String() = New String() {}
        Friend FrameObjects As List(Of T) = New List(Of T)
        Friend FilePath As String

        Public ReadOnly Property Index As String()
            Get
                Return _Index
            End Get
        End Property

        Public ReadOnly Property First As T
            Get
                Return FrameObjects.First
            End Get
        End Property

        Public ReadOnly Property Last As T
            Get
                Return FrameObjects.Last
            End Get
        End Property

        ''' <summary>
        ''' BaseType Attribute List is empty.
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared ReadOnly AttributeList As String() = {}

        ''' <summary>
        ''' The length of the current list objetc.(当前的列表对象的长度)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Count As Integer Implements IReadOnlyCollection(Of T).Count
            Get
                Return FrameObjects.Count
            End Get
        End Property

        ''' <summary>
        ''' Clear all of the data that exists in this list object.(将本列表对象中的所有的数据进行清除操作)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Clear()
            Call FrameObjects.Clear()
            _Index = New String() {}
            Call _InnerDictionary.Clear()
        End Sub

        ''' <summary>
        ''' Gene the index propety for this list table object using the UniqueId property from each object in the list 
        ''' for a easy object query operation.
        ''' (使用Unique-Id属性值生成索引，方便查找)
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub Indexing() Implements MetaCyc.File.FileSystem.PGDB.MetaCycTable.Indexing
            Dim i1 = Sub() _Index = (From [Object] In FrameObjects Select [Object].Identifier).ToArray
            Dim i2 = Sub() InternalIndexing()
            Dim asy1 = i1.BeginInvoke(Nothing, Nothing)
            Dim asy2 = i2.BeginInvoke(Nothing, Nothing)

            Call i1.EndInvoke(asy1)
            Call i2.EndInvoke(asy2)
        End Sub

        Private Sub InternalIndexing()
            For Each Item As T In FrameObjects
                Call _InnerDictionary.Add(Item.Identifier, Item)
            Next
        End Sub

        ''' <summary>
        ''' Get the property list of each object that in the table of type T.
        ''' (获取目标类型的数据表之中的每一个对象元素的所有的属性列表)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Overridable Function GetAttributeList() As String()
            Return DataFile(Of T).AttributeList
        End Function

        ''' <summary>
        ''' Locate the target object using its unique id property, this function will return the location point of 
        ''' the target object in the list if we found it or return -1 if the object was not found.
        ''' (使用目标对象的唯一标识符属性对其进行在本列表对象中的定位操作，假若查找到了目标对象则返回其位置，反之则返回-1值)
        ''' </summary>
        ''' <param name="UniqueId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function IndexOf(UniqueId As String) As Integer
            Return Array.IndexOf(Index, UniqueId)
        End Function

        Public Function GetTypes() As String()
            Dim TypeList As New List(Of String)

            For Each [Object] In FrameObjects
                TypeList.AddRange([Object].Types)
            Next

            Dim LQuery As Generic.IEnumerable(Of String) =
                From e As String In TypeList
                Select e
                Distinct Order By e '

            Return LQuery.ToArray
        End Function

        'Public Shared Function Load(Path As String) As DataFile(Of T)
        '    Dim NewDataFile As DataFile(Of T) = New DataFile(Of T)
        '    Dim File As MetaCyc.File.AttributeValue = Path

        '    Dim Query As Generic.IEnumerable(Of T) = From e In File.Objects Select CType(e, T)

        '    NewDataFile.DbProperty = File.DbProperty
        '    NewDataFile.FrameObjects = Query.ToList

        '    Return NewDataFile
        'End Function

        Public Shared Function Cast(Of T2 As MetaCyc.File.DataFiles.Slots.Object)(Array As Generic.IEnumerable(Of T2)) As DataFile(Of T2)
            Return New DataFile(Of T2) With {.FrameObjects = Array.ToList}
        End Function

        ''' <summary>
        ''' Adding the target element object instance into the current list object, if the target element is 
        ''' already exists in the list object then not add the target object and return its current position 
        ''' in the list or add the target object into the list when it is not appear in the list object and 
        ''' then return the length of the current list object.
        ''' (将目标元素对象添加至当前的列表之中，假若目标对象存在于列表之中，则进行添加并返回列表的最后一个元素的位置，
        ''' 否则不对目标元素进行添加并返回目标元素在列表中的当前位置)
        ''' </summary>
        ''' <param name="e">
        ''' The target element that want to added into the list object.(将要添加进入列表之中的目标元素对象)
        ''' </param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Add(e As T) As Long
            Dim Handle As Long = Array.IndexOf(_Index, e.Identifier)
            If Handle = -1 Then
                Call FrameObjects.Add(e)
                Return FrameObjects.Count - 1
            Else
                Return Handle
            End If
        End Function

        ''' <summary>
        ''' Just add the element into the current list object and return the length of it, this method is fast than [Add(T) As Long] function, 
        ''' but an element duplicate error may occur.
        ''' (仅仅只是将目标元素添加进入当前的列表对象之中并返回添加了该元素的列表对象的新长度，本方法的速度要快于Add方法，但是可能会出现列表元素重复的错误) 
        ''' </summary>
        ''' <param name="e">
        ''' The element that will be add into the current list object.(将要添加进入当前的列表对象的目标元素对象)
        ''' </param>
        ''' <returns>The length of the current list object.(当前列表元素的长度)</returns>
        ''' <remarks></remarks>
        Public Function Append(e As T) As Long
            Call FrameObjects.Add(item:=e)
            Return FrameObjects.LongCount
        End Function

        Public Sub AddRange(TCollection As Generic.IEnumerable(Of T))
            For i As Integer = 0 To TCollection.Count - 1
                Call Add(TCollection(i))
            Next
        End Sub

        Public Overridable Sub Save(Optional File As String = "") Implements MetaCyc.File.FileSystem.PGDB.MetaCycTable.Save
            If String.IsNullOrEmpty(File) Then
                File = Me.FilePath
            End If
            Call Reflection.FileStream.Write(Of T, DataFile(Of T))(File, Me)
        End Sub

        Public Overrides Function ToString() As String
            Return DbProperty.ToString
        End Function

        Public Overridable Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Overridable Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For i As Integer = 0 To FrameObjects.Count - 1
                Yield FrameObjects(i)
            Next
        End Function

        Public Shared Narrowing Operator CType(Table As DataFile(Of T)) As T()
            Return Table.FrameObjects.ToArray
        End Operator

        Dim _InnerDictionary As Dictionary(Of String, T) = New Dictionary(Of String, T)

        Public Function ContainsKey(key As String) As Boolean
            Return _InnerDictionary.ContainsKey(key)
        End Function

        Default Public Overloads ReadOnly Property Item(key As String) As T
            Get
                If _InnerDictionary.ContainsKey(key) Then
                    Return _InnerDictionary(key)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Public Function TryGetValue(key As String, ByRef value As T) As Boolean
            Return _InnerDictionary.TryGetValue(key, value)
        End Function

        Default Public Overloads ReadOnly Property Item(index As Integer) As T Implements IReadOnlyList(Of T).Item
            Get
                Return FrameObjects(index)
            End Get
        End Property
    End Class
End Namespace