﻿Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports LANS.SystemsBiology.ComponentModel.EquaionModel
Imports LANS.SystemsBiology.Assembly.KEGG.WebServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq

Namespace Assembly.KEGG.DBGET.bGetObject

    ''' <summary>
    ''' KEGG reaction annotation data.
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Reaction : Implements sIdEnumerable

        <XmlAttribute> Public Property Entry As String Implements sIdEnumerable.Identifier
        Public Property CommonNames As String()
        Public Property Definition As String
        Public Property Equation As String

        ''' <summary>
        ''' 标号： <see cref="Expasy.Database.Enzyme.Identification"></see>
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property ECNum As String()
        Public Property Comments As String
        Public Property Pathway As KeyValuePair()
        Public Property [Module] As KeyValuePair()
        Public Property Orthology As TripleKeyValuesPair()

        Const URL As String = "http://www.kegg.jp/dbget-bin/www_bget?rn:{0}"

        Public ReadOnly Property ReactionModel As DefaultTypes.Equation
            Get
                Try
                    Return EquationBuilder.CreateObject(Me.Equation)
                Catch ex As Exception
                    ex = New Exception(Me.ToString, ex)
                    Throw ex
                End Try
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return String.Format("[{0}] {1}:  {2}", ECNum, Entry, Definition)
        End Function

        Public Shared Function Download(Id As String) As Reaction
            Return DownloadFrom(String.Format(URL, Id))
        End Function

        Public Shared Function DownloadFrom(url As String) As Reaction
            Dim WebForm = New InternalWebFormParsers.WebForm(url)

            If WebForm.Count = 0 Then
                Return Nothing
            Else
                Return __webFormParser(Of Reaction)(WebForm)
            End If
        End Function

        Friend Shared Function __webFormParser(Of ReactionType As Reaction)(WebForm As InternalWebFormParsers.WebForm) As ReactionType
            Dim Reaction As ReactionType = Activator.CreateInstance(Of ReactionType)()

            On Error Resume Next

            Reaction.Entry = Regex.Match(WebForm.GetValue("Entry").FirstOrDefault, "[a-z]+\d+", RegexOptions.IgnoreCase).Value
            Reaction.Comments = __trimComments(WebForm.GetValue("Comment").FirstOrDefault)
            Reaction.Definition = WebForm.GetValue("Definition").FirstOrDefault.Replace("<br>", "").Replace("&lt;", "<").Replace("&gt;", ">")
            Reaction.Pathway = KEGG.WebServices.InternalWebFormParsers.WebForm.parseList(WebForm.GetValue("Pathway").FirstOrDefault, "<a href="".+?"">.+?</a>")
            Reaction.Module = KEGG.WebServices.InternalWebFormParsers.WebForm.parseList(WebForm.GetValue("Module").FirstOrDefault, "<a href="".+?"">.+?</a>")
            Reaction.CommonNames = __getCommonNames(WebForm.GetValue("Name").FirstOrDefault)
            Reaction.Equation = __parsingEquation(WebForm.GetValue("Equation").FirstOrDefault)
            Reaction.Orthology = __orthologyParser(WebForm.GetValue("Orthology").FirstOrDefault)

            Dim ecTmp As String = WebForm.GetValue("Enzyme").FirstOrDefault
            Reaction.ECNum = Regex.Matches(ecTmp, "\d+(\.\d+)+").ToArray.Distinct.ToArray

            Return Reaction
        End Function

        Private Shared Function __orthologyParser(s As String) As TripleKeyValuesPair()
            Dim ms As String() = Regex.Matches(s, "K\d+<.+?\[EC.+?\]", RegexOptions.IgnoreCase).ToArray
            Dim result As TripleKeyValuesPair() = ms.ToArray(Function(x) __innerOrthParser(x))
            Return result
        End Function

        ''' <summary>
        ''' K01509&lt;/a> adenosinetriphosphatase [EC:&lt;a href="/dbget-bin/www_bget?ec:3.6.1.3">3.6.1.3&lt;/a>]
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Private Shared Function __innerOrthParser(s As String) As TripleKeyValuesPair
            Dim Tokens As String() = Regex.Split(s, "<[/]?a>", RegexOptions.IgnoreCase)
            Dim KO As String = Tokens.Get(Scan0)
            Dim def As String = Tokens.Get(1).Split("["c).First.Trim
            Dim EC As String = Regex.Match(s, "\d+(\.\d+)+").Value
            Return New TripleKeyValuesPair With {
                .Key = KO,
                .Value1 = EC,
                .Value2 = def
            }
        End Function

        Public ReadOnly Property Reversible As Boolean
            Get
                Return InStr(Equation, " <=> ") > 0
            End Get
        End Property

        Const KEGG_COMPOUND_ID As String = "[A-Z]+\d+"

        ''' <summary>
        ''' 得到本反应过程对象中的所有的代谢底物的KEGG编号，以便于查询和下载
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSubstrateCompounds() As String()
            Dim FluxModel = EquationBuilder.CreateObject(Of DefaultTypes.CompoundSpecieReference, DefaultTypes.Equation)(Regex.Replace(Equation, "(\s*\(.+?\))|(n )", ""))
            Dim Compounds = (From csr As DefaultTypes.CompoundSpecieReference
                             In {FluxModel.Reactants, FluxModel.Products}.MatrixAsIterator
                             Select csr.Identifier
                             Distinct).ToArray
            Return Compounds
        End Function

        Public Function IsConnectWith([next] As Reaction) As Boolean
            Dim a = GetSubstrateCompounds(), b = [next].GetSubstrateCompounds
            For Each s In a
                If Array.IndexOf(b, s) > -1 Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="lstId"></param>
        ''' <param name="outDIR"></param>
        ''' <returns>返回成功下载的对象的数目</returns>
        ''' <remarks></remarks>
        Public Shared Function FetchTo(lstId As String(), outDIR As String) As Integer
            Dim i As Integer = 0

            For Each Id As String In lstId
                Dim ReactionData As Reaction = Download(Id)
                If ReactionData Is Nothing Then
                    Continue For
                End If

                Dim Path As String = String.Format("{0}/{1}.xml", outDIR, Id)
                Call ReactionData.GetXml.SaveTo(Path)
            Next

            Return i
        End Function

        Private Shared Function __trimComments(strData As String) As String
            If String.IsNullOrEmpty(strData) Then
                Return ""
            End If

            Dim Links As KeyValuePair(Of String, String)() =
                Regex.Matches(strData, "<a href="".+?"">.+?</a>").ToArray(
                Function(m) New KeyValuePair(Of String, String)(m, m.GetValue))
            Dim sBuilder As StringBuilder = New StringBuilder(strData)
            For Each item As KeyValuePair(Of String, String) In Links
                Call sBuilder.Replace(item.Key, item.Value)
            Next
            Call sBuilder.Replace("<br>", "")

            Return sBuilder.ToString
        End Function

        Private Shared Function __parsingEquation(strData As String) As String
            Dim Links As KeyValuePair(Of String, String)() =
                Regex.Matches(strData, "<a href="".+?"">.+?</a>").ToArray(
                Function(m) New KeyValuePair(Of String, String)(m, m.GetValue))
            Dim sBuilder As StringBuilder = New StringBuilder(strData)
            For Each item In Links
                Call sBuilder.Replace(item.Key, item.Value)
            Next
            Call sBuilder.Replace("<br>", "")
            Call sBuilder.Replace("&lt;", "<")
            Call sBuilder.Replace("&gt;", ">")

            Dim s$ = ""

            Return sBuilder.ToString
        End Function

        Private Shared Function __getCommonNames(strData As String) As String()
            Return (From stritem As String
                    In Strings.Split(strData, "<br>")
                    Where Not String.IsNullOrEmpty(stritem)
                    Select stritem).ToArray
        End Function
    End Class
End Namespace