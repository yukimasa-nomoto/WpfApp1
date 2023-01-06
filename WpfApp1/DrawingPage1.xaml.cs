using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;
using SharpVectors.Renderers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Markup;
using SharpVectors.Runtime;
using System.Globalization;
using System.Windows.Threading;

namespace WpfApp1
{


    /// <summary>
    /// DrawingPage1.xaml の相互作用ロジック
    /// </summary>
    public partial class DrawingPage1 : Page
    {
        private const double ZoomChange = 0.1;


        public const string TemporalDirName = "_Drawings";

        #region 変数
        private MainWindow5 _mainWindow;
        private OptionSettings _optionSettings;
        private string _drawingDir;
        private DirectoryInfo _directoryInfo;
        private FileSvgReader _fileReader;

        private bool _saveXaml;
        private WpfDrawingSettings _wpfSettings;

        private bool _isLoadingDrawing;
        private string _svgFilePath;

        private DirectoryInfo _workingDir;

        private EmbeddedImageSerializerVisitor _embeddedImageVisitor;
        private IList<EmbeddedImageSerializerArgs> _embeddedImages;

        private WpfDrawingDocument _drawingDocument;

        private SharpVectors.Runtime.DpiScale _dpiScale;

        private bool _prevZoomRectSet;
        private bool _nextZoomRectSet;

        private Rect _prevZoomRect;
        private double _prevZoomScale;

        private MouseButton _mouseButtonDown;
        private Point _origZoomAndPanControlMouseDownPoint;
        private Point _origContentMouseDownPoint;

        private ZoomPanMouseHandlingMode _mouseHandlingMode;

        private Cursor _panToolCursor;
        private Cursor _panToolDownCursor;

        private DispatcherTimer _dispatcherTimer;

        #endregion

        #region Property
        public WpfDrawingSettings ConversionSettings
        {
            get
            {
                return _wpfSettings;
            }
            set
            {
                if (value != null)
                {
                    _wpfSettings = value;

                    // Recreated the conveter
                    _fileReader = new FileSvgReader(_wpfSettings);
                    _fileReader.SaveXaml = true;
                    _fileReader.SaveZaml = false;

                    if (!string.IsNullOrWhiteSpace(_drawingDir) &&
                        Directory.Exists(_drawingDir))
                    {
                        _fileReader.SaveXaml = Directory.Exists(_drawingDir);
                    }
                }
            }
        }

        public OptionSettings OptionSettings
        {
            get
            {
                return _optionSettings;
            }
            set
            {
                if (value != null)
                {
                    _optionSettings = value;
                    this.ConversionSettings = value.ConversionSettings;
                }
            }
        }

        public MainWindow5 MainWindow
        {
            get
            {
                return _mainWindow;
            }
            set
            {
                _mainWindow = value;

                if (_optionSettings == null)
                {
                    this.OptionSettings = _mainWindow.OptionSettings;
                }
            }
        }

        public string WorkingDrawingDir
        {
            get
            {
                return _drawingDir;
            }
            set
            {
                _drawingDir = value;

                if (!string.IsNullOrWhiteSpace(_drawingDir))
                {
                    _directoryInfo = new DirectoryInfo(_drawingDir);

                    if (_fileReader != null)
                    {
                        _fileReader.SaveXaml = Directory.Exists(_drawingDir);
                    }
                }
            }
        }

        public bool SaveXaml
        {
            get
            {
                return _saveXaml;
            }
            set
            {
                _saveXaml = value;
                if (_fileReader != null)
                {
                    _fileReader.SaveXaml = _saveXaml;
                    _fileReader.SaveZaml = false;
                }
            }
        }

        #endregion


