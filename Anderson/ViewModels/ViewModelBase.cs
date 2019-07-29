using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anderson.ViewModels
{
    public delegate void ViewModelSwitchHandler(string target);
    abstract class ViewModelBase : INotifyPropertyChanged
    {
        public abstract string Name { get; }

        private string _errorMessage;
        public virtual string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public event ViewModelSwitchHandler ViewChanged;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string info)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(info));
            }
        }

        protected void SendViewChange(string name)
        {
            ViewChanged?.BeginInvoke(name, null, null);
        }

        public virtual void SwitchedToThis() { }
    }
}
