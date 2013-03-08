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
Imports System.IO
Imports System.Xml.Linq
Imports System.Windows.Markup
Imports System.Windows.Browser



  Partial Public Class Page
    Inherits UserControl
    Private _scale As New ScaleTransform()
    Private _categoryList As List(Of Category)
    Private _xmlFile As String
    Private _item As CategoryItem
    Private _control As UserControl = Nothing

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
      ' set up the binding with the TreeView
      treeSamples.ItemsSource = _categoryList
      treeSamples.Visibility = Visibility.Visible
      Dim sp As New StackPanel()


      ' Select the first item automatically
      'SelectFirstTreeItem();
      'Dispatcher.BeginInvoke(() => { SelectFirstTreeItem(); });
    End Sub

    Private Sub SelectFirstTreeItem()

      If treeSamples.Items.Count > 0 Then
        Dim i1 As System.Windows.Controls.TreeViewItem = CType(treeSamples.Items(0), System.Windows.Controls.TreeViewItem)
        i1.IsExpanded = True

        'Category cat = (Category)treeSamples.Items[0];
        '((TreeViewItem)cat.CategoryItems[0]).IsExpanded = true;
        '((Category)treeSamples.Items[0]).CategoryItems


        '    Dispatcher.BeginInvoke(() => { item.IsSelected = true; });
      End If
    End Sub

    Private Sub TreeViewItem_Expanded(ByVal sender As Object, ByVal e As RoutedEventArgs)

    End Sub

    Private Sub treeSamples_SelectedItemChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Object))
      If TypeOf e.NewValue Is CategoryItem Then
        _item = TryCast(e.NewValue, CategoryItem)
        processitem(_item)
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
        _control = TryCast(System.Activator.CreateInstance(t), UserControl)
        tabSample.Content = _control
      End If
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

    Private Sub sideBar_SizeChanged(ByVal sender As Object, ByVal e As SizeChangedEventArgs)
      Dim height As Double = sideBar.ActualHeight - 10
      Dim width As Double = sideBar.ActualWidth - 10
      treeSamples.Width = If(width > 0, width, 1)
      treeSamples.Height = If(height > 0, height, 1)
    End Sub

  End Class

