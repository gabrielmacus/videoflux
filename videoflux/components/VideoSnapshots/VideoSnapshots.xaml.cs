using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel; 
using Swordfish.NET.Collections;
using System.Diagnostics;

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


    }


    public class SnapshotsGroup : INotifyPropertyChanged
    {
        string licensePlate;
        ObservableDictionary<int,Snapshot> snapshots = new ObservableDictionary<int,Snapshot>();
         

      
        public ObservableDictionary<int, Snapshot> Snapshots
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
        string src;
        int number;

        public int Number
        {
            get { return number; }
            set
            {
                number = value;
                NotifyPropertyChanged("Number");
            }
        }

        public string Src
        {
            get { return src; }
            set
            {
                src = value;
                NotifyPropertyChanged("Src");
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
