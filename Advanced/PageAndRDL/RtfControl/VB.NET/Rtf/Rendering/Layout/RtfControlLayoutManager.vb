﻿Imports System
Imports System.Drawing
Imports System.Windows.Forms

Imports GrapeCity.ActiveReports.Samples.Rtf.Control
Imports GrapeCity.ActiveReports.Extensibility.Layout
Imports GrapeCity.ActiveReports.Extensibility.Rendering
Imports GrapeCity.ActiveReports.Extensibility.Rendering.Components

Namespace Rendering.Layout
    Public Class RtfControlLayoutManager
        Implements ILayoutManager
        
        Private _item As IReportItem
        Private _control As RtfControl
        Private _status As LayoutStatus = LayoutStatus.None
        Private _computedSize As SizeF
        
        Public ReadOnly Property Capabilities As LayoutCapabilities Implements ILayoutManager.Capabilities
            Get
                Return LayoutCapabilities.CanGrowVertically Or LayoutCapabilities.CanShrinkVertically
            End Get
        End Property
        
        Public Sub Initialize(forReportItem As IReportItem, targetDevice As ITargetDevice) Implements ILayoutManager.Initialize
            _item = forReportItem
            _control = CType(_item, RtfControl)
            _computedSize = New SizeF(CType(_item.Width.ToTwips, Integer), CType(_item.Height.ToTwips, Integer))

            ProcessGrow()
            ProcessShrink()
        End Sub
        
        Public Function Measure(ctx As LayoutContext) As LayoutResult Implements ILayoutManager.Measure
            Dim content = CType(ctx.ContentRange, RtfControlContentRange)

            If _computedSize.Width = 0 Then _computedSize.Width = ctx.ReportItemSize.Width
            If _computedSize.Height = 0 Then _computedSize.Height = ctx.ReportItemSize.Height

            If ctx.VerticalLayout Then
                If (content Is Nothing) Then
                    Dim fitHeight = Math.Min(ctx.AvailableSize.Height, _computedSize.Height)
                    Dim range = New Range(0, fitHeight, 0, -1)
                    Dim rtf = GetNextRtf(range.Bottom)
                    Dim info = New RtfContentInfo(rtf, range, _computedSize)
                    
                    content = New RtfControlContentRange(_control, info)
                    _computedSize.Height = fitHeight
                Else
                    Dim restHeight = (content.Info.TotalSize.Height - content.EndVertRange)
                    Dim fitHeight = Math.Min(ctx.AvailableSize.Height, restHeight)
                    Dim rtf = GetNextRtf((content.EndVertRange + fitHeight))
                    
                    content = content.GetNextRange(fitHeight, rtf)
                    _computedSize.Height = fitHeight
                End If

                _status = _status Or LayoutStatus.ContinueVertically Or LayoutStatus.SomeContent
                
                If content.isLastPage Then
                    Return New LayoutResult(content, (LayoutStatus.SomeContent Or LayoutStatus.Complete), _computedSize)
                End If
            End If
            
            Return New LayoutResult(content, _status, _computedSize)
        End Function
        
        Public Function Layout(ctx As LayoutContext) As LayoutResult Implements ILayoutManager.Layout
            Dim content = CType(ctx.ContentRange,RtfControlContentRange)
            Return New LayoutResult(content, _status, _computedSize)
        End Function
        
        Private Sub ResizeBoxToContent(sender As Object, e As ContentsResizedEventArgs)
            Dim rtb = CType(sender,RichTextBox)
            rtb.Height = e.NewRectangle.Height
        End Sub
        
        Private Function GetNeededSize() As SizeF
            Dim width = TwipsToPixels(_computedSize.Width)
            Dim box = New RichTextBoxFixed
            
            AddHandler box.ContentsResized, AddressOf ResizeBoxToContent
            box.Width = width
            box.SetRtfOrText(_control.Rtf)
            
            Return New SizeF(PixelsToTwips(box.Size.Width), PixelsToTwips(box.Size.Height))
        End Function
        
        Private Sub ProcessGrow()
            If Not _control.CanGrow Then
                Return
            End If
            
            Dim actualSize = GetNeededSize
            If (_computedSize.Height < actualSize.Height) Then
                _computedSize.Height = actualSize.Height
            End If
        End Sub
        
        Private Sub ProcessShrink()
            If Not _control.CanShrink Then
                Return
            End If
            
            Dim actualSize = GetNeededSize
            If (_computedSize.Height > actualSize.Height) Then
                _computedSize.Height = actualSize.Height
            End If
        End Sub
        
        Private _firstChar As Integer
        Private _lastChar As Integer
        
        Private Function GetNextRtf(target As Single) As String
            Dim bottom = TwipsToPixels(target)
            Dim rtb = New RichTextBoxFixed
            
            AddHandler rtb.ContentsResized, AddressOf ResizeBoxToContent
            rtb.Width = TwipsToPixels(_computedSize.Width)
            rtb.SetRtfOrText(_control.Rtf)

            Dim lastCharPoint = rtb.GetPositionFromCharIndex(rtb.TextLength - 1)
            Dim neededPoint = New Point(lastCharPoint.X, Math.Min(lastCharPoint.Y, bottom + rtb.PreferredHeight))

            _lastChar = rtb.GetCharIndexFromPosition(neededPoint)
            _lastChar = If(rtb.TextLength - 1 = _lastChar, rtb.TextLength, _lastChar)

            rtb.Select(_firstChar, _lastChar - _firstChar)
            _firstChar = _lastChar
            
            Return rtb.SelectedRtf
        End Function

        Private TWIPS_PER_PIXEL As Single = (GlobalConstants.FTwipsPerInch / RenderUtils.VerticalResolution)

        Private Function TwipsToPixels(twips As Single) As Integer
            Return CType(Math.Ceiling(twips / TWIPS_PER_PIXEL), Integer)
        End Function
        
        Private Function PixelsToTwips(pixels As Integer) As Single
            Return pixels * TWIPS_PER_PIXEL
        End Function
    End Class
End Namespace