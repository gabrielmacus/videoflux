using System; 
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
    }

}
