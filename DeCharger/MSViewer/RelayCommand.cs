using System;
using System.Windows.Input;

// Based on: http://stackoverflow.com/questions/783104/refresh-wpf-command

public interface IRelayCommand : ICommand
{
    void UpdateCanExecuteState();
}

public class RelayCommand : IRelayCommand
{
    public event EventHandler CanExecuteChanged;


    readonly Predicate<Object> _canExecute = null;
    readonly Action<Object> _executeAction = null;

    public RelayCommand(Action<object> executeAction, Predicate<Object> canExecute = null)
    {
        _canExecute = canExecute;
        _executeAction = executeAction;
    }


    public bool CanExecute(object parameter)
    {
        if (_canExecute != null)
            return _canExecute(parameter);
        return true;
    }

    public void UpdateCanExecuteState()
    {
        if (CanExecuteChanged != null)
            CanExecuteChanged(this, new EventArgs());
    }



    public void Execute(object parameter)
    {
        if (_executeAction != null)
            _executeAction(parameter);
        UpdateCanExecuteState();
    }
}



// Based on: http://www.dotnetcurry.com/wpf/1130/wpf-commanding-enable-button




/// <summary>
/// The Command class
/// </summary>
public class RelayActionCommand : ICommand
    {
        /// <summary>
        /// The Action Delegate representing a method with input parameter 
        /// </summary>
        public Action<object> ExecuteAction { get; set; }
        /// <summary>
        /// The Delegate, used to represent the method which defines criteria for the execution 
        /// </summary>
        public Predicate<object> CanExecuteAction { get; set; }

        public bool CanExecute(object parameter)
        {
            if (CanExecuteAction != null)
            {
                return CanExecuteAction(parameter);
            }
            return true;
        }

        /// <summary>
        /// The event which will be raised based upon the
        /// value set for the command.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            if (ExecuteAction != null)
            {
                ExecuteAction(parameter);
            }
        }
    }

