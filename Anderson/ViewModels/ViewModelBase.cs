using System.ComponentModel;

namespace Anderson.ViewModels
{
    public delegate void ViewModelSwitchHandler(ViewModelID target);
    public enum ViewModelID { Application, Login, Start, User }

    abstract class ViewModelBase : INotifyPropertyChanged
    {
        public abstract ViewModelID ID { get; }

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

        protected void SendViewChange(ViewModelID type)
        {
            ViewChanged?.BeginInvoke(type, null, null);
        }

        public virtual void SwitchedToThis() { }
    }
}
