Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic

Namespace ComponentModel.Loci.Abstract

    Public Interface ITagSite
        Property Address As String
        ''' <summary>
        ''' 当前的这个位点对象距离<see cref="Address"/>所标记的位点的距离
        ''' </summary>
        ''' <returns></returns>
        Property Distance As Integer
    End Interface

    Public Module TagSiteExtensions

        <Extension>
        Public Iterator Function Groups(Of T As ITagSite)(source As IEnumerable(Of T), Optional offset As Integer = 10) As IEnumerable(Of GroupResult(Of T, String))
            Dim Grouping = (From x As T In source Select x Group x By x.Address Into Group)  ' 首先按照位点的tag标记进行分组
            Dim locis As GroupResult(Of T, String)() = (From x
                                                        In Grouping'.AsParallel
                                                        Select x.Group.__innerGroup(offset, tag:=x.Address)).MatrixToVector
            For Each x As GroupResult(Of T, String) In locis
                Yield x
            Next
        End Function

        <Extension> Private Function __innerGroup(Of T As ITagSite)(source As IEnumerable(Of T),
                                                     offset As Integer,
                                                     tag As String) As GroupResult(Of T, String)()

            Dim locis = (From x As T In source Select x Group x By x.Distance Into Group)
            locis = (From x In locis Select x Order By x.Distance Ascending).ToArray

            Dim result As New List(Of GroupResult(Of T, String))(
                New GroupResult(Of T, String)($"{tag}:{locis.First.Distance}", locis.First.Group))
            Dim last As Integer = locis.First.Distance

            For Each x In locis.Skip(1)
                If Math.Abs(x.Distance - last) <= offset Then
                    result.Last.Add(x.Group)
                Else
                    last = x.Distance
                    result += New GroupResult(Of T, String)($"{tag}:{x.Distance}", x.Group)
                End If
            Next

            Return result
        End Function
    End Module
End Namespace