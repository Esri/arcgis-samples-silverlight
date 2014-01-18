Imports System.Windows
Imports System.Windows.Controls
Imports ESRI.ArcGIS.Client.Symbols


Partial Public Class MarkerSymbolAngle
    Inherits UserControl
    Public Sub New()
        InitializeComponent()
    End Sub

    Private Sub RadioButton_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim angleTag As String = CStr((CType(sender, RadioButton)).Tag)
        TryCast(LayoutRoot.Resources("MyAngleSymbol"), MarkerSymbol).AngleAlignment = CType(System.Enum.Parse(GetType(MarkerSymbol.MarkerAngleAlignment), angleTag, True), MarkerSymbol.MarkerAngleAlignment)
    End Sub
End Class

