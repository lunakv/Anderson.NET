using System.Windows;

namespace Anderson
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var app = new ApplicationWindow();
            var context = new ViewModels.ApplicationViewModel();
            app.DataContext = context;
            app.Show();
        }
    }
}
