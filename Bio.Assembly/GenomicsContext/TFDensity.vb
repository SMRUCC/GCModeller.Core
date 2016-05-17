Imports System.Runtime.CompilerServices
Imports LANS.SystemsBiology.ComponentModel
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports LANS.SystemsBiology.ComponentModel.Loci.Abstract
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic

Public Module TFDensity

    ''' <summary>
    ''' 虽然名称是调控因子的密度，但是也可以用作为其他类型的基因的密度的计算，这个函数是非顺式的，即只要在ATG前面的范围内或者TGA后面的范围内出现都算存在
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="genome"></param>
    ''' <param name="TF"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Density(Of T As I_GeneBrief)(genome As IGenomicsContextProvider(Of T),
                                                 TF As IEnumerable(Of String),
                                                 Optional ranges As Integer = 10000,
                                                 Optional stranded As Boolean = False) As Density()
        Dim TFs As T() = TF.ToArray(AddressOf genome.GetByName)
        Dim getTF As Func(Of Strands, T()) = New __sourceHelper(Of T)(TFs, stranded).__gets
        Dim LQuery As Density() =
            LinqAPI.Exec(Of Density) <= From gene As T
                                        In genome.AllFeatures
                                        Let sides As T() = getTF(gene.Location.Strand)
                                        Let related As T() = gene.__getGenes(sides, ranges)
                                        Select New Density With {
                                            .locus_tag = gene.Identifier,
                                            .Abundance = related.Length / TFs.Length,
                                            .Hits = related.ToArray(Function(g) g.Identifier),
                                            .loci = gene.Location
                                        }
        Return LQuery
    End Function

    <Extension>
    Private Function __getGenes(Of T As I_GeneBrief)(g As T, TFs As T(), ranges As Integer) As T()
        Dim result As New List(Of T)

        If g.Location.Strand = Strands.Forward Then ' 上游是小于ATG，下游是大于TGA
            Dim ATG As Integer = g.Location.Left
            Dim TGA As Integer = g.Location.Right

            For Each loci In TFs
                If ATG - loci.Location.Right <= ranges Then
                    result += loci
                ElseIf loci.Location.Left - TGA <= ranges Then
                    result += loci
                End If
            Next
        Else
            Dim ATG As Integer = g.Location.Right
            Dim TGA As Integer = g.Location.Left

            For Each loci In TFs
                If TGA - loci.Location.Right <= ranges Then
                    result += loci
                ElseIf loci.Location.Left - ATG <= ranges Then
                    result += loci
                End If
            Next
        End If

        Return result
    End Function

    Private Structure __sourceHelper(Of T As I_GeneBrief)
        Dim data As T()

        Sub New(source As T(), stranded As Boolean)
            If stranded Then
                forwards = (From gene As T In data Where gene.Location.Strand = Strands.Forward Select gene).ToArray
                reversed = (From gene As T In data Where gene.Location.Strand = Strands.Reverse Select gene).ToArray
                __gets = AddressOf __stranded
            Else
                __gets = AddressOf __unstranded
            End If

            data = source
        End Sub

        Dim forwards As T()
        Dim reversed As T()
        Dim __gets As Func(Of Strands, T())

        Private Function __stranded(strand As Strands) As T()
            Select Case strand
                Case Strands.Forward
                    Return forwards
                Case Strands.Reverse
                    Return reversed
                Case Else
                    Return data
            End Select
        End Function

        Private Function __unstranded(strand As Strands) As T()
            Return data
        End Function
    End Structure

    ''' <summary>
    ''' 顺式调控，只有TF出现在上游，并且二者链方向相同才算存在
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="genome"></param>
    ''' <param name="TF"></param>
    ''' <param name="ranges"></param>
    ''' <returns></returns>
    <Extension>
    Public Function DensityCis(Of T As I_GeneBrief)(genome As IGenomicsContextProvider(Of T),
                                                    TF As IEnumerable(Of String),
                                                    Optional ranges As Integer = 10000) As Density()
        Dim TFs As T() = TF.ToArray(AddressOf genome.GetByName)
        Dim getTF As Func(Of Strands, T()) = New __sourceHelper(Of T)(TFs, True).__gets
        Dim LQuery As Density() =
            LinqAPI.Exec(Of Density) <= From gene As T
                                        In genome.AllFeatures
                                        Let sides As T() = getTF(gene.Location.Strand)
                                        Let related As T() = gene.__getCisGenes(sides, ranges)
                                        Select New Density With {
                                            .locus_tag = gene.Identifier,
                                            .Abundance = related.Length / TFs.Length,
                                            .Hits = related.ToArray(Function(g) g.Identifier),
                                            .loci = gene.Location
                                        }
        Return LQuery
    End Function

    ''' <summary>
    ''' 查找当前的基因的上游符合距离范围内的TF目标
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="g"></param>
    ''' <param name="TFs"></param>
    ''' <param name="ranges"></param>
    ''' <returns></returns>
    <Extension>
    Private Function __getCisGenes(Of T As I_GeneBrief)(g As T, TFs As T(), ranges As Integer) As T()
        Dim result As New List(Of T)

        If g.Location.Strand = Strands.Forward Then ' 上游是小于ATG，下游是大于TGA
            Dim ATG As Integer = g.Location.Left

            For Each loci In TFs
                If ATG - loci.Location.Right <= ranges Then
                    result += loci
                End If
            Next
        Else
            Dim ATG As Integer = g.Location.Right

            For Each loci In TFs
                If loci.Location.Left - ATG <= ranges Then
                    result += loci
                End If
            Next
        End If

        Return result
    End Function
End Module

Public Structure Density : Implements sIdEnumerable

    Public Property locus_tag As String Implements sIdEnumerable.Identifier
    Public Property loci As NucleotideLocation
    Public Property Abundance As Double
    Public Property Hits As String()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Structure
