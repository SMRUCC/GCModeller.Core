﻿Imports System.ComponentModel
Imports System.Text.RegularExpressions
Imports System.Xml.Serialization
Imports LANS.SystemsBiology.ComponentModel
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic

Namespace Assembly.NCBI.COG

    ''' <summary>
    '''
    ''' </summary>
    ''' <remarks>
    ''' INFORMATION STORAGE AND PROCESSING
    ''' [J] Translation, ribosomal structure and biogenesis
    ''' [A] RNA processing and modification
    ''' [K] Transcription
    ''' [L] Replication, recombination and repair
    ''' [B] Chromatin structure and dynamics
    '''
    ''' CELLULAR PROCESSES AND SIGNALING
    ''' [D] Cell cycle control, cell division, chromosome partitioning
    ''' [Y] Nuclear structure
    ''' [V] Defense mechanisms
    ''' [T] Signal transduction mechanisms
    ''' [M] Cell wall/membrane/envelope biogenesis
    ''' [N] Cell motility
    ''' [Z] Cytoskeleton
    ''' [W] Extracellular structures
    ''' [U] Intracellular trafficking, secretion, and vesicular transport
    ''' [O] Posttranslational modification, protein turnover, chaperones
    '''
    ''' METABOLISM
    ''' [C] Energy production and conversion
    ''' [G] Carbohydrate transport and metabolism
    ''' [E] Amino acid transport and metabolism
    ''' [F] Nucleotide transport and metabolism
    ''' [H] Coenzyme transport and metabolism
    ''' [I] Lipid transport and metabolism
    ''' [P] Inorganic ion transport and metabolism
    ''' [Q] Secondary metabolites biosynthesis, transport and catabolism
    '''
    ''' POORLY CHARACTERIZED
    ''' [R] General function prediction only
    ''' [S] Function unknown
    '''
    ''' </remarks>
    Public Class [Function]

        <XmlElement> Public Property Categories As Category()

        Public Function IndexOf(Id As Char) As Integer
            For i As Integer = 0 To Categories.Count - 1
                Dim Category = Categories(i)
                For Each item In Category.SubClasses
                    If String.Equals(item.Key, Id) Then
                        Return i
                    End If
                Next
            Next
            Return -1
        End Function

        ''' <summary>
        ''' 当具有多个COG分类的时候，可以用这个来判断该基因是否可以被分类为指定的大分类
        ''' </summary>
        ''' <returns></returns>
        ''' <param name="COG">必须是经过<see cref="BioAssemblyExtensions.GetCOGCategory(String)"/>修剪的字符串</param>
        Public Function CanbeCategoryAs(COG As String, category As COGCategories) As Boolean
            For Each COGChar In COG
                Dim cat = __getCategory(COGChar)

                If cat > 0 Then
                    If category = CType(cat, COGCategories) Then
                        Return True
                    End If
                End If
            Next

            Return False
        End Function

        Public Function Category(COG As String) As Integer
            Dim n As Integer

            For Each cogChar In COG
                Dim m = __getCategory(cogChar)
                n += m  ' 分类编号是互斥的，可以直接相加
            Next

            Return n
        End Function

        ''' <summary>
        ''' 得到COG分类
        ''' </summary>
        ''' <param name="cogChar"></param>
        ''' <returns></returns>
        Private Function __getCategory(cogChar As Char) As COGCategories
            For Each [class] As COG.Category In Me._Categories  '' ??? 有BUG？？？
                If [class].GetDescription(cogChar, "") Then
                    Return [class].Class
                End If
            Next

            Return 0
        End Function

        ''' <summary>
        ''' 请注意，这个函数只会返回一个COG编号
        ''' </summary>
        ''' <param name="COG">已经自动处理好所有事情了</param>
        ''' <returns></returns>
        Public Function GetCategory(COG As String) As COGCategories
            If String.IsNullOrEmpty(__trimCOGs(COG).ShadowCopy(COG)) OrElse
                String.Equals(COG, "-") Then
                Return COGCategories.NotAssigned
            Else
                Dim COGChar As Char = COG.First
                Dim Category As Integer = __getCategory(COGChar)

                If Category = 0 Then
                    Return COGCategories.NotAssigned
                Else
                    Return CType(Category, COGCategories)
                End If
            End If
        End Function

        Public Function GetCategories(COG As String) As COGCategories()
            If String.IsNullOrEmpty(__trimCOGs(COG).ShadowCopy(COG)) OrElse
                String.Equals(COG, "-") Then
                Return {COGCategories.NotAssigned}
            End If

            Return COG.ToArray(Function(x) __getCategory(x))
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="value">COGxxxZZZ</param>
        ''' <returns></returns>
        Friend Shared Function __trimCOGs(value As String) As String
            If String.IsNullOrEmpty(value) Then
                Return ""
            End If

            Dim COG As String = Regex.Replace(value, "cog\d+", "", RegexOptions.IgnoreCase)
            Return COG
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="category">相加得到的值，可以在这里使用这个函数进行分解</param>
        ''' <returns></returns>
        Public Function GetCategories(category As Integer) As COGCategories()
            If category = COGCategories.NotAssigned OrElse
                category = 0 Then
                Return New COGCategories() {}
            End If

            Dim list As New List(Of COGCategories)

            For Each cat In Me.Categories
                If category Or cat.Class Then
                    Call list.Add(cat.Class)
                End If
            Next

            Return list.ToArray
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="lstId">List COG id</param>
        ''' <returns></returns>
        Public Function Statistics(lstId As String()) As Double()
            Dim Data As Double() = New Double(Me.Categories.Length - 1) {}
            For Each id As String In lstId
                If Not String.IsNullOrEmpty(id.Trim) Then
                    For Each ch In id
                        Dim idx = Me.IndexOf(ch)
                        If idx = -1 Then
                            Continue For
                        End If
                        Data(idx) += 1
                    Next
                End If
            Next

            Return Data
        End Function

        Public Overloads Function ClassCategory(Of T As I_COGEntry)(source As IEnumerable(Of T)) As Dictionary(Of COGCategories, String())
            Dim hash As New Dictionary(Of COGCategories, String()) From {  '主要是为了填满所有分类，因为source之中可能并不包含有所有的cog分类
                {COGCategories.Genetics, New String() {}},
                {COGCategories.Metabolism, New String() {}},
                {COGCategories.NotAssigned, New String() {}},
                {COGCategories.Signaling, New String() {}},
                {COGCategories.Unclassified, New String() {}}
            }
            Dim LQuery = (From x In (From gene As T In source.AsParallel
                                     Let categories As COGCategories() = GetCategories(gene.Address)
                                     Select (From cat As COGCategories In categories
                                             Select geneId = gene.Identifier,
                                                 category = GetCategory(gene.Address)).ToArray).MatrixToList
                          Select geneid = x.geneId,
                              category = x.category
                          Group By category Into Group).ToArray
            For Each cat In LQuery
                If hash.ContainsKey(cat.category) Then
                    Call hash.Remove(cat.category)
                End If
                Call hash.Add(cat.category, cat.Group.ToArray(Of String)(Function(obj) obj.geneId))
            Next

            Return hash
        End Function

        Protected Sub New()
        End Sub

        Public Shared Function [Default]() As [Function]
            Dim Functions = New [Function] With {
                .Categories = New Category() {
                    New Category With {
                        .Class = COGCategories.Genetics,
                        .Description = "INFORMATION STORAGE AND PROCESSING",
                        .SubClasses = New LANS.SystemsBiology.ComponentModel.KeyValuePair() {
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "J", .Value = "Translation, ribosomal structure and biogenesis"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "A", .Value = "RNA processing and modification"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "K", .Value = "Transcription"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "L", .Value = "Replication, recombination and repair"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "B", .Value = "Chromatin structure and dynamics"}}
            },
                    New Category With {
                        .Class = COGCategories.Signaling,
                        .Description = "CELLULAR PROCESSES AND SIGNALING",
                        .SubClasses = New LANS.SystemsBiology.ComponentModel.KeyValuePair() {
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "D", .Value = "Cell cycle control, cell division, chromosome partitioning"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "Y", .Value = "Nuclear structure"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "V", .Value = "Defense mechanisms"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "T", .Value = "Signal transduction mechanisms"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "M", .Value = "Cell wall/membrane/envelope biogenesis"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "N", .Value = "Cell motility"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "Z", .Value = "Cytoskeleton"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "W", .Value = "Extracellular structures"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "U", .Value = "Intracellular trafficking, secretion, and vesicular transport"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "O", .Value = "Posttranslational modification, protein turnover, chaperones"}}
            },
                    New Category With {
                        .Class = COGCategories.Metabolism,
                        .Description = "METABOLISM",
                        .SubClasses = New LANS.SystemsBiology.ComponentModel.KeyValuePair() {
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "C", .Value = "Energy production and conversion"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "G", .Value = "Carbohydrate transport and metabolism"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "E", .Value = "Amino acid transport and metabolism"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "F", .Value = "Nucleotide transport and metabolism"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "H", .Value = "Coenzyme transport and metabolism"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "I", .Value = "Lipid transport and metabolism"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "P", .Value = "Inorganic ion transport and metabolism"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "Q", .Value = "Secondary metabolites biosynthesis, transport and catabolism"}}
            },
                    New Category With {
                        .Class = COGCategories.Unclassified,
                        .Description = "POORLY CHARACTERIZED",
                        .SubClasses = New LANS.SystemsBiology.ComponentModel.KeyValuePair() {
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "R", .Value = "General function prediction only"},
                            New LANS.SystemsBiology.ComponentModel.KeyValuePair With {.Key = "S", .Value = "Function unknown"}}
                    }
                }
            }
            Return Functions
        End Function
    End Class

End Namespace