        public DrawingPage1()
        {
            InitializeComponent();

            _saveXaml = true;
            _wpfSettings = new WpfDrawingSettings();
            _wpfSettings.CultureInfo = _wpfSettings.NeutralCultureInfo;

            _fileReader = new FileSvgReader(_wpfSettings);
            _fileReader.SaveXaml = _saveXaml;
            _fileReader.SaveZaml = false;

            _embeddedImageVisitor = new EmbeddedImageSerializerVisitor(true);
            _wpfSettings.Visitors.ImageVisitor = _embeddedImageVisitor;

            _embeddedImageVisitor.ImageCreated += OnEmbeddedImageCreated;

            this.Loaded += OnPageLoaded;
            //this.Unloaded += OnPageUnloaded;
            //this.SizeChanged += OnPageSizeChanged;

        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_svgFilePath) || !File.Exists(_svgFilePath))
            {
                zoomPanControl.ContentScale = 1.0;

                if (zoomPanControl != null)
                {
                    zoomPanControl.IsMouseWheelScrollingEnabled = true;
                }

                if (string.IsNullOrWhiteSpace(_svgFilePath))
                {
                    this.UnloadDocument();
                }
            }

            try
            {
                if (_panToolCursor == null)
                {
                    var panToolStream = Application.GetResourceStream(new Uri("Resources/PanTool.cur", UriKind.Relative));
                    using (panToolStream.Stream)
                    {
                        _panToolCursor = new Cursor(panToolStream.Stream);
                    }
                }
                if (_panToolDownCursor == null)
                {
                    var panToolDownStream = Application.GetResourceStream(new Uri("Resources/PanToolDown.cur", UriKind.Relative));
                    using (panToolDownStream.Stream)
                    {
                        _panToolDownCursor = new Cursor(panToolDownStream.Stream);
                    }
                }

                //  DispatcherTimer setup
                if (_dispatcherTimer == null)
                {
                    _dispatcherTimer = new DispatcherTimer();
                    _dispatcherTimer.Tick += OnUpdateUITick;
                    _dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }

            if (zoomPanControl != null && zoomPanControl.ScrollOwner == null)
            {
                if (canvasScroller != null)
                {
                    zoomPanControl.ScrollOwner = canvasScroller;
                }
            }

            if (_dispatcherTimer != null)
            {
                _dispatcherTimer.Start();
            }
        }

        private void OnUpdateUITick(object sender, EventArgs e)
        {
            // Forcing the CommandManager to raise the RequerySuggested event
            CommandManager.InvalidateRequerySuggested();
        }

