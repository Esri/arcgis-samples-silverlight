Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Shapes


	Partial Public Class TimeImageService
		Inherits UserControl
		Public Sub New()
			InitializeComponent()
		End Sub

		Private Sub MyTimeSlider_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim DateTimeMonths As New List(Of DateTime)()
			For i As Integer = 1 To 12
				DateTimeMonths.Add(New DateTime(2004, i, 1, 0, 0, 0, DateTimeKind.Utc))
			Next i

			MyTimeSlider.Intervals = DateTimeMonths
		End Sub
	End Class

