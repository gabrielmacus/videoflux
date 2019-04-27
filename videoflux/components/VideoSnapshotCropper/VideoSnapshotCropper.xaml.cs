using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
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
        #region Event handlers  
        public delegate void SnapshotCroppedEventHandler(object sender, RoutedEventArgs e, Snapshot snapshot);
        public event SnapshotCroppedEventHandler SnapshotCropped;
        #endregion


        public VideoSnapshotCropper()
        {
            InitializeComponent();

           
        }

        public Crop Crop
        {
            set {

                if(value != null)
                {
                    value.Src = GenerateCrop();
                }
                this.DataContext = value;

            }
            get { return (Crop)this.DataContext;  }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        { 

            this.rectangle.Fill = new SolidColorBrush(Color.FromArgb(0,0,0,0));
            this.rectangle.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
            this.rectangle.StrokeThickness = 5;
            this.rectangle.StrokeDashArray = new DoubleCollection() { 3 };
            this.rectangle.MouseDown += Canvas_MouseDown;
            this.rectangle.Width = 300 + (this.rectangle.StrokeThickness * 2);
            this.rectangle.Height = 155 + (this.rectangle.StrokeThickness * 2);
            this.rectangle.Loaded += Rectangle_Loaded;
            this.canvas.Children.Add(this.rectangle);

            Canvas.SetLeft(this.rectangle, 200);
            Canvas.SetTop(this.rectangle, 200);

            image.MouseLeftButtonDown += image_MouseLeftButtonDown;
            image.MouseLeftButtonUp += image_MouseLeftButtonUp;
            image.MouseMove += image_MouseMove;
            Application.Current.MainWindow.MouseWheel += MainWindow_MouseWheel;

            
              
           }

        private void Rectangle_Loaded(object sender, RoutedEventArgs e)
        {

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
            rectangle.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
           Crop.Src = GenerateCrop();
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if(rectangleDragging == true)
            {
                rectangleEnd = e.MouseDevice.GetPosition(border);
                
                var newX = rectangleEnd.X - inRectangleStart.X;
                var newY = rectangleEnd.Y - inRectangleStart.Y;

                if(newX < 0)
                {
                    newX = 0;
                }
                else if((newX + this.rectangle.ActualWidth) > border.ActualWidth)
                {
                    newX = border.ActualWidth - this.rectangle.ActualWidth;
                } 
                if(newY < 0)
                {
                    newY = 0;
                }
                else if ((newY + this.rectangle.ActualHeight) > border.ActualHeight)
                {
                    newY = border.ActualHeight - this.rectangle.ActualHeight;
                }

                

                Canvas.SetLeft(this.rectangle, newX);
                Canvas.SetTop(this.rectangle, newY);

                Crop.Src = GenerateCrop();


            }

        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            rectangle.Stroke = new SolidColorBrush(Color.FromRgb(13, 93, 163));
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
             
            Crop.Src = GenerateCrop();
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

            Crop.Src = GenerateCrop();
        }

        private CroppedBitmap GenerateCrop()
        {
            double width = border.ActualWidth;
            double height = border.ActualHeight;

            RenderTargetBitmap bmpCopied = new RenderTargetBitmap((int)Math.Round(width), (int)Math.Round(height), 0, 0, PixelFormats.Default);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush vb = new VisualBrush(border);
                vb.Stretch = Stretch.None;
                dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
            }

            bmpCopied.Render(dv);

            var cropRectWidth = (int)(Math.Round(Canvas.GetLeft(rectangle)) + rectangle.StrokeThickness);
            var cropRectHeight = (int)(Math.Round(Canvas.GetTop(rectangle)) + rectangle.StrokeThickness);
            var cropRectX = (int)(rectangle.ActualWidth - rectangle.StrokeThickness * 2);
            var cropRectY = (int)(rectangle.ActualHeight - rectangle.StrokeThickness * 2);
 
            Int32Rect cropRect = new Int32Rect(cropRectWidth, cropRectHeight, cropRectX, cropRectY);
             CroppedBitmap bmpCropped = new CroppedBitmap(bmpCopied,cropRect);

            return bmpCropped;
        }

        private void SaveCrop_Click(object sender, RoutedEventArgs e)
        {

            Crop.Save();
            SnapshotCropped.Invoke(sender, e, Crop.Snapshot);
        }

        #endregion

   
    }


    public class Crop : INotifyPropertyChanged
    {

        protected Snapshot snapshot;
        protected CroppedBitmap src;
        public Snapshot Snapshot
        {
            get { return snapshot; }
            set
            {
                snapshot = value;
                NotifyPropertyChanged("Snapshot");
            }
        }
 

        public void Save()
        {
            var fattr = new FileAttributes();
            if(File.Exists(Snapshot.Src))
            {
                fattr = File.GetAttributes(Snapshot.Src);
                File.Delete(Snapshot.Src);
            }
            FileStream fs = new FileStream(Snapshot.Src,FileMode.Create);
            File.SetAttributes(Snapshot.Src, fattr);
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(Src));
            encoder.Save(fs);
        }

        public CroppedBitmap Src
        {
            get
            {
                return src;
            }
            set
            {
                src = value;
                NotifyPropertyChanged("Src");
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
