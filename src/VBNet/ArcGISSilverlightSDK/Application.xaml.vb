Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls

Partial Public Class App
    Inherits Application

    Public Sub New()
        AddHandler Me.Startup, AddressOf Application_Startup
        AddHandler Me.Exit, AddressOf Application_Exit
        AddHandler Me.UnhandledException, AddressOf Application_UnhandledException

        InitializeComponent()
    End Sub

    Private Sub Application_Startup(ByVal sender As Object, ByVal e As StartupEventArgs)
        Dim xmlConfig As String = e.InitParams("xml")

        'this.RootVisual = new Page(xmlConfig);
        Me.RootVisual = New LightPage(xmlConfig)

        System.Windows.Browser.HtmlPage.RegisterScriptableObject("mySLapp", Me.RootVisual)
    End Sub

    Private Sub Application_Exit(ByVal sender As Object, ByVal e As EventArgs)

    End Sub
    Private Sub Application_UnhandledException(ByVal sender As Object, ByVal e As ApplicationUnhandledExceptionEventArgs)
        ' If the app is running outside of the debugger then report the exception using
        ' the browser's exception mechanism. On IE this will display it a yellow alert 
        ' icon in the status bar and Firefox will display a script error.
        If (Not System.Diagnostics.Debugger.IsAttached) Then

            ' NOTE: This will allow the application to continue running after an exception has been thrown
            ' but not handled. 
            ' For production applications this error handling should be replaced with something that will 
            ' report the error to the website and stop the application.
            e.Handled = True
            Deployment.Current.Dispatcher.BeginInvoke(New Action(Of ApplicationUnhandledExceptionEventArgs)(AddressOf ReportErrorToDOM), e)
            'Deployment.Current.Dispatcher.BeginInvoke(delegate())
            '					ReportErrorToDOM(e)
        End If
    End Sub
    Private Sub ReportErrorToDOM(ByVal e As ApplicationUnhandledExceptionEventArgs)
        Try
            Dim errorMsg As String = e.ExceptionObject.Message + e.ExceptionObject.StackTrace
            errorMsg = errorMsg.Replace(""""c, "'"c).Replace(Constants.vbCrLf, Constants.vbLf)

            System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(""Unhandled Error in Silverlight Application " & errorMsg & """);")
        Catch e1 As Exception
        End Try
  End Sub

End Class

