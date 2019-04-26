using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using videoflux.components.VideoSnapshots;

namespace videoflux.components.VideoSnapshotCropper
{
    /// <summary>
    /// Lógica de interacción para VideoSnapshotCropper.xaml
    /// </summary>
    public partial class VideoSnapshotCropper : UserControl
    {
        public VideoSnapshotCropper()
        {
            InitializeComponent();
        }

        public Crop Crop
        {
            set { this.DataContext = value; }
            get { return (Crop)this.DataContext;  }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        { 

            this.rectangle.Width = 300;
            this.rectangle.Height = 155;
            this.rectangle.Fill = new SolidColorBrush(Color.FromArgb(0,0,0,0));
            this.rectangle.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            this.rectangle.StrokeThickness = 5;
            this.rectangle.StrokeDashArray = new DoubleCollection() { 3 };
            this.rectangle.MouseDown += Canvas_MouseDown;
    
            Canvas.SetLeft(this.rectangle, 100);
            Canvas.SetTop(this.rectangle, 100);
            this.canvas.Children.Add(this.rectangle);

            image.MouseLeftButtonDown += image_MouseLeftButtonDown;
            image.MouseLeftButtonUp += image_MouseLeftButtonUp;
            image.MouseMove += image_MouseMove;
            Application.Current.MainWindow.MouseWheel += MainWindow_MouseWheel;

        } 

        #region Area selection
        private Rectangle rectangle =new Rectangle();
        private Point origin;
        private Point start;

        private Point inRectangleStart;
        private Point rectangleEnd;
        private Point rectangleStart;
        private bool rectangleDragging = false;

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            rectangleDragging = false;

            Console.WriteLine("END DRAGGING");
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if(rectangleDragging == true)
            {
                rectangleEnd = e.MouseDevice.GetPosition(border);
                
                var newX = rectangleEnd.X - inRectangleStart.X;
                var newY = rectangleEnd.Y - inRectangleStart.Y;

                Canvas.SetLeft(this.rectangle, newX);
                Canvas.SetTop(this.rectangle, newY);
            }

        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

            rectangleStart = e.MouseDevice.GetPosition(border);
            inRectangleStart = e.MouseDevice.GetPosition(rectangle);
            rectangleDragging = true;
        }




        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            image.ReleaseMouseCapture();
        }

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!image.IsMouseCaptured) return;
            Point p = e.MouseDevice.GetPosition(border);

            Matrix m = image.RenderTransform.Value;
            m.OffsetX = origin.X + (p.X - start.X);
            m.OffsetY = origin.Y + (p.Y - start.Y);

            image.RenderTransform = new MatrixTransform(m);
        }

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (image.IsMouseCaptured) return;
            image.CaptureMouse();

            start = e.GetPosition(border);
            origin.X = image.RenderTransform.Value.OffsetX;
            origin.Y = image.RenderTransform.Value.OffsetY;
        }

        private void MainWindow_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(image);

            Matrix m = image.RenderTransform.Value;
            if (e.Delta > 0)
                m.ScaleAtPrepend(1.1, 1.1, p.X, p.Y);
            else
                m.ScaleAtPrepend(1 / 1.1, 1 / 1.1, p.X, p.Y);

            image.RenderTransform = new MatrixTransform(m);
        }
        
        #endregion

          
    }


    public class Crop : INotifyPropertyChanged
    {

        protected Snapshot snapshot;

        public Snapshot Snapshot
        {
            get { return snapshot; }
            set
            {
                snapshot = value;
                NotifyPropertyChanged("Snapshot");
            }
        }

        public Crop(Snapshot snapshot)
        {
            this.Snapshot = snapshot;
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
