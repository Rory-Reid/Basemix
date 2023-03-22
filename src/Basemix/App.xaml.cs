using Basemix.Db;

namespace Basemix
{
    public partial class App : Application
    {
        public App(Migrator migrator)
        {
            InitializeComponent();

            MainPage = new MainPage();
            migrator.Start();
        }
    }
}