using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using videoflux.components.VideoPlayer;

using MessageBox = System.Windows.Forms.MessageBox;
using Button = System.Windows.Controls.Button;
using System.Collections.ObjectModel;

namespace videoflux.components.VideoPlaylist
{
    /// <summary>
    /// Lógica de interacción para VideoPlaylist.xaml
    /// </summary>
    public partial class VideoPlaylist : System.Windows.Controls.UserControl
    {
        #region Dependency Properties
        /*public static readonly DependencyProperty PlaylistProperty =
            DependencyProperty.Register("Playlist",
                typeof(Playlist),
                typeof(VideoPlaylist),
                new PropertyMetadata(null));

        [Bindable(true)]
        public Playlist Playlist
        {
            get { return (Playlist)this.GetValue(PlaylistProperty); }
            set { this.SetValue(PlaylistProperty, value); }
        }*/
        /*
        public static readonly DependencyProperty SelectedVideoProperty =
              DependencyProperty.Register("SelectedVideo",
                  typeof(Video),
                  typeof(VideoPlaylist),
                  new PropertyMetadata(null));

        [Bindable(true)]
        public Video SelectedVideo
        {
            get { return (Video)this.GetValue(SelectedVideoProperty); }
            set { this.SetValue(SelectedVideoProperty, value); }
        }
        */
        /*
        public static readonly DependencyProperty VideosProperty =
              DependencyProperty.Register("Videos",
                  typeof(List<Video>),
                  typeof(VideoPlaylist),
                  new PropertyMetadata(null));

        [Bindable(true)]
        public List<Video> Videos
        {
            get { return (List<Video>)this.GetValue(VideosProperty); }
            set { this.SetValue(VideosProperty, value); }
        }*/


        #endregion
        #region Event handlers
        public event EventHandler<RoutedEventArgs> SelectedVideo;
        public event EventHandler<RoutedEventArgs> LoadedPlaylist;
        #endregion

        Playlist playlist;

        public Playlist Playlist
        {
            get { return playlist;  }
        }

        public VideoPlaylist()
        {
            InitializeComponent();

        }

        public void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            playlist = new Playlist();
            this.DataContext = playlist;
            

        }
        public void selectVideo(object sender, RoutedEventArgs e)
        {
            var button =(Button)sender;
            playlist.CurrentVideo = ((Video)button.Tag);

            e.Source = playlist.CurrentVideo;
            SelectedVideo.Invoke(sender,e);

        }
        public void loadPlaylist(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if(!playlist.loadFromFolder(dialog.SelectedPath))
                {
                    MessageBox.Show("No se encontraron videos compatibles");
                }
                else
                {
                   e.Source = dialog.SelectedPath;
                   LoadedPlaylist.Invoke(sender, e);
                }
            } 
 
        }

        public void markVideoAsDone(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            ((Video)button.Tag).VideoStatus = VIDEO_STATUS.DONE;
        }
        public void markVideoAsNotDone(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            ((Video)button.Tag).VideoStatus = VIDEO_STATUS.NOT_DONE;
        }

    }



    public class Playlist : INotifyPropertyChanged
    {

        ObservableCollection<Video> videos = new ObservableCollection<Video>();
        protected Video currentVideo;

        public static string[] allowedExtensions = { ".mp4",".avi" };

        public Playlist()
        { 
        }

        public bool HasVideos
        {
            get { return videos.Count > 0; }
        }
 
        
        public Video CurrentVideo
        {
            get { return currentVideo; }
            set {
                currentVideo = value;
                NotifyPropertyChanged("CurrentVideo");

            }
        }


        public ObservableCollection<Video> Videos
        {
            get { return videos; }
            set
            {
                videos = value;
                NotifyPropertyChanged("Videos");
                NotifyPropertyChanged("HasVideos");
            }
        }
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public bool loadFromFolder(string path)
        {
            ObservableCollection<Video> videos = new ObservableCollection<Video>();

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.GetFiles();

            foreach (FileInfo file in files)
            {
                if (Array.Exists(allowedExtensions, element => element == file.Extension))
                {
                    Video video = new Video();
                    video.Src = file.FullName;
                    video.Name = file.Name;
                    videos.Add(video);
                    
                }
            }


            this.Videos = videos;
            if (this.Videos.Count == 0)
            {
                return false;

            }

            return true;
        }

    }


}
