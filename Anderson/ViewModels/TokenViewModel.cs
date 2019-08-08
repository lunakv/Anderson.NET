using Anderson.Structures;
using Prism.Commands;

namespace Anderson.ViewModels
{
    public delegate void TokenDeleteHandler(TokenViewModel token);
    public class TokenViewModel
    {
        public TokenViewModel(TokenKey token)
        {
            Login = token;

            TokenDelete_Click = new DelegateCommand(() => TokenDeleted?.Invoke(this));
        }

        public event TokenDeleteHandler TokenDeleted;
        public TokenKey Login { get; }
        public DelegateCommand TokenDelete_Click { get; }
        
    }
}
