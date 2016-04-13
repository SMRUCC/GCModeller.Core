Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic

Namespace Assembly.MetaCyc.File

    ''' <summary>
    ''' Attribute-Value: Each attribute-value file contains data for one class of objects,
    ''' such as genes or proteins. A file is divided into entries, where one entry describes
    ''' one database object.
    ''' </summary>
    ''' <remarks>
    ''' An entry consists of a set of attribute-value pairs, •which describe properties of
    ''' the object, and relationships of the object to other object. Each attribute-value
    ''' pair typically resides on a single line of the file, although in some cases for
    ''' values that are long strings, the value will reside on multiple lines. An attribute-
    ''' value pair consists of an attribute name, followed by the string " - " and a value,
    ''' for example:
    '''
    ''' LEFT - NADP
    '''
    ''' A value that requires more than one line is continued by a newline followed by a /.
    ''' Thus, literal slashes at the beginning of a line must be escaped as //. A line that
    ''' contains only // separates objects. Comment lines can be anywhere in the file and
    ''' must begin with the following symbol:
    '''
    ''' #
    '''
    ''' Starting in version 6.5 of Pathway Tools, attribute-value files can also contain
    ''' annotation-value pairs. Annotations are a mechanism for attaching labeled values
    ''' to specific attribute values. For example, we might want to specify a coefficient
    ''' for a reactant in a chemical reaction. An annotations refers to the attribute value
    ''' that immediately precedes the annotation.
    ''' An annotation-value pair consists of a caret symbol "^" that points upward to indicate
    ''' that the annotation annotates the preceding attribute value, followed by the annotation
    ''' label, followed by the string " - ", followed by a value. The same attribute name or
    ''' annotation label with different values can appear any number of times in an object.
    ''' An example annotation-value pair that refers to the preceding attribute-value pair is:
    '''
    ''' LEFT - NADP
    ''' ^COEFFICIENT - 1
    ''' </remarks>
    Public Class AttributeValue : Implements IEnumerable(Of ObjectBase)

        Public Property DbProperty As [Property]
        Public Property Objects As ObjectBase()

        Public Shared Widening Operator CType(Path As String) As AttributeValue
            Dim File As ObjectModel() = Nothing
            Dim prop As [Property] = Nothing
            Dim message As String = FileReader.TryParse(Path, prop, File)
            Dim Objects As New List(Of ObjectBase)
            Dim p As Integer, [Next] As Integer = 0
            Dim NewObject As ObjectBase

            If Not String.IsNullOrEmpty(message) Then
                Call VBDebugger.Warning(message)
                Return New AttributeValue With {.Objects = New ObjectBase() {}}
            End If

            Dim DataLines As String() = Nothing
            Dim HasAdditionalAttributes As Boolean = (From strValue As String In DataLines.AsParallel Where strValue.First = "^"c Select 1).ToArray.Sum > 0

            For LineIdx As Long = 0 To DataLines.Length - 1 '遍历所有行
                If String.Compare(DataLines(LineIdx), "//") Then   '对象之间的分隔行
                    p += 1
                Else
                    NewObject = New ObjectBase
                    ReDim NewObject.TextLine(p - 1)
                    Call Array.ConstrainedCopy(DataLines, [Next], NewObject.TextLine, Scan0, p)
                    Call Objects.Add(NewObject)

                    p = 0
                    [Next] = LineIdx + 1
                End If
            Next

            If Objects.Count = 0 AndAlso p > 0 Then
                NewObject = New ObjectBase With {.TextLine = DataLines}
                Call Objects.Add(NewObject)
            End If
            If HasAdditionalAttributes Then
                Call Console.WriteLine("Contacts additional attributes...")
                Objects = (From item In Objects.AsParallel Select item.ContactAdditionalAttribute).ToList
            End If

            Return New AttributeValue With {
                .Objects = Objects.ToArray,
                .DbProperty = prop
            }
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of ObjectBase) Implements IEnumerable(Of ObjectBase).GetEnumerator
            For i As Integer = 0 To Objects.Count - 1
                Yield Objects(i)
            Next
        End Function

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace

