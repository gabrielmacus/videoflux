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
using videoflux.components.DeviceInfo;
using videoflux.components.VideoPlayer;
using videoflux.components.VideoPlaylist;
using videoflux.components.VideoSnapshots;
using videoflux.components.VideoSnapshotCropper;
using System.IO;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
            bool unsavedChanges = false;
            if(this.vsnapshots.SnapshotsGroup != null)
            {
                foreach(KeyValuePair<int,Snapshot> entry in this.vsnapshots.SnapshotsGroup.Snapshots)
                {
                    FileInfo finfo = new FileInfo(entry.Value.Src);
                    if(finfo.Name.Contains("tmp"))
                    {
                        unsavedChanges = true;
                        break;
                    }
                }
            }

            if(!unsavedChanges || MessageBox.Show("¿Seguro que desea cambiar de video? Tiene cambios sin guardar","Atención", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                this.vplayer.Video = (Video)e.Source;
                this.vsnapshots.SnapshotsGroup.Dispose();
                this.vsnapshots.SnapshotsGroup.Video = this.vplayer.Video;
                this.vinfo.Info.LastVideo = this.vplayer.Video;
            }

        }

        public void onSnapshotsGroupSaved(object sender,RoutedEventArgs e,SnapshotsGroup snapshotsGroup)
        {
            this.vinfo.Info.TotalFines = this.vinfo.Info.TotalFines + 1;
            this.vplayer.Video.Fines = this.vplayer.Video.Fines + 1;
            this.vsnapshots.SnapshotsGroup.Dispose();
            this.vsnapshots.SnapshotsGroup.Video = this.vplayer.Video;

        }

        public void onSnapshotTaken(object sender,RoutedEventArgs e,Snapshot snapshot)
        {
            

            var snapshotsGroup = this.vsnapshots.SnapshotsGroup.Snapshots;
            snapshotsGroup[snapshot.Number] = snapshot;
            this.vsnapshots.SnapshotsGroup.Snapshots = snapshotsGroup;
        
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
            Dictionary<int, Snapshot> snapshots = this.vsnapshots.SnapshotsGroup.Snapshots;
            snapshots[1] = snapshot;
            this.vsnapshots.SnapshotsGroup.Snapshots = snapshots;
            this.vplayer.Visibility = Visibility.Visible;
            this.vplayer.Focus();
        }

        public void onLoadedPlaylist(object sender, RoutedEventArgs e)
        {
            Info info = new Info((string)e.Source); 
            this.vinfo.Info = info;

            #region Save state
            info.PropertyChanged += delegate
             {
                //Console.WriteLine("Info changed");

                 //SaveState(info);
             };
            this.vplaylist.Playlist.PropertyChanged += delegate {

                Console.WriteLine("Playlist changed");
                //SaveState(this.vplaylist.Playlist);

            };
            foreach(Video video in this.vplaylist.Playlist.Videos)
            {
                /*
                video.PropertyChanged += (object sender, PropertyChangedEventArgs e) => {

                };*/
            }

            #endregion

            this.vsnapshots.SnapshotsGroup = new SnapshotsGroup(info.DeviceNumber);

 
        }


        private void SaveState(Object obj)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(@"I:\2019-02-16_4(DALILA)\demo.dat", FileMode.Create,FileAccess.Write);
            formatter.Serialize(stream, obj);
            stream.Close();

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
             

            /*
            Snapshot snapshot = new Snapshot(@"C:\Users\Gabriel\Pictures\Demo\highway-cars-wallpaper-1.jpg", 1,10000);
            Crop Crop = new Crop(snapshot);
            this.vcropper.Crop = Crop;*/
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            //Console.WriteLine(e.Key);
        }
    }

}
