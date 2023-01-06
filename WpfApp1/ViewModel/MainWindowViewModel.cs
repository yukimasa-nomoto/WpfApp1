using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.Model;

namespace WpfApp1.ViewModel
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private MainWindowModel _model;
        private string _resultValue = string.Empty;
        private ConcatCommand _concatCommand;

        public MainWindowViewModel()
        {
            _model= new MainWindowModel();
            _concatCommand = new ConcatCommand(this);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Value1 {
            get { return _model.Value1; }
            set
            {
                if(_model.Value1 != value)
                {
                    _model.Value1 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Value2
        {
            get { return _model.Value2; }
            set
            {
                if (_model.Value2 != value)
                {
                    _model.Value2 = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ResultValue
        {
            get { return _resultValue; }
            set
            {
                if (_resultValue != value)
                {
                    _resultValue = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand ConcatCommand => _concatCommand;

    }
}
