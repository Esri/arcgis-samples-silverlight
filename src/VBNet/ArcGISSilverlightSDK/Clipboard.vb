Imports Microsoft.VisualBasic
Imports System
Imports System.Windows.Browser


  ''' <summary>
  ''' Summary description for Clipboard
  ''' </summary>
  Public NotInheritable Class Clipboard
    Private Const HostNoClipboard As String = "The clipboard isn't available in the current host."
    Private Const ClipboardFailure As String = "The text couldn't be copied into the clipboard."
    Private Const BeforeFlashCopy As String = "The text will now attempt to be copied..."
    Private Const FlashMimeType As String = "application/x-shockwave-flash"

    ' HARD-CODED!   
    Private Const ClipboardFlashMovie As String = "http://appengine.bravo9.com/copy-into-clipboard/clipboard.swf"
    'const string ClipboardFlashMovie = "Assets/javascript/clipboard.swf";

    ''' <summary>   
    ''' Write to the clipboard (IE and/or Flash)   
    ''' </summary>   
    Private Sub New()
    End Sub
    Public Shared Sub SetText(ByVal text As String)
      ' document.window.clipboardData.setData(format, data);   
      Dim clipboardData = CType(HtmlPage.Window.GetProperty("clipboardData"), ScriptObject)
      If clipboardData IsNot Nothing Then
        Dim success As Boolean = CBool(clipboardData.Invoke("setData", "text", text))
        If (Not success) Then
          HtmlPage.Window.Alert(ClipboardFailure)
        End If
      Else
        HtmlPage.Window.Alert(BeforeFlashCopy)

        ' Append a Flash embed element with the data encoded   
        Dim safeText As String = HttpUtility.UrlEncode(text)
        Dim elem = HtmlPage.Document.CreateElement("div")
        HtmlPage.Document.Body.AppendChild(elem)
        elem.SetProperty("innerHTML", "<embed src=""" & ClipboardFlashMovie & """ " & "FlashVars=""clipboard=" & safeText & """ width=""0"" " & "height=""0"" type=""" & FlashMimeType & """></embed>")
      End If
    End Sub

  End Class


