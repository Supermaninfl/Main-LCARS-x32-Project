﻿Option Strict On

Imports System.Drawing
Imports System.ComponentModel

Namespace Controls
    Public Class Slider
        Inherits System.Windows.Forms.Control
        Implements IColorable, IAlertable

#Region " LCARS colors and alerts "
        Private WithEvents _colors As New LCARScolor
        Private _redAlert As LCARSalert = LCARSalert.Normal
        Private _customAlertColor As Color = Nothing

        <DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
        Public Property ColorsAvailable() As LCARScolor Implements IColorable.ColorsAvailable
            Get
                Return _colors
            End Get
            Set(ByVal value As LCARScolor)
                _colors = value
                Me.Invalidate()
            End Set
        End Property

        Public Property CustomAlertColor() As System.Drawing.Color Implements IAlertable.CustomAlertColor
            Get
                Return _customAlertColor
            End Get
            Set(ByVal value As System.Drawing.Color)
                If value = _customAlertColor Then Return
                _customAlertColor = value
                If _redAlert = LCARSalert.Custom Then
                    Me.Invalidate()
                End If
            End Set
        End Property

        <DefaultValue(LCARSalert.Normal)> _
        Public Property RedAlert() As LCARSalert Implements IAlertable.RedAlert
            Get
                Return _redAlert
            End Get
            Set(ByVal value As LCARSalert)
                If value = _redAlert Then Return
                _redAlert = value
                Me.Invalidate()
            End Set
        End Property
