﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using videoflux.components.VideoPlayer;

using MessageBox = System.Windows.Forms.MessageBox;
using Button = System.Windows.Controls.Button;
using System.Collections.ObjectModel;
using videoflux.components.DeviceInfo;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Linq;
using FFMpegCore;
using static System.Net.WebRequestMethods;

namespace videoflux.components.VideoPlaylist
{
    /// <summary>
    /// Lógica de interacción para VideoPlaylist.xaml
    /// </summary>
    public partial class VideoPlaylist : System.Windows.Controls.UserControl
    {
        #region Dependency Properties
        /*public static readonly DependencyProperty PlaylistProperty =
            DependencyProperty.Register("Playlist",
                typeof(Playlist),
                typeof(VideoPlaylist),
                new PropertyMetadata(null));

        [Bindable(true)]
        public Playlist Playlist
        {
            get { return (Playlist)this.GetValue(PlaylistProperty); }
            set { this.SetValue(PlaylistProperty, value); }
        }*/
        /*
        public static readonly DependencyProperty SelectedVideoProperty =
              DependencyProperty.Register("SelectedVideo",
                  typeof(Video),
                  typeof(VideoPlaylist),
                  new PropertyMetadata(null));

        [Bindable(true)]
        public Video SelectedVideo
        {
            get { return (Video)this.GetValue(SelectedVideoProperty); }
            set { this.SetValue(SelectedVideoProperty, value); }
        }
        */
        /*
        public static readonly DependencyProperty VideosProperty =
              DependencyProperty.Register("Videos",
                  typeof(List<Video>),
                  typeof(VideoPlaylist),
                  new PropertyMetadata(null));

        [Bindable(true)]
        public List<Video> Videos
        {
            get { return (List<Video>)this.GetValue(VideosProperty); }
            set { this.SetValue(VideosProperty, value); }
        }*/


        #endregion
        #region Event handlers
        public event EventHandler<RoutedEventArgs> SelectedVideo;
        public event EventHandler<RoutedEventArgs> LoadedPlaylist;
        #endregion

        Playlist playlist;

        public Playlist Playlist
        {
            get { return playlist;  }
        }

        public VideoPlaylist()
        {
            InitializeComponent();

        }

        public void UserControlLoaded(object sender, RoutedEventArgs e)
        {
            playlist = new Playlist();
            this.DataContext = playlist;
            

        }
        public void selectVideo(object sender, RoutedEventArgs e)
        {
            var button =(Button)sender;
            var video = ((Video)button.Tag);

            foreach(Video v in Playlist.Videos)
            {
                if(v.Active)
                {
                    //Saves last position to be resumed from there
                    v.StartPosition = v.PositionProgress; 
                }  

                v.Active = false;
            }

            video.Active = true;
            e.Source =  video; 
            SelectedVideo.Invoke(sender,e);

        }
        public void loadPlaylist(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                if(!playlist.loadFromFolder(dialog.SelectedPath))
                {
                    MessageBox.Show("El nombre de la carpeta es inválido o no se encontraron videos compatibles","Error al carga videos",MessageBoxButtons.OK,MessageBoxIcon.Error);
                }
                else
                {
                   e.Source = dialog.SelectedPath;
                   LoadedPlaylist.Invoke(sender, e);
                }
            } 
 
        }

        public void markVideoAsDone(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            ((Video)button.Tag).VideoStatus = VIDEO_STATUS.DONE;
        }
        public void markVideoAsNotDone(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            ((Video)button.Tag).VideoStatus = VIDEO_STATUS.NOT_DONE;
        }

