Imports Microsoft.VisualBasic.Linq.Extensions
Imports System.Xml.Serialization

Namespace SequenceModel.NucleotideModels.Translation

    ''' <summary>
    ''' 密码子对象
    ''' </summary>
    Public Class Codon

        ''' <summary>
        ''' 密码子中的第一个碱基
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property X As NucleicAcid.Deoxyribonucleotides
        ''' <summary>
        ''' 密码子中的第二个碱基
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property Y As NucleicAcid.Deoxyribonucleotides
        ''' <summary>
        ''' 密码子中的第三个碱基
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property Z As NucleicAcid.Deoxyribonucleotides

        '''' <param name="X">密码子中的第一个碱基</param>
        '''' <param name="Y">密码子中的第二个碱基</param>
        '''' <param name="Z">密码子中的第三个碱基</param>
        Public Shared Function CalTranslHash(X As NucleicAcid.Deoxyribonucleotides,
                                             Y As NucleicAcid.Deoxyribonucleotides,
                                             Z As NucleicAcid.Deoxyribonucleotides) As Integer
            Return X * 1000 + Y * 100 + Z * 10000
        End Function

        ''' <summary>
        ''' 非翻译用途的
        ''' </summary>
        Sub New()
        End Sub

        ''' <summary>
        ''' 翻译用途的
        ''' </summary>
        ''' <param name="Tokens"></param>
        Friend Sub New(Tokens As String())
            If Tokens.Length = 4 Then
                IsInitCodon = True
            End If

            If String.Equals(Tokens(1), "*") Then
                IsStopCodon = True
            End If

            Dim Codon = Tokens(Scan0).ToArray(Function(ntch) NucleicAcid.NucleotideConvert(ntch))
            X = Codon(Scan0)
            Y = Codon(1)
            Z = Codon(2)
        End Sub

        ''' <summary>
        ''' 第一个碱基*1000+第二个碱基*100+第三个碱基
        ''' </summary>
        ''' <returns>哈希值</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property TranslHash As Integer
            Get
                Return CalTranslHash(X, Y, Z)
            End Get
        End Property

        ''' <summary>
        ''' 返回三联体密码子的核酸片段
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CodonValue As String
            Get
                Return NucleicAcid.ToString({X, Y, Z})
            End Get
        End Property

        ''' <summary>
        ''' 是否为终止密码子
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property IsStopCodon As Boolean
        Public ReadOnly Property IsInitCodon As Boolean

        Public Overrides Function ToString() As String
            If IsStopCoden Then
                Return $"[STOP_CODON] {Me.CodonValue}"
            Else
                Return $"[{TranslationTable.CodenTable(Me.TranslHash).ToString}] {Me.CodonValue}"
            End If
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            If obj.GetType = GetType(Codon) Then
                Dim cd = DirectCast(obj, Codon)
                Return cd.X = X AndAlso cd.Y = Y AndAlso cd.Z = Z
            Else
                Return False
            End If
        End Function

        Public Overloads Shared Narrowing Operator CType(obj As Codon) As Integer
            Return obj.TranslHash
        End Operator

        ''' <summary>
        ''' 生成非翻译用途的密码子列表
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function CreateHashTable() As Codon()
            Dim NNCols As NucleicAcid.Deoxyribonucleotides() = {
                SequenceModel.NucleotideModels.NucleicAcid.Deoxyribonucleotides.dAMP,
                SequenceModel.NucleotideModels.NucleicAcid.Deoxyribonucleotides.dCMP,
                SequenceModel.NucleotideModels.NucleicAcid.Deoxyribonucleotides.dGMP,
                SequenceModel.NucleotideModels.NucleicAcid.Deoxyribonucleotides.dTMP
            }
            Dim Combos = Microsoft.VisualBasic.ComponentModel.Comb.CreateCombos(NNCols, NNCols)
            Dim TripleCombos = Microsoft.VisualBasic.ComponentModel.Comb.CreateCombos(Combos, NNCols)
            Dim Codens = (From coden As KeyValuePair(Of
                              KeyValuePair(Of
                              NucleicAcid.Deoxyribonucleotides,
                              NucleicAcid.Deoxyribonucleotides),
                              NucleicAcid.Deoxyribonucleotides)
                          In TripleCombos
                          Select New Codon With {
                              .X = coden.Key.Key,
                              .Y = coden.Key.Value,
                              .Z = coden.Value}).ToArray
            Return Codens
        End Function
    End Class
End Namespace