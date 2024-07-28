using RemindersADHD.MVVM.Views;

namespace RemindersADHD
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}
