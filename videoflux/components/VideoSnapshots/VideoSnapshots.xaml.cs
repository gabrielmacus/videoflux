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
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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
        private void validateLicensePlate(object sender, RoutedEventArgs e)
        {
    
        }
        private void validateTime(object sender,RoutedEventArgs e)
        { 
            var input = (TextBox)sender;
            var tag = (string)input.Tag;

            input.Text = Regex.Replace(input.Text, "[^0-9]", "");
      


        }

        private void saveSnapshotsGroup(object sender,RoutedEventArgs e)
        {
            

            if (snapshotsGroup == null || snapshotsGroup.Snapshots == null || snapshotsGroup.Snapshots.Count != 3)
            {
                MessageBox.Show("Debe realizar las 3 capturas requeridas antes de guardar", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            /*
            if (snapshotsGroup.Snapshots[2].Position >= snapshotsGroup.Snapshots[3].Position)
            {
                MessageBox.Show("La foto 2 debe ser anterior en el video a la foto 3", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }*/


            #region Form validation


            if (SnapshotsGroup.LicensePlate == null || SnapshotsGroup.LicensePlate.Trim() == "")
            {
                MessageBox.Show("Debe especificar la patente", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            SnapshotsGroup.LicensePlate = Regex.Replace(SnapshotsGroup.LicensePlate, "[^A-Za-z0-9]", "");
            bool validHour = true;
            bool validMinutes = true;
            bool validSeconds = true;

            if (SnapshotsGroup.H > 23 || SnapshotsGroup.H < 0)
            {
                validHour = false;

            }

            if (SnapshotsGroup.M > 59 || SnapshotsGroup.M < 0)
            {
                validMinutes = false;
            }

            if (SnapshotsGroup.S > 59 || SnapshotsGroup.S < 0)
            {
                validSeconds = false;
            }

            if (!validHour)
            {
                MessageBox.Show("La hora debe ser un valor válido, entre 0 y 23", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            if (!validMinutes)
            {
                MessageBox.Show("Los minutos deben ser un valor válido, entre 0 y 59", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!validSeconds)
            {
                MessageBox.Show("Los segundos deben ser un valor válido, entre 0 y 59", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion



            var Handler = new Action<FFMpegException>((FFMpegException exception) => {
                //MessageBox.Show($"Error al generar el corte de video para la patente {snapshotsGroup.LicensePlate} (Tiempo de video: {snapshotsGroup.H}:{snapshotsGroup.M}:{snapshotsGroup.S}). Inténtelo nuevamente", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);

                MessageBox.Show($"Error al generar el corte de video para la patente {snapshotsGroup.LicensePlate} (Tiempo de video: {snapshotsGroup.H}:{snapshotsGroup.M}:{snapshotsGroup.S}). Inténtelo nuevamente. ({exception.Message} - {exception.StackTrace})", "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Error);

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
            #region Save Images
            var folderPath = new FileInfo(SnapshotsCollection[0].Src).Directory.FullName;
            foreach (Snapshot entry in SnapshotsCollection)
            {
                if(File.Exists(entry.Src)) 
                {
                    var t = new TimeSpan(entry.Time);
                    var dt = new DateTime(t.Ticks);


                    var ts = new TimeSpan(H, M, S);
                    var time = new DateTime(ts.Ticks);
                   
                    var dest = $"{folderPath}/SF1100-P{DeviceNumber.ToString().PadLeft(3,'0')}_F{entry.Number}_{this.video.DateTime.ToString("yyyyMMdd")}_{time.ToString("HHmmss")}.JPG";
                    if(dest != entry.Src)
                    {
                        
                        /*
                        if (File.Exists(dest))
                        {
                            File.Delete(dest);
                        }*/

                        var src = entry.Src;
                        File.Copy(src, dest,true);
                        File.SetAttributes(dest, ~FileAttributes.Hidden & ~FileAttributes.ReadOnly & ~FileAttributes.System);

                        ThreadPool.QueueUserWorkItem(delegate {

                            File.Delete(src);
                        });

                        entry.Src = dest;
                    }
                
                }
              
            }
            #endregion


            #region Save video
            /*
            int from = 0;
            //For solving error that shows up when i try to snapshot the first second (Gets me a very big time value)

            if (Snapshots[2].Time < Snapshots[3].Time)
            {
               from = Convert.ToInt32(Math.Floor((double)Snapshots[2].Time / 1000));
            }
           
            var to = Convert.ToInt32(Math.Ceiling((double)Snapshots[3].Time / 1000));
             
            if(from > 2)
            {
                //Gets 2 second before the photo was taken, to ensure the correct visualization
                from = from - 2;
            }
        
            var i = this.Video.Src;
            var finfo = new FileInfo(this.Video.Src);
            var ext = finfo.Extension.Remove(0,1);
            var o = $@"{finfo.Directory.FullName}\capturas\{LicensePlate}-{TimeFormatted}-{DeviceNumber}.{ext}";
            var ffMpegConverter = new FFMpegConverter();
            var t = to - from;
            var cmd = $"-i \"{i}\" -ss {from} -t {t} -c:v copy  -an {o}";

            //Process video in background
            var task = new Task(new Action(delegate {

                try
                {
                    if(File.Exists(o))
                    {
                        File.Delete(o);
                    }
                    ffMpegConverter.Invoke(cmd);
                }
                catch (FFMpegException e)
                {
                    handler(e);
                }

            }));
            task.Start();
            */
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
        protected float position;
        protected DateTime videoDateTime;

        public Snapshot(string src,int number, long time, float position, DateTime videoDateTime)
        {
            Src = src;
            Position = position;

            //Console.WriteLine(src);

            Number = number;
            Time = time;
            this.videoDateTime = videoDateTime;
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

        public float Position
        {
            get { return position; }
            set
            {
                position = value;
                NotifyPropertyChanged("Position");
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

                //TODO: Review
                try
                {
                    var dateTime = new DateTime(TimeSpan.FromMilliseconds(t).Ticks);
                    return dateTime.ToString("HH:mm:ss");
                }
                catch (OverflowException e)
                {
                    return "00:00:00";

                }


             
                    
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