        public Task<bool> LoadDocumentAsync(string svgFilePath)
        {
            if (_isLoadingDrawing || string.IsNullOrWhiteSpace(svgFilePath) || !File.Exists(svgFilePath))
            {
#if DOTNET40
                return TaskEx.FromResult<bool>(false);
#else
                return Task.FromResult<bool>(false);
#endif
            }

            string fileExt = System.IO.Path.GetExtension(svgFilePath);

            if (!(string.Equals(fileExt, SvgConverter.SvgExt, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileExt, SvgConverter.CompressedSvgExt, StringComparison.OrdinalIgnoreCase)))
            {
                _svgFilePath = null;
#if DOTNET40
                return TaskEx.FromResult<bool>(false);
#else
                return Task.FromResult<bool>(false);
#endif
            }

            _isLoadingDrawing = true;

            this.UnloadDocument(true);

            DirectoryInfo workingDir = _workingDir;
            if (_directoryInfo != null)
            {
                workingDir = _directoryInfo;
            }

            _svgFilePath = svgFilePath;
            _saveXaml = _optionSettings.ShowOutputFile;

            _embeddedImageVisitor.SaveImages = !_wpfSettings.IncludeRuntime;
            _embeddedImageVisitor.SaveDirectory = _drawingDir;
            _wpfSettings.Visitors.ImageVisitor = _embeddedImageVisitor;

            if (_fileReader == null)
            {
                _fileReader = new FileSvgReader(_wpfSettings);
                _fileReader.SaveXaml = _saveXaml;
                _fileReader.SaveZaml = false;
            }

            var drawingStream = new MemoryStream();

            // Get the UI thread's context
            var context = TaskScheduler.FromCurrentSynchronizationContext();

            return Task<bool>.Factory.StartNew(() =>
            {
                //                var saveXaml = _fileReader.SaveXaml;
                //                _fileReader.SaveXaml = true; // For threaded, we will save to avoid loading issue later...

                //Stopwatch stopwatch = new Stopwatch();

                //stopwatch.Start();

                //DrawingGroup drawing = _fileReader.Read(svgFilePath, workingDir);

                //stopwatch.Stop();

                //Trace.WriteLine(string.Format("FileName={0}, Time={1}", 
                //    Path.GetFileName(svgFilePath), stopwatch.ElapsedMilliseconds));

                Stopwatch watch = new Stopwatch();
                watch.Start();

                DrawingGroup drawing = _fileReader.Read(svgFilePath, workingDir);

                watch.Stop();

                Debug.WriteLine("{0}: {1}", System.IO.Path.GetFileName(svgFilePath), watch.ElapsedMilliseconds);

                //                _fileReader.SaveXaml = saveXaml;
                _drawingDocument = _fileReader.DrawingDocument;
                if (drawing != null)
                {
                    XamlWriter.Save(drawing, drawingStream);
                    drawingStream.Seek(0, SeekOrigin.Begin);

                    return true;
                }
                _svgFilePath = null;
                return false;
            }).ContinueWith((t) =>
            {
                try
                {
                    if (!t.Result)
                    {
                        _isLoadingDrawing = false;
                        _svgFilePath = null;
                        return false;
                    }
                    if (drawingStream.Length != 0)
                    {
                        DrawingGroup drawing = (DrawingGroup)XamlReader.Load(drawingStream);

                        svgViewer.UnloadDiagrams();
                        svgViewer.RenderDiagrams(drawing);

                        Rect bounds = svgViewer.Bounds;

                        if (bounds.IsEmpty)
                        {
                            bounds = new Rect(0, 0, svgViewer.ActualWidth, svgViewer.ActualHeight);
                        }

                        zoomPanControl.AnimatedZoomTo(bounds);
                        CommandManager.InvalidateRequerySuggested();

                        // The drawing changed, update the source...
                        _fileReader.Drawing = drawing;
                    }

                    _isLoadingDrawing = false;

                    return true;
                }
                catch
                {
                    _isLoadingDrawing = false;
                    throw;
                }
            }, context);

        }

        public void UnloadDocument(bool displayMessage = false)
        {

            try
            {
                _svgFilePath = null;
                _drawingDocument = null;

                if (svgViewer != null)
                {
                    svgViewer.UnloadDiagrams();

                    if (displayMessage)
                    {
                        var drawing = this.DrawText("Loading...");

                        svgViewer.RenderDiagrams(drawing);

                        Rect bounds = svgViewer.Bounds;
                        if (bounds.IsEmpty)
                        {
                            bounds = drawing.Bounds;
                        }

                        zoomPanControl.ZoomTo(bounds);
                        return;
                    }
                }

                var drawRect = this.DrawRect();
                svgViewer.RenderDiagrams(drawRect);

                zoomPanControl.ZoomTo(drawRect.Bounds);
                ClearPrevZoomRect();
                ClearNextZoomRect();
            }
            finally
            {
                if (_embeddedImages != null && _embeddedImages.Count != 0)
                {
                    foreach (var embeddedImage in _embeddedImages)
                    {
                        try
                        {
                            if (embeddedImage.Image != null)
                            {
                                if (embeddedImage.Image.StreamSource != null)
                                {
                                    embeddedImage.Image.StreamSource.Dispose();
                                }
                            }

                            var imagePath = embeddedImage.ImagePath;
                            if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                            {
                                File.Delete(imagePath);
                            }
                        }
                        catch (IOException ex)
                        {
                            Trace.TraceError(ex.ToString());
                            // Image this, WPF will typically cache and/or lock loaded images
                        }
                    }

                    _embeddedImages.Clear();
                }
            }


        }

