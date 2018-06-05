using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;




/// <summary>
    /// The Command class
/// Based on http://www.dotnetcurry.com/wpf/1130/wpf-commanding-enable-button
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

    public bool CanExecute(object parameter = null)
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

    public void Execute(object parameter = null)
    {
        if (ExecuteAction != null)
        {
            ExecuteAction(parameter);
        }
    }
}



