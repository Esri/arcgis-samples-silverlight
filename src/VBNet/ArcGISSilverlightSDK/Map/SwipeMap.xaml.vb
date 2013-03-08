Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports ESRI.ArcGIS.Client
Imports System
Imports System.Windows.Shapes

Partial Public Class SwipeMap
  Inherits UserControl
  'INSTANT VB NOTE: The variable isMouseCaptured was renamed since Visual Basic does not allow class members with the same name:
  Private is_MouseCaptured As Boolean
  Private mouseHorizontalPosition As Double

  Public Sub New()
    InitializeComponent()
    AddHandler AboveMap.Layers.LayersInitialized, Sub(a, b)
                                                    slider.Height = rootCanvas.ActualHeight
                                                    slider.SetValue(Canvas.LeftProperty, rootCanvas.ActualWidth - slider.ActualWidth)
                                                  End Sub
  End Sub


  Public Sub Handle_MouseDown(ByVal sender As Object, ByVal args As MouseEventArgs)
    Dim item As Rectangle = TryCast(sender, Rectangle)
    mouseHorizontalPosition = args.GetPosition(Nothing).X

    BelowMap.Extent = AboveMap.Extent
    BelowMap.Visibility = Visibility.Visible

    is_MouseCaptured = True
    item.CaptureMouse()
  End Sub

  Public Sub Handle_MouseMove(ByVal sender As Object, ByVal args As MouseEventArgs)
    Dim item As Rectangle = TryCast(sender, Rectangle)

    If is_MouseCaptured Then
      ' Calculate the current position of the object.        
      Dim deltaH As Double = args.GetPosition(Nothing).X - mouseHorizontalPosition
      Dim newLeft As Double = deltaH + CDbl(item.GetValue(Canvas.LeftProperty))

      'if slider is pulled beyond start of map, default it back to start.
      If newLeft < 0.0 Then
        item.ReleaseMouseCapture()
        newLeft = 0.0
        is_MouseCaptured = False
      End If

      'if slider is pulled beyond screen, default it back to end.
      If newLeft > rootCanvas.ActualWidth Then
        item.ReleaseMouseCapture()
        newLeft = rootCanvas.ActualWidth - (Slider.ActualWidth)
        is_MouseCaptured = False
      End If

      item.SetValue(Canvas.LeftProperty, newLeft)

      ' Update position global variables.
      mouseHorizontalPosition = args.GetPosition(Nothing).X


      Dim mouse As Point = args.GetPosition(Me.AboveMap)
      ' You can modify StartPoint and Endpoint to change the slope of the swipe
      ' as well as where it starts and ends. Using a range from 0 to 1 allows
      ' the mouse position (X,Y) to be used relative to the grid's ActualWidth or
      ' ActualHeight to keep the cursor synchronized with the edge of the mask.
      Dim mask As New LinearGradientBrush()
      mask.StartPoint = New Point(0, 1)
      mask.EndPoint = New Point(1, 1)

      Dim transparentStop As New GradientStop()
      transparentStop.Color = Colors.Black
      transparentStop.Offset = (mouse.X / Me.LayoutRoot.ActualWidth)
      mask.GradientStops.Add(transparentStop)

      ' The color property must be set, but the OpacityMask ignores color details.
      Dim visibleStop As New GradientStop()
      visibleStop.Color = Colors.Transparent
      visibleStop.Offset = (mouse.X / Me.LayoutRoot.ActualWidth)
      mask.GradientStops.Add(visibleStop)

      ' Apply the OpacityMask to the map.
      Me.AboveMap.OpacityMask = mask
    End If

  End Sub

  Public Sub Handle_MouseUp(ByVal sender As Object, ByVal args As MouseEventArgs)
    Dim item As Rectangle = TryCast(sender, Rectangle)
    is_MouseCaptured = False
    item.ReleaseMouseCapture()
    mouseHorizontalPosition = -1
  End Sub

  ' Synchronize both maps by matching the extents only when below map is visible
  Private Sub AboveMap_ExtentChanged(ByVal sender As Object, ByVal e As ExtentEventArgs)
    If BelowMap.Visibility = System.Windows.Visibility.Visible Then
      Me.BelowMap.Extent = Me.AboveMap.Extent
    End If
  End Sub

  
End Class
