using System;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Devices;
using System.Windows.Threading;
using System.Threading;
using Microsoft.Xna.Framework.Media;
using System.IO.IsolatedStorage;

namespace AppSense
{
    public class CameraCode
    {
        PhotoCamera cam;
        VideoBrush vb;
        LogFormat log = new LogFormat();
        EventSource source = default(EventSource);
        MediaLibrary library = new MediaLibrary();
        MemoryStream captStr = new MemoryStream();

        int savedCounter = 0, itCount = 0,count=0;
        public CameraCode()
        {
            
        }


        /// <summary>
        /// initialize the camera by setting it as the source of the videoBrush oobject
        /// </summary>
        /// <param name="vb">the videoBrush object</param>
       public void InitializeCamera(VideoBrush vb)
        {
            try
            {
                //cam = camx;
                this.vb = vb;                
                cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
                vb.SetSource(cam);
                cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);
                cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_CaptureCompleted);
                cam.CaptureImageAvailable += new EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);

             //   log.WriteLog("Camera Initialization Complete");
            }
            catch(Exception ex)
            {
                log.WriteLog("Exc:" + ex.Message +"\n"+ ex.StackTrace);
            }
        }

        /// <summary>
        /// captures an image
        /// </summary>
        public void CaptureImage()
        {
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true))
            {
               try
                {                   
                    log.WriteLog("Capturing Image");                         
                   cam.CaptureImage();
                }
                catch (Exception ex)
                {
                    log.WriteLog(ex.Message);
                }
            }
            else
            {
                // The camera is not supported on the device.
                log.WriteLog("A Camera is not available on this device.");
            }
        }

        /// <summary>
        /// saves the captured image to isolatedStorage of the device
        /// </summary>
        public void SaveToStorage()
        {
            string fileName = savedCounter + ".jpg";

            // Save picture as JPEG to isolated storage.
           // if (savedCounter != 0)
            {
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // Initialize the buffer for 4KB disk pages.
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        captStr.Seek(0, SeekOrigin.Begin);
                        // Copy the image to isolated storage. 
                        while ((bytesRead = captStr.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            targetStream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }

                // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    log.WriteLog("Picture has been saved to isolated storage.");
                });
            }
        }

        /// <summary>
        /// release all resources
        /// </summary>
        /// <param name="vb">the videoBrush in which the camera screen is visible</param>
              public void ReleaseCameraResources(VideoBrush vb)
        {
            if (cam != null)
            {
                // Dispose camera to minimize power consumption and to expedite shutdown.
                cam.Dispose();
                // Release memory, ensure garbage collection.
                cam.Initialized -= cam_Initialized;
                cam.CaptureCompleted -= cam_CaptureCompleted;
                cam.CaptureImageAvailable -= cam_CaptureImageAvailable;
                log.WriteLog("Camera Resources Released");
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                { vb.SetSource(new MediaElement()); });
            }
        }
      
        void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            if (e.Succeeded)
            {
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    // Write message.
                    log.WriteLog("Camera initialized.");
                });
            }
        }

        void cam_CaptureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            // Increments the savedCounter variable used for generating JPEG file names.
            savedCounter++;
        }


      
        void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {
            
            try
            {   // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    log.WriteLog("Captured image available, saving picture in buffer.");
                });

                //// Save picture to the library camera roll.
                //library.SavePictureToCameraRoll(fileName, e.ImageStream);

                //// Write message to the UI thread.
                //Deployment.Current.Dispatcher.BeginInvoke(delegate()
                //{
                //    log.WriteLog(fileName + "Picture has been saved to camera roll.");

                //});

                //// Set the position of the stream back to start
                e.ImageStream.Seek(0, SeekOrigin.Begin);
                captStr = new MemoryStream();
                e.ImageStream.CopyTo(captStr);
                SaveToStorage();
            }
            finally
            {
                e.ImageStream.Close();
            }

        }

        /// <summary>
        /// captures images at specified time intervals
        /// </summary>
        /// <param name="vb">a videobrush object for the camera</param>
        /// <param name="samplingFrequency">time between image captures(in minutes)</param>
        /// <param name="count">number of times images are to be captured</param>
        public void CaptureImage(VideoBrush vb,int samplingFrequency, int count)
        {
            this.count=count;
            source = new EventSource(samplingFrequency, 0);
            source.OnEvent += new EventSource.TickEventHandler(Source_OnEvent);
            source.Start();            
            source.OffEvent += new EventSource.TickEventHandler(Source_OffEvent);
           InitializeCamera(vb);
        }

        void Source_OnEvent()
        {
            log.WriteLog("Collecting Data");
            if (itCount >= count)
            {
                log.WriteLog("Finished succesfully!");
                source.Stop();
            }
            else
            {
                CaptureImage();
            }
            itCount++;
        }

        void Source_OffEvent()
        {
            log.WriteLog("Waiting/Saving to file . . .");
        //   SaveToStorage();           
        }
    }
}
