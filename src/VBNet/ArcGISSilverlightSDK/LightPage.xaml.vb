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
Imports System.Windows.Media.Imaging
Imports System.Windows.Shapes
Imports System.IO
Imports System.Xml.Linq
Imports System.Windows.Markup
Imports System.Windows.Browser


	Partial Public Class LightPage
		Inherits UserControl
		Private _scale As New ScaleTransform()
		Private _categoryList As List(Of Category)
		Private _xmlFile As String
		Private _item As CategoryItem
		Private _control As UserControl = Nothing
		Private _target As String
		Private _targetListBoxItem As ListBoxItem

		Public Sub New()
			InitializeComponent()
		End Sub
		Public Sub New(ByVal xmlFile As String)
			Me.New()
			_xmlFile = xmlFile
		End Sub

		Private Sub UserControl_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim client As New WebClient()
			AddHandler client.OpenReadCompleted, AddressOf client_OpenReadCompleted
			Dim uri As New Uri(_xmlFile, UriKind.RelativeOrAbsolute)
			client.OpenReadAsync(uri)

			InitializeMouseWheel()
		End Sub

		Private Sub client_OpenReadCompleted(ByVal sender As Object, ByVal e As OpenReadCompletedEventArgs)
			If e.Error IsNot Nothing Then
				Return
			End If

			Dim doc As XDocument = Nothing
			Using s As Stream = e.Result
				doc = XDocument.Load(s)
			End Using
			_categoryList = ( _
				From f In doc.Root.Elements("Category") _
				Select New Category With {.Name = CStr(f.Element("name")), .Icon = CStr(f.Element("icon")), .CategoryItems = ( _
					From o In f.Elements("items").Elements("item") _
					Select New CategoryItem With {.ID = CStr(o.Element("id")), .XAML = CStr(o.Element("xaml")), .Source = CStr(o.Element("source")), .Code = CStr(o.Element("code")), .Icon = CStr(o.Element("icon"))}).ToArray()}).ToList()

			CreateList()

			ProcessTarget()
		End Sub

		Private Sub CreateList()
			Dim listCount As Integer = ListOfSamples.Children.Count
			For Each category As Category In _categoryList
				Dim g As New Grid() With {.Margin = New Thickness(0, 5, 0, 0)}
				Dim rect As New Rectangle() With {.Fill = ExpanderGradient, .Stroke = New SolidColorBrush(Color.FromArgb(255, 70, 86, 109)), .Margin = New Thickness(0), .RadiusX = 5, .RadiusY = 5}
				'Fill = new SolidColorBrush(Color.FromArgb(255, 96, 120, 132)),
				g.Children.Add(rect)
				Dim exp As New Expander() With {.IsExpanded = False, .Name = String.Format("Category_{0}", listCount), .Foreground = New SolidColorBrush(Colors.Black), .Background = New SolidColorBrush(Colors.Transparent), .FontWeight = FontWeights.Bold, .FontSize = 11, .Header = category.Name, .Tag = listCount, .Cursor = Cursors.Hand, .Margin = New Thickness(4)}
				'BorderBrush= new SolidColorBrush(Color.FromArgb(255,70,86,109)),
				'Margin = new Thickness(0, 0, 4, 4)
				AddHandler exp.Expanded, AddressOf exp_Expanded
				Dim sp2 As New StackPanel()
				sp2.Orientation = Orientation.Horizontal
				Dim lb As New ListBox() With {.Name = String.Format("List_{0}", listCount), .Background = New SolidColorBrush(Colors.Transparent), .BorderBrush = New SolidColorBrush(Colors.Transparent), .Foreground = New SolidColorBrush(Colors.Black), .Margin = New Thickness(5, 0, 5, 5), .FontWeight = FontWeights.Normal, .ItemContainerStyle = SDKListBoxtItemStyle}
				'FontSize = 10,
				Dim itemCount As Integer = 0
				For Each ci As CategoryItem In category.CategoryItems
					Dim sp1 As New StackPanel()
					sp1.Orientation = Orientation.Horizontal
					Dim img As New Image()
					img.Source = New BitmapImage(New Uri(ci.Icon, UriKind.RelativeOrAbsolute))
					sp1.Children.Add(img)
					Dim tb1 As New TextBlock() With {.FontSize = 11, .Text = ci.ID}
					sp1.Children.Add(tb1)
					Dim item As New ListBoxItem() With {.Content = sp1, .Background = New SolidColorBrush(Colors.Transparent), .BorderBrush = New SolidColorBrush(Colors.Transparent), .Name = String.Format("Item_{0}_{1}", listCount, itemCount), .Tag = ci, .Cursor = Cursors.Hand}
					lb.Items.Add(item)
					itemCount += 1
				Next ci
				AddHandler lb.SelectionChanged, AddressOf lb_SelectionChanged
				exp.Content = lb
				listCount += 1
				g.Children.Add(exp)
				ListOfSamples.Children.Add(g)
			Next category
		End Sub

		Private Sub exp_Expanded(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim exp1 As Expander = TryCast(sender, Expander)
			Dim exp As Expander
			Dim listCount As Integer = ListOfSamples.Children.Count
			For i As Integer = 0 To listCount - 1
				exp = TryCast(FindName(String.Format("Category_{0}", i)), Expander)
				If exp.Name <> exp1.Name Then
					exp.IsExpanded = False
				End If
			Next i
			Dim listIndex As Integer = Convert.ToInt32(exp1.Tag)
			Dim listName As String = String.Format("List_{0}", listIndex)
			Dim lb As ListBox = TryCast(FindName(listName), ListBox)
			If lb IsNot Nothing Then
				lb.SelectedIndex = -1
			End If
			'SampleCaption.Text = String.Format("{0}", Convert.ToString(exp1.Header));
		End Sub

		Private Sub pline_MouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
			Dim pline As Polygon = TryCast(sender, Polygon)
			Dim index As String = Convert.ToString(pline.Tag)
			Dim name As String = _categoryList(Convert.ToInt32(index)).Name
			'ToggleCategoryVisibility(index, name);
		End Sub

		Private Sub lb_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
			If _targetListBoxItem IsNot Nothing Then
				_targetListBoxItem.IsSelected = False
				_targetListBoxItem = Nothing
			End If

			If e.AddedItems IsNot Nothing AndAlso e.AddedItems.Count > 0 Then
				Dim item As ListBoxItem = TryCast(e.AddedItems(0), ListBoxItem)
				If item IsNot Nothing Then
					Dim ci As CategoryItem = TryCast(item.Tag, CategoryItem)
					_item = ci
					processitem(ci)
				End If
			End If
		End Sub

		Private Sub processitem(ByVal item As CategoryItem)
			_control = Nothing
			tabSample.Content = Nothing
			'tabXamlScrollView.Content = null;
			txtSrc.Text = ""
			txtXaml.Text = ""
			'tabSrcScrollView.Content = null;
			tabPanel.SelectedIndex = 0

			Dim t As Type = Type.GetType(item.XAML)
    If t IsNot Nothing Then
      ' Reset the IdentityManager in order not to impact other samples
      ESRI.ArcGIS.Client.IdentityManager.Current.ChallengeMethod = Nothing
      For Each credential As ESRI.ArcGIS.Client.IdentityManager.Credential In ESRI.ArcGIS.Client.IdentityManager.Current.Credentials
        ESRI.ArcGIS.Client.IdentityManager.Current.RemoveCredential(credential)
      Next credential


      _control = TryCast(System.Activator.CreateInstance(t), UserControl)
      tabSample.Content = _control
      SampleCaption.Text = item.ID
    End If
		End Sub

		Private Sub sideBar_SizeChanged(ByVal sender As Object, ByVal e As SizeChangedEventArgs)
			Dim height As Double = sideBar.ActualHeight - 10
			Dim width As Double = sideBar.ActualWidth - 10
			'treeSamples.Width = width > 0 ? width : 1;
			'treeSamples.Height = height > 0 ? height : 1;
		End Sub

		Private Sub sourceViewer(ByVal srcFile As String)
			Dim client As New WebClient()
			AddHandler client.OpenReadCompleted, AddressOf sourceView_OpenReadCompleted
			client.OpenReadAsync(New Uri(srcFile, UriKind.RelativeOrAbsolute))
		End Sub

		Private Sub sourceView_OpenReadCompleted(ByVal sender As Object, ByVal e As OpenReadCompletedEventArgs)
			If e.Error IsNot Nothing Then
				Return
			End If

			Dim src As String = Nothing
			Using s As Stream = e.Result
				Dim sr As New StreamReader(s)
				src = sr.ReadToEnd()
			End Using

			Select Case tabPanel.SelectedIndex
				Case 1
					txtXaml.Text = src
				Case 2
					txtSrc.Text = src
			End Select
		End Sub

		Private Sub tabPanel_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
			If tabPanel Is Nothing Then
				Return
			End If
			If _item IsNot Nothing Then
				Select Case tabPanel.SelectedIndex
					Case 1
						sourceViewer(_item.Source)
						copyToClipboard.Visibility = Visibility.Visible
					Case 2
						sourceViewer(_item.Code)
						copyToClipboard.Visibility = Visibility.Visible
					Case Else
						copyToClipboard.Visibility = Visibility.Collapsed
				End Select
			End If
		End Sub

		Public Sub copyToClipboard_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
			Dim text As String = String.Empty
			Select Case tabPanel.SelectedIndex
				Case 1
					If txtXaml Is Nothing Then
						Return
					End If
					If String.IsNullOrEmpty(txtXaml.SelectedText) Then
						txtXaml.SelectAll()
					End If
					text = txtXaml.SelectedText
				Case 2
					If txtSrc Is Nothing Then
						Return
					End If
					If String.IsNullOrEmpty(txtSrc.SelectedText) Then
						txtSrc.SelectAll()
					End If
					text = txtSrc.SelectedText
			End Select

			Clipboard.SetText(text)
		End Sub

		Private Sub InitializeMouseWheel()
			HtmlPage.Window.AttachEvent("DOMMouseScroll", AddressOf Me.OnMouseWheel) ' Mozilla
			HtmlPage.Window.AttachEvent("onmousewheel", AddressOf Me.OnMouseWheel)
			HtmlPage.Document.AttachEvent("onmousewheel", AddressOf Me.OnMouseWheel) ' IE
		End Sub

		Protected Overloads Sub OnMouseWheel(ByVal sender As Object, ByVal e As HtmlEventArgs)
			Dim mouseDelta As Double = 0
			Dim eventObject As ScriptObject = e.EventObject

			' Mozilla and Safari
			If eventObject.GetProperty("detail") IsNot Nothing Then
				mouseDelta = (CDbl(eventObject.GetProperty("detail"))) * (-1)

				' IE and Opera
			ElseIf eventObject.GetProperty("wheelDelta") IsNot Nothing Then
				mouseDelta = (CDbl(eventObject.GetProperty("wheelDelta")))
			End If

			mouseDelta = Math.Sign(mouseDelta)

			If mouseDelta > 0 Then
				If tabPanel.SelectedIndex = 1 Then
					tabXamlScrollView.ScrollToVerticalOffset(tabXamlScrollView.VerticalOffset - 50)
				End If
				If tabPanel.SelectedIndex = 2 Then
					tabSrcScrollView.ScrollToVerticalOffset(tabSrcScrollView.VerticalOffset - 50)
				End If
			End If

			If mouseDelta < 0 Then
				If tabPanel.SelectedIndex = 1 Then
					tabXamlScrollView.ScrollToVerticalOffset(tabXamlScrollView.VerticalOffset + 50)
				End If
				If tabPanel.SelectedIndex = 2 Then
					tabSrcScrollView.ScrollToVerticalOffset(tabSrcScrollView.VerticalOffset + 50)
				End If

			End If
		End Sub


		<System.Windows.Browser.ScriptableMember()> _
		Public Sub SetTargetSample(ByVal hash As String)
			Dim targetValue As String = hash.Trim().TrimStart("#"c)
			If targetValue.Length > 0 AndAlso targetValue.Length < 30 Then
				_target = targetValue
			End If
		End Sub

		Public Sub ProcessTarget()
			If String.IsNullOrEmpty(_target) Then
				Return
			End If

			Dim targetXAML As String = "ArcGISSilverlightSDK." & _target

			Dim categoryIndex As Integer = 0
			For Each category As Category In _categoryList
				Dim categoryItemIndex As Integer = 0
				For Each categoryItem As CategoryItem In category.CategoryItems
					If categoryItem.XAML = targetXAML Then
						_item = categoryItem
						processitem(categoryItem)

						Dim exp As Expander = TryCast(FindName(String.Format("Category_{0}", categoryIndex)), Expander)
						exp.IsExpanded = True

						_targetListBoxItem = TryCast(FindName(String.Format("Item_{0}_{1}", categoryIndex, categoryItemIndex)), ListBoxItem)
						_targetListBoxItem.IsSelected = True

						Return
					End If
					categoryItemIndex += 1
				Next categoryItem
				categoryIndex += 1
			Next category
		End Sub

		Private Sub TopText_MouseRightButtonUp(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
			For Each ap As AssemblyPart In Deployment.Current.Parts
				If ap.Source = "ESRI.ArcGIS.Client.dll" Then
					Dim sri As System.Windows.Resources.StreamResourceInfo = Application.GetResourceStream(New Uri(ap.Source, UriKind.Relative))
					Dim a As System.Reflection.Assembly = New AssemblyPart().Load(sri.Stream)
					Dim assemblyName As New System.Reflection.AssemblyName(a.FullName)
					MessageBox.Show(assemblyName.Version.ToString(), "ArcGIS Silverlight Assembly Version", MessageBoxButton.OK)
				End If
			Next ap
		End Sub

		Private Sub TopText_MouseRightButtonDown(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
			e.Handled = True
		End Sub
	End Class

