Imports LANS.SystemsBiology.Assembly.NCBI.GenBank.TabularFormat.ComponentModels

Namespace ComponentModel.Loci

    ''' <summary>
    ''' 描述位点在基因组上面的位置，可以使用<see cref="ToString"/>函数获取得到位置描述
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Relationship(Of T As I_GeneBrief)
        Public Property Gene As T
        Public Property Relation As  SegmentRelationships

        Sub New()
        End Sub

        Sub New(gene As T, rel As  SegmentRelationships)
            Me.Gene = gene
            Me.Relation = rel
        End Sub

        ''' <summary>
        ''' 位置关系的描述信息
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return Relation.LocationDescription(Gene)
        End Function
    End Class
End Namespace