        private void OnZoomPanMouseDown(object sender, MouseButtonEventArgs e)
        {
            zoomPanControl.Focus();
            Keyboard.Focus(zoomPanControl);

            _mouseButtonDown = e.ChangedButton;
            _origZoomAndPanControlMouseDownPoint = e.GetPosition(zoomPanControl);
            _origContentMouseDownPoint = e.GetPosition(svgViewer);

            if (_mouseHandlingMode == ZoomPanMouseHandlingMode.SelectPoint ||
                _mouseHandlingMode == ZoomPanMouseHandlingMode.SelectRectangle)
            {
            }
            else
            {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0 &&
                    (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right))
                {
                    // Shift + left- or right-down initiates zooming mode.
                    _mouseHandlingMode = ZoomPanMouseHandlingMode.Zooming;

                    //if (zoomPanControl != null && _canvasCursor != null)
                    //{
                    //    zoomPanControl.Cursor = _canvasCursor;
                    //}
                }
                else if (_mouseButtonDown == MouseButton.Left)
                {
                    // Just a plain old left-down initiates panning mode.
                    _mouseHandlingMode = ZoomPanMouseHandlingMode.Panning;
                }

                if (_mouseHandlingMode != ZoomPanMouseHandlingMode.None)
                {
                    // Capture the mouse so that we eventually receive the mouse up event.
                    zoomPanControl.CaptureMouse();
                    e.Handled = true;
                }
            }

        }

        private void OnZoomPanMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_mouseHandlingMode == ZoomPanMouseHandlingMode.SelectPoint ||
                _mouseHandlingMode == ZoomPanMouseHandlingMode.SelectRectangle)
            {
            }
            else
            {
                if (_mouseHandlingMode != ZoomPanMouseHandlingMode.None)
                {
                    if (_mouseHandlingMode == ZoomPanMouseHandlingMode.Zooming)
                    {
                        if (_mouseButtonDown == MouseButton.Left)
                        {
                            // Shift + left-click zooms in on the content.
                            ZoomIn(_origContentMouseDownPoint);
                        }
                        else if (_mouseButtonDown == MouseButton.Right)
                        {
                            // Shift + left-click zooms out from the content.
                            ZoomOut(_origContentMouseDownPoint);
                        }
                    }
                    else if (_mouseHandlingMode == ZoomPanMouseHandlingMode.DragZooming)
                    {
                        // When drag-zooming has finished we zoom in on the rectangle that was highlighted by the user.
                        ApplyDragZoomRect();
                    }

                    zoomPanControl.ReleaseMouseCapture();
                    _mouseHandlingMode = ZoomPanMouseHandlingMode.None;
                    e.Handled = true;
                }

                //if (zoomPanControl != null && _canvasCursor != null)
                //{
                //    zoomPanControl.Cursor = _canvasCursor;
                //}
            }

        }

        private void ApplyDragZoomRect()
        {
            //
            // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
            //
            SavePrevZoomRect();

            //
            // Retreive the rectangle that the user draggged out and zoom in on it.
            //
            double contentX = Canvas.GetLeft(dragZoomBorder);
            double contentY = Canvas.GetTop(dragZoomBorder);
            double contentWidth = dragZoomBorder.Width;
            double contentHeight = dragZoomBorder.Height;
            zoomPanControl.AnimatedZoomTo(new Rect(contentX, contentY, contentWidth, contentHeight));

            FadeOutDragZoomRect();

            ClearNextZoomRect();
        }
        private void FadeOutDragZoomRect()
        {
            ZoomPanAnimationHelper.StartAnimation(dragZoomBorder, OpacityProperty, 0.0, ZoomChange,
                delegate (object sender, EventArgs e)
                {
                    dragZoomCanvas.Visibility = Visibility.Collapsed;
                });
        }


        private void OnZoomPanMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void OnZoomPanMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseHandlingMode == ZoomPanMouseHandlingMode.SelectPoint ||
                _mouseHandlingMode == ZoomPanMouseHandlingMode.SelectRectangle)
            {
            }
            else
            {
                if (_mouseHandlingMode == ZoomPanMouseHandlingMode.Panning)
                {
                    if (zoomPanControl != null)
                    {
                        zoomPanControl.Cursor = _panToolCursor;
                    }

                    //
                    // The user is left-dragging the mouse.
                    // Pan the viewport by the appropriate amount.
                    //
                    Point curContentMousePoint = e.GetPosition(svgViewer);
                    Vector dragOffset = curContentMousePoint - _origContentMouseDownPoint;

                    zoomPanControl.ContentOffsetX -= dragOffset.X;
                    zoomPanControl.ContentOffsetY -= dragOffset.Y;

                    e.Handled = true;
                }
                else if (_mouseHandlingMode == ZoomPanMouseHandlingMode.Zooming)
                {
                    //if (zoomPanControl != null && _canvasCursor != null)
                    //{
                    //    zoomPanControl.Cursor = _canvasCursor;
                    //}

                    Point curZoomAndPanControlMousePoint = e.GetPosition(zoomPanControl);
                    Vector dragOffset = curZoomAndPanControlMousePoint - _origZoomAndPanControlMouseDownPoint;
                    double dragThreshold = 10;
                    if (_mouseButtonDown == MouseButton.Left &&
                        (Math.Abs(dragOffset.X) > dragThreshold ||
                         Math.Abs(dragOffset.Y) > dragThreshold))
                    {
                        //
                        // When Shift + left-down zooming mode and the user drags beyond the drag threshold,
                        // initiate drag zooming mode where the user can drag out a rectangle to select the area
                        // to zoom in on.
                        //
                        _mouseHandlingMode = ZoomPanMouseHandlingMode.DragZooming;

                        Point curContentMousePoint = e.GetPosition(svgViewer);
                        InitDragZoomRect(_origContentMouseDownPoint, curContentMousePoint);
                    }

                    e.Handled = true;
                }
                else if (_mouseHandlingMode == ZoomPanMouseHandlingMode.DragZooming)
                {
                    //if (zoomPanControl != null && _canvasCursor != null)
                    //{
                    //    zoomPanControl.Cursor = _canvasCursor;
                    //}

                    //
                    // When in drag zooming mode continously update the position of the rectangle
                    // that the user is dragging out.
                    //
                    Point curContentMousePoint = e.GetPosition(svgViewer);
                    SetDragZoomRect(_origContentMouseDownPoint, curContentMousePoint);

                    e.Handled = true;
                }
            }

        }

        private void OnZoomPanMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            Point curContentMousePoint = e.GetPosition(svgViewer);

            this.Zoom(curContentMousePoint, e.Delta);

            if (svgViewer.IsKeyboardFocusWithin)
            {
                Keyboard.Focus(zoomPanControl);
            }

        }

        private async void OnOpenFileClick(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                await _mainWindow.BrowseForFile();
            }
        }

        private void OnOpenFolderClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnEmbeddedImageCreated(object sender, EmbeddedImageSerializerArgs args)
        {
            if (args == null)
            {
                return;
            }
            if (_embeddedImages == null)
            {
                _embeddedImages = new List<EmbeddedImageSerializerArgs>();
            }
            _embeddedImages.Add(args);
        }

        private DrawingGroup DrawText(string textString)
        {
            // Create a new DrawingGroup of the control.
            DrawingGroup drawingGroup = new DrawingGroup();

            drawingGroup.Opacity = 0.8;

            if (_dpiScale == null)
            {
                _dpiScale = DpiUtilities.GetWindowScale(this);
            }

            // Open the DrawingGroup in order to access the DrawingContext.
            using (DrawingContext drawingContext = drawingGroup.Open())
            {
                // Create the formatted text based on the properties set.
                FormattedText formattedText = null;
#if DOTNET40 || DOTNET45 || DOTNET46
                formattedText = new FormattedText(textString, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Tahoma"), FontStyles.Normal, 
                    FontWeights.Normal, FontStretches.Normal), 72, Brushes.Black);
#else
                formattedText = new FormattedText(textString, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
                    new Typeface(new FontFamily("Tahoma"), FontStyles.Normal,
                    FontWeights.Normal, FontStretches.Normal), 72, Brushes.Black, _dpiScale.PixelsPerDip);
#endif

                // Build the geometry object that represents the text.
                Geometry textGeometry = formattedText.BuildGeometry(new Point(20, 0));

                drawingContext.DrawRoundedRectangle(Brushes.Transparent, null,
                    new Rect(new Size(formattedText.Width + 50, formattedText.Height + 5)), 5.0, 5.0);

                // Draw the outline based on the properties that are set.
                drawingContext.DrawGeometry(null, new Pen(Brushes.DarkGray, 1.5), textGeometry);
            }

            // Return the updated DrawingGroup content to be used by the control.
            return drawingGroup;
        }

        private DrawingGroup DrawRect()
        {
            // Create a new DrawingGroup of the control.
            DrawingGroup drawingGroup = new DrawingGroup();

            // Open the DrawingGroup in order to access the DrawingContext.
            using (DrawingContext drawingContext = drawingGroup.Open())
            {
                drawingContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, 280, 300));
            }
            // Return the updated DrawingGroup content to be used by the control.
            return drawingGroup;
        }


        /// <summary>
        /// Clear the memory of the previous zoom level.
        /// </summary>
        private void ClearPrevZoomRect()
        {
            _prevZoomRectSet = false;
        }

        /// <summary>
        /// Clear the memory of the next zoom level.
        /// </summary>
        private void ClearNextZoomRect()
        {
            _nextZoomRectSet = false;
        }

        private void Zoom(Point contentZoomCenter, int wheelMouseDelta)
        {
            SavePrevZoomRect();

            // Found the division by 3 gives a little smoothing effect
            var zoomFactor = zoomPanControl.ContentScale + ZoomChange * wheelMouseDelta / (120 * 3);

            zoomPanControl.ZoomAboutPoint(zoomFactor, contentZoomCenter);

            ClearNextZoomRect();
        }

        //
        // Record the previous zoom level, so that we can jump back to it when the backspace key is pressed.
        //
        private void SavePrevZoomRect()
        {
            _prevZoomRect = new Rect(zoomPanControl.ContentOffsetX, zoomPanControl.ContentOffsetY,
                zoomPanControl.ContentViewportWidth, zoomPanControl.ContentViewportHeight);
            _prevZoomScale = zoomPanControl.ContentScale;
            _prevZoomRectSet = true;
        }

        private void ZoomIn(Point contentZoomCenter)
        {
            SavePrevZoomRect();

            zoomPanControl.ZoomAboutPoint(zoomPanControl.ContentScale + ZoomChange, contentZoomCenter);

            ClearNextZoomRect();

        }

        private void ZoomOut(Point contentZoomCenter)
        {
            SavePrevZoomRect();

            zoomPanControl.ZoomAboutPoint(zoomPanControl.ContentScale - ZoomChange, contentZoomCenter);

            ClearNextZoomRect();
        }


        private void InitDragZoomRect(Point pt1, Point pt2)
        {
            SetDragZoomRect(pt1, pt2);

            dragZoomCanvas.Visibility = Visibility.Visible;
            dragZoomBorder.Opacity = 0.5;
        }

        private void SetDragZoomRect(Point pt1, Point pt2)
        {
            double x, y, width, height;

            //
            // Deterine x,y,width and height of the rect inverting the points if necessary.
            // 

            if (pt2.X < pt1.X)
            {
                x = pt2.X;
                width = pt1.X - pt2.X;
            }
            else
            {
                x = pt1.X;
                width = pt2.X - pt1.X;
            }

            if (pt2.Y < pt1.Y)
            {
                y = pt2.Y;
                height = pt1.Y - pt2.Y;
            }
            else
            {
                y = pt1.Y;
                height = pt2.Y - pt1.Y;
            }

            //
            // Update the coordinates of the rectangle that is being dragged out by the user.
            // The we offset and rescale to convert from content coordinates.
            //
            Canvas.SetLeft(dragZoomBorder, x);
            Canvas.SetTop(dragZoomBorder, y);
            dragZoomBorder.Width = width;
            dragZoomBorder.Height = height;
        }

    }
}
