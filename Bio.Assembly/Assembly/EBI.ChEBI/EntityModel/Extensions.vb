Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports SMRUCC.genomics.Assembly.EBI.ChEBI.Database.IO.StreamProviders.Tsv.Tables

Namespace Assembly.EBI.ChEBI

    Public Module Extensions

        ''' <summary>
        ''' 从用户所提供的有限的信息之中获取得到chebi编号结果
        ''' </summary>
        ''' <param name="chebi">可以使用<see cref="DATA.LoadNameOfDatabaseFromTsv"/>函数来构建出数据库参数</param>
        ''' <param name="id$"></param>
        ''' <param name="idtype"></param>
        ''' <param name="mass#"></param>
        ''' <param name="names$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function FindChEBI(chebi As [NameOf], id$, idtype As AccessionTypes, Optional mass# = -1, Optional names$() = Nothing) As IEnumerable(Of String)
            Dim chebiIDlist$() = {""}
            Dim acc As Accession() = chebi.MatchByID(id, idtype, chebiIDlist(0))

            If chebiIDlist(Scan0).StringEmpty Then
                ' 按照编号没有查找结果
                ' 则尝试按照名字和质量来模糊查找
                acc = names _
                    .SafeQuery _
                    .Select(Function(name) chebi.MatchByName(name, fuzzy:=True)) _
                    .IteratesALL _
                    .Distinct _
                    .ToArray
                chebiIDlist = acc _
                    .Select(Of String)("COMPOUND_ID") _
                    .Distinct _
                    .ToArray
                ' 这些所获取得到的编号可能是一种物质的不同的化学形式
                ' 也可能是字符串模糊匹配出错了
                ' 之后还需要依靠mass来确定一个符合结果要求的编号
                Dim comfirm As New List(Of String)

                For Each id$ In chebiIDlist
                    Dim chemical = chebi.GetChemicalDatas(chebiID:=id)
                    Dim MASS_find = Val(chemical.TryGetValue("MASS")?.CHEMICAL_DATA)

                    If Math.Abs(MASS_find - mass) <= 0.5 Then
                        comfirm += id
                    End If
                Next

                chebiIDlist = comfirm
            End If

            Return chebiIDlist
        End Function
    End Module
End Namespace