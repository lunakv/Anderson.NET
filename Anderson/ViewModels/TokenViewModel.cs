using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Anderson.ViewModels
{
    public delegate void TokenDeleteHandler(TokenViewModel token);
    public class TokenViewModel
    {
        public TokenViewModel(string userId, string server)
        {
            UserId = userId;
            Server = server;

            TokenDelete_Click = new DelegateCommand(() => TokenDeleted?.Invoke(this));
        }

        public event TokenDeleteHandler TokenDeleted;
        public string UserId { get; }

        public string Server { get; }

        public DelegateCommand TokenDelete_Click { get; }
        
    }
}
