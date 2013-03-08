Imports System
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.ComponentModel
Imports System.Collections.ObjectModel

Partial Public Class UsingPointDataSource
  Inherits UserControl
  Public Sub New()
    InitializeComponent()
  End Sub
End Class

Public Class MainModel
  Implements INotifyPropertyChanged
  Private _PointsOfInterest As ObservableCollection(Of DataPoint)
  Public Property PointsOfInterest() As ObservableCollection(Of DataPoint)
    Get
      Return _PointsOfInterest
    End Get
    Set(ByVal value As ObservableCollection(Of DataPoint))
      If _PointsOfInterest IsNot value Then
        _PointsOfInterest = value
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("PointsOfInterest"))
      End If
    End Set
  End Property
  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class

Public Class DataPoint
  Implements INotifyPropertyChanged
  Private _X As Double
  Private _Y As Double
  Private _IsSelected As Boolean

  Public Property X() As Double
    Get
      Return _X
    End Get
    Set(ByVal value As Double)
      If _X <> value Then
        _X = value
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("X"))
      End If
    End Set
  End Property

  Public Property Y() As Double
    Get
      Return _Y
    End Get
    Set(ByVal value As Double)
      If _Y <> value Then
        _Y = value
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Y"))
      End If
    End Set
  End Property

  Public _Name As String

  Public Property Name() As String
    Get
      Return _Name
    End Get
    Set(ByVal value As String)
      If _Name <> value Then
        _Name = value
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("Name"))
      End If
    End Set
  End Property

  Public Property IsSelected() As Boolean
    Get
      Return _IsSelected
    End Get
    Set(ByVal value As Boolean)
      If _IsSelected <> value Then
        _IsSelected = value
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("IsSelected"))
      End If
    End Set
  End Property
  Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
End Class

Public Class MainViewModel
  Private Const width As Double = 360
  Private Const height As Double = 180
  Private Shared r As New Random()

  Public Property Data() As MainModel
  Private Shared mvm_instance As MainViewModel
  Public ReadOnly Property Instance() As MainViewModel
    Get
      If mvm_instance Is Nothing Then
        mvm_instance = New MainViewModel()
      End If
      Return mvm_instance
    End Get
  End Property
  Private privateRandomize As ICommand
  Public Property Randomize() As ICommand
    Get
      Return privateRandomize
    End Get
    Private Set(ByVal value As ICommand)
      privateRandomize = value
    End Set
  End Property
  Private privateAddRandom As ICommand
  Public Property AddRandom() As ICommand
    Get
      Return privateAddRandom
    End Get
    Private Set(ByVal value As ICommand)
      privateAddRandom = value
    End Set
  End Property
  Private privateRemoveFirst As ICommand
  Public Property RemoveFirst() As ICommand
    Get
      Return privateRemoveFirst
    End Get
    Private Set(ByVal value As ICommand)
      privateRemoveFirst = value
    End Set
  End Property

  Public Sub New()
    Data = New MainModel() With
           {
             .PointsOfInterest = New System.Collections.ObjectModel.ObservableCollection(Of DataPoint)()
           }
    GenerateDataSet()

    AddRandom = New DelegateCommand(Sub(a) AddRandomEntry(), Function(b)
                                                               Return True
                                                             End Function)
    RemoveFirst = New DelegateCommand(Sub(a)
                                        If Data.PointsOfInterest.Count > 0 Then
                                          Data.PointsOfInterest.RemoveAt(0)
                                        End If
                                      End Sub, Function(b) True)
    Randomize = New DelegateCommand(Sub(a) RandomizeEntries(), Function(b)
                                                                 Return True
                                                               End Function)
  End Sub

#Region "Generate random data "
  Private Sub GenerateDataSet()
    For i As Integer = 0 To 9
      AddRandomEntry()
    Next i
  End Sub

  Private Sub AddRandomEntry()
    Data.PointsOfInterest.Add(CreateRandomEntry(Data.PointsOfInterest.Count))
  End Sub

  Private Function CreateRandomEntry(ByVal i As Integer) As DataPoint
    Return New DataPoint() With {.X = r.NextDouble() * width - width * 0.5, .Y = r.NextDouble() * height - height * 0.5, .Name = String.Format("Item #{0}", i)}
  End Function

  Private Sub RandomizeEntries()
    For i As Integer = 0 To Data.PointsOfInterest.Count - 1
      Dim pnt = Data.PointsOfInterest(r.Next(Data.PointsOfInterest.Count))
      pnt.X = r.NextDouble() * width - width * 0.5
      pnt.Y = r.NextDouble() * height - height * 0.5
    Next i
  End Sub
#End Region

  
End Class


Public Class DelegateCommand
  Implements ICommand

  Dim can_Execute As Func(Of Object, Boolean)
  Dim executeAction As Action(Of Object)
  Dim canExecuteCache As Boolean

  Public Sub New(ByVal executeAction As Action(Of Object), ByVal canExecute As Func(Of Object, Boolean))
    Me.executeAction = executeAction
    Me.can_Execute = canExecute
  End Sub


  Public Function CanExecute(ByVal parameter As Object) As Boolean Implements System.Windows.Input.ICommand.CanExecute
    Dim temp As Boolean = can_Execute(parameter)
    If canExecuteCache <> temp Then
      canExecuteCache = temp
      If Not CanExecuteChangedEvent Is Nothing Then
        RaiseEvent CanExecuteChanged(Me, New EventArgs())
      End If
    End If

    Return canExecuteCache
  End Function
  Public Event CanExecuteChanged(sender As Object, e As System.EventArgs) Implements System.Windows.Input.ICommand.CanExecuteChanged

  Public Sub Execute(parameter As Object) Implements System.Windows.Input.ICommand.Execute
    executeAction(parameter)
  End Sub

  
End Class

