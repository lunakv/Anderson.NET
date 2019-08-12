using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Anderson.Views
{
    /// <summary>
    /// Interaction logic for UserView.xaml
    /// </summary>
    public partial class UserView : UserControl
    {
        public UserView()
        {
            InitializeComponent();
        }

        private void InsertNewline_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            InputBox.Text += Environment.NewLine;
            InputBox.CaretIndex = InputBox.Text.Length;
        }
    }
}
