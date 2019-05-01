
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

         

        public void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            var videoVlc = new Vlc.DotNet.Forms.VlcControl();
            videoVlc.BeginInit();

            var arch = (IntPtr.Size == 4) ? "x86" : "x64";
            videoVlc.VlcLibDirectory = new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"libvlc\win-{arch}"));
           // videoVlc.VlcMediaplayerOptions = new[] { "--input-fast-seek" }; 
            videoVlc.EndInit();
            
            this.videoVlc.Child = videoVlc;
             

            //var window = Window.GetWindow(this);
            //window.KeyDown += onKeyDown;

            //this.Focusable = true;
            this.Focus();
            this.KeyDown += onKeyDown;
            /*
            Keyboard.Focus(this);
            EventManager.RegisterClassHandler(typeof(Window),
            Keyboard.KeyDownEvent, new KeyEventHandler(onKeyDown), true);*/


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
                    if (Video.Status == MEDIA_STATUS.PAUSED /*(e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control*/)
                    {
                        nextFrame(sender, e);

                    }
                    else
                    { 

                        fastForward(sender, e);
                        Thread.Sleep(200);
                    }

                    e.Handled = true;

                    break;

                case Key.Left:

                    if (Video.Status == MEDIA_STATUS.PAUSED /*(e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control*/)
                    {
                        
            
                        prevFrame(sender, e);

                        Thread.Sleep(200);
                    }
                    else
                    {
                        rewind(sender, e);

                    }

                    Thread.Sleep(200);
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

        private void focusOnVideo(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }


      

    }





    public enum MEDIA_STATUS : int { STOPPED = 1, PAUSED = 2, PLAYING = 3,NO_MEDIA=4 };
    public enum VIDEO_STATUS : int { NOT_DONE = 2, DONE = 1};

    [Serializable]
    public class Video : INotifyPropertyChanged
    {

        protected int fines = 0;
        protected MEDIA_STATUS status = MEDIA_STATUS.NO_MEDIA;
        protected string thumbnail;
        protected string name;
        protected string src;
        protected long duration = -1;
        protected uint speed = 1; 
        protected Vlc.DotNet.Forms.VlcControl control;
        protected uint maxSpeed = 5;
        protected VIDEO_STATUS videoStatus = VIDEO_STATUS.NOT_DONE;

        #region Constructors

        public Video()
        {

        }

        #endregion

        #region Setters/Getters

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
                control.LengthChanged += delegate
                {
                      duration = control.Length;

                }; 
            }
            get { return control; }
        }

        public long Duration
        {
            get { return duration;  }
        }

        public long Time
        {
            get { return (IsMediaPresent) ? Control.Length : -1; }
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
        #endregion

        #region Actions

            
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

            Status = MEDIA_STATUS.PLAYING;
             
            control.Play();
 
             

            return true;

        }


        public bool Pause()
        {

            if (!IsMediaPresent || Status != MEDIA_STATUS.PLAYING)
            {
                return false;
            }

            control.Pause();
            Status = MEDIA_STATUS.PAUSED;
            return true;


        }
        public bool Stop()
        {
            if (!IsMediaPresent)
            {
                return false;
            }

            control.Stop();
            Status = MEDIA_STATUS.STOPPED;
            return true;

        }

        public bool FastForward(uint seconds)
        {
            if (!IsMediaPresent)
            {
                return false;
            } 
            control.Time = control.Time + (seconds * 1000);
            return true;

        }

        public bool Rewind(uint seconds)
        {
            if (!IsMediaPresent /*|| control.Time < (seconds * 1000)*/)
            {
                return false;
            }
            

            control.Time = control.Time - (seconds * 1000);
            return true;
        }

        public bool NextFrame()
        {
            if (!IsMediaPresent)
            {
                return false;
            }
            control.VlcMediaPlayer.NextFrame();
            //control.Time = control.Time + 350;
            return true;
        }
        public bool PrevFrame()
        {
            if (!IsMediaPresent)
            {
                return false;
            } 
            control.Time = control.Time - 350;
            return true;

        }
        public Snapshot Snapshot(int number)
        {
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
                    File.SetAttributes(path, FileAttributes.Hidden);
                    Snapshot snapshot = new Snapshot(path, number, control.Time);
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

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

    }


}
