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
using System.Windows.Controls.Primitives;
using System.Threading;

namespace videoflux.components.VideoSnapshots
{

    /// <summary>
    /// Lógica de interacción para VideoSnapshots.xaml
    /// </summary>
    public partial class VideoSnapshots : UserControl
    {
        #region Event handlers  
        public delegate void SnapshotsGroupSavedEventHandler(object sender, RoutedEventArgs e, SnapshotsGroup snapshotsGroup);
        public event SnapshotsGroupSavedEventHandler SnapshotsGroupSaved;
        #endregion


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
            if (snapshotsGroup == null || snapshotsGroup.Snapshots == null || snapshotsGroup.Snapshots.Count != 3)
            {
                MessageBox.Show("Debe realizar las 3 capturas requeridas antes de guardar", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (snapshotsGroup.Snapshots[2].Time >= snapshotsGroup.Snapshots[3].Time)
            {
                MessageBox.Show("La foto 2 debe ser anterior en el video a la foto 3", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var Handler = new Action<FFMpegException>((FFMpegException exception) => {
                MessageBox.Show($"Error al generar el corte de video para la patente {snapshotsGroup.LicensePlate} (Tiempo de video: {snapshotsGroup.H}:{snapshotsGroup.M}:{snapshotsGroup.S}). Inténtelo nuevamente", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(exception.Message);
            });

            try
            {
                snapshotsGroup.Save(Handler);

            }
            catch(Exception exception)
            {
                MessageBox.Show("Error al guardar las capturas. Inténtelo nuevamente", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
                return;
            }

            SnapshotsGroupSaved.Invoke(sender, e, snapshotsGroup);

         
   
        }

         
    }



    public class SnapshotsGroup : INotifyPropertyChanged,IDisposable
    {
        string licensePlate;
        Dictionary<int,Snapshot> snapshots = new Dictionary<int,Snapshot>();

        protected Video video;
        protected int deviceNumber;
        protected int h;
        protected int m;
        protected int s; 

        public SnapshotsGroup(int deviceNumber)
        {
            this.DeviceNumber = deviceNumber;
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
                NotifyPropertyChanged("HasSnapshots");

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


        public void Save(Action<FFMpegException> handler)
        {
            #region Save video
            var from = Convert.ToInt32(Math.Floor((double)Snapshots[2].Time / 1000));
            var to = Convert.ToInt32(Math.Ceiling((double)Snapshots[3].Time / 1000));
            var i = this.Video.Src;
            var o = $@"{new FileInfo(this.Video.Src).Directory.FullName}\capturas\{LicensePlate}-{TimeFormatted}-{DeviceNumber}.mp4";
            var ffMpegConverter = new FFMpegConverter();
            var t = to - from;
            var cmd = $"-i \"{i}\" -ss {from} -t {t} -preset superfast -c:v libx264 -an -crf 25  {o}";

            //Process video in background
            new Thread(new ThreadStart(() =>
            {
                try
                {
                    ffMpegConverter.Invoke(cmd);
                }
                catch(FFMpegException e)
                {
                    handler(e);
                }

            })).Start();
            #endregion
             
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

        public void Dispose()
        {
            Snapshots = new Dictionary<int, Snapshot>();
            LicensePlate = null;
            Video = null;
            H = 0;
            M = 0;
            S = 0;
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
            Src = src;
             
   
            //Console.WriteLine(src);

            Number = number;
            Time = time;
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


                using (var fs = new FileStream(src, FileMode.Open))
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

        public string TimeFormatted
        {
            get {

                var t = Math.Round((double)Time);
                if (t < 0) { t = 0; }
                var dateTime = new DateTime(TimeSpan.FromMilliseconds(t).Ticks);
                return dateTime.ToString("HH:mm:ss"); 
                    
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
