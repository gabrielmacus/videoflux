using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using videoflux.components.VideoSnapshots;
using Vlc.DotNet.Forms;

namespace videoflux.components.VideoPlayer
{
    public partial class VideoPlayer : UserControl
    {
        #region Event handlers  
        public delegate void SnapshotTakenEventHandler(object sender,RoutedEventArgs e,Snapshot snapshot);
        public event SnapshotTakenEventHandler SnapshotTaken;
        #endregion


        public Video video;

        public Video Video
        {
            get { return video;  }
            set {

                if(video != null)
                {
                    video.MediaPlayer.Stop();
                }

                video = value;
                video.VideoView = this.videoView;
                this.DataContext = video;
            }
        }

      
        #region Dependency Properties
     
        /*
        public static readonly DependencyProperty VideoProperty =
              DependencyProperty.Register("Video",
                  typeof(Video),
                  typeof(VideoPlayer),
                  new PropertyMetadata(null));

        [Bindable(true)]
        public Video Video
        {
            get { return (Video)this.GetValue(VideoProperty); }
            set {


                this.SetValue(VideoProperty, value);
            }
        }*/
        /*
        public static readonly DependencyProperty SrcProperty =
              DependencyProperty.Register("Src",
                  typeof(string),
                  typeof(VideoPlayer),
                  new PropertyMetadata(null));

        [Bindable(true)]
        public string Src
        {
            get { return (string)this.GetValue(SrcProperty); }
            set { this.SetValue(SrcProperty, value); }
        }*/


        #endregion

        public VideoPlayer()
        {

            InitializeComponent(); 

        }

         

        public void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += onKeyDown;
            /*
            this.Video = new Video();
            this.Video.Src = @"C:\Users\Gabriel\Videos\demo.avi";
            this.Video.VideoView = videoView;*/


            /*
            this.Video.VideoView = videoView;
            this.DataContext = this.Video;*/


        }




        public void play(object sender, RoutedEventArgs e)
        {

            this.Video.MediaPlayer.Play();


        }

        public void pause(object sender, RoutedEventArgs e)
        {
            this.Video.MediaPlayer.Pause();
            
        }

        public void onKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    if (this.Video.Status == MEDIA_STATUS.PLAYING)
                    {
                        pause(sender, e);
                    }
                    else
                    {

                        play(sender, e);

                    }


                    break;
                case Key.Right:
                    if (this.Video.Status == MEDIA_STATUS.PAUSED)
                    {
                        nextFrame(sender, e);

                    }
                    else
                    {
                        fastForward(sender, e);

                    }
                    break;

                case Key.Left:

                    if (this.Video.Status == MEDIA_STATUS.PAUSED)
                    {
                        prevFrame(sender, e);

                    }
                    else
                    {
                        rewind(sender, e);

                    }
                    break;

                case Key.F1:

                    SnapshotTaken.Invoke(sender, e, this.Video.Snapshot(1));
      
                    break;
                case Key.F2:

                    SnapshotTaken.Invoke(sender, e, this.Video.Snapshot(2));

                    break;
                case Key.F3:
                    SnapshotTaken.Invoke(sender, e, this.Video.Snapshot(3));

