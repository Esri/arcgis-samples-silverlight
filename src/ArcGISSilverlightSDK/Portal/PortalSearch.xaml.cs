using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using ESRI.ArcGIS.Client;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using ESRI.ArcGIS.Client.WebMap;
using ESRI.ArcGIS.Client.Portal;


namespace ArcGISSilverlightSDK
{
    public partial class PortalSearch : UserControl
    {
        private const string DEFAULT_SERVER_URL = "https://www.arcgis.com/sharing/rest";
        private const string DEFAULT_TOKEN_URL = "https://www.arcgis.com/sharing/generateToken";

        ArcGISPortal portal = new ArcGISPortal() { Url = DEFAULT_SERVER_URL };

        public PortalSearch()
        {
            InitializeComponent();
            IdentityManager.Current.RegisterServers(new IdentityManager.ServerInfo[]
                        {
                            new IdentityManager.ServerInfo() 
                            {
                                ServerUrl = DEFAULT_SERVER_URL,
                                TokenServiceUrl = DEFAULT_TOKEN_URL
                            }
                        });

            IdentityManager.Current.ChallengeMethod = Challenge;

            // Initial search on load
            DoSearch();
        }

        // Activate IdentityManager but don't accept any challenge.
        // User has to use the 'SignIn' button for getting its own maps.
        private static void Challenge(string url,
                               Action<IdentityManager.Credential, Exception> callback,
                               IdentityManager.GenerateTokenOptions options)
        {
            callback(null, new NotImplementedException());
        }

