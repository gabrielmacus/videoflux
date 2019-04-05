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

namespace videoflux.components.VideoSnapshots
{
    /// <summary>
    /// Lógica de interacción para VideoSnapshots.xaml
    /// </summary>
    public partial class VideoSnapshots : UserControl
    {
        public VideoSnapshots()
        {
            InitializeComponent();
        }

    }



    public class Snapshot : INotifyPropertyChanged
    {

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
