using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using videoflux.components.VideoPlayer;
using System.IO;
using System.Threading;

namespace videofluxTest
{
    /// <summary>
    /// Descripción resumida de VideoPlayerTest
    /// </summary>
    [TestClass]
    public class VideoPlayerTest
    {
        public VideoPlayerTest()
        {
            //
            // TODO: Agregar aquí la lógica del constructor
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Obtiene o establece el contexto de las pruebas que proporciona
        ///información y funcionalidad para la serie de pruebas actual.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Atributos de prueba adicionales
        //
        // Puede usar los siguientes atributos adicionales conforme escribe las pruebas:
        //
        // Use ClassInitialize para ejecutar el código antes de ejecutar la primera prueba en la clase
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup para ejecutar el código una vez ejecutadas todas las pruebas en una clase
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Usar TestInitialize para ejecutar el código antes de ejecutar cada prueba 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup para ejecutar el código una vez ejecutadas todas las pruebas
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        Vlc.DotNet.Forms.VlcControl vlcControl;
        Video video;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            vlcControl = new Vlc.DotNet.Forms.VlcControl();
            vlcControl.BeginInit();
            vlcControl.VlcLibDirectory = new DirectoryInfo(@"C:\Users\Gabriel\Downloads\vlc-3.0.6"); ;
            vlcControl.EndInit();
            video = new Video();
        } 
            

        [TestMethod]
        public void TestPlay()
        { 

            Assert.AreEqual(false, video.Play());
            video.Control = vlcControl;
            Assert.AreEqual(false, video.Play());
            video.Src = @"I:\2019-02-16_4(DALILA)\+Cámara desconoc.1_20190216085017.avi";
            ManualResetEvent statsUpdatedEvent = new ManualResetEvent(false);
            bool playingEventFired = false;
            vlcControl.Playing += delegate
            {
                playingEventFired = true;
            };
            Assert.AreEqual(true, video.Play());
            statsUpdatedEvent.WaitOne(4500, false);
            Assert.AreEqual(true, playingEventFired);
        }

        [TestMethod]
        public void TestPause()
        { 
            Assert.AreEqual(false, video.Pause());
            video.Control = vlcControl;
            Assert.AreEqual(false, video.Pause());
            video.Src = @"C:\Users\Gabriel\Videos\bird.avi";
            video.Play();
            ManualResetEvent statsUpdatedEvent = new ManualResetEvent(false);

            bool eventFired = false;

            vlcControl.Playing += delegate
            {
                Assert.AreEqual(true, video.Pause());
            }; 

            vlcControl.Paused += delegate
            { 
                eventFired = true;
                Assert.AreEqual(false, video.Pause());
            };

            statsUpdatedEvent.WaitOne(2500, false);

            Assert.AreEqual(true, eventFired);
        }


        [TestMethod]
        public void TestFastForward()
        {

            Assert.AreEqual(false, video.FastForward(5));

            video.Control = vlcControl; 
            video.Src = @"C:\Users\Gabriel\Videos\bird.avi";
            video.Play();

            Assert.AreEqual(true, video.FastForward(5));
            Assert.AreEqual(5000, vlcControl.Time);

            ManualResetEvent statsUpdatedEvent = new ManualResetEvent(false);
            vlcControl.LengthChanged += delegate
            {
                Console.WriteLine(vlcControl.Length);
            };
            statsUpdatedEvent.WaitOne(2500, false);


             
        }

        [TestMethod]
        public void TestRewind()
        {
            Assert.AreEqual(false, video.Rewind(5));

            Assert.AreEqual(false, video.Stop());

            video.Control = vlcControl;

            Assert.AreEqual(false, video.Stop());

            video.Src = @"C:\Users\Gabriel\Videos\bird.avi";
            video.Play();
         
//            Assert.AreEqual(false, video.Rewind(5));
            video.FastForward(10);
            Assert.AreEqual(true, video.Rewind(4));
            video.FastForward(5);
            Assert.AreEqual(true, video.Rewind(2));
            Assert.AreEqual(9000, vlcControl.Time);
            

        }


    }
}
