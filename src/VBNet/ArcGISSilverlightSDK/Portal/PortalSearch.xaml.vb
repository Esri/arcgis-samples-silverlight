Imports System
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports ESRI.ArcGIS.Client
Imports System.Text.RegularExpressions
Imports System.Windows.Data
Imports System.Windows.Input
Imports ESRI.ArcGIS.Client.WebMap
Imports ESRI.ArcGIS.Client.Portal


Partial Public Class PortalSearch
  Inherits UserControl
  Private Const DEFAULT_SERVER_URL As String = "http://www.arcgis.com/sharing/rest"
  Private Const DEFAULT_TOKEN_URL As String = "https://www.arcgis.com/sharing/generateToken"

  Private portal As New ArcGISPortal() With {.Url = DEFAULT_SERVER_URL}

  Public Sub New()
    InitializeComponent()
    IdentityManager.Current.RegisterServers(New IdentityManager.ServerInfo() {New IdentityManager.ServerInfo() With {.ServerUrl = DEFAULT_SERVER_URL, .TokenServiceUrl = DEFAULT_TOKEN_URL}})

    IdentityManager.Current.ChallengeMethod = AddressOf Challenge

    ' Initial search on load
    DoSearch()
  End Sub

  ' Activate IdentityManager but don't accept any challenge.
  ' User has to use the 'SignIn' button for getting its own maps.
  Private Sub Challenge(ByVal url As String, ByVal callback As Action(Of IdentityManager.Credential, Exception), ByVal options As IdentityManager.GenerateTokenOptions)
    callback(Nothing, New NotImplementedException())
  End Sub

  Private Sub QueryText_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)
    If e.Key = Key.Enter Then
      DoSearch()
      e.Handled = True
    End If
  End Sub

  Private Sub BackToResults_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    ResetVisibility()
  End Sub

  Private Sub DoSearch_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    DoSearch()
  End Sub

#Region "Sign in/out"
  Private Sub ShowSignIn_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    If SignInButton.Content.ToString() = "Sign In" Then
      ShadowGrid.Visibility = Visibility.Visible
      LoginGrid.Visibility = Visibility.Visible
    Else 'Sign Out
      ResultsListBox.ItemsSource = Nothing
      WebmapContent.Children.Clear()
      Dim crd = IdentityManager.Current.FindCredential(DEFAULT_SERVER_URL, portal.CurrentUser.UserName)
      IdentityManager.Current.RemoveCredential(crd)
      portal.InitializeAsync(portal.Url, Sub(credential, exception)
                                           If exception Is Nothing Then
                                             ResetVisibility()
                                             SignInButton.Content = "Sign In"
                                           Else
                                             MessageBox.Show("Error initializing portal : " & exception.Message)
                                             ShadowGrid.Visibility = Visibility.Collapsed
                                           End If
                                         End Sub)
    End If
  End Sub

  Private Sub SignIn_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    ResultsListBox.ItemsSource = Nothing
    WebmapContent.Children.Clear()
    IdentityManager.Current.GenerateCredentialAsync(DEFAULT_SERVER_URL, UserTextBox.Text, PasswordTextBox.Password, Sub(crd, ex)
                                                                                                                      If crd IsNot Nothing Then
                                                                                                                        IdentityManager.Current.AddCredential(crd)
                                                                                                                        portal.InitializeAsync(DEFAULT_SERVER_URL, Sub(credential, exception)
                                                                                                                                                                     If credential IsNot Nothing AndAlso credential.CurrentUser IsNot Nothing Then
                                                                                                                                                                       ResetVisibility()
                                                                                                                                                                       SignInButton.Content = "Sign Out"
                                                                                                                                                                     End If
                                                                                                                                                                   End Sub)
                                                                                                                      Else
                                                                                                                        MessageBox.Show("Could not log in. Please check credentials.")
                                                                                                                        ShadowGrid.Visibility = Visibility.Collapsed
                                                                                                                      End If
                                                                                                                    End Sub)
  End Sub
