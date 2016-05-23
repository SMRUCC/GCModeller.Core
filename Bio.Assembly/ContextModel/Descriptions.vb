﻿Imports System.Runtime.CompilerServices
Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels
Imports LANS.SystemsBiology.ComponentModel
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic

Namespace ContextModel

    Public Module LocationDescriptions

        ''' <summary>
        ''' 判断本对象是否是由<see cref="BlankSegment"></see>方法所生成的空白片段
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function IsBlankSegment(Gene As IGeneBrief) As Boolean
            If Gene Is Nothing Then
                Return True
            End If
            Return String.Equals(Gene.Identifier, BLANK_VALUE) OrElse
                Gene.Location.FragmentSize = 0
        End Function

        Public Const BLANK_VALUE As String = "Blank"

        <Extension> Public Function BlankSegment(Of T As IGeneBrief)(Location As NucleotideLocation) As T
            Dim BlankData = Activator.CreateInstance(Of T)()
            BlankData.COG = BLANK_VALUE
            BlankData.Product = BLANK_VALUE
            BlankData.Identifier = BLANK_VALUE
            BlankData.Length = Location.FragmentSize
            BlankData.Location = Location
            Return BlankData
        End Function

        Public Function AtgDistance(Of T As IGeneBrief)(gene As T, nucl As NucleotideLocation) As Integer
            Call nucl.Normalization()
            Call gene.Location.Normalization()

            If gene.Location.Strand = Strands.Forward Then
                Return Math.Abs(gene.Location.Right - nucl.Left)
            Else
                Return Math.Abs(gene.Location.Left - nucl.Right)
            End If
        End Function

        ''' <summary>
        ''' Calculates the ATG distance between the target gene and a loci segment on.(计算位点相对于某一个基因的ATG距离)
        ''' </summary>
        ''' <param name="loci"></param>
        ''' <param name="gene"></param>
        ''' <returns>总是计算最大的距离</returns>
        ''' <remarks></remarks>
        <Extension> Public Function GetATGDistance(loci As Location, gene As IGeneBrief) As Integer
            Call loci.Normalization()
            Call gene.Location.Normalization()

            If gene.Location.Strand = Strands.Forward Then '直接和左边相减
                Return loci.Right - gene.Location.Left
            ElseIf gene.Location.Strand = Strands.Reverse Then  '互补链方向的基因，则应该减去右边
                Return gene.Location.Right - loci.Left
            Else
                Return loci.Left - gene.Location.Left
            End If
        End Function

        Const Intergenic As String = "Intergenic region"
        Const DownStream As String = "In the downstream of [{0}] gene ORF."
        Const IsORF As String = "Is [{0}] gene ORF."
        Const Inside As String = "Inside the [{0}] gene ORF."
        Const OverloapsDownStream As String = "Overlap on down_stream with [{0}] gene ORF."
        Const OverlapsUpStream As String = "Overlap on up_stream with [{0}] gene ORF."
        Const PromoterRegion As String = "In the promoter region of [{0}] gene ORF."

        ''' <summary>
        ''' Gets the loci location description data.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="posi"></param>
        ''' <param name="data"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Extension> Public Function LocationDescription(Of T As IGeneBrief)(posi As SegmentRelationships, data As T) As String
            If IsBlankSegment(data) Then
                Return Intergenic

            ElseIf posi = SegmentRelationships.DownStream Then
                Return String.Format(DownStream, data.Identifier)
            ElseIf posi = SegmentRelationships.Equals Then
                Return String.Format(IsORF, data.Identifier)
            ElseIf posi = SegmentRelationships.Inside Then
                Return String.Format(Inside, data.Identifier)
            ElseIf posi = SegmentRelationships.DownStreamOverlap Then
                Return String.Format(OverloapsDownStream, data.Identifier)
            ElseIf posi = SegmentRelationships.UpStreamOverlap Then
                Return String.Format(OverlapsUpStream, data.Identifier)
            Else
                Return String.Format(PromoterRegion, data.Identifier)
            End If
        End Function
    End Module
End Namespace