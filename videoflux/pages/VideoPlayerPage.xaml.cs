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
using videoflux.components.VideoSnapshotCropper;

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

            this.vsnapshots.SnapshotsGroup = new SnapshotsGroup(this.vinfo.Info.DeviceNumber, this.vplayer.Video);
        }

        

        public void onSnapshotTaken(object sender,RoutedEventArgs e,Snapshot snapshot)
        {
            var sg = this.vsnapshots.SnapshotsGroup.Snapshots;
            sg[snapshot.Number] = snapshot;
            this.vsnapshots.SnapshotsGroup.Snapshots = sg;
        
            if(snapshot.Number == 1)
            {
                this.vplayer.Video.Pause();
                this.vplayer.Visibility = Visibility.Hidden; 
    
                Crop Crop = new Crop(snapshot);
                this.vcropper.Crop = Crop;
            }

        }
       
        public void onSnapshotCropped(object sender, RoutedEventArgs e, Snapshot snapshot)
        {
            this.vcropper.Crop = null;
            this.vsnapshots.SnapshotsGroup.Snapshots[1] = snapshot;
            this.vplayer.Visibility = Visibility.Visible;
       
            GC.Collect();
        }

        public void onLoadedPlaylist(object sender, RoutedEventArgs e)
        {
            Info info = new Info();
            info.VideosDir = (string)e.Source;
            this.vinfo.Info = info; 

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            
            /*
            Snapshot snapshot = new Snapshot(@"C:\Users\Gabriel\Pictures\Demo\highway-cars-wallpaper-1.jpg", 1,10000);
            Crop Crop = new Crop(snapshot);
            this.vcropper.Crop = Crop;*/
        }
    }

}
