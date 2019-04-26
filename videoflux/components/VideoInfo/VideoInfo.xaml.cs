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

namespace videoflux.components.VideoInfo
{
    /// <summary>
    /// Lógica de interacción para VideoInfo.xaml
    /// </summary>
    public partial class VideoInfo : UserControl
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

        public VideoInfo()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
 
        }
    }

    public class Info : INotifyPropertyChanged
    { 
        string videosDir;
 
        public string VideosDir
        {
            get { return videosDir; }
            set
            {
                videosDir = value;
                NotifyPropertyChanged("VideosDir");
                NotifyPropertyChanged("DeviceNumber");
            }
        }
        public int DeviceNumber
        {
            get {

                DirectoryInfo directoryInfo = new DirectoryInfo(videosDir);
                string directoryName = directoryInfo.Name;
                string [] arr = directoryName.Split('_');
                int deviceNumber = int.Parse(Regex.Replace(arr.Last(), "[^0-9]", ""));
                 
                return deviceNumber;

            }
         
        }
        
        public String CurrentTime
        {
            get {
                return "Hora actual: "+DateTime.Now.ToString("HH:mm:ss");
            }
        }


        public Info()
        {
            Timer t = new Timer();
            t.Elapsed += (object source, ElapsedEventArgs e) => { NotifyPropertyChanged("CurrentTime");  };
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
        public event PropertyChangedEventHandler PropertyChanged;

    }

}
