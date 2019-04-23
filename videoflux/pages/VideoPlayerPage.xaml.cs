using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text; 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using videoflux.components.VideoInfo;
using videoflux.components.VideoPlayer;
using videoflux.components.VideoPlaylist;
using videoflux.components.VideoSnapshots;

namespace videoflux.pages
{
    /// <summary>
    /// Lógica de interacción para VideoPlayerPage.xaml
    /// </summary>
    public partial class VideoPlayerPage : Page , INotifyPropertyChanged
    {
       
        /*
        Video selectedVideo;

        public Video SelectedVideo
        {
            get { return selectedVideo;  }
            set {
                selectedVideo = value;
                NotifyPropertyChanged("SelectedVideo");
            }
        }*/

        public VideoPlayerPage()
        { 

            InitializeComponent();
            this.DataContext = this;
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

         public void onSelectedVideo(object sender, RoutedEventArgs e)
        { 
            this.vplayer.Video = (Video)e.Source; 
        }
        public void onSnapshotTaken(object sender,RoutedEventArgs e,Snapshot snapshot)
        {
            var sg = vsnapshots.SnapshotsGroup.Snapshots;
         
            sg[snapshot.Number] = snapshot;
            vsnapshots.SnapshotsGroup.Snapshots = sg;

        }
        public void onLoadedPlaylist(object sender, RoutedEventArgs e)
        {
            Info info = new Info();
            info.VideosDir = (string)e.Source;
            this.vinfo.VInfo = info;



        }
    }

}