#End Region

        'TODO: Properties for min, max, value, padding, buttonheight
        'TODO: Make text configurable
        Private _min As Integer = 0
        Private _max As Integer = 100
        Private _value As Integer = 50
        Private _mouseDown As Boolean = False
        Private _padding As Integer = 5
        Private _buttonHeight As Integer = 30
        Private _color As LCARScolorStyles = LCARScolorStyles.MiscFunction
        Private _color2 As LCARScolorStyles = LCARScolorStyles.PrimaryFunction
        Private _lit As Boolean = True

        <DefaultValue(LCARScolorStyles.MiscFunction)> _
        Public Property MainColor() As LCARScolorStyles
            Get
                Return _color
            End Get
            Set(ByVal value As LCARScolorStyles)
                If value = _color Then Return
                _color = value
                Me.Invalidate()
            End Set
        End Property

        <DefaultValue(LCARScolorStyles.PrimaryFunction)> _
        Public Property ButtonColor() As LCARScolorStyles
            Get
                Return _color2
            End Get
            Set(ByVal value As LCARScolorStyles)
                If value = _color2 Then Return
                _color2 = value
                Me.Invalidate()
            End Set
        End Property

        Public Property Lit() As Boolean
            Get
                Return _lit
            End Get
            Set(ByVal value As Boolean)
                If value = _lit Then Return
                _lit = value
                Me.Invalidate()
            End Set
        End Property

        Public Sub New()
            MyBase.New()
            Me.Size = New Size(30, 200)
        End Sub

        Protected Overrides Sub OnMouseDown(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseDown(e)
            If e.Button = Windows.Forms.MouseButtons.Left And buttonBounds.Contains(PointToClient(MousePosition)) Then
                _mouseDown = True
                Me.InvalidateBar()
            End If
        End Sub

        Protected Overrides Sub OnMouseUp(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseUp(e)
            If e.Button = Windows.Forms.MouseButtons.Left And buttonBounds.Contains(PointToClient(MousePosition)) Then
                _mouseDown = False
                Me.InvalidateBar()
            End If
        End Sub

        Protected Overrides Sub OnMouseMove(ByVal e As System.Windows.Forms.MouseEventArgs)
            MyBase.OnMouseMove(e)
            If e.Button = Windows.Forms.MouseButtons.Left And _mouseDown Then
                'TODO: Record old position to avoid jumping
                'TODO: Move to sub to support event pattern
                Dim h As Integer = Me.Height - Me.Width \ 2 - 2 * _padding - _buttonHeight
                Dim y As Integer = PointToClient(MousePosition).Y - _padding - Me.Width \ 4 - _buttonHeight \ 2
                Dim newValue As Integer = CInt(Math.Round(y * (_min - _max) / h + _max))
                'Clip to range
                If _min < _max Then
                    If newValue < _min Then
                        newValue = _min
                    ElseIf newValue > _max Then
                        newValue = _max
                    End If
                Else
                    If newValue > _min Then
                        newValue = _min
                    ElseIf newValue < _max Then
                        newValue = _max
                    End If
                End If
                'If _value <> newValue Or True Then
                _value = newValue
                Me.InvalidateBar()
                'End If
            End If
        End Sub

        Protected ReadOnly Property buttonBounds() As Rectangle
            Get
                Dim h As Integer = Me.Height - Me.Width \ 2 - 2 * _padding - _buttonHeight
                Dim y As Double
                If _mouseDown Then
                    y = PointToClient(MousePosition).Y - _buttonHeight / 2 - Me.Width / 4 - _padding
                    If y < 0 Then
                        y = 0
                    ElseIf y > h Then
                        y = h
                    End If
                Else
                    y = (_value - _max) / (_min - _max) * h
                End If
                Return New Rectangle(0, CInt(y) + _padding + Me.Width \ 4, Me.Width, _buttonHeight)
            End Get
        End Property

        Protected Sub InvalidateBar()
            Me.Invalidate(New Rectangle(0, Me.Width \ 4, Me.Width, Me.Height - Me.Width \ 2))
        End Sub

        Protected Overrides Sub OnPaint(ByVal e As System.Windows.Forms.PaintEventArgs)
            Dim g As Graphics = Nothing
            Dim mybitmap As Bitmap = Nothing
            'Setup buffering
            Dim useBuffer As Boolean = False
            If useBuffer Then
                mybitmap = New Bitmap(Me.Width, Me.Height)
                g = Graphics.FromImage(mybitmap)
            Else
                g = e.Graphics()
            End If
            Dim c As Color
            Dim c2 As Color
            Select Case _redAlert
                Case LCARSalert.Normal
                    c = ColorsAvailable.getColor(_color)
                Case LCARSalert.Red
                    c = Color.Red
                Case LCARSalert.White
                    c = Color.White
                Case LCARSalert.Yellow
                    c = Color.Yellow
                Case LCARSalert.Custom
                    c = _customAlertColor
                Case Else
                    c = Color.Red
            End Select
            If Not _lit Then
                c = Color.FromArgb(128, c)
            End If
            Dim btnRect As Rectangle = buttonBounds
            If _redAlert = LCARSalert.Normal Then
                If _mouseDown And btnRect.Contains(PointToClient(MousePosition)) Then
                    c2 = Color.Red
                Else
                    c2 = ColorsAvailable.getColor(_color2)
                End If
            Else
                c2 = c
            End If

            'Setup brushes
            Dim b As New SolidBrush(c)
            Dim b2 As New SolidBrush(c2)

            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.PixelOffsetMode = Drawing2D.PixelOffsetMode.HighQuality
            'Draw top/bottom ellipse sections if required
            If e.ClipRectangle.Height <> Me.Height - Me.Width \ 2 Then
                g.FillPie(b, 0, 0, Me.Width, Me.Width \ 2, 180, 180)
                g.FillPie(b, 0, Me.Height - Me.Width \ 2, Me.Width, Me.Width \ 2, 0, 180)
            End If

            'Draw main rectangles
            Dim railStart As Integer = Me.Width \ 4
            g.FillRectangle(b, 0, railStart, Me.Width, btnRect.Top - railStart - _padding)
            g.FillRectangle(b, 0, btnRect.Bottom + _padding, Me.Width, Me.Height - (btnRect.Bottom + _padding) - Me.Width \ 4)
            'Draw button
            g.FillRectangle(b2, btnRect)
            Dim f As New Font("LCARS", 16, FontStyle.Regular, GraphicsUnit.Point)
            g.DrawString(_value.ToString, f, Brushes.Black, btnRect)

            'Draw final image if buffered
            If useBuffer Then
                Dim myG As Graphics = e.Graphics()
                myG.DrawImage(mybitmap, New PointF(0, 0))
                myG.Dispose()
                g.Dispose()
                mybitmap.Dispose()
            Else
                g.Dispose()
            End If
        End Sub
    End Class
End Namespace