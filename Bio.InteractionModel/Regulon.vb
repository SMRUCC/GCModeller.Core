Imports System.Runtime.CompilerServices

''' <summary>
''' Regulon的模型的抽象
''' </summary>
Public Interface IRegulon

    Property TFlocusId As String
    Property RegulatedGenes As String()

End Interface

''' <summary>
''' 调控作用的抽象
''' </summary>
Public Interface IRegulatorRegulation

    ''' <summary>
    ''' Gene id
    ''' </summary>
    ''' <returns></returns>
    Property LocusId As String
    ''' <summary>
    ''' Regulators gene id
    ''' </summary>
    ''' <returns></returns>
    Property Regulators As String()

End Interface

Public Class RegulatorRegulation
    Implements IRegulatorRegulation

    Public Property LocusId As String Implements IRegulatorRegulation.LocusId
    Public Property Regulators As String() Implements IRegulatorRegulation.Regulators
End Class

Public Interface ISpecificRegulation
    Property LocusId As String
    Property Regulator As String
End Interface

Public MustInherit Class Regulon : Implements IRegulon

    Public Property Id As String
    Public Property RegulatedGenes As String() Implements IRegulon.RegulatedGenes
    Public Property Regulator As String Implements IRegulon.TFlocusId
End Class

''' <summary>
''' 用于生成调控数据的数据库的接口
''' </summary>
Public Interface IRegulationDatabase
    ''' <summary>
    ''' 得到数据库之中的所有的调控因子的编号
    ''' </summary>
    ''' <returns></returns>
    Function listRegulators() As String()
    Function IsRegulates(regulator As String, site As String) As Boolean
    Function GetRegulators(site As String) As String()
    Function GetRegulatesSites(regulator As String) As String()
End Interface

Public Structure RelationshipScore
    ''' <summary>
    ''' 通常为Regulator
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property InteractorA As String
    ''' <summary>
    ''' 通常为目标调控对象
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property InteractorB As String
    Public Property Score As Double

    Public Function GetConnectedId(Id As String) As String
        If String.Equals(InteractorA, Id) Then
            Return InteractorB
        ElseIf String.Equals(InteractorB, Id) Then
            Return InteractorA
        Else
            Return ""
        End If
    End Function
End Structure

Public Module RegulationModel

    Public Structure Regulon
        Implements IRegulon

        Public Property RegulatedGenes As String() Implements IRegulon.RegulatedGenes

        Public Property Regulator As String Implements IRegulon.TFlocusId
    End Structure

    <Extension> Public Function GetRegulators(Of TRegulation As IRegulatorRegulation)(data As IEnumerable(Of TRegulation)) As String()
        Dim LQuery = (From item In data Select item.Regulators).ToArray.MatrixToVector.Distinct.ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="TRegulation"></typeparam>
    ''' <param name="Regulations">已经经过<see cref="Trim"></see>操作所处理的关系对</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function SignificantModel(Of TRegulation As IRegulatorRegulation)(Regulations As IEnumerable(Of TRegulation)) As RelationshipScore()
        Dim LQuery = (From item As TRegulation In Regulations
                      Where Not item.Regulators.IsNullOrEmpty
                      Let Score As Double = 1 / item.Regulators.Length
                      Select (From Regulator As String In item.Regulators
                              Select New RelationshipScore With {
                                  .InteractorA = Regulator,
                                  .InteractorB = item.LocusId,
                                  .Score = Score}).ToArray).ToArray.MatrixToVector
        Return LQuery
    End Function

    Public Function GenerateRegulons(Of TRegulon As IRegulon)(Regulations As IEnumerable(Of IRegulatorRegulation)) As Regulon()
        Dim Regulators = (From item As IRegulatorRegulation In Regulations
                          Let RegulatorIdlist As String() = item.Regulators
                          Where Not RegulatorIdlist.IsNullOrEmpty
                          Select RegulatorIdlist).ToArray.MatrixToVector.Distinct.ToArray
        Dim LQuery = (From Regulator As String
                      In Regulators.AsParallel
                      Let RegulatedGene As String() = (From item In Regulations Where Array.IndexOf(item.Regulators, Regulator) > -1 Select item.LocusId Distinct).ToArray
                      Let Regulon As Regulon = New Regulon With {.Regulator = Regulator, .RegulatedGenes = RegulatedGene}
                      Select Regulon).ToArray
        Return LQuery
    End Function

    ''' <summary>
    ''' 去除不完整的调控数据，即将所有的不包含有目标基因或者不包含有调控因子的数据记录进行剔除
    ''' </summary>
    ''' <typeparam name="TRegulation"></typeparam>
    ''' <param name="Regulations"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <Extension> Public Function Trim(Of TRegulation As IRegulatorRegulation)(Regulations As IEnumerable(Of TRegulation)) As RegulatorRegulation()
        Dim GeneIdList = (From item As TRegulation In Regulations
                          Where Not String.IsNullOrEmpty(item.LocusId)
                          Select item.LocusId
                          Distinct).ToArray
        Dim LQuery = (From locusId As String
                      In GeneIdList.AsParallel
                      Select locusId,
                          Regulators = (From item As TRegulation In Regulations
                                        Where String.Equals(locusId, item.LocusId)
                                        Let Regulators = item.Regulators
                                        Where Not Regulators.IsNullOrEmpty
                                        Select Regulators).ToArray.MatrixToVector.Distinct.ToArray).ToArray
        Dim ObjectCreation = (From Pair In LQuery
                              Let Regulation As RegulatorRegulation = New RegulatorRegulation With {.LocusId = Pair.locusId, .Regulators = Pair.Regulators}
                              Select Regulation).ToArray
        Return ObjectCreation
    End Function
End Module