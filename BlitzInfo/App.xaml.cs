using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BlitzInfo
{

    public partial class App : Application
    {
        private Model.BlitzModel _blitzmodel;
        private ViewModel.BlitzViewModel _blitzviewmodel;
        private MainWindow _view;

        public App()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _blitzmodel = new Model.BlitzModel();
            _blitzmodel.onAdressNotExisting += new EventHandler<Model.NoSettlemEventArgs>(AddressNotExists);
            _blitzviewmodel = new ViewModel.BlitzViewModel(_blitzmodel);
            _blitzviewmodel.onAddressInputError += new EventHandler(OnAddressError);

            _view = new MainWindow();
            _view.DataContext = _blitzviewmodel;
            _view.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _blitzmodel.Disconnect();
        }

        private void AddressNotExists(object sender, Model.NoSettlemEventArgs e)
        {
            MessageBox.Show("A megadott település ("+e.ProbedAddress+") nem létezik!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnAddressError(object sender, EventArgs e)
        {
            MessageBox.Show("A településnév megadása kötelező, és minimum 2 karakter.", "Hiba", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }

}
