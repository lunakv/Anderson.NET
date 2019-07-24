using Anderson.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Anderson.ViewModels
{
    class ApplicationViewModel : ViewModelBase
    {
        List<ViewModelBase> _pageViewModels = new List<ViewModelBase>();

        public ApplicationViewModel()
        {
            var factory = new ModelFactory();
            Action connect = factory.EstablishConnection;
            var wait = connect.BeginInvoke(null, null);

            connect.EndInvoke(wait);
            var loginM = factory.GetLoginModel();
            _pageViewModels.Add(new StartViewModel { LoginBack = loginM });
            _pageViewModels.Add(new LoginViewModel(loginM));
            _pageViewModels.Add(new UserViewModel { UserBack = factory.GetUserModel() });

            foreach(var vm in _pageViewModels)
            {
                vm.ViewChanged += ChangeViewModel;
            }

            CurrentPageViewModel = _pageViewModels[0];
        }

        public override string Name => "Application";

        private ViewModelBase _currentPageViewModel;
        public ViewModelBase CurrentPageViewModel
        {
            get { return _currentPageViewModel; }
            set
            {
                if (_currentPageViewModel != value)
                {
                    _currentPageViewModel = value;
                    OnPropertyChanged("CurrentPageViewModel");
                }

            }
        }

        private void ChangeViewModel(string vmName)
        {
            ViewModelBase newVM = _pageViewModels.FirstOrDefault(vm => vm.Name == vmName);
            CurrentPageViewModel = newVM ?? throw new NotImplementedException($"No ViewModel with name {vmName} exists.");
        }
    }
}
