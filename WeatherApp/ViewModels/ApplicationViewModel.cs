using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using WeatherApp.Commands;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        #region Membres

        private BaseViewModel currentViewModel;
        private List<BaseViewModel> viewModels;
        private TemperatureViewModel tvm;
        private OpenWeatherService ows;
        private string filename;

        private VistaSaveFileDialog saveFileDialog;
        private VistaOpenFileDialog openFileDialog;

        #endregion

        #region Propriétés
        /// <summary>
        /// Model actuellement affiché
        /// </summary>
        public BaseViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set {
                currentViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// String contenant le nom du fichier
        /// </summary>
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }

        /// <summary>
        /// Commande pour changer la page à afficher
        /// </summary>
        public DelegateCommand<string> ChangePageCommand { get; set; }

        /// <summary>
        /// TODO 02 [x] : Ajouter ImportCommand
        /// </summary>
        public DelegateCommand<string> ImportCommand { get; set; }

        /// <summary>
        /// TODO 02 [x] : Ajouter ExportCommand
        /// </summary>
        public DelegateCommand<string> ExportCommand { get; set; }

        /// <summary>
        /// TODO 13a [x] : Ajouter ChangeLanguageCommand
        /// </summary>
        public DelegateCommand<string> ChangeLanguageCommand { get; set; }

        public List<BaseViewModel> ViewModels
        {
            get {
                if (viewModels == null)
                    viewModels = new List<BaseViewModel>();
                return viewModels;
            }
        }
        #endregion

        public ApplicationViewModel()
        {
            ChangePageCommand = new DelegateCommand<string>(ChangePage);

            /// TODO 06 [x] : Instancier ExportCommand qui doit appeler la méthode Export
            /// Ne peut s'exécuter que la méthode CanExport retourne vrai
            ExportCommand = new DelegateCommand<string>(Export, CanExport);

            /// TODO 03 [x] : Instancier ImportCommand qui doit appeler la méthode Import
            ImportCommand = new DelegateCommand<string>(Import);

            /// TODO 13b [x] : Instancier ChangeLanguageCommand qui doit appeler la méthode ChangeLanguage
            ChangeLanguageCommand = new DelegateCommand<string>(ChangeLanguage);

            initViewModels();

            CurrentViewModel = ViewModels[0];

        }

        #region Méthodes
        void initViewModels()
        {
            /// TemperatureViewModel setup
            tvm = new TemperatureViewModel();
            tvm.Temperatures.CollectionChanged += Temperatures_CollectionChanged;
            tvm.PropertyChanged += Tvm_PropertyChanged;

            string apiKey = "";

            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "DEVELOPMENT")
            {
                apiKey = AppConfiguration.GetValue("OWApiKey");
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.apiKey) && apiKey == "")
            {
                tvm.RawText = "Aucune clé API, veuillez la configurer";
            } else
            {
                if (apiKey == "")
                    apiKey = Properties.Settings.Default.apiKey;

                ows = new OpenWeatherService(apiKey);
            }

            tvm.SetTemperatureService(ows);
            ViewModels.Add(tvm);

            var cvm = new ConfigurationViewModel();
            ViewModels.Add(cvm);
        }

        private void Temperatures_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ExportCommand.RaiseCanExecuteChanged();
        }

        private void Tvm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TemperatureViewModel.Temperatures))
            {
                tvm.Temperatures.CollectionChanged += Temperatures_CollectionChanged;
                ExportCommand.RaiseCanExecuteChanged();
            }
        }

        private void ChangePage(string pageName)
        {
            if (CurrentViewModel is ConfigurationViewModel)
            {
                ows.SetApiKey(Properties.Settings.Default.apiKey);

                var vm = (TemperatureViewModel)ViewModels.FirstOrDefault(x => x.Name == typeof(TemperatureViewModel).Name);
                if (vm.TemperatureService == null)
                    vm.SetTemperatureService(ows);
            }

            CurrentViewModel = ViewModels.FirstOrDefault(x => x.Name == pageName);
        }

        /// <summary>
        /// TODO 07 [x] : Méthode CanExport ne retourne vrai que si la collection a du contenu
        /// </summary>
        private bool CanExport(string _) => tvm.Temperatures.Count > 0;

        /// <summary>
        /// Méthode qui exécute l'exportation
        /// </summary>
        /// <param name="obj"></param>
        private void Export(string obj)
        {

            if (saveFileDialog == null)
            {
                saveFileDialog = new VistaSaveFileDialog();
                saveFileDialog.Filter = "Json file|*.json|All files|*.*";
                saveFileDialog.DefaultExt = "json";
            }

            /// TODO 08 [x] : Code pour afficher la boîte de dialogue de sauvegarde
            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : demo_openFolderDialog
            /// ---
            /// Algo
            /// Si la réponse de la boîte de dialogue est vrai
            ///   Garder le nom du fichier dans Filename
            ///   Appeler la méthode saveToFile
            ///   
            var result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                Filename = saveFileDialog.FileName;
                saveToFile();
            }
        }

        private void saveToFile()
        {
            /// TODO 09 [x] : Code pour sauvegarder dans le fichier
            /// Voir 
            /// Solution : 14_pratique_examen
            /// Projet : serialization_object
            /// Méthode : serialize_array()
            /// 
            /// ---
            /// Algo
            /// Initilisation du StreamWriter
            /// Sérialiser la collection de températures
            /// Écrire dans le fichier
            /// Fermer le fichier           
            using var sw = new StreamWriter(Filename);
            var json = JsonConvert.SerializeObject(tvm.Temperatures);
            sw.Write(json);
            sw.Close();
        }

        private void openFromFile()
        {

            /// TODO 05 [x] : Code pour lire le contenu du fichier
            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : serialization_object
            /// Méthode : deserialize_from_file_to_object
            /// 
            /// ---
            /// Algo
            /// Initilisation du StreamReader
            /// Lire le contenu du fichier
            /// Désérialiser dans un liste de TemperatureModel
            /// Remplacer le contenu de la collection de Temperatures avec la nouvelle liste
            using var sr = new StreamReader(Filename);
            var contents = sr.ReadToEnd();
            var json = JsonConvert.DeserializeObject<List<TemperatureModel>>(contents);
            tvm.Temperatures = new System.Collections.ObjectModel.ObservableCollection<TemperatureModel>(json);
        }

        private void Import(string obj)
        {
            if (openFileDialog == null)
            {
                openFileDialog = new VistaOpenFileDialog();
                openFileDialog.Filter = "Json file|*.json|All files|*.*";
                openFileDialog.DefaultExt = "json";
            }

            /// TODO 04 [x] : Commande d'importation : Code pour afficher la boîte de dialogue
            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : demo_openFolderDialog
            /// ---
            /// Algo
            /// Si la réponse de la boîte de dialogue est vraie
            ///   Garder le nom du fichier dans Filename
            ///   Appeler la méthode openFromFile
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                Filename = openFileDialog.FileName;
                openFromFile();
            }
        }

        private void ChangeLanguage (string language)
        {
            /// TODO 13c [x] : Compléter la méthode pour permettre de changer la langue
            /// Ne pas oublier de demander à l'utilisateur de redémarrer l'application
            /// Aide : ApiConsumerDemo
            if (Properties.Settings.Default.Language == language)
            {
                return;
            }

            Properties.Settings.Default.Language = language;
            Properties.Settings.Default.Save();
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(language);

            var res = MessageBox.Show(Properties.Resources.msg_restart, Properties.Resources.wn_warning, MessageBoxButton.YesNo);
            if (res == MessageBoxResult.Yes)
            {
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                Application.Current.Shutdown();
            }
        }

        #endregion
    }
}
