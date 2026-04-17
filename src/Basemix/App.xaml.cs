using Basemix.Db;

namespace Basemix
{
    public partial class App : Application
    {
        private readonly Migrator migrator;
        private readonly MediaMigrator mediaMigrator;

        public App(Migrator migrator, MediaMigrator mediaMigrator)
        {
            this.InitializeComponent();
            this.migrator = migrator;
            this.mediaMigrator = mediaMigrator;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            this.migrator.Start();
            this.mediaMigrator.Start();
            return new Window(new MainPage());
        }
    }
}