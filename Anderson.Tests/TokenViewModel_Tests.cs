using Anderson.ViewModels;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anderson.Tests
{
    [TestFixture]
    class TokenViewModel_Tests
    {
        [Test]
        public void TokenDeleted()
        {
            var t = new TokenViewModel(Utils.ValidKey);
            bool called = false;

            t.TokenDeleted += _ => called = true;

            Assert.True(t.TokenDelete_Click.CanExecute());
            t.TokenDelete_Click.Execute();
            Assert.True(called);
        }
    }
}
