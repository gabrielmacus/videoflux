using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using videoflux.components.VideoPlayer;

namespace videoflux.components.DeviceInfo
{
    /// <summary>
    /// Lógica de interacción para DeviceInfo.xaml
    /// </summary>
    public partial class DeviceInfo : UserControl
    {
        Info info;

        public Info Info
        {
            get { return info; }
            set {
                info = value;
                this.DataContext = info;
            }
        }

        public DeviceInfo()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
 
        }
        
    }
    public class WrongFolderException : Exception
    {
        public WrongFolderException():base("El nombre de la carpeta seleccionada no tiene el formato correcto")
        {
            
        }
    }
     
    [Serializable]
    public class Info : INotifyPropertyChanged
    { 
        string videosFolder;
        int deviceNumber;
        int totalFines = 0;
        DateTime timeStarted = DateTime.Now;
        Video lastVideo;

        public int TotalFines
        {
            get { return totalFines; }
            set
            {
                totalFines = value;
                NotifyPropertyChanged("TotalFines");
            }
        }

        public Video LastVideo
        {
            get { return lastVideo; }
            set { lastVideo = value; NotifyPropertyChanged("LastVideo"); }
        }

        public string CurrentTime
        {
            get
            {
                return DateTime.Now.ToString("HH:mm:ss");
            }
        }

        public string TimeElapsedFormatted
        {
            get
            {
                DateTime currentTime = DateTime.Now;
                var ts = currentTime.Subtract(timeStarted);

                return string.Format("{0:00}:{1:00}:{2:00}", ts.TotalHours, ts.Minutes, ts.Seconds);
                

            }
        }
        public string VideosFolder
        {
            get { return videosFolder; }
            set
            {
                videosFolder = value;
                NotifyPropertyChanged("VideosFolder");
                NotifyPropertyChanged("DeviceNumber");
            }
        }
        public int DeviceNumber
        {
            get { 
                return deviceNumber; 
            }
         
        }
        
        public String TimeElapsed
        {
            get {
                return DateTime.Now.ToString("HH:mm:ss");
            }
        }

        public static int ExtractDeviceNumber(string folder)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folder);
            string directoryName = directoryInfo.Name;
            string[] arr = directoryName.Split('_');
            var number = "";
            if(arr.Length == 1)
            {
                throw new WrongFolderException();
            }

            foreach (char c in arr.Last())
            {

                if (!Regex.IsMatch(c.ToString(), @"\d"))
                {
                    break;
                }
                number += c;
            }
            int parsedNumber;
            if (!int.TryParse(number, out parsedNumber))
            {
                throw new WrongFolderException();
            }

            return parsedNumber;
        }

        public Info(string videosFolder)
        {
            VideosFolder = videosFolder;
            deviceNumber = ExtractDeviceNumber(VideosFolder);

            Timer t = new Timer();
            t.Elapsed += (object source, ElapsedEventArgs e) => {
                NotifyPropertyChanged("CurrentTime");
                NotifyPropertyChanged("TimeElapsedFormatted");
            };
            t.Interval = 1000;
            t.Enabled = true;
            t.Start();
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


    }

}
