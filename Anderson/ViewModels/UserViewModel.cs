using Anderson.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anderson.ViewModels
{
    class UserViewModel : ViewModelBase
    {
        public PersonModel UserBack { get; set; }
        public override string Name => "User";
    }
}
