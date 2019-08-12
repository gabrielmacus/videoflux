
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using videoflux.components.DeviceInfo;
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
                video.Speed = 1;

                if (video.RelatedVideo != null)
                {
                    var relatedVideo = video.RelatedVideo;
                    relatedVideo.Stop();
                    relatedVideo.Control = (Vlc.DotNet.Forms.VlcControl)this.videoVlc2.Child;
                    relatedVideo.Speed = 1;
                    video.RelatedVideo = relatedVideo;
                    
                }

                this.Focus();

                this.DataContext = video;

            }
        }


        #region Dependency Properties
        /*
           public static readonly DependencyProperty VideoInfoProperty =
                    DependencyProperty.Register("DeviceInfo",
                        typeof(Info),
                        typeof(VideoPlayer),
                        new PropertyMetadata(null));

           [Bindable(true)]
           public Info DeviceInfo
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

        double doublePbarPositionStart = 0;
        bool pbarSliding = false;
        public void progressbarSlideEnd(object sender, MouseEventArgs e)
        {
            pbarSliding = false;
        }
        public void progresssbarSliding(object sender,MouseEventArgs e)
        {
            if(pbarSliding)
            {
                var pbar = (ProgressBar)sender;
                doublePbarPositionStart = e.GetPosition(pbar).X;

                var percent = (doublePbarPositionStart * 100) / pbar.ActualWidth;
                percent = percent > 100 ? 100 : percent;

                Video.Play();


                pbar.Value = percent;
                Video.Position = (float)percent;
            }
       

        }
        public void seekOnProgressbar(object sender, MouseEventArgs e)
        {
            //Vuelvo el video a velocidad normal, para evitar desincronización cuando reproduzco 2 videos simultaneamente
            Video.Speed = 1;
            pbarSliding = true;
            progresssbarSliding(sender, e);
             
        }

        public void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            var arch = (IntPtr.Size == 4) ? "x86" : "x64";

            //Inicializo el reproductor 1
            var videoVlc = new Vlc.DotNet.Forms.VlcControl();
            videoVlc.BeginInit();
            videoVlc.VlcLibDirectory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"libvlc\win-{arch}"));
            //videoVlc.VlcMediaplayerOptions = new[] { "--input-fast-seek" }; 
            videoVlc.EndInit();
            this.videoVlc.Child = videoVlc;

            //Inicializo el reproductor 2
            var videoVlc2 = new Vlc.DotNet.Forms.VlcControl();
            videoVlc2.BeginInit();
            videoVlc2.VlcLibDirectory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"libvlc\win-{arch}"));
            //videoVlc2.VlcMediaplayerOptions = new[] { "--input-fast-seek" };
            videoVlc2.EndInit();
            this.videoVlc2.Child = videoVlc2;


            this.Focus();
            this.KeyDown += onKeyDown;
            this.KeyUp += onKeyUp;

        }
       


        public void play(object sender, RoutedEventArgs e)
        {

            this.Video.Play();

        }

        public void pause(object sender, RoutedEventArgs e)
        {
            this.Video.Pause();
        }

        public void setSpeed(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var speed = UInt16.Parse(button.Tag.ToString());
  
            Video.Speed = speed;

        }

        public void onKeyUp(object sender, KeyEventArgs e)
        {
            /*
            if (Video.RelatedVideo != null)
            {
                Video.RelatedVideo.Position = Video.Position;
            }*/
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
                    e.Handled = true;
                    break;
                case Key.Right:

                   
                    var tr = new Thread(new ThreadStart(delegate
                    {
                        
                        if (Video.Status == MEDIA_STATUS.PAUSED /*(e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control*/)
                        {

                            nextFrame(sender, e);
                        }
                        else
                        {
                             
                            fastForward(sender, e);
                            Thread.Sleep(250);

                        }

                    }));
                    tr.Start(); 
                    e.Handled = true;

                    break;

                case Key.Left:

                    
                    var tl = new Thread(new ThreadStart(delegate
                    { 

                        if (Video.Status == MEDIA_STATUS.PAUSED /*(e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control*/)
                        {
                            prevFrame(sender, e);
                        }
                        else
                        {
                           
                            rewind(sender, e);
                            Thread.Sleep(250);

                        }

                    }));
                    tl.Start();
                    e.Handled = true;

                    break;

                case Key.F1:
                    Snapshot s1 = this.Video.Snapshot(1);
                    if (s1 != null)
                    {
                        SnapshotTaken.Invoke(sender, e, s1);
                    }
                    else
                    {
                        MessageBox.Show("Error al capturar la imagen. Inténtelo nuevamente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    e.Handled = true;
                    break;
                case Key.F2:
                    Snapshot s2 = this.Video.Snapshot(2);
                    if(s2 != null)
                    {
                        SnapshotTaken.Invoke(sender, e, s2);
                    }
                    else
                    {
                        MessageBox.Show("Error al capturar la imagen. Inténtelo nuevamente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    e.Handled = true;
                    break;
                case Key.F3:
                    Snapshot s3 = this.Video.Snapshot(3);
                    if(s3 != null)
                    {
                        SnapshotTaken.Invoke(sender, e, s3);
                    }
                    else
                    {
                        MessageBox.Show("Error al capturar la imagen. Inténtelo nuevamente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    e.Handled = true;
                    break;
                case Key.Add:

                    Video.Speed = Video.Speed + 1;
                    e.Handled = true; 
                    break;

                case Key.Subtract:
                    Video.Speed = Video.Speed - 1;
                    e.Handled = true;
                    break;
                default:
                    var pressedNumber = 0;

                    if(e.Key == Key.D1 || e.Key == Key.NumPad1)
                    {
                        pressedNumber = 1;
                    }
                    else if (e.Key == Key.D2 || e.Key == Key.NumPad2)
                    {
                        pressedNumber = 2;
                    }
                    else if (e.Key == Key.D3 || e.Key == Key.NumPad3)
                    {
                        pressedNumber = 3;
                    }
                    else if (e.Key == Key.D4 || e.Key == Key.NumPad4)
                    {
                        pressedNumber = 4;
                    }
                    else if (e.Key == Key.D5 || e.Key == Key.NumPad5)
                    {
                        pressedNumber = 5;
                    }
                    else if (e.Key == Key.D6 || e.Key == Key.NumPad6)
                    {
                        pressedNumber = 6;
                    }
                    else if (e.Key == Key.D7 || e.Key == Key.NumPad7)
                    {
                        pressedNumber = 7;
                    }
                    else if (e.Key == Key.D8 || e.Key == Key.NumPad8)
                    {
                        pressedNumber = 8;
                    }
                    else if (e.Key == Key.D9 || e.Key == Key.NumPad9)
                    {
                        pressedNumber = 9;
                    }

                    if(pressedNumber > 0)
                    {
                        Video.Speed = (uint)pressedNumber;
                    }


                    break;

            }
   
        }

      


        public void rewind(object sender, RoutedEventArgs e)
        {
            //Vuelvo el video a velocidad normal, para evitar desincronización cuando reproduzco 2 videos simultaneamente
            Video.Speed = 1;
            this.Video.Rewind(5);

        }
        public void fastForward(object sender, RoutedEventArgs e)
        {
            //Vuelvo el video a velocidad normal, para evitar desincronización cuando reproduzco 2 videos simultaneamente
            Video.Speed = 1;
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

    
        private void focusOnVideo(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }


      

    }





    public enum MEDIA_STATUS : int { STOPPED = 1, PAUSED = 2, PLAYING = 3,NO_MEDIA=4 };
    public enum VIDEO_STATUS : int { NOT_DONE = 2, DONE = 1};

    [Serializable]
    public class Video : INotifyPropertyChanged,IDisposable
    {

        protected int fines = 0;
        protected MEDIA_STATUS status = MEDIA_STATUS.NO_MEDIA;
        protected string thumbnail;
        protected string name;
        protected string src;
        protected long duration = -1;
        protected float position = 0;
        protected float startPosition = 0;
        protected uint speed = 1;
        [field: NonSerialized]
        protected Vlc.DotNet.Forms.VlcControl control;
        protected uint maxSpeed = 9;
        protected VIDEO_STATUS videoStatus = VIDEO_STATUS.NOT_DONE;
        protected bool active = false;
        protected Video relatedVideo;

        #region Constructors

        public Video()
        {

        }

        #endregion

        #region Setters/Getters
        public Video RelatedVideo
        {
            get
            {
                return relatedVideo;
            }
            set
            {
                relatedVideo = value;
                NotifyPropertyChanged("RelatedVideo");

            }

        }
        public float PositionProgress
        {
            set
            {
                 
            }
            get
            { 

                if (StartPosition > 0)
                {
                    return StartPosition;
                }
                else
                {
                    return Position;
                }
            }
        }

        public bool Active
        {
            get { return active;  }
            set
            {
                active = value;

               
                NotifyPropertyChanged("Active");
            }
        }

        public float Position
        {
            get { return position * 100; }
            set
            {
                this.Control.Position = (value > 0) ? value / 100 : value;
                if(this.relatedVideo != null)
                {
                    this.relatedVideo.Position = value;
                }
            }

        }
        public float StartPosition
        {
            get { return startPosition;  }
            set
            {
                startPosition = value; 
                NotifyPropertyChanged("StartPosition");
            }
        }

        public int Fines
        {
            get { return fines; }
            set
            {
                fines = value;
                NotifyPropertyChanged("Fines");
            }
        }

        public uint Speed
        {
            get { return speed; }
            set {

                if(IsMediaPresent)
                {
                    if (value < 1)
                    {
                        value = 1;
                    }
                    else if (value > maxSpeed)
                    {
                        value = maxSpeed;
                    }

                    control.Rate = (float)value;
                }
        
                speed = value ;
                NotifyPropertyChanged("Speed");


                if (this.relatedVideo != null)
                {
                    this.relatedVideo.Speed = value;
                }

            }
        }

      
        public Vlc.DotNet.Forms.VlcControl Control
        {
            set
            {
                control = value;
                SuscribeEvents();

                if (Src != null)
                {

                    control.SetMedia(new Uri(src)); 
                } 
            }
            get { return control; }
        }


        public long Duration
        {
            get { return duration;  }
        }

        public long Time
        {
            get {
                if (Control != null)
                {
                    return Control.Time;//Control.Length;
                }
                else
                { return -1; }
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
                    control.SetMedia(new Uri(src));
                }

            

                NotifyPropertyChanged("Src");
                generateThumbnail();
              
            }
        }
        public Uri ThumbnailUri
        {
            get { return new Uri(thumbnail); }

        }

        
        public BitmapImage ThumbnailSrc
        {
            get
            {

                using (var fs = new FileStream(thumbnail, FileMode.Open))
                { 
                    var srcBitmap = new BitmapImage();
                    srcBitmap.BeginInit();
                    srcBitmap.CacheOption = BitmapCacheOption.OnLoad;
                    //srcBitmap.Freeze();
                    srcBitmap.StreamSource = fs;
                    srcBitmap.EndInit();

                    fs.Close();
                    fs.Dispose();
                    return srcBitmap;
                }
            }
        }

        public string Thumbnail
        {
            get { return thumbnail; }
            set
            {

                thumbnail = value;
                NotifyPropertyChanged("Thumbnail");
                NotifyPropertyChanged("ThumbnailUri");
                NotifyPropertyChanged("ThumbnailSrc");

            }
        }

        public VIDEO_STATUS VideoStatus
        {
            get { return videoStatus; }
            set
            {
                videoStatus = value;
                NotifyPropertyChanged("VideoStatus");
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

        public string TimeRemaining
        {
            get
            {
                var t = (Time == -1) ? 0 : Time;
                var d = Duration == -1 ? 0 : Duration; 
                var ts = TimeSpan.FromMilliseconds(d - t);
                return string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);


            }
        }

        public string TimeElapsed
        {
            get
            {
                var t = (Time == -1)?0:Time; 
                var ts = TimeSpan.FromMilliseconds(t);  
                return string.Format("{0:00}:{1:00}:{2:00}",ts.Hours,ts.Minutes, ts.Seconds);


            }
        }

        #endregion


        #region Event handlers
        private void Control_MediaChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerMediaChangedEventArgs e)
        { 
            if (Src == null)
            {

                Status = MEDIA_STATUS.NO_MEDIA;
            }
            else
            {
             
                Status = MEDIA_STATUS.STOPPED;
            }
        
        }
        private void Control_EndReached(object sender, Vlc.DotNet.Core.VlcMediaPlayerEndReachedEventArgs e)
        {
            
            Task.Factory.StartNew(delegate {
                 
                Stop();
                NotifyPropertyChanged("Src");
                
            });
        }
        private void Control_TimeChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerTimeChangedEventArgs e)
        {
            NotifyPropertyChanged("Time");
            NotifyPropertyChanged("TimeElapsed");
            NotifyPropertyChanged("TimeRemaining"); 


        }

        private void Control_LengthChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerLengthChangedEventArgs e)
        {
            duration = control.Length;

            if (StartPosition > 0)
            {
                Console.WriteLine("Start position overwrite");
                Position = StartPosition;
                StartPosition = 0;
            }
        }
        private void Control_PositionChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerPositionChangedEventArgs e)
        {
            position = this.Control.Position;
            NotifyPropertyChanged("Position");
            NotifyPropertyChanged("PositionProgress"); 


        }
        private void Control_Playing(object sender, Vlc.DotNet.Core.VlcMediaPlayerPlayingEventArgs e)
        {
            Status = MEDIA_STATUS.PLAYING;
          
          
        }
        private void Control_Paused(object sender, Vlc.DotNet.Core.VlcMediaPlayerPausedEventArgs e)
        {
            Status = MEDIA_STATUS.PAUSED;
        }
        private void Control_Stopped(object sender, Vlc.DotNet.Core.VlcMediaPlayerStoppedEventArgs e)
        {
            Status = MEDIA_STATUS.STOPPED;
        }
        #endregion


        #region Actions

        private void UnsuscribeEvents()
        {
            if (IsMediaPresent)
            { 
                control.LengthChanged -= Control_LengthChanged;
                control.PositionChanged -= Control_PositionChanged;
                control.Paused -= Control_Paused;
                control.Stopped -= Control_Stopped;
                control.Playing -= Control_Playing;
                control.TimeChanged -= Control_TimeChanged;
                control.EndReached -= Control_EndReached;
                control.MediaChanged -= Control_MediaChanged;

            }
        }
        private void SuscribeEvents()
        {
            if (Control != null)
            { 
                control.LengthChanged += Control_LengthChanged;
                control.PositionChanged += Control_PositionChanged;
                control.Paused += Control_Paused;
                control.Stopped += Control_Stopped;
                control.Playing += Control_Playing;
                control.TimeChanged += Control_TimeChanged;
                control.EndReached += Control_EndReached;
                control.MediaChanged += Control_MediaChanged;
            }
        }
        public bool IsMediaPresent
        {
            get
            {
                return (control != null && control.VlcMediaPlayer != null && control.VlcMediaPlayer.GetMedia() != null);
            }
        }

        public bool Play()
        { 

            if(!IsMediaPresent || Status == MEDIA_STATUS.PLAYING)
            {
                return false;
            }
            
            control.Play();

            if(this.relatedVideo != null)
            {
                this.relatedVideo.Play();
            }
 
            return true;

        }


        public bool Pause()
        {

            if (!IsMediaPresent || Status != MEDIA_STATUS.PLAYING)
            {
                return false;
            }

            control.Pause();

            if(this.relatedVideo != null)
            {
                this.relatedVideo.Pause();
            }

            return true;


        }
        public bool Stop()
        {
            if (!IsMediaPresent)
            {
                return false;
            }

            control.Stop();

            if (this.relatedVideo != null)
            {
                this.relatedVideo.Stop();
            }

            return true;

        }

        public bool FastForward(uint seconds)
        {
            if (!IsMediaPresent)
            {
                return false;
            } 
            control.Time = control.Time + (seconds * 1000);


            if (relatedVideo != null)
            {
                relatedVideo.Position = Position;
            }

            /*
            if (this.relatedVideo != null)
            {
                this.relatedVideo.FastForward(seconds);
            }*/

            return true;

        }



        public bool Rewind(uint seconds)
        {
            if (!IsMediaPresent /*|| control.Time < (seconds * 1000)*/)
            {
                return false;
            }
            
            control.Time = control.Time - (seconds * 1000);

            if (relatedVideo != null)
            {
                relatedVideo.Position = Position;
            }

            /*
            if (this.relatedVideo != null)
            {
                this.relatedVideo.Rewind(seconds);
            }*/

            return true;
        }

        public bool NextFrame()
        {
            if (!IsMediaPresent)
            {
                return false;
            }
            control.VlcMediaPlayer.NextFrame();
             
            
            NotifyPropertyChanged("TimeElapsed");
            NotifyPropertyChanged("TimeRemaining");

            if (this.relatedVideo != null)
            {
                this.relatedVideo.NextFrame();
            }

            return true;
        }
        public bool PrevFrame()
        {
            if (!IsMediaPresent)
            {
                return false;
            } 
            control.Time = control.Time - 500;

            if (this.relatedVideo != null)
            {
                this.relatedVideo.PrevFrame();
            }

            return true;

        }
        public Snapshot Snapshot(int number)
        {

            if (this.relatedVideo != null && number != 1)
            {
              return  this.relatedVideo.Snapshot(number);
            }


            if (IsMediaPresent)
            {
                FileInfo fileInfo = new FileInfo(new Uri(this.Src).LocalPath);
                string dir = Path.Combine(fileInfo.DirectoryName, "capturas");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir,"tmp-F" + number + ".png");

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                 
                if(control.TakeSnapshot(path, 0, 0) && File.Exists(path))
                {
                    var time = (control.Time >= 0) ? control.Time : 0;
                 

                    File.SetAttributes(path, FileAttributes.Hidden);
                    Snapshot snapshot = new Snapshot(path, number,time,control.Position);
              
                    return snapshot;
                }
              
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

 

        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Dispose()
        {
            
            UnsuscribeEvents();
            if (IsMediaPresent)
            {
                control.ResetMedia();
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

    }


}
