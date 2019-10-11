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
                if(this.vplayer.Video != null)
                {
                    if(this.vplayer.Video.RelatedVideo != null)
                    {
                        this.vplayer.Video.RelatedVideo.Dispose();
                    }
                    this.vplayer.Video.Dispose();
                }
 

                this.vplayer.Video = (Video)e.Source;

                if (this.vplayer.Video.RelatedVideo != null)
                {
                    this.vplayer2Container.Visibility = Visibility.Visible;
                    this.vplayer2.Video = this.vplayer.Video.RelatedVideo;
                    this.vplayer2.Video.CanModifySpeed = false;
                }
                else
                {
                    this.vplayer2Container.Visibility = Visibility.Collapsed;
                }

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
            this.vplayer.Focus();
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
                this.vplayer2.Visibility = Visibility.Hidden;
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
            if(this.vplayer.Video.RelatedVideo != null)
            {
                this.vplayer2.Visibility = Visibility.Visible;
            }
            this.vplayer.Focus();
        }

        public void onLoadedPlaylist(object sender, RoutedEventArgs e)
        {
            Info info = new Info((string)e.Source); 
            this.vinfo.Info = info; 
            this.vsnapshots.SnapshotsGroup = new SnapshotsGroup(info.DeviceNumber);
            LoadState(sender,e);
             
        }
        #region Save state
        string stateFileName = "state.dat";
        Dictionary<string, object> stateData = new Dictionary<string, object>();

        private void LoadState(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.vplaylist.Playlist != null && this.vplaylist.Playlist.HasVideos)
                {
                    var finfo = new FileInfo(this.vplaylist.Playlist.Videos[0].Src);
                    var statePath = System.IO.Path.Combine(finfo.Directory.FullName, stateFileName);
                    if (File.Exists(statePath))
                    {
                        FileStream fs = new FileStream(statePath, FileMode.Open);
                        IFormatter formatter = new BinaryFormatter();
                        stateData = (Dictionary<string, object>)formatter.Deserialize(fs);

                        ProcessSerializedData(stateData,sender,e);

                        fs.Close();
                        fs.Dispose();
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
          
        }
        private void ProcessSerializedData(Dictionary<string, object> stateData,object sender, RoutedEventArgs e)
        {
            var playlist = (Playlist)stateData["playlist"];
            foreach(Video v1 in vplaylist.Playlist.Videos)
            {
                foreach(Video v2 in playlist.Videos)
                {
                    if(v1.Src == v2.Src)
                    { 
                        v1.Fines = v2.Fines;
                        v1.VideoStatus = v2.VideoStatus;
                        v1.Active = v2.Active; 
                        v1.StartPosition = v2.StartPosition; 
 
                        
                        if(v1.Active)
                        {
                            e.Source = v1; 
                            onSelectedVideo(sender,e);
                            //v1.Play();
                        }
                      

                    }
                }
            }

        }
        private void SaveState(object sender, RoutedEventArgs e,bool fromTimer = false)
        {
            if (this.vplaylist.Playlist != null && this.vplaylist.Playlist.HasVideos)
            {
                
                var finfo = new FileInfo(this.vplaylist.Playlist.Videos[0].Src);
                var statePath = System.IO.Path.Combine(finfo.Directory.FullName, stateFileName);
                
                if(!fromTimer)
                {
                    //Saves start point for next load
                    foreach (Video v in this.vplaylist.Playlist.Videos)
                    {
                        if (v.Active)
                        {
                            v.StartPosition = v.PositionProgress;
                        }
                    }
                }
            

                stateData["playlist"] = this.vplaylist.Playlist;  
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(statePath, FileMode.Create, FileAccess.Write);
                formatter.Serialize(stream, stateData);
                stream.Close();
            }
               
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
           
            window.Closed += delegate {
                SaveState(sender, e);
            };

            var t = new System.Timers.Timer(15000);
            t.AutoReset = true;
            t.Enabled = true;
            t.Elapsed += delegate
            {
                SaveState(sender, e,true);
            };
            t.Start();
        

        }

        #endregion


        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.S)
            {

                if(this.vplayer2.IsFocused)
                {
                    this.vplayer.Focus();
                    this.vplayer.Video.Pause();
                    this.vplayer2.Video.Pause();
                }
                else
                {
                    this.vplayer2.Focus();
                    this.vplayer2.Video.Pause();
                    this.vplayer.Video.Pause();
                }
               
            }
            //Console.WriteLine(e.Key);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
         }

        private void Page_KeyUp(object sender, KeyEventArgs e)
        {
         
            /*
            if(vplayer.Video != null && vplayer.Video.RelatedVideo != null)
            {
                if (e.Key == Key.F1)
                {
                    e.Handled = true;

                    this.vplayer2.Focus();
                    var e1 = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.F1) { RoutedEvent = Keyboard.KeyDownEvent };
                    InputManager.Current.ProcessInput(e1);
                }
                else if (e.Key == Key.F2 || e.Key == Key.F3)
                {

                    e.Handled = true;

                    this.vplayer.Focus();
                    var e1 = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, e.Key) { RoutedEvent = Keyboard.KeyDownEvent };
                    InputManager.Current.ProcessInput(e1);

                }
                else if (e.Key == Key.Space)
                {
                    e.Handled = true;

                    this.vplayer.Focus();
                    var e1 = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Space) { RoutedEvent = Keyboard.KeyDownEvent };
                    InputManager.Current.ProcessInput(e1);

                    this.vplayer2.Focus();
                    InputManager.Current.ProcessInput(e1);

                }
                else if(e.Key == Key.S)
                {
                    e.Handled = true;

                    var e1 = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.S) { RoutedEvent = Keyboard.KeyDownEvent };
                    this.vplayer.Focus();
                    InputManager.Current.ProcessInput(e1);
                }
            }*/

        }

        private void Vplayer_GotFocus(object sender, RoutedEventArgs e)
        {
            this.vplayerSelectionLine.Background = new SolidColorBrush(Color.FromRgb(0, 150, 136));
            this.vplayer.timeRemaining.Foreground = new SolidColorBrush(Color.FromRgb(45, 196, 123));
            this.vplayer.timeElapsed.Foreground = new SolidColorBrush(Color.FromRgb(25, 79, 227));
        }

        private void Vplayer2_GotFocus(object sender, RoutedEventArgs e)
        {
            this.vplayer2SelectionLine.Background = new SolidColorBrush(Color.FromRgb(0, 150, 136));
            this.vplayer2.timeRemaining.Foreground = new SolidColorBrush(Color.FromRgb(45, 196, 123));
            this.vplayer2.timeElapsed.Foreground = new SolidColorBrush(Color.FromRgb(25, 79, 227));
        }

        private void Vplayer_LostFocus(object sender, RoutedEventArgs e)
        {
            this.vplayerSelectionLine.Background = new SolidColorBrush(Color.FromArgb(0,0,0,0));
            this.vplayer.timeRemaining.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            this.vplayer.timeElapsed.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

        }

        private void Vplayer2_LostFocus(object sender, RoutedEventArgs e)
        {
            this.vplayer2SelectionLine.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            this.vplayer2.timeRemaining.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            this.vplayer2.timeElapsed.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));

        }
    }

}
