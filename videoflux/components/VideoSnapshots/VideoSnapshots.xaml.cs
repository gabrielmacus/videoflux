using System;
using System.Collections.Generic;
using System.ComponentModel; 
using System.Windows;
using System.Windows.Controls; 
using System.Windows.Media.Imaging; 
using System.Collections.ObjectModel;  
using System.Diagnostics;
using System.IO;

namespace videoflux.components.VideoSnapshots
{

    /// <summary>
    /// Lógica de interacción para VideoSnapshots.xaml
    /// </summary>
    public partial class VideoSnapshots : UserControl
    {
        
        SnapshotsGroup snapshotsGroup = new SnapshotsGroup();

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
            this.DataContext = snapshotsGroup; 
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

                return new ObservableCollection<Snapshot>(snapshots.Values);
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

        public void Save()
        {
            foreach(KeyValuePair<int,Snapshot> entry in Snapshots)
            {
                var finfo = new FileInfo(entry.Value.Src);
                var dest = finfo.Directory.FullName + "/dest.png";

                if (File.Exists(dest))
                {
                    File.Delete(dest);
                }

                File.Copy(entry.Value.Src, dest);
            }
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

     
        public SnapshotsGroup()
        {
            
            /*
            var s = new Snapshot();
            s.Src = "A";
            snapshots.Add(1, s);
            snapshots.Add(2, s);
            snapshots.Add(3, s);*/


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
        protected int deviceNumber;


        public Snapshot(string src,int number, long time, int deviceNumber)
        {
            this.src = src;
            this.number = number;
            this.time = time;
            this.deviceNumber = deviceNumber;
        }
        public int DeviceNumber
        {
            get { return deviceNumber;  }
            set
            {
                deviceNumber = value;
                NotifyPropertyChanged("DeviceNumber");
            }
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
