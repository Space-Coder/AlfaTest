using log4net;
using System.Windows;

namespace AlfaTest
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(App));
        protected override void OnStartup(StartupEventArgs e)
        {
            log4net.Config.XmlConfigurator.Configure();
            base.OnStartup(e);
            log.Info("Application startup");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            log.Info("Application shutdown");
        }
    }
}