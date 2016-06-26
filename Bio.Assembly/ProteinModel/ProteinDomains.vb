﻿Imports LANS.SystemsBiology.Assembly
Imports LANS.SystemsBiology.SequenceModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ProteinModel

    ''' <summary>
    ''' A type of data structure for descript the protein domain architecture distribution.(一个用于描述蛋白质结构域分布的数据结构)
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <XmlType("ProtDomains", [Namespace]:="http://gcmodeller.org/models/protein")>
    Public Class Protein : Implements sIdEnumerable, I_PolymerSequenceModel

        ''' <summary>
        ''' 该目标蛋白质的唯一标识符
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlAttribute("Identifier", [Namespace]:="http://gcmodeller.org/programming/language/visualbasic/Identifier")>
        Public Overridable Property Identifier As String Implements sIdEnumerable.Identifier
        Public Property Organism As String

        <XmlElement> Public Property Description As String
        <XmlElement> Public Property SequenceData As String Implements I_PolymerSequenceModel.SequenceData
        ''' <summary>
        ''' 结构域分布
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Domains As DomainObject()

        Public ReadOnly Property Length As Integer
            Get
                Return Len(SequenceData)
            End Get
        End Property

        Default Public ReadOnly Property Domain(index As Integer) As DomainObject
            Get
                Return Domains(index)
            End Get
        End Property

        Public Function EXPORT() As FASTA.FastaToken
            Return New FASTA.FastaToken({Identifier, Description}, SequenceData)
        End Function

        Public Function ContainsDomain(DomainAccession As String) As Boolean
            Dim LQuery = From Domain In Domains
                         Where String.Equals(DomainAccession, Domain.Identifier)
                         Select 100 '
            Return LQuery.FirstOrDefault > 50
        End Function

        ''' <summary>
        ''' 本蛋白质之中是否包含有目标结构域编号列表中的任何结构域信息，返回所包含的编号列表
        ''' </summary>
        ''' <param name="DomainAccessions"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ContainsAnyDomain(DomainAccessions As IEnumerable(Of String)) As String()
            Dim LQuery = From Id As String
                         In DomainAccessions
                         Where ContainsDomain(DomainAccession:=Id) = True
                         Select Id '
            Return LQuery.ToArray
        End Function

        ''' <summary>
        ''' 获取与本蛋白质相互作用的结构域列表
        ''' </summary>
        ''' <param name="DOMINE"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function InteractionWith(DOMINE As DOMINE.Database) As String()
            If Me.Domains.IsNullOrEmpty Then
                Return New String() {}
            Else
                Dim lstInteracts As IEnumerable(Of String) = (From motif As DomainObject
                                                              In Me.Domains
                                                              Select motif.GetInteractionDomains(DOMINE)).MatrixAsIterator
                Return lstInteracts.Distinct.ToArray
            End If
        End Function

        ''' <summary>
        ''' 简单的根据结构域分布来判断两个蛋白质是否相似，两个蛋白质的结构域分布必须以相似的方式进行排布
        ''' </summary>
        ''' <param name="Protein1"></param>
        ''' <param name="Protein2"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function SimilarTo(Protein1 As Protein, Protein2 As Protein, Optional threshold As Double = 0.3) As Boolean
            Dim a As String() = Protein1.Domains.ToArray(Function(x) x.Identifier)
            Dim b As String() = Protein2.Domains.ToArray(Function(x) x.Identifier)
            Dim edits As DistResult = LevenshteinDistance.ComputeDistance(a, b, AddressOf __equals, Function(c) "")
            Return edits.MatchSimilarity >= threshold
        End Function

        Private Shared Function __equals(m1 As String, m2 As String) As Boolean
            Return String.Equals(m1, m2, StringComparison.OrdinalIgnoreCase)
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class
End Namespace