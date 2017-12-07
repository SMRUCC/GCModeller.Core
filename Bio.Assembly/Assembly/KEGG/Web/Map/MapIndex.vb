Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.Repository
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq

Namespace Assembly.KEGG.WebServices

    Public Class MapIndex : Implements INamedValue

        <XmlAttribute>
        Public Property MapID As String Implements IKeyedEntity(Of String).Key
        Public Property Index As TermsVector
        Public Property Map As Map

        Public ReadOnly Property MapTitle As String
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Map.Name
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return MapID
        End Function
    End Class

    Public Class MapRepository : Implements IRepositoryRead(Of String, MapIndex)

        <XmlElement(NameOf(MapIndex))> Public Property Maps As MapIndex()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return table.Values.ToArray
            End Get
            Set(value As MapIndex())
                table = value.ToDictionary(Function(map) map.MapID)
            End Set
        End Property

        ''' <summary>
        ''' Get by ID
        ''' </summary>
        Dim table As Dictionary(Of String, MapIndex)

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Exists(key As String) As Boolean Implements IRepositoryRead(Of String, MapIndex).Exists
            Return table.ContainsKey(key)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetByKey(key As String) As MapIndex Implements IRepositoryRead(Of String, MapIndex).GetByKey
            Return table(key)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetWhere(clause As Func(Of MapIndex, Boolean)) As IReadOnlyDictionary(Of String, MapIndex) Implements IRepositoryRead(Of String, MapIndex).GetWhere
            Return table.Values.Where(clause).ToDictionary
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetAll() As IReadOnlyDictionary(Of String, MapIndex) Implements IRepositoryRead(Of String, MapIndex).GetAll
            Return New Dictionary(Of String, MapIndex)(table)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="directory">The reference map download directory</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function BuildRepository(directory As String) As MapRepository
            Return New MapRepository With {
                .Maps = (ls - l - r - "*.XML" <= directory) _
                    .Select(AddressOf LoadXml(Of Map)) _
                    .Select(Function(map)
                                Return New MapIndex With {
                                    .Map = map,
                                    .MapID = map.ID,
                                    .Index = New TermsVector With {
                                        .Terms = map _
                                            .Areas _
                                            .Select(Function(a) a.IDVector) _
                                            .IteratesALL _
                                            .Distinct _
                                            .OrderBy(Function(s) s) _
                                            .ToArray
                                    }
                                }
                            End Function)
            }
        End Function
    End Class
End Namespace