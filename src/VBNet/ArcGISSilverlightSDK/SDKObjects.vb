Imports Microsoft.VisualBasic
Imports System
Imports System.Net
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Documents
Imports System.Windows.Ink
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation
Imports System.Windows.Shapes


  Public Class Category ': Microsoft.Windows.Controls.TreeViewItem
    Private privateName As String
    Public Property Name() As String
      Get
        Return privateName
      End Get
      Set(ByVal value As String)
        privateName = value
      End Set
    End Property
    Private privateIcon As String
    Public Property Icon() As String
      Get
        Return privateIcon
      End Get
      Set(ByVal value As String)
        privateIcon = value
      End Set
    End Property
    Private privateCategoryItems As CategoryItem()
    Public Property CategoryItems() As CategoryItem()
      Get
        Return privateCategoryItems
      End Get
      Set(ByVal value As CategoryItem())
        privateCategoryItems = value
      End Set
    End Property

    Public Sub New()
    End Sub
    Public Sub New(ByVal name As String)
      name = name
    End Sub
  End Class

  Public Class CategoryItem ': Microsoft.Windows.Controls.TreeViewItem
    Private privateID As String
    Public Property ID() As String
      Get
        Return privateID
      End Get
      Set(ByVal value As String)
        privateID = value
      End Set
    End Property
    Private privateXAML As String
    Public Property XAML() As String
      Get
        Return privateXAML
      End Get
      Set(ByVal value As String)
        privateXAML = value
      End Set
    End Property
    Private privateSource As String
    Public Property Source() As String
      Get
        Return privateSource
      End Get
      Set(ByVal value As String)
        privateSource = value
      End Set
    End Property
    Private privateCode As String
    Public Property Code() As String
      Get
        Return privateCode
      End Get
      Set(ByVal value As String)
        privateCode = value
      End Set
    End Property
    Private privateIcon As String
    Public Property Icon() As String
      Get
        Return privateIcon
      End Get
      Set(ByVal value As String)
        privateIcon = value
      End Set
    End Property

    Public Sub New()
    End Sub
    Public Sub New(ByVal name As String, ByVal xaml As String)
      ID = name
      xaml = xaml
    End Sub
  End Class

