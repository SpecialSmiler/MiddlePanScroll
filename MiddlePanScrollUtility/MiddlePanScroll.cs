namespace MiddlePanScroll
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Text.Editor;

    internal sealed class MiddlePanScroll : MouseProcessorBase
    {
        public static IMouseProcessor Create(IWpfTextView view)
        {
            return view.Properties.GetOrCreateSingletonProperty(delegate () { return new MiddlePanScroll(view); });
        }

        private IWpfTextView _view;
        private Cursor _oldCursor;
        private Point _lastMousePos;
        private bool _isCursorJumpedInLastUpdate = false;
        private bool _isPanning = false;
        private bool _isReadyToPan = false;

        private bool _isScrollByLine = true;
        private double _accumulateYPixel = 0.0;

        private MiddlePanScroll(IWpfTextView view)
        {
            _view = view;
            _view.Closed += OnClosed;
            _view.VisualElement.IsVisibleChanged += OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!_view.VisualElement.IsVisible)
            {
                this.StopPanning();
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            this.StopPanning();

            _view.VisualElement.IsVisibleChanged -= OnIsVisibleChanged;
            _view.Closed -= OnClosed;
        }

        // These methods get called for the entire mouse processing chain before calling PreprocessMouseDown
        public override void PreprocessMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this.PreprocessMouseDown(e);
        }

        public override void PreprocessMouseRightButtonDown(MouseButtonEventArgs e)
        {
            this.PreprocessMouseDown(e);
        }

        public override void PreprocessMouseDown(MouseButtonEventArgs e)
        {
            if (_isPanning)
            {
                this.StopPanning();
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Middle)
            {
                if (!_view.IsClosed && _view.VisualElement.IsVisible && _view.VisualElement.CaptureMouse())
                {
                    _isReadyToPan = true;
                    _lastMousePos = e.GetPosition(_view.VisualElement);
                    e.Handled = true;
                }
            }
        }

        public override void PreprocessMouseMove(MouseEventArgs e)
        {
            if (_isReadyToPan)
            {
                _isReadyToPan = false;
                _isPanning = true;
                _oldCursor = _view.VisualElement.Cursor;
                _view.VisualElement.Cursor = Cursors.ScrollAll;
            }

            if (_isPanning)
            {
                if (_isCursorJumpedInLastUpdate)
                {
                    _isCursorJumpedInLastUpdate = false;
                    e.Handled = true;
                    return;
                }

                Point position = e.GetPosition(_view.VisualElement);
                Vector delta = position - _lastMousePos;

                if (Math.Abs(delta.X) > Math.E * Math.Abs(delta.Y))
                {
                    _view.ViewScroller.ScrollViewportHorizontallyByPixels(-delta.X);
                }
                else
                {
                    if (_isScrollByLine)
                    {
                        _accumulateYPixel += delta.Y;
                        int lineCount = (int)(Math.Abs(_accumulateYPixel) / _view.LineHeight);
                        if (lineCount > 0)
                        {
                            ScrollDirection dir = _accumulateYPixel > 0.0 ? ScrollDirection.Up : ScrollDirection.Down;
                            _view.ViewScroller.ScrollViewportVerticallyByLines(dir, lineCount);
                            if (_accumulateYPixel > 0)
                            {
                                _accumulateYPixel -= lineCount * _view.LineHeight;
                            }
                            else
                            {
                                _accumulateYPixel += lineCount * _view.LineHeight;
                            }
                        }
                    }
                    else
                    {
                        _view.ViewScroller.ScrollViewportVerticallyByPixels(delta.Y);
                    }
                }

                Size visualSize = _view.VisualElement.RenderSize;
                Rect visualRect = new Rect(0.0, 0.0, visualSize.Width, visualSize.Height);

                Point newCursorPos = new Point(position.X, position.Y);
                bool needUpdateCursorX = true;
                bool needUpdateCursorY = true;

                if (position.Y > visualRect.Top && position.Y < visualRect.Bottom)
                {
                    needUpdateCursorY = false;
                }
                else if (position.Y <= visualRect.Top)
                {
                    newCursorPos.Y = visualRect.Bottom - 1.0;
                }
                else if (position.Y >= visualRect.Bottom)
                {
                    newCursorPos.Y = visualRect.Top + 1.0;
                }

                if (position.X > visualRect.Left && position.X < visualRect.Right)
                {
                    needUpdateCursorX = false;
                }
                else if (position.X <= visualRect.Left)
                {
                    newCursorPos.X = visualRect.Right - 1.0;
                }
                else if (position.X >= visualRect.Right)
                {
                    newCursorPos.X = visualRect.Left + 1.0;
                }

                if (needUpdateCursorX || needUpdateCursorY)
                {
                    _lastMousePos = newCursorPos;
                    Point newCursorScreenPos = _view.VisualElement.PointToScreen(newCursorPos);
                    User32.SetCursorPos(Convert.ToInt32(newCursorScreenPos.X), Convert.ToInt32(newCursorScreenPos.Y));
                    _isCursorJumpedInLastUpdate = true;
                }
                else
                {
                    _lastMousePos = position;
                }
                e.Handled = true;
            }
        }

        public override void PreprocessMouseUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {
                this.StopPanning();
                e.Handled = true;
            }
        }

        private void StopPanning()
        {
            if (_isPanning || _isReadyToPan)
            {
                _isPanning = false;
                _isReadyToPan = false;
                _isCursorJumpedInLastUpdate = false;
                if (_oldCursor != null)
                {
                    _view.VisualElement.Cursor = _oldCursor;
                    _oldCursor = null;
                }
                _view.VisualElement.ReleaseMouseCapture();
            }
        }
    }

    internal static class User32
    {
        [DllImport("user32.dll", EntryPoint = "SetCursorPos")]
        public static extern int SetCursorPos(int x, int y);
    }

}
