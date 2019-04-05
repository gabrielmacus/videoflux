using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using videoflux.components.VideoPlayer;

namespace videoflux.components.VideoPlaylist
{
    /// <summary>
    /// Lógica de interacción para VideoPlaylist.xaml
    /// </summary>
    public partial class VideoPlaylist : UserControl
    {
        Playlist playlist;

        public VideoPlaylist()
        {
            InitializeComponent();
        }

        public void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            playlist = new Playlist();

        }


    }


    public class Playlist : INotifyPropertyChanged
    {

        List<Video> videos = new List<Video>();

        public Playlist()
        {
           
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

    }


}
