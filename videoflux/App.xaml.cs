using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace videoflux
{


    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        public static void Main()
        {
            

            var application = new App();
            application.InitializeComponent();


            application.Run();

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //initialize the splash screen and set it as the application main window
            var splashScreen = new SplashScreenWindow();
            this.MainWindow = splashScreen;
            splashScreen.Show();

            //in order to ensure the UI stays responsive, we need to
            //do the work on a different thread
            Task.Factory.StartNew(() =>
            {
                #region Product status validation
                try
                {
                    //
                    string URL = "https://sheets.googleapis.com/v4/spreadsheets/1mREYPSZnu4siohHRvfnSXC7SdMlsfaOtCQ2tesNZZj4/values/Datos?key=AIzaSyCe_MOXImgiT-c0TfkfI6EC_Ycdq60t-uQ";
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
                    request.ContentType = "application/json; charset=utf-8";
                    HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                    string json = "";
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                        json += reader.ReadToEnd();
                    }
                    json = Regex.Replace(json, @"\r\n?|\n", "").Replace(" ", "");
                    

                    if (json.IndexOf($"[\"{videoflux.Properties.Resources.AppName}\",\"0\"]") > -1)
                    {
                        MessageBox.Show("El sistema se encuentra bloqueado. Contacte un administrador para más información", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Environment.Exit(0);
                    }
                }
                catch(WebException webException)
                {

                    Console.WriteLine(webException.Message);
                    Console.WriteLine(webException.StackTrace);
                } 
                #endregion


                this.Dispatcher.Invoke((Action)delegate ()
                {
                    //initialize the main window, set it as the application main window
                    //and close the splash screen
                    var mainWindow = new MainWindow();
                    this.MainWindow = mainWindow;
                    mainWindow.Show();
                    splashScreen.Close();
                });
               
            });

        }

    }

}
