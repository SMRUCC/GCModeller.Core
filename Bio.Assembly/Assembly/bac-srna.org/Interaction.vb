Imports LANS.SystemsBiology.ComponentModel.Loci
Imports System.Xml.Serialization

Namespace Assembly.Bac_sRNA.org

    Public Class Interaction

        <XmlAttribute> Public Property sRNAid As String
        <XmlElement> Public Property Organism As String
        <XmlAttribute> Public Property Name As String
        <XmlElement> Public Property Regulation As String
        <XmlElement> Public Property TargetName As String

        ''' <summary>
        ''' Reference (PMID)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlAttribute> Public Property Reference As String

        Public Overrides Function ToString() As String
            Return sRNAid
        End Function
    End Class
End Namespace