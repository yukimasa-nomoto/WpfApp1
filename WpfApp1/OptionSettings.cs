using SharpVectors.Renderers.Wpf;
using SharpVectors.Renderers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace WpfApp1
{
    public sealed class OptionSettings : INotifyPropertyChanged , ICloneable
    {
        private WpfDrawingSettings _wpfSettings;
        private string _defaultSvgPath;
        private bool _showOutputFile;
        private bool _hidePathsRoot;
        private string _currentSvgPath;
        private bool _showInputFile;
        private bool _recursiveSearch;
        private string _selectedValuePath;
        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private const string ParentSymbol = "..\\";
        private const string SharpVectors = "SharpVectors";

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
                    bool isChanged = (_wpfSettings != value);

                    _wpfSettings = value;

                    if (isChanged)
                    {
                        this.RaisePropertyChanged("ConversionSettings");
                    }
                }
            }
        }

        public string DefaultSvgPath
        {
            get
            {
                return _defaultSvgPath;
            }
            set
            {
                bool isChanged = !string.Equals(_defaultSvgPath, value, StringComparison.OrdinalIgnoreCase);
                _defaultSvgPath = value;

                if (isChanged)
                {
                    this.RaisePropertyChanged("DefaultSvgPath");
                }
            }
        }

        public string CurrentSvgPath
        {
            get
            {
                return _currentSvgPath;
            }
            set
            {
                bool isChanged = !string.Equals(_currentSvgPath, value, StringComparison.OrdinalIgnoreCase);
                _currentSvgPath = value;

                if (isChanged)
                {
                    this.RaisePropertyChanged("CurrentSvgPath");
                }
            }
        }

        public bool ShowOutputFile
        {
            get
            {
                return _showOutputFile;
            }
            set
            {
                bool isChanged = (_showOutputFile != value);
                _showOutputFile = value;

                if (isChanged)
                {
                    this.RaisePropertyChanged("ShowOutputFile");
                }
            }
        }

        public bool ShowInputFile
        {
            get
            {
                return _showInputFile;
            }
            set
            {
                bool isChanged = (_showInputFile != value);
                _showInputFile = value;

                if (isChanged)
                {
                    this.RaisePropertyChanged("ShowInputFile");
                }
            }
        }


        public OptionSettings()
        {
            _wpfSettings = new WpfDrawingSettings();
            string currentDir = Path.GetFullPath(Path.Combine("..\\", "Samples"));
            if (!Directory.Exists(currentDir))
            {
                Directory.CreateDirectory(currentDir);
            }
            _currentSvgPath = currentDir;
            _defaultSvgPath = currentDir;

            _showInputFile = false;
            _showOutputFile = false;
            _recursiveSearch = true;
        }

        public OptionSettings(OptionSettings source)
        {
            if (source == null)
            {
                return;
            }
            _hidePathsRoot = source._hidePathsRoot;
            _defaultSvgPath = source._defaultSvgPath;
            _currentSvgPath = source._currentSvgPath;
            _showInputFile = source._showInputFile;
            _showOutputFile = source._showOutputFile;
            _recursiveSearch = source._recursiveSearch;
            _wpfSettings = source._wpfSettings;
        }


        public void Load(string settingsPath)
        {
            if (string.IsNullOrWhiteSpace(settingsPath) ||
                File.Exists(settingsPath) == false)
            {
                return;
            }

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = false;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;

            using (XmlReader reader = XmlReader.Create(settingsPath, settings))
            {
                this.Load(reader);
            }
        }


        #region  ICloneable Members

        public OptionSettings Clone()
        {
            OptionSettings optSettings = new OptionSettings(this);

            if (_wpfSettings != null)
            {
                optSettings._wpfSettings = _wpfSettings.Clone();
            }
            //if (_defaultSvgPath != null)
            //{
            //    optSettings._defaultSvgPath = new string(_defaultSvgPath.ToCharArray());
            //}
            //if (_currentSvgPath != null)
            //{
            //    optSettings._currentSvgPath = new string(_currentSvgPath.ToCharArray());
            //}

            return optSettings;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion

        #region private

        private void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Load(XmlReader reader)
        {
            var comparer = StringComparison.OrdinalIgnoreCase;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element &&
                    string.Equals(reader.Name, "option", comparer))
                {
                    string optionName = reader.GetAttribute("name");
                    string optionType = reader.GetAttribute("type");
                    if (string.Equals(optionType, "String", comparer))
                    {
                        string optionValue = reader.ReadElementContentAsString();

                        switch (optionName)
                        {
                            case "DefaultSvgPath":
                                if (optionValue.StartsWith(ParentSymbol, comparer))
                                {
                                    var inputPath = new string(_defaultSvgPath.ToCharArray());
                                    int indexOf = inputPath.IndexOf(SharpVectors, comparer);

                                    if (indexOf > 0)
                                    {
                                        var basePath = inputPath.Substring(0, indexOf);
                                        _defaultSvgPath = Path.Combine(basePath, optionValue.Replace(ParentSymbol, string.Empty));
                                    }
                                    else
                                    {
                                        _defaultSvgPath = optionValue;
                                    }
                                }
                                else
                                {
                                    _defaultSvgPath = optionValue;
                                }
                                break;
                            case "CurrentSvgPath":
                                if (optionValue.StartsWith(ParentSymbol, comparer))
                                {
                                    var inputPath = new string(_currentSvgPath.ToCharArray());
                                    int indexOf = inputPath.IndexOf(SharpVectors, comparer);

                                    if (indexOf > 0)
                                    {
                                        var basePath = inputPath.Substring(0, indexOf);
                                        _currentSvgPath = Path.Combine(basePath, optionValue.Replace(ParentSymbol, string.Empty));
                                    }
                                    else
                                    {
                                        _currentSvgPath = optionValue;
                                    }
                                }
                                else
                                {
                                    _currentSvgPath = optionValue;
                                }
                                break;
                            case "SelectedValuePath":
                                _selectedValuePath = optionValue;
                                break;
                        }
                    }
                    else if (string.Equals(optionType, "Boolean", comparer))
                    {
                        bool optionValue = reader.ReadElementContentAsBoolean();
                        switch (optionName)
                        {
                            case "HidePathsRoot":
                                _hidePathsRoot = optionValue;
                                break;
                            case "ShowInputFile":
                                _showInputFile = optionValue;
                                break;
                            case "ShowOutputFile":
                                _showOutputFile = optionValue;
                                break;
                            case "RecursiveSearch":
                                _recursiveSearch = optionValue;
                                break;

                            case "TextAsGeometry":
                                _wpfSettings.TextAsGeometry = optionValue;
                                break;
                            case "IncludeRuntime":
                                _wpfSettings.IncludeRuntime = optionValue;
                                break;

                            case "IgnoreRootViewbox":
                                _wpfSettings.IgnoreRootViewbox = optionValue;
                                break;
                            case "EnsureViewboxSize":
                                _wpfSettings.EnsureViewboxSize = optionValue;
                                break;
                            case "EnsureViewboxPosition":
                                _wpfSettings.EnsureViewboxPosition = optionValue;
                                break;
                        }
                    }

                }
            }
        }

        #endregion

    }
}
