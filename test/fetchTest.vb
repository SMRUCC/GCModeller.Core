﻿Imports SMRUCC.genomics.Assembly.KEGG.DBGET.bGetObject

Module fetchTest

    Sub Main()
        Dim pathway = PathwayMap.Download("map00062")

        Call pathway.GetXml.SaveTo("./test_pathwaydata.xml")
    End Sub
End Module
