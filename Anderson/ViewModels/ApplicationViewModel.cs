using Anderson.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anderson.ViewModels
{
    class ApplicationViewModel : ViewModelBase, IDisposable
    {
        private List<ViewModelBase> _pageViewModels = new List<ViewModelBase>();
        private bool _clientSyncRunning;

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

        public override ViewModelID ID => ViewModelID.Application;

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

        private void ChangeViewModel(ViewModelID vmName)
        {
            ViewModelBase newVM = _pageViewModels.FirstOrDefault(vm => vm.ID == vmName);
            CurrentPageViewModel = newVM ?? throw new NotImplementedException($"No ViewModel with name {vmName} exists.");
            if (newVM.ID == ViewModelID.Login)
            {
                _clientSyncRunning = false;
            } 
            else if (newVM.ID == ViewModelID.User)
            {
                _clientSyncRunning = true;
            }
            newVM.SwitchedToThis();
        }

        public void Dispose()
        {
            if (_clientSyncRunning)
                ModelFactory.DisposeApiClient();
        }
    }
}