        private void fixVideos(object sender, RoutedEventArgs e)
        {
            var projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            //string ffmpegPath = Path.Combine(projectPath, "Resources", "ffmpeg.exe");
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = Path.Combine(projectPath, "Resources") });
            var result = MessageBox.Show("¿Corregir videos?", "Atención", MessageBoxButtons.YesNo);
            if (result != DialogResult.Yes) return;

            var videosCount = playlist.Videos.Count;
            pbar.Maximum = videosCount;

            foreach (var video in playlist.Videos)
            {
                video.Stop();

                if (Regex.IsMatch(video.Src, @".*__FIXED\..*$"))
                    continue;

                var extension = Path.GetExtension(video.Src);
                var oldPath = Path.Combine(Path.GetDirectoryName(video.Src),$"{Path.GetFileNameWithoutExtension(video.Src)}__OLD{extension}");
                var outPath = Path.Combine(Path.GetDirectoryName(video.Src), $"{Path.GetFileNameWithoutExtension(video.Src)}__FIXED{extension}");

                var success = FFMpegArguments
                    .FromFileInput(video.Src)
                    .OutputToFile(outPath, false, options => options
                        .CopyChannel(FFMpegCore.Enums.Channel.Both))
                    .ProcessSynchronously();

                if(success)
                {
                    System.IO.File.Move(video.Src, oldPath);
                    pbar.Value = pbar.Value + 1;
                }
                else if(System.IO.File.Exists(outPath))
                {
                    System.IO.File.Delete(outPath);
                    MessageBox.Show($"Error al corregir el video {Path.GetFileName(video.Src)}");
                }

            }
            playlist.loadFromFolder(Path.GetDirectoryName(playlist.Videos.First().Src));
            MessageBox.Show("Proceso finalizado");
            pbar.Value = 0;
        }


        private void ProgressBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }


    [Serializable]
    public class Playlist : INotifyPropertyChanged
    {

        ObservableCollection<Video> videos = new ObservableCollection<Video>();


        public static string[] allowedExtensions = { ".mp4",".avi" };

        public Playlist()
        { 
        }

        public bool HasVideos
        {
            get { return videos.Count > 0; }
        }
 
   

        public ObservableCollection<Video> Videos
        {
            get { return videos; }
            set
            {
                videos = value;
                NotifyPropertyChanged("Videos");
                NotifyPropertyChanged("HasVideos");
            }
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

        public bool loadFromFolder(string path)
        {

            try
            {

                Info.ExtractDeviceNumber(path);
            }
            catch(WrongFolderException e)
            {
                return false;
            }



            ObservableCollection<Video> videos = new ObservableCollection<Video>();

            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.GetFiles();
           
            Regex two_cameras_filename_regex_1 = new Regex(@"(?<camera>[1-2]{1})__[0-9]{1,4}-[0-9]{1,2}-[0-9]{1,2}_[0-9]{1,2}-[0-9]{1,2}-[0-9]{1,2}.mp4");
            Regex two_cameras_filename_regex_2 = new Regex(@"([0-9\sa-z]{1,})_(?<camera>[1-2]{1})__[0-9]{1,2}_[0-9]{1,2}_[0-9]{1,4}_[0-9]{1,2}_[0-9]{1,2}_[0-9]{1,2}.mp4");

            foreach (FileInfo file in files)
            {
                //Si tiene el prefijo __OLD, ignoro el archivo
                if(Regex.IsMatch(file.FullName, @".*__OLD\..*$"))
                    continue;

                if (Array.Exists(allowedExtensions, element => element == file.Extension))
                {
                    Match match_1 = two_cameras_filename_regex_1.Match(file.Name);
                    Match match_2 = two_cameras_filename_regex_2.Match(file.Name);

                    var dt = DateTime.ParseExact(
                        Regex.Match(file.DirectoryName, @"\\([0-9]{4}-[0-9]{2}-[0-9]{2})_[0-9].").Groups[1].Value,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture
                        );


                    Video secondary_video = null;
                    if ((match_1.Success && match_1.Groups["camera"].Value == "1") 
                        || (match_2.Success && match_2.Groups["camera"].Value == "1"))
                    {

                        string secondary_camera_file = match_1.Success 
                            ? Regex.Replace(file.FullName, "([1-2]{1})__", "2__") : 
                            Regex.Replace(file.FullName, "_([1-2]{1})__", "_2__");

                        //string secondary_camera_file = Regex.Replace(file.FullName, "_([1-2]{1})__", "_2__");

                        if (System.IO.File.Exists(secondary_camera_file))
                        {
                            secondary_video = new Video(dt);
                            secondary_video.Src = file.FullName;
                            secondary_video.Name = file.Name;

                            var secondary_file = new FileInfo(secondary_camera_file);
                            Video video = new Video(dt);
                            video.Src = secondary_file.FullName;
                            video.Name = secondary_file.Name;
                            video.RelatedVideo = secondary_video;
                            videos.Add(video);
                        }


                    }
                    else if (!match_1.Success && !match_2.Success)
                    {
                        Video video = new Video(dt);
                        video.Src = file.FullName;
                        video.Name = file.Name;
                        videos.Add(video);
                    }

                }

            }

            /*
            foreach (FileInfo file in files)
            {
                var split = file.Name.Split('_');
                var date = file.Name.Split(new[] { "__" }, StringSplitOptions.None)[1].TrimEnd(new[] { '.','m','p','4' }).Split('_');
                var newName = split[1]+"__"+date[2]+"-"+date[1]+"-"+date[0]+"_"+date[3]+"-"+date[4]+"-"+date[5]+".mp4";
                //File.Move(file.FullName, newName);
                Console.WriteLine(file.FullName);
                Console.WriteLine(newName);
            }*/

            /*
            foreach (FileInfo file in files)
            {
                if (Array.Exists(allowedExtensions, element => element == file.Extension))
                {
                    Match match = two_cameras_filename_regex.Match(file.Name);
                    Video secondary_video = null;
                    if ( match.Success && match.Groups["camera"].Value == "1")
                    {
                        string secondary_camera_file = Regex.Replace(file.FullName, "_([1-2]{1})__", "_2__");
                        if (File.Exists(secondary_camera_file))
                        {
                           
                            secondary_video = new Video();
                            secondary_video.Src = file.FullName;
                            secondary_video.Name = file.Name;

                            var secondary_file = new FileInfo(secondary_camera_file);
                            Video video = new Video();
                            video.Src = secondary_file.FullName;
                            video.Name = secondary_file.Name;
                            video.RelatedVideo = secondary_video;
                            videos.Add(video);
                        }


                    }
                    else if (!match.Success)
                    {
                        Video video = new Video();
                        video.Src = file.FullName;
                        video.Name = file.Name;
                        video.RelatedVideo = secondary_video;
                        videos.Add(video);
                    }
    
                }

            }
            */


            this.Videos = videos;
            if (this.Videos.Count == 0)
            {
                return false;

            }

            return true;
        }

    }


}
