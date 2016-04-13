Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports LANS.SystemsBiology.ProteinModel
Imports LANS.SystemsBiology.SequenceModel
Imports Microsoft.VisualBasic

Namespace Assembly.MiST2

    ''' <summary>
    ''' Signal Transduction Protein
    ''' </summary>
    ''' <remarks></remarks>
    '''
    Public Class Transducin : Inherits LANS.SystemsBiology.ProteinModel.Protein

        Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable

        <XmlElement> Public Property Inputs As String()
        <XmlElement> Public Property Outputs As String()
        ''' <summary>
        ''' Image file url
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlAttribute> Public Property ImageUrl As String
        <XmlAttribute> Public Property GeneName As String
        <XmlAttribute> Public Property [Class] As String

        Public Overrides Function ToString() As String
            Return Identifier
        End Function

        ''' <summary>
        ''' 从DomainImage字符串之中解析出结构域数据，并返回自身
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' http://mistdb.com/arch.php?l=646;seg(115|136)+seg(242|254)+seg(517|521)+coil(266|286)+PAS_4(145|244)+HisKA(284|350)+HK_CA:3(353|508)+RR(8|127)+RR(530|646)+
        ''' </remarks>
        Public Function GetDomainArchitecture() As LANS.SystemsBiology.ProteinModel.Protein
            Dim strTemp As String = Mid(ImageUrl, InStr(ImageUrl, ";") + 1)
            Dim Domains As String() = strTemp.Split(CChar("+"))
            Dim LQuery = (From str As String
                          In Domains.Take(Domains.Count - 1)
                          Let dm = __convert(str)
                          Select dm
                          Order By dm.Position.Left Ascending).ToArray

            MyBase.Domains = LQuery

            Return Me
        End Function

        Private Function __convert(pfam As String) As LANS.SystemsBiology.ProteinModel.DomainObject
            Dim p As Integer = InStr(pfam, "(")
            Dim TempTokens As String() = (From m As Match
                                          In Regex.Matches(Mid(pfam, p), "\d+")
                                          Select m.Value).ToArray
            Dim Domain As LANS.SystemsBiology.ProteinModel.DomainObject =
                New DomainObject With {
                    .Identifier = Mid(pfam, 1, p - 1)
            }
            Domain.CommonName = Domain.Identifier
            Domain.Position = New ComponentModel.Loci.Location With {
                .Left = System.Convert.ToInt64(TempTokens(0)),
                .Right = System.Convert.ToInt64(TempTokens(1))
            }
            Return Domain
        End Function
    End Class

    Public Class TwoComponent

        ''' <summary>
        ''' Histidine kinase
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlArray("HisK")> Public Property HisK As Transducin()
        ''' <summary>
        ''' Hybrid histidine kinase
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlArray("HybridHisK")> Public Property HHK As Transducin()
        ''' <summary>
        ''' Response regulator
        ''' </summary>
        ''' <remarks></remarks>
        <XmlArray("RespRegulator")> Public Property RR As Transducin()
        ''' <summary>
        ''' Hybrid response regulator
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlArray("HybridRR")> Public Property HRR As Transducin()
        <XmlArray("Others")> Public Property Other As Transducin()

        ''' <summary>
        ''' 获取所有的双组份系统之中的RR蛋白质的基因编号
        ''' </summary>
        ''' <returns></returns>
        Public Function GetRR() As String()
            Dim LQuery = (From transducin As Transducin
                          In {RR, HRR}.MatrixToVector
                          Select transducin.Identifier
                          Distinct).ToArray
            Return LQuery
        End Function

        ''' <summary>
        ''' 获取所有的双组份系统之中的Hisk蛋白质的基因编号
        ''' </summary>
        ''' <returns></returns>
        Public Function get_HisKinase() As String()
            Dim LQuery = (From item In {HisK, HHK}.MatrixToVector Select item.Identifier Distinct).ToArray
            Return LQuery
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("HK: {0}, HHK: {1}, RR: {2}, HRR: {3}, Other: {4}", HisK.Count, HHK.Count, RR.Count, HRR.Count, Other.Count)
        End Function

        Public Shared Narrowing Operator CType(TwoComponent As TwoComponent) As Transducin()
            Dim List As List(Of Transducin) = New List(Of Transducin)
            Call List.AddRange(TwoComponent.HisK)
            Call List.AddRange(TwoComponent.HHK)
            Call List.AddRange(TwoComponent.RR)
            Call List.AddRange(TwoComponent.HRR)
            Call List.AddRange(TwoComponent.Other)

            Return List.ToArray
        End Operator
    End Class

    ''' <summary>
    ''' 基因组之中的一个复制子
    ''' </summary>
    Public Class Replicon

        <XmlAttribute> Public Property Accession As String
        <XmlAttribute> Public Property Replicon As String

        ''' <summary>
        ''' Replicon Size (Mbp)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlAttribute> Public Property Size As String

        ''' <summary>
        ''' One-component
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property OneComponent As Transducin()
        Public Property TwoComponent As TwoComponent
        Public Property Chemotaxis As Transducin()
        ''' <summary>
        ''' Extra Cytoplasmic Function
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlArray("ExtraCytoFunc")> Public Property ECF As Transducin()
        Public Property Other As Transducin()

        Public Overrides Function ToString() As String
            Return String.Format("({0}) {1}, {2}Mbp", Replicon, Accession, Size)
        End Function

        ''' <summary>
        ''' 获取在本对象中所存储的所有的蛋白质对象
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function SignalProteinCollection() As Transducin()
            Dim List As List(Of Transducin) = New List(Of Transducin)
            Call List.AddRange(OneComponent)
            Call List.AddRange(CType(TwoComponent, Assembly.MiST2.Transducin()))
            Call List.AddRange(Chemotaxis)
            Call List.AddRange(ECF)
            Call List.AddRange(Other)

            Return List.ToArray
        End Function
    End Class

End Namespace