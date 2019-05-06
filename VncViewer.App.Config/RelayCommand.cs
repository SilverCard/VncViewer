using System;
using System.Windows.Input;

namespace VncViewer.App.Config
{
    public class RelayCommand : ICommand
    {
        private Func<bool> canExecute;
        private Action execute;

        public RelayCommand(Func<bool> canExecute, Action execute)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => canExecute();  
        public void Execute(object parameter) => execute();
       
    }
}