                    break;
            }
   
        }

      


        public void rewind(object sender, RoutedEventArgs e)
        {

            this.Video.Rewind(5);

        }
        public void fastForward(object sender, RoutedEventArgs e)
        {

            this.Video.FastForward(5);
        }


        public void prevFrame(object sender, RoutedEventArgs e)
        {

            this.Video.PrevFrame();
        }
        public void nextFrame(object sender, RoutedEventArgs e)
        {

            this.Video.NextFrame();
        }

        public void seekProgressBar(object sender, RoutedEventArgs e)
        {

        }
    }

 

    public enum MEDIA_STATUS : int { STOPPED = 1, PAUSED = 2, PLAYING = 3,NO_MEDIA=4 };


    public class Video : INotifyPropertyChanged
    {
        protected LibVLCSharp.WPF.VideoView videoView;
        protected LibVLC libVlc;
        LibVLCSharp.Shared.MediaPlayer mediaPlayer;
        protected MEDIA_STATUS status = MEDIA_STATUS.NO_MEDIA;

        protected string thumbnail;
        protected string name;
    

        #region Constructors

        public Video()
        { 
            Core.Initialize();
            libVlc = new LibVLC();
            this.mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVlc); 
        }
   
        #endregion
        public string ParsedDuration
        {
            get {

                TimeSpan ts = TimeSpan.FromMilliseconds(mediaPlayer.Length);
                return mediaPlayer.Media.Duration.ToString();//string.Format("{0:hh\\:mm\\:ss}", ts); 
            }
        }


        public Uri ThumbnailUri
        {
            get { return new Uri(thumbnail); }
             
        }


        public string Thumbnail
        {
            get { return thumbnail; }
            set {

                thumbnail = value;
                NotifyPropertyChanged("Thumbnail");
                NotifyPropertyChanged("ThumbnailUri");

            }
        }

        protected void init()
        {
            videoView.MediaPlayer = mediaPlayer;
            videoView.MediaPlayer.TimeChanged += (object sender, MediaPlayerTimeChangedEventArgs e) =>
            {
                NotifyPropertyChanged("Time");

            };
            videoView.MediaPlayer.Playing += (object sender,EventArgs e) =>
            {
                Status = MEDIA_STATUS.PLAYING;

            };
            videoView.MediaPlayer.Paused += (object sender, EventArgs e) =>
            {
                Status = MEDIA_STATUS.PAUSED;

            };
            videoView.MediaPlayer.Stopped += (object sender, EventArgs e) =>
            {
                Status = MEDIA_STATUS.STOPPED;

            };
            videoView.MediaPlayer.PositionChanged += (object sender, MediaPlayerPositionChangedEventArgs e) =>
            {
                NotifyPropertyChanged("Position");

            };


          
        }
         
        public float Speed
        {
            get { return mediaPlayer.Rate; }
            set {

                mediaPlayer.SetRate(value);
                NotifyPropertyChanged("Speed");
            }
        }

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                NotifyPropertyChanged("Name");

            }
        }


        public float Position
        {
            get { return mediaPlayer.Position * 100; }
            set {
                mediaPlayer.Position = value / 100;
                NotifyPropertyChanged("Position");

            }
        }


        public LibVLCSharp.WPF.VideoView VideoView
        {
            set {
                this.videoView = value;
                init();
                NotifyPropertyChanged("VideoView");

            }
            get { return videoView; }
        }



        public LibVLCSharp.Shared.MediaPlayer MediaPlayer
        {
            get { return mediaPlayer; }
         
        }


        public MEDIA_STATUS Status
        {
            get { return status; }
            set
            {
                status = value;
                NotifyPropertyChanged("Status");
            }
        }
        public string Src
        {
            get { return (mediaPlayer.Media != null) ? mediaPlayer.Media.Mrl : null; }
            set {
                mediaPlayer.Stop();
                mediaPlayer.Media = new Media(libVlc, value,FromType.FromPath);

                 

                Status = MEDIA_STATUS.STOPPED;
                NotifyPropertyChanged("Src"); 
                generateThumbnail();

            }
        }

        protected void generateThumbnail()
        {

            //Updates thumbail if exists 
            var ffMpeg = new NReco.VideoConverter.FFMpegConverter();
            FileInfo file = new FileInfo(new Uri(Src).LocalPath);
            String thumbailPath = file.DirectoryName + "\\thumb_" + file.Name + ".jpg";
            File.Delete(thumbailPath);
            ffMpeg.GetVideoThumbnail(file.FullName, thumbailPath);

            File.SetAttributes(thumbailPath, FileAttributes.Hidden);
            Thumbnail = thumbailPath;
            ffMpeg.Abort();
        }

        public long Time
        {
            get { return mediaPlayer.Time; }
            set
            { 
                mediaPlayer.Time = value;
                NotifyPropertyChanged("Time");
            }
        }
       
        public void FastForward(int seconds)
        { 
            mediaPlayer.Time = mediaPlayer.Time +(seconds * 1000);
   

        }

        public void Rewind(int seconds)
        { 
            mediaPlayer.Time = mediaPlayer.Time - (seconds * 1000);
 
        }

        public void NextFrame()
        {

            mediaPlayer.Time = mediaPlayer.Time + 500;

        }
        public void PrevFrame()
        {
            mediaPlayer.Time = mediaPlayer.Time - 500;

        }

        public Snapshot Snapshot(int number)
        {
            FileInfo fileInfo = new FileInfo(new Uri(this.Src).LocalPath);
            string dir = fileInfo.DirectoryName + "/capturas";
            Directory.CreateDirectory(dir);
            string path = dir + "/tmp-F" + number + ".png";
            mediaPlayer.TakeSnapshot(0,path,0,0);

            Snapshot snapshot = new Snapshot();
            snapshot.Src = path;
            snapshot.Number = number;

            return snapshot;
        
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
