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
using Vlc.DotNet.Forms;

namespace videoflux.components.VideoPlayer
{
    public partial class VideoPlayer : UserControl
    {
         
        protected Video videoPlayer;

        #region Dependency Properties

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
        }


        #endregion

        public VideoPlayer()
        {

            InitializeComponent(); 

        }

         

        public void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += onKeyDown;

            #region Model initialization
            videoPlayer = new Video(this.videoView); 
           // videoPlayer.Src = Src;//@"C:\Users\Gabriel\Videos\demo.avi";
            this.DataContext = videoPlayer;
            #endregion



        }




        public void play(object sender, RoutedEventArgs e)
        {

            videoPlayer.MediaPlayer.Play();


        }

        public void pause(object sender, RoutedEventArgs e)
        {
            videoPlayer.MediaPlayer.Pause();
            
        }

        public void onKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    if (videoPlayer.Status == MEDIA_STATUS.PLAYING)
                    {
                        pause(sender, e);
                    }
                    else
                    {

                        play(sender, e);

                    }


                    break;
                case Key.Right:
                    if (videoPlayer.Status == MEDIA_STATUS.PAUSED)
                    {
                        nextFrame(sender, e);

                    }
                    else
                    {
                        fastForward(sender, e);

                    }
                    break;

                case Key.Left:

                    if (videoPlayer.Status == MEDIA_STATUS.PAUSED)
                    {
                        prevFrame(sender, e);

                    }
                    else
                    {
                        rewind(sender, e);

                    }
                    break;
            }
   
        }

        public void screenshot(object sender, RoutedEventArgs e)
        {
           
        }


        public void rewind(object sender, RoutedEventArgs e)
        {

            videoPlayer.Rewind(5);

        }
        public void fastForward(object sender, RoutedEventArgs e)
        {
            
            videoPlayer.FastForward(5);

        }

        public void prevFrame(object sender, RoutedEventArgs e)
        {

            videoPlayer.PrevFrame();
        }
        public void nextFrame(object sender, RoutedEventArgs e)
        {

            videoPlayer.NextFrame();
        }

        public void seekProgressBar(object sender, RoutedEventArgs e)
        {

        }
    }

    public enum MEDIA_STATUS : int { STOPPED = 1, PAUSED = 2, PLAYING = 3 };

    public class Video : INotifyPropertyChanged
    {
        protected LibVLCSharp.WPF.VideoView videoView;
        protected LibVLC libVlc;

        protected MEDIA_STATUS status = MEDIA_STATUS.STOPPED;

        protected void init()
        {

            //Directory.GetCurrentDirectory() + "\\Resources\\"
            //@"C:\libvlc\win-x86"
            Core.Initialize();
            libVlc = new LibVLC(); 
            LibVLCSharp.Shared.MediaPlayer mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(libVlc);
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
            get { return videoView.MediaPlayer.Rate; }
            set {

                videoView.MediaPlayer.SetRate(value);
                NotifyPropertyChanged("Speed");
            }
        }

        public float Position
        {
            get { return videoView.MediaPlayer.Position * 100; }
            set {
                videoView.MediaPlayer.Position = value / 100;
                NotifyPropertyChanged("Position");

            }
        }

        public Video(LibVLCSharp.WPF.VideoView videoView)
        {
            this.videoView = videoView;
            init();
            
        }
        public LibVLCSharp.Shared.MediaPlayer MediaPlayer
        {
            get { return videoView.MediaPlayer; }
         
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
            get { return (videoView.MediaPlayer.Media != null) ? videoView.MediaPlayer.Media.Mrl : null; }
            set {
                
                this.videoView.MediaPlayer.Media = new Media(libVlc, value,FromType.FromPath);
                NotifyPropertyChanged("Src");

            }
        }

        public long Time
        {
            get { return videoView.MediaPlayer.Time; }
            set
            { 
                videoView.MediaPlayer.Time = value;
                NotifyPropertyChanged("Time");
            }
        }
       
        public void FastForward(int seconds)
        { 
            videoView.MediaPlayer.Time = videoView.MediaPlayer.Time +(seconds * 1000);
   

        }

        public void Rewind(int seconds)
        { 
            videoView.MediaPlayer.Time = videoView.MediaPlayer.Time - (seconds * 1000);
 
        }

        public void NextFrame()
        {
            
            videoView.MediaPlayer.Time = videoView.MediaPlayer.Time + 100;

        }
        public void PrevFrame()
        {
            videoView.MediaPlayer.Time = videoView.MediaPlayer.Time - 100;

        }

        public void Screenshot()
        {  

            //videoView.MediaPlayer.TakeSnapshot(0,"",0,0);
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
