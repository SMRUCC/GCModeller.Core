Imports System.Text.RegularExpressions
Imports System.Xml.Serialization

Namespace Assembly.KEGG.DBGET.bGetObject.SSDB

    ''' <summary>
    ''' 蛋白质直系同源比对blastp结果
    ''' </summary>
    ''' 
    <XmlType("Ortholog")>
    Public Class OrthologREST

        Public Const URL As String = "http://www.kegg.jp/ssdb-bin/ssdb_best?org_gene={0}:{1}"

        <XmlAttribute> Public Property KEGG_ID As String
        <XmlAttribute> Public Property Definition As String
        <XmlElement> Public Property Sequence As String

        <XmlElement> Public Property Orthologs As SShit()

        Public Const SEPRATOR As String = "------------------------------------------------------------------ -------------------------------------------------------------"

        '<INPUT TYPE="checkbox" NAME="ckid" VALUE="xca:xccb100_1230" CHECKED><A HREF="http://www.kegg.jp/dbget-bin/www_bget?xca:xccb100_1230" TARGET="_blank">xca:xccb100_1230</A> type IV pilus response regulator PilH  <A HREF="http://www.kegg.jp/dbget-bin/www_bget?ko:K02658"  TARGET="_blank">K02658</a>     120      771 (  378)     182    1.000    120     &lt;-&gt; <a href='/ssdb-bin/ssdb_ortholog_view?org_gene=xcb:XC_1184&org=xca&threshold=&type=' target=_ortholog>66</a>
        Public Const REGEX_ORTHO_ITEM As String = "<INPUT TYPE=""checkbox"" .+? target=_ortholog>\d+</a>"

        Public Shared Function Download(Entry As KEGG.WebServices.QueryEntry) As OrthologREST
            Dim pageContent As String = String.Format(KEGG.DBGET.bGetObject.SSDB.OrthologREST.URL, Entry.SpeciesId, Entry.LocusId).GET
            Dim Ortholog As OrthologREST = New OrthologREST
            Dim Tokens = Strings.Split(pageContent, KEGG.DBGET.bGetObject.SSDB.OrthologREST.SEPRATOR)
            Dim Items As String() = (From strData As Match
                                     In Regex.Matches(Tokens.Last, REGEX_ORTHO_ITEM, RegexOptions.IgnoreCase + RegexOptions.Singleline)
                                     Select strData.Value).ToArray
            Dim LQuery = (From strData As String
                          In Items
                          Select KEGG.DBGET.bGetObject.SSDB.SShit.CreateObject(strData)).ToArray
            Ortholog.Orthologs = LQuery
            Ortholog.KEGG_ID = Entry.ToString

            Dim Fa = KEGG.WebServices.WebRequest.FetchSeq(Entry)

            If Not Fa Is Nothing Then
                Ortholog.Definition = Fa.Title
                Ortholog.Sequence = Fa.SequenceData
            End If

            Return Ortholog
        End Function
    End Class
End Namespace