
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using videoflux.components.VideoInfo;
using videoflux.components.VideoSnapshots;
using Vlc.DotNet;

namespace videoflux.components.VideoPlayer
{
    public partial class VideoPlayer : System.Windows.Controls.UserControl
    {
        #region Event handlers  
        public delegate void SnapshotTakenEventHandler(object sender,RoutedEventArgs e,Snapshot snapshot);
        public event SnapshotTakenEventHandler SnapshotTaken;
        #endregion


        public Video video = new Video();

        public Video Video
        {
            get { return video;  }
            set {

                if(video != null)
                {
                    video.Stop();
                }
                video = value;
                video.Control = (Vlc.DotNet.Forms.VlcControl)this.videoVlc.Child;
          


                this.DataContext = video;
            }
        }


        #region Dependency Properties
        /*
           public static readonly DependencyProperty VideoInfoProperty =
                    DependencyProperty.Register("VideoInfo",
                        typeof(Info),
                        typeof(VideoPlayer),
                        new PropertyMetadata(null));

           [Bindable(true)]
           public Info VideoInfo
           {
               get { return (Info)this.GetValue(VideoInfoProperty); }
               set
               {


                   this.SetValue(VideoInfoProperty, value);
               }
           } 

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
            var videoVlc = new Vlc.DotNet.Forms.VlcControl();
            videoVlc.BeginInit();
            videoVlc.VlcLibDirectory = new DirectoryInfo(@"C:\Users\Gabriel\Downloads\vlc-3.0.6-win64\vlc-3.0.6"); ;
            //((Vlc.DotNet.Forms.VlcControl)this.videoVlc.Child).VlcMediaplayerOptions = new[] { "-vv" };
            videoVlc.EndInit();
            this.videoVlc.Child = videoVlc;

            var window = Window.GetWindow(this);
            window.KeyUp += onKeyDown;
            
        }

        
        public void play(object sender, RoutedEventArgs e)
        {

            this.Video.Play();


        }

        public void pause(object sender, RoutedEventArgs e)
        {
            this.Video.Pause();
            
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
       
        protected MEDIA_STATUS status = MEDIA_STATUS.NO_MEDIA;
        protected string thumbnail;
        protected string name;
        protected string src;
        protected uint speed = 1;
        protected Vlc.DotNet.Forms.VlcControl control;

        #region Constructors

        public Video()
        {

        }

        #endregion

        #region Setters/Getters

        public uint Speed
        {
            get { return speed; }
            set {
                speed = value ;
                NotifyPropertyChanged("Speed");
            }
        }

        public Vlc.DotNet.Forms.VlcControl Control
        {
            set
            {
                control = value;
                if(Src != null)
                {
                 
                control.SetMedia(new Uri(src));

                }

            }
            get { return control; }
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

        public string Src
        {
            get { return src; }
            set
            {
                src = value;
                
                Status = MEDIA_STATUS.STOPPED;

                if(control != null)
                {
                    control.Stop();
                    control.SetMedia(new Uri(Src));
                }

                NotifyPropertyChanged("Src");
                generateThumbnail();
              
            }
        }
        public Uri ThumbnailUri
        {
            get { return new Uri(thumbnail); }

        }
        public string Thumbnail
        {
            get { return thumbnail; }
            set
            {

                thumbnail = value;
                NotifyPropertyChanged("Thumbnail");
                NotifyPropertyChanged("ThumbnailUri");

            }
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
        #endregion

        #region Actions
    
     

        public void Play()
        {
            if(control != null && control.VlcMediaPlayer.GetMedia() != null)
            {
                Status = MEDIA_STATUS.PLAYING;
                control.Play();
            }
        
        }


        public void Pause()
        {
            if (control != null && control.VlcMediaPlayer.GetMedia() != null)
            {
                control.Pause();
                Status = MEDIA_STATUS.PAUSED;
            }
                

        }
        public void Stop()
        {
            if(control != null && control.VlcMediaPlayer.GetMedia() != null)
            {
                control.Stop();
                Status = MEDIA_STATUS.STOPPED; 
            }
       
        }

        public  void FastForward(int seconds)
        {
            if(control != null && control.VlcMediaPlayer.GetMedia() != null)
                control.Time = control.Time + (seconds * 1000);

        }

        public  void Rewind(int seconds)
        {
            if (control != null && control.VlcMediaPlayer.GetMedia() != null)
                control.Time = control.Time - (seconds * 1000);
        }

        public void NextFrame()
        {
            if (control != null && control.VlcMediaPlayer.GetMedia() != null)
                control.Time = control.Time + 250;
        }
        public void PrevFrame()
        {
            if (control != null && control.VlcMediaPlayer.GetMedia() != null)
                control.Time = control.Time - 250;

        }
        public Snapshot Snapshot(int number)
        {
            if (control != null && control.VlcMediaPlayer.GetMedia() != null)
            {
                FileInfo fileInfo = new FileInfo(new Uri(this.Src).LocalPath);
                string dir = fileInfo.DirectoryName + "/capturas";
                Directory.CreateDirectory(dir);
                string path = dir + "/tmp-F" + number + ".png";

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                control.TakeSnapshot(path, 0, 0);
                File.SetAttributes(path, FileAttributes.Hidden);

                Snapshot snapshot = new Snapshot(path,number,control.Time); 
                return snapshot;
            }
            return null;
            

        }
        #endregion

        #region Utils
        protected void generateThumbnail()
        {
            if (Src != null)
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
     
        }

  
        #endregion


        /*
       public string ParsedDuration
        {
            get {

                TimeSpan ts = TimeSpan.FromMilliseconds(mediaPlayer.Length);
                return mediaPlayer.Media.Duration.ToString();//string.Format("{0:hh\\:mm\\:ss}", ts); 
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


        public float Position
        {
            get { return mediaPlayer.Position * 100; }
            set {
                mediaPlayer.Position = value / 100;
                NotifyPropertyChanged("Position");

            }
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
       

     
        */

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
