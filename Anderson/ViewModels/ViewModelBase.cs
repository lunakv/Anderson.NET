using System.ComponentModel;

namespace Anderson.ViewModels
{
    public delegate void ViewModelSwitchHandler(ViewModelID target);
    public enum ViewModelID { Application, Login, Start, User, Invite }


    /// <summary>
    /// Base abstract class for all ViewModels
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Unique identifier set for each ViewModel
        /// </summary>
        public abstract ViewModelID ID { get; }

        /// <summary>
        /// An error message to be displayed in a ViewModel
        /// </summary>
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

        /// <summary>
        /// Implementation of INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        /// <summary>
        /// ViewModel switching system. Registered by ApplicationViewModel
        /// </summary>
        public event ViewModelSwitchHandler ViewChanged;
        protected void RaiseViewChanged(ViewModelID type)
        {
            ViewChanged?.Invoke(type);
        }

        public virtual void SwitchedToThis() { }
    }
}
