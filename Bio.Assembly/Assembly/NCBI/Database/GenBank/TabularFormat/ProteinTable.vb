﻿#Region "Microsoft.VisualBasic::f81ec1ba0307960638982c66f7708ea8, ..\GCModeller\core\Bio.Assembly\Assembly\NCBI\Database\GenBank\TabularFormat\ProteinTable.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Text
Imports SMRUCC.genomics.ComponentModel.Loci
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace Assembly.NCBI.GenBank.TabularFormat

    Public Class ProteinDescription : Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.INamedValue

        ''' <summary>
        ''' #Replicon Name
        ''' </summary>
        ''' <returns></returns>
        Public Property RepliconName As String
        ''' <summary>
        ''' Replicon Accession
        ''' </summary>
        ''' <returns></returns>
        Public Property RepliconAccession As String
        Public Property Start As Integer
        Public Property [Stop] As Integer
        Public Property Strand As String
        Public Property GeneID As String
        Public Property Locus As String
        ''' <summary>
        ''' Locus tag.(基因号)
        ''' </summary>
        ''' <returns></returns>
        Public Property Locus_tag As String Implements INamedValue.Identifier
        ''' <summary>
        ''' Protein product
        ''' </summary>
        ''' <returns></returns>
        Public Property Product As String
        Public Property Length As Integer
        ''' <summary>
        ''' COG(s)
        ''' </summary>
        ''' <returns></returns>
        Public Property COG As String
        ''' <summary>
        ''' Protein name
        ''' </summary>
        ''' <returns></returns>
        Public Property ProteinName As String

        Public Function ToPTTGene() As NCBI.GenBank.TabularFormat.ComponentModels.GeneBrief
            Dim strand As Strands = GetStrand(Me.Strand)
            Dim loci As New NucleotideLocation(Me.Start, Me.Stop, strand)
            Return New NCBI.GenBank.TabularFormat.ComponentModels.GeneBrief With {
                .Code = Me.Product,
                .COG = Me.COG,
                .Gene = Me.GeneID,
                .Location = loci,
                .PID = Me.GeneID,
                .Product = Me.ProteinName,
                .Synonym = Me.Locus_tag
            }
        End Function

        Public Function GetLoci() As NucleotideLocation
            Dim strand = GetStrand(Me.Strand)
            Dim loci As New ComponentModel.Loci.NucleotideLocation(Me.Start, Me.Stop, strand)
            Return loci
        End Function

        Public Overrides Function ToString() As String
            Return Locus_tag & vbTab & GetLoci.ToString
        End Function
    End Class

    Public Class ProteinTable : Inherits Microsoft.VisualBasic.ComponentModel.ITextFile

        Public Property Proteins As ProteinDescription()

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            Throw New NotImplementedException()
        End Function

        Public Shared Function Load(Path As String) As ProteinTable
            Dim ChunkBuffer As String() = IO.File.ReadAllLines(Path)
            Dim LQuery = (From str As String In ChunkBuffer.Skip(1).AsParallel Select CreateObject(str)).ToArray
            Return New ProteinTable With {
                .FilePath = Path,
                .Proteins = LQuery
            }
        End Function

        Public Overloads Shared Function CreateObject(str As String) As ProteinDescription
            Dim Tokens As String() = Strings.Split(str, vbTab)
            Dim Protein As New ProteinDescription
            Dim p As Integer = 0

            With Protein

                .RepliconName = Tokens(p.MoveNext)
                .RepliconAccession = Tokens(p.MoveNext)
                .Start = CInt(Val(Tokens(p.MoveNext)))
                .Stop = CInt(Val(Tokens(p.MoveNext)))
                .Strand = Tokens(p.MoveNext)
                .GeneID = Tokens(p.MoveNext)
                .Locus = Tokens(p.MoveNext)
                .Locus_tag = Tokens(p.MoveNext)
                .Product = Tokens(p.MoveNext)
                .Length = CInt(Val(Tokens(p.MoveNext)))
                .COG = Tokens(p.MoveNext)
                .ProteinName = Tokens(p.MoveNext)

            End With

            Return Protein
        End Function

        Public Function ToPTT() As NCBI.GenBank.TabularFormat.PTT
            Dim LQuery = (From Protein As ProteinDescription In Me.Proteins.AsParallel Select Protein.ToPTTGene).ToArray
            Return New NCBI.GenBank.TabularFormat.PTT With {
                .GeneObjects = LQuery
            }
        End Function
    End Class
End Namespace
