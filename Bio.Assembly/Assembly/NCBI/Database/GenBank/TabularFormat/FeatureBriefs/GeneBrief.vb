﻿Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports LANS.SystemsBiology.ComponentModel.Loci
Imports LANS.SystemsBiology.ComponentModel
Imports LANS.SystemsBiology.ComponentModel.Loci.NucleotideLocation

Namespace Assembly.NCBI.GenBank.TabularFormat.ComponentModels

    ''' <summary>
    ''' The gene brief information data in a ptt document.(Ptt文件之中的一行，即一个基因的对象摘要信息)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class GeneBrief : Implements sIdEnumerable
        Implements I_GeneBrief

        ''' <summary>
        ''' 包含有Ptt文件之中的Location, Strand和Length列
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Location As ComponentModel.Loci.NucleotideLocation Implements I_GeneBrief.Location
        <XmlAttribute> Public Property PID As String
        ''' <summary>
        ''' 基因号，这个应该是GI编号，而非平常比较熟悉的字符串编号
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlAttribute> Public Property Gene As String
        <XmlAttribute> Public Property Code As String
        <XmlAttribute> Public Property COG As String Implements I_GeneBrief.COG
        ''' <summary>
        ''' 基因的蛋白质产物的功能的描述
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlText> Public Property Product As String Implements I_GeneBrief.Product
        <XmlAttribute> Public Property Length As Integer Implements I_GeneBrief.Length
        ''' <summary>
        ''' 我们所正常熟知的基因编号，<see cref="PTT"/>对象主要是使用这个属性值来生成字典对象的
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <XmlAttribute> Public Property Synonym As String Implements sIdEnumerable.Identifier

        ''' <summary>
        ''' *.ptt => TRUE;  *.rnt => FALSE
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property IsORF As Boolean = True

        Public Overrides Function ToString() As String
            Return String.Format("{0}: {1}", Gene, Product)
        End Function

        Public Function getCOGEntry(Of T_Entry As I_COGEntry)() As T_Entry
            Dim obj As T_Entry = Activator.CreateInstance(Of T_Entry)()
            obj.COG = COG
            obj.Length = Length
            obj.Product = Product
            obj.Identifier = Synonym

            Return obj
        End Function

        Public ReadOnly Property ATG As Long
            Get
                If Me.Location.Strand = Strands.Forward Then
                    Return Me.Location.Left
                Else
                    Return Me.Location.Right
                End If
            End Get
        End Property
        Public ReadOnly Property TGA As Long
            Get
                If Me.Location.Strand = Strands.Forward Then
                    Return Me.Location.Right
                Else
                    Return Me.Location.Left
                End If
            End Get
        End Property

        Public Shared Function CreateObject(obj As I_GeneBrief) As GeneBrief
            Return New GeneBrief With {
                .COG = obj.COG,
                .Length = obj.Length,
                .Location = obj.Location,
                .Product = obj.Product,
                .Synonym = obj.Identifier
            }
        End Function

        Public Shared Function CreateObject(data As Generic.IEnumerable(Of I_GeneBrief)) As GeneBrief()
            Dim LQuery As GeneBrief() = (From gene As I_GeneBrief
                                         In data.AsParallel
                                         Select CreateObject(obj:=gene)).ToArray
            Return LQuery
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="strLine">Ptt文档之中的一行数据</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function DocumentParser(strLine As String, FillBlankGeneName As Boolean) As GeneBrief
            Dim Tokens As String() = strLine.Split(CChar(vbTab))
            Dim Gene As GeneBrief = New GeneBrief

            Dim strLocation As Long() = (From str As String
                                         In Strings.Split(Tokens(Scan0), "..")
                                         Let n = CType(Val(str), Long)
                                         Select n).ToArray

            Gene.Location = New NucleotideLocation(
                strLocation(0), strLocation(1),
                If(Tokens(1)(0) = "+"c, Strands.Forward, Strands.Reverse))
            Call Gene.Location.Normalization()

            Dim p As Integer = 2
            Gene.Length = Tokens(p.MoveNext)
            Gene.PID = Tokens(p.MoveNext)
            Gene.Gene = Tokens(p.MoveNext)
            Gene.Synonym = Tokens(p.MoveNext)
            If (String.Equals(Gene.Gene, "-") OrElse String.IsNullOrEmpty(Gene.Gene)) AndAlso FillBlankGeneName Then
                Gene.Gene = Gene.Synonym  '假若基因名称为空值的话，假设填充则使用基因号进行填充
            End If
            Gene.Code = Tokens(p.MoveNext)
            Gene.COG = Tokens(p.MoveNext)
            Gene.Product = Tokens(p.MoveNext)

            Return Gene
        End Function

        ''' <summary>
        ''' 42..1370	+	442	66766353	dnaA	XC_0001	-	COG0593L	chromosome replication initiator DnaA
        ''' </summary>
        ''' <param name="strLine"></param>
        ''' <returns></returns>
        Public Shared Function DocumentParser(strLine As String) As GeneBrief
            Return DocumentParser(strLine, False)
        End Function

        ''' <summary>
        ''' 判断本对象是否是由<see cref="BlankSegment"></see>方法所生成的空白片段
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsBlankSegment As Boolean
            Get
                Return String.Equals(Gene, BLANK_VALUE) OrElse String.Equals(Synonym, BLANK_VALUE)
            End Get
        End Property

        Public Function Strand() As Char
            If Location.Strand = Strands.Forward Then
                Return "+"c
            ElseIf Location.Strand = Strands.Reverse Then
                Return "-"c
            Else
                Return "?"c
            End If
        End Function
    End Class
End Namespace