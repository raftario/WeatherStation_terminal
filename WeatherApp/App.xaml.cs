using System.Threading;
using System.Windows;

namespace WeatherApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// TODO 11 [x] : Ajouter les ressources linguistiques au projet
        /// TODO 12 [x] : Charger la langue d'affichage

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var lang = WeatherApp.Properties.Settings.Default.Language;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);

            ApplicationView app = new ApplicationView();

            app.Show();

        }
    }
}
