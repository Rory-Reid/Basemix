using Basemix.Db;

namespace Basemix
{
    public partial class App : Application
    {
        private readonly Migrator migrator;

        public App(Migrator migrator)
        {
            this.InitializeComponent();
            this.migrator = migrator;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            this.migrator.Start();
            return new Window(new MainPage());
        }
    }
}