Imports LANS.SystemsBiology.Assembly.MetaCyc.File.DataFiles.Reflection
Imports LANS.SystemsBiology.Assembly.MetaCyc.Schema.Reflection
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Assembly.MetaCyc.File.DataFiles.Slots

    ''' <summary>
    ''' The Class Compounds describe small-molecular-weight chemical compounds — typically,
    ''' compounds that are substrates of metabolic reactions or compounds that activate or
    ''' inhibit metabolic enzymes.
    ''' </summary>
    ''' <remarks>
    ''' One of the component in the Class ProtLigandCplxe (Protein-Small-Molecule-Complexes) with class protein
    ''' </remarks>
    Public Class Compound : Inherits Slots.Object
        Implements Regulation.IRegulator
        Implements sIdEnumerable

        <MetaCycField> Public Overrides Property CommonName As String Implements Regulation.IRegulator.CommonName
            Get
                Return MyBase.CommonName
            End Get
            Set(value As String)
                MyBase.CommonName = value
            End Set
        End Property
        <ExternalKey("compounds", "", ExternalKey.Directions.Out)> <MetaCycField(Type:=MetaCycField.Types.TStr)>
        Public Property Components As List(Of String) Implements Regulation.IRegulator.Components
        <ExternalKey("compounds,proteins,protligandcplxes", "", ExternalKey.Directions.In)> <MetaCycField(Type:=MetaCycField.Types.TStr)>
        Public Property ComponentOf As List(Of String) Implements Regulation.IRegulator.ComponentOf

        <MetaCycField(Type:=MetaCycField.Types.TStr)>
        Public Overrides Property Types As List(Of String) Implements Regulation.IRegulator.Types
            Get
                Return MyBase.Types
            End Get
            Set(value As List(Of String))
                MyBase.Types = value
            End Set
        End Property

        <MetaCycField(Name:="UNIQUE-ID")>
        Public Overrides Property Identifier As String Implements Regulation.IRegulator.locusId, sIdEnumerable.Identifier
            Get
                Return MyBase.Identifier
            End Get
            Set(value As String)
                MyBase.Identifier = value
            End Set
        End Property

        <MetaCycField(Name:="MOLECULAR-WEIGHT")> Public Property MolecularWeight As String
        <MetaCycField(Name:="MONOISOTOPIC-MW")> Public Property MonoisotopicMW As String

        <MetaCycField(Type:=MetaCycField.Types.TStr)> Public Shadows Property Names As String()
            Get
                Dim List As List(Of String) = New List(Of String)
                Call List.Add(Me.AbbrevName)
                Call List.Add(Me.CommonName)
                Call List.AddRange(MyBase.Names)
                Call List.AddRange(Me.Synonyms)
                Call List.AddRange(Me.Types)

                Return (From strValue As String In List Let strItem As String = strValue.Trim.ToLower Where Not String.IsNullOrEmpty(strItem) Select strItem Distinct).ToArray
            End Get
            Set(value As String())
                MyBase.Names = value.ToList
            End Set
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <remarks></remarks>
        <ExternalKey("regulations", "involved in", ExternalKey.Directions.Out)> <MetaCycField(Type:=MetaCycField.Types.TStr)> Public Property Regulates As List(Of String) Implements Regulation.IRegulator.Regulates

        Public Overrides ReadOnly Property Table As [Object].Tables
            Get
                Return Tables.compounds
            End Get
        End Property

        Public Function GetMolecularWeight() As Double
            If Not String.IsNullOrEmpty(MolecularWeight) Then
                Return Val(MolecularWeight)
            Else
                If Not String.IsNullOrEmpty(MonoisotopicMW) Then
                    Return Val(MonoisotopicMW)
                Else
                    Return 0
                End If
            End If
        End Function

        Public Function Trim() As Compound
            Me.CommonName = TrimHTML(Me.CommonName)
            Me.Synonyms = (From s As String In Me.Synonyms Select TrimHTML(s)).ToArray
            Me.AbbrevName = TrimHTML(Me.AbbrevName)
            Return Me
        End Function

        Private Shared Function TrimHTML(str As String) As String
            If String.IsNullOrEmpty(str) Then
                Return ""
            Else
                Return Regex.Replace(str, "<i>|</i>|<SUB>|</SUB>|<SUP>|</SUP>|[&]", "", RegexOptions.IgnoreCase)
            End If
        End Function

        Public ReadOnly Property CHEBI As String()
            Get
                Return _CheBI
            End Get
        End Property

        Dim _CheBI As String()

        <MetaCycField(Name:="DBLINKS", Type:=MetaCycField.Types.TStr)> Public Overrides Property DBLinks As String()
            Get
                Return _DBLinks.DBLinks
            End Get
            Set(value As String())
                _DBLinks = Schema.DBLinkManager.CreateFromMetaCycFormat(value)
                _CheBI = (From item In _DBLinks.CHEBI Select item.AccessionId).ToArray
            End Set
        End Property

        Public ReadOnly Property PUBCHEM As String
            Get
                If _DBLinks.PUBCHEM Is Nothing Then
                    Return ""
                End If
                Return _DBLinks.PUBCHEM.AccessionId
            End Get
        End Property

        Public Function GetDBLinkManager() As MetaCyc.Schema.DBLinkManager
            Return MyBase._DBLinks
        End Function

        Public ReadOnly Property KEGGCompound As String
            Get
                Dim DBLinks = _DBLinks.Item("LIGAND-CPD")
                If DBLinks.IsNullOrEmpty Then
                    Return ""
                Else
                    Return DBLinks.First.AccessionId
                End If
            End Get
        End Property
    End Class
End Namespace