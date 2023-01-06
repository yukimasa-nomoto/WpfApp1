using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfApp1.ViewModel;

namespace WpfApp1
{
    public class ConcatCommand : ICommand
    {
        private MainWindowViewModel _vm;

        public ConcatCommand(MainWindowViewModel vm)
        {
            _vm = vm;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return
            !string.IsNullOrEmpty(_vm.Value1) && !string.IsNullOrEmpty(_vm.Value2);
        }

        public void Execute(object? parameter)
        {
            _vm.ResultValue = _vm.Value1 + _vm.Value2;
        }
    }
}
