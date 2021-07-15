Public Class AirfoilSketch
    Private Structure AirFoilData
        Public SketchName As String
        Public FullPath_Sketch As String
        Public Airfoil_Coord() As Coordinate
    End Structure

    Private Structure Coordinate
        Public X_Coordinate As Double
        Public Y_Coordinate As Double
    End Structure
    Private Airfoil As AirFoilData
    Public CATIAFactory As CATIA_Property = New CATIA_Property
    Private AirfoilLocation As String
    Private oPart As MECMOD.Part
    Private oPartDoc As MECMOD.PartDocument
    Private Sub CATMain()
        oPartDoc = CATIAFactory.MyCATIA.ActiveDocument
        oPart = oPartDoc.Part
        Dim Airfoil_Geo As MECMOD.HybridBody
        Try
            Airfoil_Geo = oPart.HybridBodies.Item("Airfoil Type")
        Catch ex As Exception
            Airfoil_Geo = oPart.HybridBodies.Add
            Airfoil_Geo.Name = "Airfoil Type"
        End Try
        Dim Sketch_Airfoil As MECMOD.Sketch
        Try
            Sketch_Airfoil = Airfoil_Geo.HybridSketches.Item(Airfoil.SketchName)
        Catch ex As Exception
            Sketch_Airfoil = Airfoil_Geo.HybridSketches.Add(oPart.OriginElements.PlaneYZ)
            Sketch_Airfoil.Name = Airfoil.SketchName
            Call DrawPoints(Sketch_Airfoil)

        End Try




        oPart.Update()
    End Sub
    Private Sub DrawPoints(ByRef oSketch As MECMOD.Sketch)
        Dim oPoints() As MECMOD.Point2D
        Dim oCPoints()
        Dim oSpline As MECMOD.Spline2D
        ReDim oPoints(0)
        ReDim oCPoints(0)
        oSketch.OpenEdition()
        For i = 1 To UBound(Airfoil.Airfoil_Coord)
            ReDim Preserve oPoints(UBound(oPoints) + 1)
            ReDim Preserve oCPoints(UBound(oPoints) + 1)
            oPoints(UBound(oPoints)) = oSketch.Factory2D.CreatePoint(Airfoil.Airfoil_Coord(i).X_Coordinate * 100, Airfoil.Airfoil_Coord(i).Y_Coordinate * 100)
            oCPoints(UBound(oCPoints)) = oSketch.Factory2D.CreateControlPoint(Airfoil.Airfoil_Coord(i).X_Coordinate * 100, Airfoil.Airfoil_Coord(i).Y_Coordinate * 100)
        Next
        oSpline = oSketch.Factory2D.CreateSpline(oCPoints)
        oSketch.CloseEdition()
    End Sub
    Private Sub AirfoilSketch_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim oFile As String
        AirfoilLocation = "D:\Work\Matlab\Airfoil_Analyzer\Airfoil_Coordinate_Files"
        For Each FileN As String In My.Computer.FileSystem.GetFiles(AirfoilLocation, FileIO.SearchOption.SearchAllSubDirectories, "*.dat")
            Dim FileN_Short As String = System.IO.Path.GetFileNameWithoutExtension(FileN)
            oFile = FileN
            ComboBox1.Items.Add(UCase(FileN_Short))
        Next
        CATIAFactory = CATIA_Property.SetInitialCATIA()
        'Call CATMain()
        'Call GetAirfoilType(oFile)
    End Sub
    Private Sub GetAirfoilType(FileN As String)


        Dim oFileInfo As System.IO.FileInfo
        Dim oStreamReader As System.IO.StreamReader
        Dim ErrorLogList As ArrayList
        Dim counterFileLine As Integer = 1
        Dim oArray(0) As Double

        oFileInfo = New IO.FileInfo(FileN)
        oStreamReader = oFileInfo.OpenText
        Dim SplitText() As String
        Dim Text As String = oStreamReader.ReadLine

        Try
            While Not Text Is Nothing
                If Text.Trim <> "" Then
                    SplitText = Text.Split(" ")
                    For i = 0 To UBound(SplitText)
                        If IsNumeric(SplitText(i)) = True Then
                            ReDim Preserve oArray(UBound(oArray) + 1)
                            oArray(UBound(oArray)) = SplitText(i)
                        End If
                    Next
                End If
                Text = oStreamReader.ReadLine
            End While

            'MsgBox(UBound(oArray).ToString)
            oStreamReader.Close()
            oStreamReader = Nothing
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
        ReDim Airfoil.Airfoil_Coord(0)
        For i = 1 To UBound(oArray) Step 2
            ReDim Preserve Airfoil.Airfoil_Coord(UBound(Airfoil.Airfoil_Coord) + 1)
            Airfoil.Airfoil_Coord(UBound(Airfoil.Airfoil_Coord)).X_Coordinate = oArray(i)
            Airfoil.Airfoil_Coord(UBound(Airfoil.Airfoil_Coord)).Y_Coordinate = oArray(i + 1)
        Next
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Airfoil.SketchName = ComboBox1.SelectedItem
        For Each FileN As String In My.Computer.FileSystem.GetFiles(AirfoilLocation, FileIO.SearchOption.SearchAllSubDirectories, "*.dat")
            Dim FileN_Short As String = System.IO.Path.GetFileNameWithoutExtension(FileN)
            If UCase(FileN_Short) = UCase(Airfoil.SketchName) Then
                Airfoil.FullPath_Sketch = FileN
                Exit For
            End If
        Next
        Call GetAirfoilType(Airfoil.FullPath_Sketch)
        Call CATMain()
    End Sub
End Class
