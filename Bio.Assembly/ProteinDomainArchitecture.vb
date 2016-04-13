Imports LANS.SystemsBiology.Assembly.Extensions

''' <summary>
''' 一个用于描述蛋白质结构域分布的数据结构
''' </summary>
''' <remarks></remarks>
Public Class ProteinDomainArchitecture ': Implements Generic.IEnumerable(Of Domain)
    Implements Assembly.Collection.Generic.IEnumerable.IAccessionIdEnumerable

    ''' <summary>
    ''' 该目标蛋白质的唯一标识符
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Xml.Serialization.XmlAttribute> Public Property UniqueId As String _
        Implements Collection.Generic.IEnumerable.IAccessionIdEnumerable.UniqueId

    <Xml.Serialization.XmlElement> Public Property Description As String
    <Xml.Serialization.XmlElement> Public Property Seq As String
    ''' <summary>
    ''' 结构域分布
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property Domains As DomainObject()

    Public ReadOnly Property Length As Integer
        Get
            Return Len(Seq)
        End Get
    End Property

    Default Public ReadOnly Property Domain(index As Integer) As DomainObject
        Get
            Return Domains(index)
        End Get
    End Property

    Public Function Export() As Assembly.SequenceModel.FASTA.File.FASTA
        Dim Fsa As Assembly.SequenceModel.FASTA.File.FASTA = New SequenceModel.FASTA.File.FASTA
        Fsa.Sequence = Seq
        Fsa.Attributes = New String() {UniqueId, Description}
        Return Fsa
    End Function

    Public Class DomainObject : Inherits CDD.Smp
        Implements Assembly.Collection.Generic.IEnumerable.IAccessionIdEnumerable

        Public Property Position As CommonElements.Location
        <Xml.Serialization.XmlAttribute> Public Overrides Property AccessionId As String _
            Implements Collection.Generic.IEnumerable.IAccessionIdEnumerable.UniqueId

        Public Overrides Function ToString() As String
            Return MyBase.AccessionId
        End Function

        ''' <summary>
        ''' 获取与本结构域相互作用的结构域的ID
        ''' </summary>
        ''' <param name="DOMINE"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetInteractionDomains(DOMINE As DOMINE.Database) As String()
            Dim Interactions = DOMINE.Interaction
            Dim LQuery = From Interaction As LANS.SystemsBiology.Assembly.DOMINE.Tables.Interaction
                         In Interactions
                         Let DomainId As String = Interaction.GetInteractionDomain(MyBase.AccessionId)
                         Where Not String.IsNullOrEmpty(DomainId)
                         Select DomainId '
            Return LQuery.ToArray
        End Function

        Sub New(SmpFile As CDD.Smp)
            MyBase.AccessionId = SmpFile.AccessionId
            MyBase.CommonName = SmpFile.CommonName
            MyBase.Describes = SmpFile.Describes
            MyBase.SeqData = SmpFile.SeqData
            MyBase.TagId = SmpFile.TagId
            MyBase.Title = SmpFile.Title
        End Sub

        Sub New()
        End Sub
    End Class

    Public Function ContainsDomain(DomainAccession As String) As Boolean
        Dim LQuery = From Domain In Domains
                     Where String.Equals(DomainAccession, Domain.AccessionId)
                     Select 1 '
        If LQuery.ToArray.IsNullOrEmpty Then
            Return False
        Else
            Return True
        End If
    End Function

    ''' <summary>
    ''' 本蛋白质之中是否包含有目标结构域编号列表中的任何结构域信息，返回所包含的编号列表
    ''' </summary>
    ''' <param name="DomainAccessions"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function ContainsDomain(DomainAccessions As String()) As String()
        Dim LQuery = From Id As String In DomainAccessions Where ContainsDomain(DomainAccession:=Id) = True Select Id '
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
            Dim InteractionList As List(Of String) = New List(Of String)
            For Each DomainObject In Me.Domains
                Call InteractionList.AddRange(DomainObject.GetInteractionDomains(DOMINE))
            Next

            Return InteractionList.Distinct.ToArray
        End If
    End Function

    ''' <summary>
    ''' 简单的根据结构域分布来判断两个蛋白质是否相似，两个蛋白质的结构域分布必须以相似的方式进行排布
    ''' </summary>
    ''' <param name="Protein1"></param>
    ''' <param name="Protein2"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function SimilarTo(Protein1 As ProteinDomainArchitecture, Protein2 As ProteinDomainArchitecture, Optional LengthError As Double = 0.05) As Boolean
        Dim p As Integer = 0, matchCounts As Integer

        For Each DomainObject In Protein1.Domains
            Dim id As String = DomainObject.AccessionId

            Do While p < Protein2.Domains.Count
                Dim Id2 As String = Protein2.Domains(p).AccessionId
                If String.Equals(id, Id2) Then
                    matchCounts += 1
                    Exit Do
                Else
                    p += 1
                End If
            Loop

            If p = Protein2.Domains.Count Then
                Exit For
            End If
        Next

        Return (matchCounts = Protein1.Domains.Count) AndAlso (Math.Abs(Protein1.Length - Protein2.Length) / Math.Min(Protein1.Length, Protein2.Length) < LengthError)
    End Function

    Public Overrides Function ToString() As String
        Return UniqueId
    End Function

    'Public Iterator Function GetEnumerator() As IEnumerator(Of Domain) Implements IEnumerable(Of Domain).GetEnumerator
    '    For i As Integer = 0 To Me.Domains.Count - 1
    '        Yield Me.Domains(i)
    '    Next
    'End Function

    'Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
    '    Yield GetEnumerator()
    'End Function
End Class
