Imports System.Text
Imports System.Text.RegularExpressions

Namespace ComponentModel.EquaionModel

    Public Module EquationBuilder

        Public Const EQUATION_DIRECTIONS_REVERSIBLE As String = " <=> "
        Public Const EQUATION_DIRECTIONS_IRREVERSIBLE As String = " --> "
        Public Const EQUATION_SPECIES_CONNECTOR As String = " + "

        Public Function CreateObject(Of TCompound As ICompoundSpecies, TEquation As IEquation(Of TCompound))(Equation As String) As TEquation
            Dim EquationObject As IEquation(Of TCompound) = Activator.CreateInstance(Of TEquation)()
            EquationObject.Reversible = CType(InStr(Equation, EQUATION_DIRECTIONS_REVERSIBLE), Boolean)

            Dim deli As String = If(EquationObject.Reversible, EQUATION_DIRECTIONS_REVERSIBLE, EQUATION_DIRECTIONS_IRREVERSIBLE)
            Dim Tokens As String() = Strings.Split(Equation, deli)

            Try
                EquationObject.Reactants = GetSides(Of TCompound)(Tokens(Scan0))
                EquationObject.Products = GetSides(Of TCompound)(Tokens(1))
            Catch ex As Exception   ' 生成字典的时候可能会因为重复的代谢物而出错
                ex = New Exception($"Could not process ""{Equation}"", duplicated found!", ex)
                Throw ex
            End Try

            Return EquationObject
        End Function

        Public Function CreateObject(Equation As String) As DefaultTypes.Equation
            Return CreateObject(Of DefaultTypes.CompoundSpecieReference, DefaultTypes.Equation)(Equation)
        End Function

        Private Function GetSides(Of T As ICompoundSpecies)(Expr As String) As T()
            If String.IsNullOrEmpty(Expr) Then
                Return New T() {}
            End If

            Dim Tokens As String() = Strings.Split(Expr, EQUATION_SPECIES_CONNECTOR)
            Dim LQuery = (From token As String In Tokens Select __tryParse(Of T)(token)).ToArray
            Return LQuery
        End Function

        Private Function __tryParse(Of T As ICompoundSpecies)(token As String) As T
            Dim CompoundSpecie As T = Activator.CreateInstance(Of T)()
            Dim SC As String = Regex.Match(token, "(^| )\d+ ", RegexOptions.Singleline).Value

            If String.IsNullOrEmpty(SC) Then
                Dim tokens As String() = token.Trim.Split
                If tokens.Length > 1 Then
                    CompoundSpecie.StoiChiometry = Scripting.CTypeDynamic(Of Double)(tokens(Scan0))
                    CompoundSpecie.Identifier = token
                Else
                    CompoundSpecie.StoiChiometry = 1
                    CompoundSpecie.Identifier = token
                End If
            Else
                CompoundSpecie.StoiChiometry = Val(SC)
                CompoundSpecie.Identifier = Trim(token.Replace(SC, ""))
            End If

            Return CompoundSpecie
        End Function

        Public Function ToString(GetLeftSide As Func(Of KeyValuePair(Of Double, String)()), GetRightSide As Func(Of KeyValuePair(Of Double, String)()), Reversible As Boolean) As String
            Dim sBuilder As StringBuilder = New StringBuilder(1024)
            Dim DirectionFlag As String = If(Reversible, EQUATION_DIRECTIONS_REVERSIBLE, EQUATION_DIRECTIONS_IRREVERSIBLE)

            Call EquationBuilder.AppendSides(sBuilder, Compounds:=GetLeftSide())
            Call sBuilder.Append(DirectionFlag)
            Call EquationBuilder.AppendSides(sBuilder, Compounds:=GetRightSide())

            Return sBuilder.ToString
        End Function

        Public Function ToString(LeftSide As KeyValuePair(Of Double, String)(), RightSide As KeyValuePair(Of Double, String)(), Reversible As Boolean) As String
            Dim sBuilder As StringBuilder = New StringBuilder(1024)
            Dim DirectionFlag As String = If(Reversible, EQUATION_DIRECTIONS_REVERSIBLE, EQUATION_DIRECTIONS_IRREVERSIBLE)

            Call EquationBuilder.AppendSides(sBuilder, Compounds:=LeftSide)
            Call sBuilder.Append(DirectionFlag)
            Call EquationBuilder.AppendSides(sBuilder, Compounds:=RightSide)

            Return sBuilder.ToString
        End Function

        Private Sub AppendSides(sBuilder As StringBuilder, Compounds As KeyValuePair(Of Double, String)())
            If Compounds.IsNullOrEmpty Then
                Return
            End If

            For Each Compound In Compounds
                If Compound.Key > 1 Then
                    Call sBuilder.Append(String.Format("{0} {1}", Compound.Key, Compound.Value))
                Else
                    Call sBuilder.Append(Compound.Value)
                End If

                Call sBuilder.Append(EQUATION_SPECIES_CONNECTOR)
            Next

            Call sBuilder.Remove(sBuilder.Length - 3, 3)
        End Sub

        Public Function ToString(Of TCompound As ICompoundSpecies)(Equation As IEquation(Of TCompound)) As String
            Dim sBuilder As StringBuilder = New StringBuilder(1024)
            Dim DirectionFlag As String = If(Equation.Reversible, EQUATION_DIRECTIONS_REVERSIBLE, EQUATION_DIRECTIONS_IRREVERSIBLE)

            Call EquationBuilder.AppendSides(sBuilder, Compounds:=Equation.Reactants)
            Call sBuilder.Append(DirectionFlag)
            Call EquationBuilder.AppendSides(sBuilder, Compounds:=Equation.Products)

            Return sBuilder.ToString
        End Function

        Public Function ToString(Equation As DefaultTypes.Equation) As String
            Return ToString(Of DefaultTypes.CompoundSpecieReference)(Equation)
        End Function

        Private Sub AppendSides(sBuilder As StringBuilder, Compounds As ICompoundSpecies())
            If Compounds.IsNullOrEmpty Then
                Return
            End If

            For Each Compound In Compounds
                If Compound.StoiChiometry > 1 Then
                    Call sBuilder.Append(String.Format("{0} {1}", Compound.StoiChiometry, Compound.Identifier))
                Else
                    Call sBuilder.Append(Compound.Identifier)
                End If

                Call sBuilder.Append(EQUATION_SPECIES_CONNECTOR)
            Next

            Call sBuilder.Remove(sBuilder.Length - 3, 3)
        End Sub
    End Module
End Namespace