        private void QueryText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DoSearch();
                e.Handled = true;
            }
        }

        private void BackToResults_Click(object sender, RoutedEventArgs e)
        {
            ResetVisibility();
        }

        private void DoSearch_Click(object sender, RoutedEventArgs e)
        {
            DoSearch();
        }

        #region Sign in/out
        private void ShowSignIn_Click(object sender, RoutedEventArgs e)
        {
            if (SignInButton.Content.ToString() == "Sign In")
            {
                ShadowGrid.Visibility = Visibility.Visible;
                LoginGrid.Visibility = Visibility.Visible;
            }
            else //Sign Out
            {
                ResultsListBox.ItemsSource = null;
                WebmapContent.Children.Clear();
                var crd = IdentityManager.Current.FindCredential(DEFAULT_SERVER_URL, portal.CurrentUser.UserName);
                IdentityManager.Current.RemoveCredential(crd);
                portal.InitializeAsync(portal.Url, (credential, exception) =>
                {
                    if (exception == null)
                    {
                        ResetVisibility();
                        SignInButton.Content = "Sign In";
                    }
                    else
                    {
                        MessageBox.Show("Error initializing portal : " + exception.Message);
                        ShadowGrid.Visibility = Visibility.Collapsed;
                    }
                });
            }
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            ResultsListBox.ItemsSource = null;
            WebmapContent.Children.Clear();
            IdentityManager.Current.GenerateCredentialAsync(DEFAULT_SERVER_URL, UserTextBox.Text, PasswordTextBox.Password, (crd, ex) =>
            {
                if (crd != null)
                {
                    IdentityManager.Current.AddCredential(crd);
                    portal.InitializeAsync(DEFAULT_SERVER_URL, (credential, exception) =>
                    {
                        if (credential != null && credential.CurrentUser != null)
                        {
                            ResetVisibility();
                            SignInButton.Content = "Sign Out";
                        }
                    });
                }
                else
                {
                    MessageBox.Show("Could not log in. Please check credentials.");
                    ShadowGrid.Visibility = Visibility.Collapsed;
                }
            });
        }     
        #endregion

        private void ResetVisibility()
        {
            WebmapContent.Visibility = Visibility.Collapsed;
            MapItemDetail.Visibility = System.Windows.Visibility.Collapsed;
            BackToResults.Visibility = System.Windows.Visibility.Collapsed;
            LoginGrid.Visibility = Visibility.Collapsed;
            ShadowGrid.Visibility = Visibility.Collapsed;
        }

        private void DoSearch()
        {
            ResultsListBox.ItemsSource = null;
            ResetVisibility();
            if (QueryText == null || string.IsNullOrEmpty(QueryText.Text.Trim())) return;
            if (string.IsNullOrEmpty(portal.Url))
                portal.Url = DEFAULT_SERVER_URL;
            var queryString = string.Format("{0} type:\"web map\" NOT \"web mapping application\"", QueryText.Text.Trim());
            if (portal.CurrentUser != null && portal.ArcGISPortalInfo != null && !string.IsNullOrEmpty(portal.ArcGISPortalInfo.Id))
                queryString = string.Format("{0} and orgid: \"{1}\"", queryString, portal.ArcGISPortalInfo.Id);
            var searchParameters = new SearchParameters()
            {
                QueryString = queryString,
                SortField = "avgrating",
                SortOrder = QuerySortOrder.Descending,
                Limit = 20
            };
            portal.SearchItemsAsync(searchParameters, (result, error) =>
            {
                ResultsListBox.ItemsSource = result.Results;
            });
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ArcGISPortalItem portalItem = (sender as Image).DataContext as ArcGISPortalItem;

            BackToResults.Visibility = System.Windows.Visibility.Visible;
            var document = new Document();
            document.GetMapCompleted += (a, b) =>
            {
                if (b.Error == null)
                {
                    WebmapContent.Children.Clear();
                    WebmapContent.Children.Add(b.Map);
                    WebmapContent.Visibility = Visibility.Visible;
                }

            };
            document.GetMapAsync(portalItem.Id);
        }

        private void ResultItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MapItemDetail.DataContext = (sender as Grid).DataContext as ArcGISPortalItem;
            MapItemDetail.Visibility = System.Windows.Visibility.Visible;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            ShadowGrid.Visibility = Visibility.Collapsed;
        }
    }

    public class RatingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double ratingvalue = (double)value;
            return ratingvalue / 5;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value.GetType() == targetType)
                return value;
            throw new NotImplementedException();
        }
    }

    public class HtmlToTextConverter : IValueConverter
    {
        private static string htmlLineBreakRegex = @"(<br */>)|(\[br */\])"; //@"<br(.)*?>";	// Regular expression to strip HTML line break tag
        private static string htmlStripperRegex = @"<(.|\n)*?>";	// Regular expression to strip HTML tags

        public static string GetHtmlToInlines(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlToInlinesProperty);
        }

        public static void SetHtmlToInlines(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlToInlinesProperty, value);
        }

        // Using a DependencyProperty as the backing store for HtmlToInlinesProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HtmlToInlinesProperty =
          DependencyProperty.RegisterAttached("HtmlToInlines", typeof(string), typeof(HtmlToTextConverter), new PropertyMetadata(null, OnHtmlToInlinesPropertyChanged));

        private static void OnHtmlToInlinesPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Paragraph)
            {
                if (e.NewValue == null)
                    (d as Paragraph).Inlines.Clear();
                else
                {
                    var splits = Regex.Split(e.NewValue as string, htmlLineBreakRegex, RegexOptions.IgnoreCase | RegexOptions.ECMAScript);
                    foreach (var line in splits)
                    {
                        string text = Regex.Replace(line, htmlStripperRegex, string.Empty);
                        Regex regex = new Regex(@"[ ]{2,}", RegexOptions.None);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            text = regex.Replace(text, @" "); //Remove multiple spaces
                            text = text.Replace("&quot;", "\""); //Unencode quotes
                            text = text.Replace("&nbsp;", " "); //Unencode spaces
                            (d as Paragraph).Inlines.Add(new Run() { Text = text });
                            (d as Paragraph).Inlines.Add(new LineBreak());
                        }
                    }
                }
            }
        }

        private static string ToStrippedHtmlText(object input)
        {
            string retVal = string.Empty;

            if (input != null)
            {
                // Replace HTML line break tags with $LINEBREAK$:
                retVal = Regex.Replace(input as string, htmlLineBreakRegex, "", RegexOptions.IgnoreCase);
                // Remove the rest of HTML tags:
                retVal = Regex.Replace(retVal, htmlStripperRegex, string.Empty);
                //retVal.Replace("$LINEBREAK$", "\n");
                retVal = retVal.Trim();
            }

            return retVal;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
                return ToStrippedHtmlText(value);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
