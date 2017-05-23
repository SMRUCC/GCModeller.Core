Imports System.Runtime.CompilerServices
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
        Public Iterator Function FindChEBI(chebi As [NameOf], id$, idtype As AccessionTypes, Optional mass# = -1, Optional names$() = Nothing) As IEnumerable(Of String)
            Dim acc As Accession() = chebi.MatchByID(id, idtype)
        End Function
    End Module
End Namespace