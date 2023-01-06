using Microsoft.Win32;
using SharpVectors.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;

using IoPath = System.IO.Path;


namespace WpfApp1
{
    /// <summary>
    /// MainWindow5.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow5 : Window
    {
        public const string AppTitle = "SharpVectors: WPF SVG Application";

        public const string SvgTestSettings = "SvgTestSettings.xml";

        #region 変数
        private DrawingPage1 _drawingPage;
        private string _drawingDir;
        private OptionSettings _optionSettings;
        private string _testSettingsPath;

        private string _svgFilePath;
        //private SvgPage _svgPage;
        //private XamlPage _xamlPage;

        #endregion

        #region Property
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
                    if (_drawingPage != null)
                    {
                        _drawingPage.ConversionSettings = value.ConversionSettings;
                    }

                    //if (_optionSettings.IsCurrentSvgPathChanged(_sourceDir))
                    //{
                    //    //this.FillTreeView(_optionSettings.CurrentSvgPath);
                    //}
                    //else if (_isRecursiveSearch != _optionSettings.RecursiveSearch)
                    //{
                    //    //this.FillTreeView(_optionSettings.CurrentSvgPath);
                    //}
                }
            }
        }

        #endregion

        public MainWindow5()
        {
            InitializeComponent();
            _drawingDir = IoPath.GetFullPath(IoPath.Combine("..\\", DrawingPage1.TemporalDirName));
            if (!Directory.Exists(_drawingDir))
            {
                Directory.CreateDirectory(_drawingDir);
            }
            _optionSettings = new OptionSettings();
            _testSettingsPath = IoPath.GetFullPath(IoPath.Combine("..\\", SvgTestSettings));
            if (!string.IsNullOrWhiteSpace(_testSettingsPath) && File.Exists(_testSettingsPath))
            {
                _optionSettings.Load(_testSettingsPath);
                // Override any saved local directory, default to sample files.
                _optionSettings.CurrentSvgPath = _optionSettings.DefaultSvgPath;
            }

            _optionSettings.PropertyChanged += OnSettingsPropertyChanged;

        }

        #region public

        public async Task BrowseForFile()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Title = "Select An SVG File";
            dlg.DefaultExt = "*.svg";
            dlg.Filter = "All SVG Files (*.svg,*.svgz)|*.svg;*.svgz"
                                + "|Svg Uncompressed Files (*.svg)|*.svg"
                                + "|SVG Compressed Files (*.svgz)|*.svgz";

            bool? isSelected = dlg.ShowDialog();

            if (isSelected != null && isSelected.Value)
            {
                await this.LoadFile(dlg.FileName);

                //TreeViewItem selItem = treeView.SelectedItem as TreeViewItem;
                //if (selItem == null || selItem.Tag == null)
                //{
                //    return;
                //}
                //selItem.IsSelected = false;
            }

        }

        private async Task LoadFile(string fileName)
        {
            string fileExt = IoPath.GetExtension(fileName);
            if (string.IsNullOrWhiteSpace(fileExt))
            {
                return;
            }

            bool generateXaml = _optionSettings.ShowOutputFile;

            if (string.Equals(fileExt, SvgConverter.SvgExt, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileExt, SvgConverter.CompressedSvgExt, StringComparison.OrdinalIgnoreCase))
            {
                _svgFilePath = fileName;

                /*
                if (_svgPage != null && _optionSettings.ShowInputFile)
                {
                    _svgPage.LoadDocument(fileName);
                }
                */

                if (_drawingPage == null)
                {
                    return;
                }
                _drawingPage.SaveXaml = generateXaml;

                try
                {
                    if (await _drawingPage.LoadDocumentAsync(fileName))
                    {
                        this.Title = AppTitle + " - " + IoPath.GetFileName(fileName);

                        /*
                        if (_xamlPage != null && !string.IsNullOrWhiteSpace(_drawingDir))
                        {
                            string xamlFilePath = IoPath.Combine(_drawingDir,
                                IoPath.GetFileNameWithoutExtension(fileName) + SvgConverter.XamlExt);

                            _xamlFilePath = xamlFilePath;
                            _canDeleteXaml = true;

                            if (File.Exists(xamlFilePath) && _optionSettings.ShowOutputFile)
                            {
                                _xamlPage.LoadDocument(xamlFilePath);
                            }
                        }
                        

                        _fileWatcher.Path = IoPath.GetDirectoryName(fileName);
                        // Only watch current file
                        _fileWatcher.Filter = IoPath.GetFileName(fileName);
                        // Begin watching.
                        _fileWatcher.EnableRaisingEvents = true;

                        */
                    }
                }
                catch
                {
                    /*
                    // Try loading the XAML, if generated but the rendering failed...
                    if (_xamlPage != null && !string.IsNullOrWhiteSpace(_drawingDir))
                    {
                        string xamlFilePath = IoPath.Combine(_drawingDir,
                            IoPath.GetFileNameWithoutExtension(fileName) + SvgConverter.XamlExt);

                        _xamlFilePath = xamlFilePath;
                        _canDeleteXaml = true;

                        if (File.Exists(xamlFilePath) && _optionSettings.ShowOutputFile)
                        {
                            _xamlPage.LoadDocument(xamlFilePath);
                        }
                    }
                    */
                    throw;
                }
            }

        }
        #endregion

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _drawingPage = frameDrawing.Content as DrawingPage1;

            if (_drawingPage != null)
            {
                _drawingPage.WorkingDrawingDir = _drawingDir;
                _drawingPage.MainWindow = this;
            }

        }

        private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!this.IsLoaded)
            {
                return;
            }

            var changedProp = e.PropertyName;
            if (string.IsNullOrWhiteSpace(changedProp))
            {
                return;
            }

            if (string.Equals(changedProp, "ShowInputFile", StringComparison.OrdinalIgnoreCase))
            {
                this.OnFillSvgInputChecked();
            }
            else if (string.Equals(changedProp, "ShowOutputFile", StringComparison.OrdinalIgnoreCase))
            {
                this.OnFillXamlOutputChecked();
            }
        }

        private void OnFillSvgInputChecked()
        {
            /*
            if (_svgPage == null)
            {
                tabSvgInput.Visibility = _optionSettings.ShowInputFile ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            Cursor saveCursor = this.Cursor;

            try
            {
                if (_optionSettings.ShowInputFile)
                {
                    this.Cursor = Cursors.Wait;
                    this.ForceCursor = true;

                    if (File.Exists(_svgFilePath))
                    {
                        _svgPage.LoadDocument(_svgFilePath);
                    }
                    else
                    {
                        _svgPage.UnloadDocument();
                    }
                }
                else
                {
                    _svgPage.UnloadDocument();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), AppErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = saveCursor;
                this.ForceCursor = false;

                tabSvgInput.Visibility = _optionSettings.ShowInputFile ? Visibility.Visible : Visibility.Collapsed;
            }

            */
        }

        private void OnFillXamlOutputChecked()
        {
            /*
            if (_xamlPage == null || string.IsNullOrWhiteSpace(_xamlFilePath))
            {
                tabXamlOutput.Visibility = _optionSettings.ShowOutputFile ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            Cursor saveCursor = this.Cursor;

            try
            {
                if (_optionSettings.ShowOutputFile)
                {
                    this.Cursor = Cursors.Wait;
                    this.ForceCursor = true;

                    if (!File.Exists(_xamlFilePath))
                    {
                        if (!_drawingPage.SaveDocument(_xamlFilePath))
                        {
                            return;
                        }
                    }

                    if (File.Exists(_xamlFilePath))
                    {
                        _xamlPage.LoadDocument(_xamlFilePath);
                    }
                    else
                    {
                        _xamlPage.UnloadDocument();
                    }
                }
                else
                {
                    _xamlPage.UnloadDocument();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), AppErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = saveCursor;
                this.ForceCursor = false;

                tabXamlOutput.Visibility = _optionSettings.ShowOutputFile ? Visibility.Visible : Visibility.Collapsed;
            }

            */
        }

    }
}
