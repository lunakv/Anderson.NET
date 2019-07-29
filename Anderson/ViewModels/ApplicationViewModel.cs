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
    class ApplicationViewModel : ViewModelBase, IDisposable
    {
        List<ViewModelBase> _pageViewModels = new List<ViewModelBase>();

        public ApplicationViewModel()
        {
            
            Action<string> connect = ModelFactory.EstablishConnection;
            var wait = connect.BeginInvoke(Url, null, null);
            connect.EndInvoke(wait);
            LoginModel loginM = ModelFactory.GetLoginModel();
            PersonModel personM = ModelFactory.GetUserModel();
            RoomModel roomM = ModelFactory.GetRoomModel();
            _pageViewModels.Add(new StartViewModel(loginM));
            _pageViewModels.Add(new LoginViewModel(loginM));
            _pageViewModels.Add(new UserViewModel(loginM, personM, roomM));

            foreach(var vm in _pageViewModels)
            {
                vm.ViewChanged += ChangeViewModel;
            }

            CurrentPageViewModel = _pageViewModels[0];
            OnClose = new DelegateCommand(ModelFactory.DisposeApiClient);
        }

        public DelegateCommand OnClose { get; set; }

        public override string Name => "Application";

        private string Url => "https://lunakv.modular.im";

        private ViewModelBase _currentPageViewModel;
        public ViewModelBase CurrentPageViewModel
        {
            get { return _currentPageViewModel; }
            set
            {
                if (_currentPageViewModel != value)
                {
                    _currentPageViewModel = value;
                    OnPropertyChanged(nameof(CurrentPageViewModel));
                }

            }
        }

        private void ChangeViewModel(string vmName)
        {
            ViewModelBase newVM = _pageViewModels.FirstOrDefault(vm => vm.Name == vmName);
            CurrentPageViewModel = newVM ?? throw new NotImplementedException($"No ViewModel with name {vmName} exists.");
            newVM.SwitchedToThis();
        }

        public void Dispose()
        {
            ModelFactory.DisposeApiClient();
        }
    }
}
