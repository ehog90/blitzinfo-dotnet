using System;
using System.Windows.Input;

namespace BlitzInfo.ViewModel
{
    public class DelegateCommand : ICommand
    {
        private readonly Func<object, bool> _canExecute; // a tevékenység feltételét ellenőző lambda-kifejezés
        private readonly Action<object> _execute; // a tevékenységet végrehajtó lambda-kifejezés

        /// <summary>
        ///     Parancs létrehozása.
        /// </summary>
        /// <param name="execute">Végrehajtandó tevékenység.</param>
        public DelegateCommand(Action<object> execute) : this(null, execute)
        {
        }

        /// <summary>
        ///     Parancs létrehozása.
        /// </summary>
        /// <param name="canExecute">Végrehajthatóság feltétele.</param>
        /// <param name="execute">Végrehajtandó tevékenység.</param>
        public DelegateCommand(Func<object, bool> canExecute, Action<object> execute)
        {
            if (execute == null) throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        ///     Végrehajthatóság változásának eseménye.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        ///     Végrehajthatóság ellenőrzése
        /// </summary>
        /// <param name="parameter">A tevékenység paramétere.</param>
        /// <returns>Igaz, ha a tevékenység végrehajtható.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        /// <summary>
        ///     Tevékenység végrehajtása.
        /// </summary>
        /// <param name="parameter">A tevékenység paramétere.</param>
        public void Execute(object parameter)
        {
            if (!CanExecute(parameter)) throw new InvalidOperationException("Command execution is disabled.");
            _execute(parameter);
        }

        /// <summary>
        ///     Végrehajthatóság változásának eseménykiváltása.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
}