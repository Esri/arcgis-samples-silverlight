Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Runtime.Serialization
Imports System.Windows.Controls
Imports System.Windows.Media
Imports ESRI.ArcGIS.Client
Imports ESRI.ArcGIS.Client.Geometry
Imports ESRI.ArcGIS.Client.Symbols


    Partial Public Class AddGraphics
        Inherits UserControl

        Private Shared mercator As New ESRI.ArcGIS.Client.Projection.WebMercator()

        Public Sub New()
            InitializeComponent()

            AddMarkerGraphics()
            AddPictureMarkerAndTextGraphics()
            AddLineGraphics()
            AddPolygonGraphics()
        End Sub

		Private Sub AddMarkerGraphics()
            Dim jsonCoordinateString As String = "{'Coordinates':[{'X':13,'Y':55.59},{'X':72.83,'Y':18.97},{'X':55.43,'Y':34.3}]}"
            Dim coordinateList As CustomCoordinateList = DeserializeJson(Of CustomCoordinateList)(jsonCoordinateString)

            Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

            For i As Integer = 0 To coordinateList.Coordinates.Count - 1
                Dim graphic As New Graphic() With
                    {
                        .Geometry = mercator.FromGeographic(New MapPoint(coordinateList.Coordinates(i).X, coordinateList.Coordinates(i).Y)),
                        .Symbol = If(i > 0, TryCast(LayoutRoot.Resources("RedMarkerSymbol"), Symbol), TryCast(LayoutRoot.Resources("BlackMarkerSymbol"), Symbol))
                    }
                graphicsLayer.Graphics.Add(graphic)
            Next i

        End Sub

        Private Sub AddPictureMarkerAndTextGraphics()
            Dim gpsNMEASentences As String = "$GPGGA, 92204.9, -35.6334, N, -60.2343, W, 1, 04, 2.4, 25.7, M,,,,*75" &
                vbCrLf & "$GPGGA, 92510.5, -49.9334, N, -65.2131, W, 1, 04, 2.6, 1.7, M,,,,*75" & vbCrLf

            Dim gpsNMEASentenceArray() As String = gpsNMEASentences.Split(ControlChars.Lf)

            Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

            For i As Integer = 0 To gpsNMEASentenceArray.Length - 2
                Dim gpsNMEASentence() As String = gpsNMEASentenceArray(i).Split(","c)

                Dim graphic As New Graphic() With
                    {
                        .Geometry = mercator.FromGeographic(New MapPoint(Convert.ToDouble(gpsNMEASentence(4)), Convert.ToDouble(gpsNMEASentence(2)))),
                        .Symbol = TryCast(LayoutRoot.Resources("GlobePictureSymbol"), Symbol)
                    }

                graphicsLayer.Graphics.Add(graphic)


                Dim textSymbol As New TextSymbol() With
                    {
                        .FontFamily = New System.Windows.Media.FontFamily("Arial"),
                        .Foreground = New System.Windows.Media.SolidColorBrush(Colors.Black),
                        .FontSize = 10,
                        .Text = gpsNMEASentence(9)
                    }

                Dim graphicText As New Graphic() With
                    {
                        .Geometry = mercator.FromGeographic(New MapPoint(Convert.ToDouble(gpsNMEASentence(4)), Convert.ToDouble(gpsNMEASentence(2)))),
                        .Symbol = textSymbol
                    }

                graphicsLayer.Graphics.Add(graphicText)
            Next i
        End Sub

		Private Sub AddLineGraphics()
            Dim geoRSSLine As String = "<?xml version='1.0' encoding='utf-8'?>" & ControlChars.CrLf &
                "  <feed xmlns='http://www.w3.org/2005/Atom' xmlns:georss='http://www.georss.org/georss'>" & ControlChars.CrLf &
                "    <georss:line>-118.169, 34.016, -104.941, 39.7072, -96.724, 32.732</georss:line>" & ControlChars.CrLf &
                "    <georss:line>-28.69, 14.16, -14.91, 23.702, -1.74, 13.72</georss:line>" & ControlChars.CrLf &
                "  </feed>"

            Dim polylineList As New List(Of ESRI.ArcGIS.Client.Geometry.Polyline)()

            Using xmlReader As System.Xml.XmlReader = System.Xml.XmlReader.Create(New System.IO.StringReader(geoRSSLine))
                Do While xmlReader.Read()
                    Select Case xmlReader.NodeType
                        Case System.Xml.XmlNodeType.Element
                            Dim nodeName As String = xmlReader.Name
                            If nodeName = "georss:line" Then
                                Dim lineString As String = xmlReader.ReadElementContentAsString()

                                Dim lineCoords() As String = lineString.Split(","c)

                                Dim pointCollection As New ESRI.ArcGIS.Client.Geometry.PointCollection()
                                For i As Integer = 0 To lineCoords.Length - 1 Step 2
                                    pointCollection.Add(New MapPoint(Convert.ToDouble(lineCoords(i)), Convert.ToDouble(lineCoords(i + 1))))
                                Next i

                                Dim polyline As New ESRI.ArcGIS.Client.Geometry.Polyline()
                                polyline.Paths.Add(pointCollection)

                                polylineList.Add(polyline)

                            End If
                    End Select
                Loop
            End Using

            Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

            For Each polyline As ESRI.ArcGIS.Client.Geometry.Polyline In polylineList
                Dim graphic As New Graphic() With
                    {
                        .Symbol = TryCast(LayoutRoot.Resources("DefaultLineSymbol"), Symbol),
                        .Geometry = mercator.FromGeographic(polyline)
                    }

                graphicsLayer.Graphics.Add(graphic)
            Next polyline
        End Sub

		Private Sub AddPolygonGraphics()
            Dim coordinateString1 As String = "130,5.59 118.42,3.92 117.3,23.3 143.2,22.9 130,5.59"

            Dim graphicsLayer As GraphicsLayer = TryCast(MyMap.Layers("MyGraphicsLayer"), GraphicsLayer)

            Dim pointConverter As New PointCollectionConverter()
            Dim pointCollection1 As ESRI.ArcGIS.Client.Geometry.PointCollection = TryCast(pointConverter.ConvertFromString(coordinateString1), ESRI.ArcGIS.Client.Geometry.PointCollection)

            Dim polygon1 As New ESRI.ArcGIS.Client.Geometry.Polygon()
            polygon1.Rings.Add(pointCollection1)

            Dim graphic As New Graphic() With
                {
                    .Geometry = mercator.FromGeographic(polygon1),
                    .Symbol = TryCast(LayoutRoot.Resources("DefaultFillSymbol"), Symbol)
                }

            graphicsLayer.Graphics.Add(graphic)
        End Sub

        Friend Shared Function DeserializeJson(Of T)(ByVal json As String) As T
            Dim objectInstance As T = Activator.CreateInstance(Of T)()
            Dim memoryStream As New System.IO.MemoryStream(System.Text.Encoding.Unicode.GetBytes(json))
            Dim jsonSerializer As New System.Runtime.Serialization.Json.DataContractJsonSerializer(objectInstance.GetType())
            objectInstance = CType(jsonSerializer.ReadObject(memoryStream), T)
            memoryStream.Close()
            Return objectInstance
        End Function

    End Class

	<DataContract>
    Public Class CustomCoordinateList
        <DataMember()>
        Public Coordinates As New List(Of CustomCoordinate)()
    End Class

    <DataContract()>
    Public Class CustomCoordinate
        Public Sub New()
        End Sub
        Public Sub New(ByVal x As Double, ByVal y As Double)
            Me.X = x
            Me.Y = y
        End Sub

        <DataMember()>
        Public Property X() As Double
        <DataMember()>
        Public Property Y() As Double
    End Class