#End Region

  Private Sub ResetVisibility()
    WebmapContent.Visibility = Visibility.Collapsed
    MapItemDetail.Visibility = System.Windows.Visibility.Collapsed
    BackToResults.Visibility = System.Windows.Visibility.Collapsed
    LoginGrid.Visibility = Visibility.Collapsed
    ShadowGrid.Visibility = Visibility.Collapsed
  End Sub

  Private Sub DoSearch()
    ResultsListBox.ItemsSource = Nothing
    ResetVisibility()
    If QueryText Is Nothing OrElse String.IsNullOrEmpty(QueryText.Text.Trim()) Then
      Return
    End If
    If String.IsNullOrEmpty(portal.Url) Then
      portal.Url = DEFAULT_SERVER_URL
    End If
    Dim queryString = String.Format("{0} type:""web map"" NOT ""web mapping application""", QueryText.Text.Trim())
    If portal.CurrentUser IsNot Nothing AndAlso portal.ArcGISPortalInfo IsNot Nothing AndAlso (Not String.IsNullOrEmpty(portal.ArcGISPortalInfo.Id)) Then
      queryString = String.Format("{0} and orgid: ""{1}""", queryString, portal.ArcGISPortalInfo.Id)
    End If
    Dim searchParameters = New SearchParameters() With {.QueryString = queryString, .SortField = "avgrating", .SortOrder = QuerySortOrder.Descending, .Limit = 20}
    portal.SearchItemsAsync(searchParameters, Sub(result, err) ResultsListBox.ItemsSource = result.Results)
  End Sub

  Private Sub Image_MouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
    Dim portalItem As ArcGISPortalItem = TryCast((TryCast(sender, Image)).DataContext, ArcGISPortalItem)

    BackToResults.Visibility = System.Windows.Visibility.Visible
    Dim document = New Document()
    AddHandler document.GetMapCompleted, Sub(a, b)
                                           If b.Error Is Nothing Then
                                             WebmapContent.Children.Clear()
                                             WebmapContent.Children.Add(b.Map)
                                             WebmapContent.Visibility = Visibility.Visible
                                           End If

                                         End Sub
    document.GetMapAsync(portalItem.Id)
  End Sub

  Private Sub ResultItem_MouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
    MapItemDetail.DataContext = TryCast((TryCast(sender, Grid)).DataContext, ArcGISPortalItem)
    MapItemDetail.Visibility = System.Windows.Visibility.Visible
  End Sub

  Private Sub Cancel_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
    LoginGrid.Visibility = Visibility.Collapsed
    ShadowGrid.Visibility = Visibility.Collapsed
  End Sub
End Class

Public Class RatingConverter
  Implements IValueConverter
  Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
    Dim ratingvalue As Double = CDbl(value)
    Return ratingvalue / 5
  End Function

  Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
    If value.GetType() Is targetType Then
      Return value
    End If
    Throw New NotImplementedException()
  End Function
End Class

Public Class HtmlToTextConverter
  Implements IValueConverter
  Private Shared htmlLineBreakRegex As String = "(<br */>)|(\[br */\])" '"<br(.)*?>"; // Regular expression to strip HTML line break tag
  Private Shared htmlStripperRegex As String = "<(.|\n)*?>" ' Regular expression to strip HTML tags

  Public Shared Function GetHtmlToInlines(ByVal obj As DependencyObject) As String
    Return CStr(obj.GetValue(HtmlToInlinesProperty))
  End Function

  Public Shared Sub SetHtmlToInlines(ByVal obj As DependencyObject, ByVal value As String)
    obj.SetValue(HtmlToInlinesProperty, value)
  End Sub

  ' Using a DependencyProperty as the backing store for HtmlToInlinesProperty.  This enables animation, styling, binding, etc...
  Public Shared ReadOnly HtmlToInlinesProperty As DependencyProperty = DependencyProperty.RegisterAttached("HtmlToInlines", GetType(String), GetType(HtmlToTextConverter), New PropertyMetadata(Nothing, AddressOf OnHtmlToInlinesPropertyChanged))

  Private Shared Sub OnHtmlToInlinesPropertyChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
    If TypeOf d Is Paragraph Then
      If e.NewValue Is Nothing Then
        TryCast(d, Paragraph).Inlines.Clear()
      Else
        Dim splits = Regex.Split(TryCast(e.NewValue, String), htmlLineBreakRegex, RegexOptions.IgnoreCase Or RegexOptions.ECMAScript)
        For Each line In splits
          Dim text As String = System.Text.RegularExpressions.Regex.Replace(line, htmlStripperRegex, String.Empty)
          Dim regex As New Regex("[ ]{2,}", RegexOptions.None)
          If Not String.IsNullOrWhiteSpace(text) Then
            text = regex.Replace(text, " ") 'Remove multiple spaces
            text = text.Replace("&quot;", """") 'Unencode quotes
            text = text.Replace("&nbsp;", " ") 'Unencode spaces
            TryCast(d, Paragraph).Inlines.Add(New Run() With {.Text = text})
            TryCast(d, Paragraph).Inlines.Add(New LineBreak())
          End If
        Next line
      End If
    End If
  End Sub

  Private Shared Function ToStrippedHtmlText(ByVal input As Object) As String
    Dim retVal As String = String.Empty

    If input IsNot Nothing Then
      ' Replace HTML line break tags with $LINEBREAK$:
      retVal = Regex.Replace(TryCast(input, String), htmlLineBreakRegex, "", RegexOptions.IgnoreCase)
      ' Remove the rest of HTML tags:
      retVal = Regex.Replace(retVal, htmlStripperRegex, String.Empty)
      'retVal.Replace("$LINEBREAK$", "\n");
      retVal = retVal.Trim()
    End If

    Return retVal
  End Function

  Public Function Convert(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.Convert
    If TypeOf value Is String Then
      Return ToStrippedHtmlText(value)
    End If
    Return value
  End Function

  Public Function ConvertBack(ByVal value As Object, ByVal targetType As Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements IValueConverter.ConvertBack
    Throw New NotImplementedException()
  End Function
End Class
