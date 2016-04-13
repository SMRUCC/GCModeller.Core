
''' <summary>
''' Enzyme Commission Number
''' </summary>
''' <remarks>EC-6.1.1.10</remarks>
Public Class ECNumber

    Public Enum ClassTypes
        ''' <summary>
        ''' 氧化还原酶
        ''' </summary>
        ''' <remarks></remarks>
        OxidoReductase = 1
        ''' <summary>
        ''' 转移酶
        ''' </summary>
        ''' <remarks></remarks>
        Transferase = 2
        ''' <summary>
        ''' 水解酶
        ''' </summary>
        ''' <remarks></remarks>
        Hydrolase = 3
        ''' <summary>
        ''' 裂合酶
        ''' </summary>
        ''' <remarks></remarks>
        Lyase = 4
        ''' <summary>
        ''' 异构酶
        ''' </summary>
        ''' <remarks></remarks>
        Isomerase = 5
        ''' <summary>
        ''' 合成酶
        ''' </summary>
        ''' <remarks></remarks>
        Synthetase = 6
    End Enum

    ''' <summary>
    ''' EC编号里面的第一个数字代表酶的分类号
    ''' </summary>
    ''' <remarks></remarks>
    <Xml.Serialization.XmlAttribute> Public Type As ECNumber.ClassTypes

    ''' <summary>
    ''' 该大类之下的亚分类
    ''' </summary>
    ''' <remarks></remarks>
    <Xml.Serialization.XmlAttribute> Public SubType As Integer
    ''' <summary>
    ''' 该亚类之下的小分类
    ''' </summary>
    ''' <remarks></remarks>
    <Xml.Serialization.XmlAttribute> Public SubCategory As Integer

    ''' <summary>
    ''' 该小分类之下的序号
    ''' </summary>
    ''' <remarks></remarks>
    <Xml.Serialization.XmlAttribute> Public SerialNumber As Integer

    Public Shared Widening Operator CType(s As String) As ECNumber
        Dim Regex As New System.Text.RegularExpressions.Regex("/d[.]/d+[.]/d+[.]/d+")
        Dim Match = Regex.Match(input:=s)

        If Match.Success Then
            s = Match.Value
            Dim Tokens As String() = s.Split(".")
            Dim NewObj As New ECNumber

            NewObj.Type = CInt(Val(Tokens(0)))
            NewObj.SubType = CInt(Val(Tokens(1)))
            NewObj.SubCategory = CInt(Val(Tokens(2)))
            NewObj.SerialNumber = CInt(Val(Tokens(3)))

            If NewObj.Type > 6 OrElse NewObj.Type < 0 Then
                Return Nothing  '格式错误
            Else
                Return NewObj
            End If
        Else  '格式错误，没有找到相应的编号格式字符串
            Return Nothing
        End If
    End Operator

    ''' <summary>
    ''' IDE debug
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function ToString() As String
        Return String.Format("EC-{0}.{1}.{2}.{3}", CInt(Type), SubType, SubCategory, SerialNumber)
    End Function
End Class
