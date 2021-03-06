﻿using Anderson.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anderson.ViewModels
{
    /// <summary>
    /// The base ViewModel of the app, delegating between all the ViewModels as needed
    /// </summary>
    public class ApplicationViewModel : ViewModelBase, IDisposable
    {
        private List<ViewModelBase> _pageViewModels = new List<ViewModelBase>();
        private bool _clientSyncRunning;        // are MatrixClient sync threads running?
        private ClientProvider cp = new ClientProvider();

        public ApplicationViewModel()
        {
            LoginModel loginM = new LoginModel(cp);
            RoomModel roomM = new RoomModel(cp);

            _pageViewModels.Add(new StartViewModel(loginM));
            _pageViewModels.Add(new LoginViewModel(loginM));
            _pageViewModels.Add(new UserViewModel(loginM, roomM));

            foreach(var vm in _pageViewModels)
            {
                vm.ViewChanged += ChangeViewModel;
            }

            CurrentPageViewModel = _pageViewModels[0];
            CurrentPageViewModel.SwitchedToThis();
        }

        public override ViewModelID ID => ViewModelID.Application;

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
            if (newVM.ID == ViewModelID.Login || newVM.ID == ViewModelID.Start)
            {
                _clientSyncRunning = false; // sync threads are stopped on startup and after logout
            } 
            else if (newVM.ID == ViewModelID.User)
            {
                _clientSyncRunning = true;
            }

            if (App.Current.Dispatcher.CheckAccess())  // setup needs to be run from the UI thread
            {
                newVM.SwitchedToThis();
            }
            else
            {
                App.Current.Dispatcher.Invoke(newVM.SwitchedToThis);
            }
        }

        public void Dispose()
        {
            // disposing nonexistent sync threads raises an exception
            if (_clientSyncRunning)
                cp.DisposeClient();
        }
    }
}
