Imports System.Text
Imports LANS.SystemsBiology.SequenceModel.ISequenceModel
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic

Namespace SequenceModel.NucleotideModels

    ''' <summary>
    ''' The nucleotide sequence object.(核酸序列对象)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class NucleicAcid : Inherits SequenceModel.ISequenceModel

        ''' <summary>
        ''' Deoxyribonucleotides NT base which consist of the DNA sequence.(枚举所有的脱氧核糖核苷酸)
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum Deoxyribonucleotides As SByte
            ''' <summary>
            ''' Gaps/Rare bases(空格或者其他的稀有碱基)
            ''' </summary>
            ''' <remarks></remarks>
            NA = -1
            ''' <summary>
            ''' Adenine, paired with <see cref="Deoxyribonucleotides.dTMP"/>(A, 腺嘌呤)
            ''' </summary>
            dAMP = 0
            ''' <summary>
            ''' Guanine, paired with <see cref="Deoxyribonucleotides.dCMP"/>(G, 鸟嘌呤)
            ''' </summary>
            dGMP = 1
            ''' <summary>
            ''' Cytosine, paired with <see cref="Deoxyribonucleotides.dGMP"/>(C, 胞嘧啶)
            ''' </summary>
            dCMP = 2
            ''' <summary>
            ''' Thymine, paired with <see cref="Deoxyribonucleotides.dAMP"/>(T, 胸腺嘧啶)
            ''' </summary>
            dTMP = 3
        End Enum

        ''' <summary>
        ''' 大小写不敏感
        ''' </summary>
        Protected Friend Shared ReadOnly Property NucleotideConvert As Dictionary(Of Char, Deoxyribonucleotides) =
            New Dictionary(Of Char, Deoxyribonucleotides) From {
 _
                {"A"c, Deoxyribonucleotides.dAMP},
                {"T"c, Deoxyribonucleotides.dTMP},
                {"G"c, Deoxyribonucleotides.dGMP},
                {"C"c, Deoxyribonucleotides.dCMP},
                {"a"c, Deoxyribonucleotides.dAMP},
                {"t"c, Deoxyribonucleotides.dTMP},
                {"g"c, Deoxyribonucleotides.dGMP},
                {"c"c, Deoxyribonucleotides.dCMP}
        }

        ''' <summary>
        '''
        ''' </summary>
        Protected Friend Shared ReadOnly __nucleotideAsChar As Dictionary(Of Deoxyribonucleotides, Char) =
            New Dictionary(Of Deoxyribonucleotides, Char) From {
 _
                {Deoxyribonucleotides.dAMP, "A"c},
                {Deoxyribonucleotides.dCMP, "C"c},
                {Deoxyribonucleotides.dGMP, "G"c},
                {Deoxyribonucleotides.dTMP, "T"c},
                {Deoxyribonucleotides.NA, "-"c}
        }

        Public Shared Function ToChar(base As Deoxyribonucleotides) As Char
            Return __nucleotideAsChar(base)
        End Function

        ''' <summary>
        ''' 这个延时加载的设计好像并没有什么卵用
        ''' </summary>
        Dim _innerSeqModel As Lazy(Of List(Of Deoxyribonucleotides))

        ''' <summary>
        ''' Cache data for maintaining the high performance on sequence operation.
        ''' </summary>
        ''' <remarks></remarks>
        Dim _innerSeqCache As String

        Public Function ToArray() As Deoxyribonucleotides()
            Return _innerSeqModel.Value.ToArray
        End Function

        ''' <summary>
        ''' 用户定义的标签数据，有时候用于在不同的序列之间唯一的标记当前的这条序列
        ''' </summary>
        ''' <returns></returns>
        Public Property UserTag As String

        ''' <summary>
        ''' 字符串形式的序列数据
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Property SequenceData As String
            Get
                Return _innerSeqCache
            End Get
            Set(value As String)
                Me._innerSeqModel = New Lazy(Of List(Of Deoxyribonucleotides))(
                    Function() (From ch As Char In value
                                Let __getNA = If(NucleotideConvert.ContainsKey(ch), NucleotideConvert(ch), Deoxyribonucleotides.NA)
                                Select __getNA).ToList)
                Call __generateSeqCache()
            End Set
        End Property

        ''' <summary>
        ''' The melting temperature of P1 is Tm(P1), which is a reference temperature for a primer to perform annealing and known as the Wallace formula
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Tm As Double
            Get
                Return NucleicAcidStaticsProperty.Tm(Me.SequenceData)
            End Get
        End Property

        ''' <summary>
        ''' Calculate the GC content of the current sequence data.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property GC As Double
            Get
                Return NucleicAcidStaticsProperty.GCContent(Me)
            End Get
        End Property

        Sub New(Sequence As Generic.IEnumerable(Of LANS.SystemsBiology.SequenceModel.NucleotideModels.NucleicAcid.Deoxyribonucleotides))
            Me._innerSeqModel = New Microsoft.VisualBasic.Lazy(Of List(Of Deoxyribonucleotides))(Sequence.ToList)
            Call __convertSequence(ToString(Sequence))
        End Sub

        ''' <summary>
        ''' Construct the nucleotide sequence from a nt sequence model interface <see cref="I_PolymerSequenceModel"/>
        ''' </summary>
        ''' <param name="SequenceData"></param>
        Sub New(SequenceData As I_PolymerSequenceModel)
            Call __convertSequence(SequenceData.SequenceData)
        End Sub

        Sub New()
        End Sub

        ''' <summary>
        ''' Construct the nucleotide sequence from a ATGC based sequence string.
        ''' (从一个序列字符串之中创建一条核酸链分子对象)
        ''' </summary>
        ''' <param name="SequenceData">This sequence data can be user input from the interface or sequence data from the <see cref="LANS.SystemsBiology.SequenceModel.FASTA.FastaToken"/> object.</param>
        Sub New(SequenceData As String)
            Call __convertSequence(SequenceData)
        End Sub

        ''' <summary>
        ''' Construct the nucleotide seuqnece form a nt sequence model object. The nt sequence object should inherits from the base class <see cref="SequenceModel.ISequenceModel"/>
        ''' </summary>
        ''' <param name="SequenceData"></param>
        Sub New(SequenceData As SequenceModel.ISequenceModel)
            Call __convertSequence(SequenceData.SequenceData)
        End Sub

        Sub New(SequenceData As SequenceModel.FASTA.FastaToken)
            Call __convertSequence(SequenceData.SequenceData)
        End Sub

        Private Sub __convertSequence(SequenceData As String)
            SequenceData = SequenceData.ToUpper.Replace("N", "-").Replace(".", "-")
            Dim LQuery = (From c As Char In SequenceData Where ISequenceModel.AA_CHARS_ALL.IndexOf(c) > -1 Select c).ToArray  '
            If Not LQuery.IsNullOrEmpty Then
                Throw New DataException("Target fasta sequence is a protein sequence. Only allows character [ATGCN-.]...")
            Else
                Me.SequenceData = SequenceData
            End If
        End Sub

        Const NTCHRS As String = "ATGC-"

        Public Shared Function CopyNT(seq As String) As NucleicAcid
            Return New NucleicAcid(Replace(seq))
        End Function

        Public Shared Function Replace(seq As String) As String
            Dim lst As New List(Of Char)

            For Each ch As Char In seq.ToUpper
                If NTCHRS.IndexOf(ch) = -1 Then
                    Call lst.Add("-"c)
                Else
                    Call lst.Add(ch)
                End If
            Next

            Return New String(lst.ToArray)
        End Function

        ''' <summary>
        ''' 假若修改了<see cref="_innerSeqModel"></see>之中的对象的话，则需要使用本方法重新生成序列缓存数据
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub __generateSeqCache()
            _innerSeqCache = New String((From ntBase As Deoxyribonucleotides
                                                     In Me._innerSeqModel.Value
                                         Select __nucleotideAsChar(ntBase)).ToArray)
            MyBase.SequenceData = _innerSeqCache
        End Sub

        ''' <summary>
        ''' 分割得到的小片段的长度
        ''' </summary>
        ''' <param name="SegmentLength"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Split(SegmentLength As Integer) As SegmentObject()
            Dim SegmentList As List(Of SegmentObject) = New List(Of SegmentObject)
            SegmentLength -= 1

            For i As Integer = 1 To Me.Length Step SegmentLength + 1
                Dim strSegmentData As String = Mid(Me.SequenceData, i, SegmentLength)
                Dim sgData = New SegmentObject With {.SequenceData = strSegmentData, .Left = i, .Right = i + Len(strSegmentData)}

                Call SegmentList.Add(sgData)
            Next

            Return SegmentList.ToArray
        End Function

        Public Overrides ReadOnly Property Length As Integer
            Get
                Return Len(_innerSeqCache)
            End Get
        End Property

        ''' <summary>
        ''' <paramref name="Start"></paramref>和<paramref name="End"></paramref>的值都是数组的下标，在本函数之中已经自动为这两个参数+1了
        ''' </summary>
        ''' <param name="Start">位置的左端的开始位置</param>
        ''' <param name="End">右端的结束位置</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetSegment(Start As Long, [End] As Long) As NucleicAcid
            Start += 1
            [End] += 1
            Return New NucleicAcid With {.SequenceData = Mid(Me.SequenceData, Start, [End] - Start)}
        End Function

        ''' <summary>
        ''' <paramref name="Left"></paramref>的值是数组的下标，在本函数之中已经自动为这个参数+1了
        ''' </summary>
        ''' <param name="Length">片段的长度</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ReadSegment(Left As Integer, Length As Integer) As String
            Left += 1
            Return Mid(Me.SequenceData, Left, Length)
        End Function

        Public Function Complement() As NucleicAcid
            Return New NucleicAcid With {.SequenceData = Complement(Me.SequenceData)}
        End Function

        Public Function Reverse() As NucleicAcid
            Return New NucleicAcid With {.SequenceData = Me.SequenceData.Reverse.ToArray}
        End Function

        Public Shared Function CreateObject(strSeq As String) As NucleicAcid
            Return New NucleicAcid With {.SequenceData = strSeq}
        End Function

        Public Function CreateReader() As SequenceModel.NucleotideModels.SegmentReader
            Return New SegmentReader(Me)
        End Function

        ''' <summary>
        ''' Gets the complement sequence of a nucleotide sequence.(获取某一条核酸序列的互补序列，但是新得到的序列并不会首尾反转)
        ''' </summary>
        ''' <param name="DNAseq">The target dna nucleotide sequence to complement.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Complement(DNAseq As String) As String
            Dim sBuilder As StringBuilder = New StringBuilder(DNAseq)

            Call sBuilder.Replace("A"c, "1"c)
            Call sBuilder.Replace("T"c, "2"c)
            Call sBuilder.Replace("G"c, "3"c)
            Call sBuilder.Replace("C"c, "4"c)

            Call sBuilder.Replace("1"c, "T"c)
            Call sBuilder.Replace("2"c, "A"c)
            Call sBuilder.Replace("3"c, "C"c)
            Call sBuilder.Replace("4"c, "G"c)

            Return sBuilder.ToString
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("({0}bp) {1}...", Len(SequenceData), Mid(Me.SequenceData, 1, 25))
        End Function

        Public Overloads Shared Function ToString(nn As SequenceModel.NucleotideModels.NucleicAcid.Deoxyribonucleotides) As String
            Return __nucleotideAsChar(nn).ToString
        End Function

        Public Overloads Shared Function ToString(nt As Generic.IEnumerable(Of Deoxyribonucleotides)) As String
            Dim array As Char() = nt.ToArray(Function(x) __nucleotideAsChar(x))
            Return New String(array)
        End Function

        Public Shared Widening Operator CType(DNAseq As String) As NucleicAcid
            Return New NucleicAcid With {
                .SequenceData = DNAseq
            }
        End Operator

        Public Shared Narrowing Operator CType(obj As NucleicAcid) As String
            Return obj.SequenceData
        End Operator

        Public Shared Narrowing Operator CType(obj As NucleicAcid) As SegmentObject
            Return New SegmentObject With {
                .Complement = False,
                .Left = 0,
                .Right = 0,
                .SequenceData = obj.SequenceData
            }
        End Operator

        Public Shared Widening Operator CType(FastaObject As SequenceModel.FASTA.FastaToken) As NucleicAcid
            Return New NucleicAcid With {
                .SequenceData = FastaObject.SequenceData
            }
        End Operator

        Public Function GetEnumerator() As Collections.IEnumerator
            Return _innerSeqModel.Value.GetEnumerator
        End Function
    End Class
End Namespace