using System;
using System.Collections.Generic;
using System.ComponentModel; 
using System.Windows;
using System.Windows.Controls; 
using System.Windows.Media.Imaging; 
using System.Collections.ObjectModel;  
using System.Diagnostics;
using System.IO;
using NReco.VideoConverter;
using System.Linq;
using videoflux.components.VideoPlayer;

namespace videoflux.components.VideoSnapshots
{

    /// <summary>
    /// Lógica de interacción para VideoSnapshots.xaml
    /// </summary>
    public partial class VideoSnapshots : UserControl
    { 
        
        

        SnapshotsGroup snapshotsGroup;

        public SnapshotsGroup SnapshotsGroup
        {
            get { return snapshotsGroup; }
            set
            {
                snapshotsGroup = value;
                this.DataContext = snapshotsGroup;
            }
        }

        public VideoSnapshots()
        {
            InitializeComponent();

    
        }

        private void viewSnapshot(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;

            Process.Start(((Snapshot)button.Tag).Src);
        }
         
       

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var ffMpegConverter = new FFMpegConverter();


        }


        private void saveSnapshotsGroup(object sender,RoutedEventArgs e)
        {
            SnapshotsGroup.Save();
        }
    }



    public class SnapshotsGroup : INotifyPropertyChanged
    {
        string licensePlate;
        Dictionary<int,Snapshot> snapshots = new Dictionary<int,Snapshot>();

        protected Video video;
        protected int deviceNumber;
        protected int h;
        protected int m;
        protected int s; 

        public SnapshotsGroup(int deviceNumber, Video video)
        {
            this.DeviceNumber = deviceNumber;
            this.Video = video;
        }

        public int H
        {
            get { return h; }
            set
            {
                h = value; 
                NotifyPropertyChanged("H");
                NotifyPropertyChanged("TimeFormatted");

            }
        }
        public int M
        {
            get { return m; }
            set
            {
                m = value;
                NotifyPropertyChanged("M");
                NotifyPropertyChanged("TimeFormatted");
            }
        }
        public int S
        {
            get { return s; }
            set
            {
                s = value;
                NotifyPropertyChanged("S");
                NotifyPropertyChanged("TimeFormatted");

            }
        }
        
        public Video Video
        {
            get { return video;  }
            set
            {
                video = value;
                NotifyPropertyChanged("Video");

            }
        }

        public int DeviceNumber
        {
            get { return deviceNumber; }
            set
            {
                deviceNumber = value;
                NotifyPropertyChanged("DeviceNumber");
            }
        }

         

        public Dictionary<int, Snapshot> Snapshots
        {
            get { return snapshots; }
            set {

                snapshots = value;
              
                NotifyPropertyChanged("Snapshots"); 
                NotifyPropertyChanged("SnapshotsCollection");
                NotifyPropertyChanged("SnapshotsPlaceholdersCollection");
            }
        }


        public ObservableCollection<Snapshot> SnapshotsCollection
        {
            get {

                IEnumerable<Snapshot> query = from s in snapshots
                                              orderby s.Key
                                              select s.Value; //new ObservableCollection<Snapshot>(snapshots.Values).OrderBy(s => s.Number);
           
                return new ObservableCollection<Snapshot>(query.ToList());
            }
        }

        public ObservableCollection<int> SnapshotsPlaceholdersCollection
        {
            get
            {
                int totalPlaceholders = 3 - SnapshotsCollection.Count;
                var placeholders = new ObservableCollection<int>();
                for(var i=1;i<=totalPlaceholders;i++)
                {
                    placeholders.Add(i);
                }
                return placeholders;
            }
        }

        public String TimeFormatted
        {

            get {
         
                var ts = new TimeSpan(H,M,S);
                var dateTime = new DateTime(ts.Ticks);
                return dateTime.ToString("HHmmss");
            }
        }


        public void Save()
        {
            #region Save Images
            foreach (Snapshot entry in SnapshotsCollection)
            {
                if(File.Exists(entry.Src)) 
                { 
                    var dest = $"{new FileInfo(entry.Src).Directory.FullName}/{LicensePlate}-{TimeFormatted}-F{entry.Number}-{DeviceNumber}.png";
                    if(dest != entry.Src)
                    {
                        if (File.Exists(dest))
                        {
                            File.Delete(dest);
                        }

                        File.Copy(entry.Src, dest);
                        File.SetAttributes(dest, ~FileAttributes.Hidden & ~FileAttributes.ReadOnly & ~FileAttributes.System);

                        File.Delete(entry.Src);
                        entry.Src = dest;
                    }
                
                }
              
            }
            #endregion

            #region Save video
             
            var from = Convert.ToInt32(Math.Floor((double)Snapshots[2].Time / 1000));
            var to = Convert.ToInt32(Math.Ceiling((double)Snapshots[3].Time / 1000));
            var i = this.Video.Src; 
            var o = $@"{new FileInfo(this.Video.Src).Directory.FullName}\capturas\{LicensePlate}-{TimeFormatted}-{DeviceNumber}.mp4"; 
            var ffMpegConverter = new FFMpegConverter();
            var t =  to - from;

            ffMpegConverter.Invoke($"-i \"{i}\" -ss {from} -t {t} -preset superfast -c:v libx264 -an -crf 30  {o}");
            
            #endregion

            NotifyPropertyChanged("Snapshots");
            
        }

        public bool  HasSnapshots
        {
            get { return snapshots.Count > 0; }
        }

        public string LicensePlate
        {
            get { return licensePlate; }
            set
            {
                licensePlate = value;
                NotifyPropertyChanged("LicensePlate");
            }
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


    public class Snapshot : INotifyPropertyChanged
    {
        protected string src;
        protected int number;
        protected long time;

        public Snapshot(string src,int number, long time)
        {
            this.src = src;
            this.number = number;
            this.time = time;
        }
 

        public int Number
        {
            get { return number; }
            set
            {
                number = value;
                NotifyPropertyChanged("Number");
            }
        }
        public long Time
        {
            get { return time;  }
            set
            {
                time = value;
                NotifyPropertyChanged("Time");
            }
        }

        public BitmapImage SrcBitmap
        {
            get {

                 

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;//IMPORTANT!
                image.UriSource = new Uri(Src);
                image.EndInit();

                return image;
            }
        }
        public string Src
        {
            get { return src; }
            set
            {
                src = value;
                NotifyPropertyChanged("Src");
                NotifyPropertyChanged("SrcBitmap");
            }
        }

        public Snapshot()
        {

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
