﻿#Region "Microsoft.VisualBasic::fe26a703d85818651b5c870eae1fe9e9, core\Bio.Assembly\Assembly\KEGG\DBGET\Objects\SSDB\EnzymeWebQuery.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    '     Class EnzymeWebQuery
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetECNumber, ParsePage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.genomics.ComponentModel

Namespace Assembly.KEGG.DBGET.bGetObject.SSDB

    Public Class EnzymeWebQuery : Inherits WebQuery(Of String)

        Public Sub New(<CallerMemberName>
                       Optional cache As String = Nothing,
                       Optional interval As Integer = -1,
                       Optional offline As Boolean = False
                   )

            Call MyBase.New(url:=AddressOf GetECNumber,
                            contextGuid:=Function(id) id,
                            parser:=AddressOf ParsePage,
                            prefix:=Nothing,
                            cache:=cache,
                            interval:=interval,
                            offline:=offline
                   )
        End Sub

        Public Shared Function GetECNumber(ec As String) As String
            Return $"https://www.genome.jp/dbget-bin/www_bget?ec:{ec}"
        End Function

        Public Shared Function ParsePage(html$, schema As Type) As Object

        End Function
    End Class
End Namespace
