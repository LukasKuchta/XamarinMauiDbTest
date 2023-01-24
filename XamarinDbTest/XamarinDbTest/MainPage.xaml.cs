using DatabaseTest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinDbTest
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public async void OnRunDbTest(object sender, EventArgs args)
        {
            await Task.Run(() => {
                PerformanceTest.TestDatabase();
            });
        }
    }
}
