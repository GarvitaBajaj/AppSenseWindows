using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Device.Location;
using System.Windows.Threading;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.Generic;

namespace AppSense
{
    public class GPSCode
    {
        LogFormat log = new LogFormat();
        GeoCoordinateWatcher gcw = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
        List<GeoCoordinate> gpsValues = new List<GeoCoordinate>();
        EventSource source = default(EventSource);
        int itCount = 0, count = 0, collectionFrequency = 0, readings = 0, collectionDuration = 0;
        string fileName;
        DispatcherTimer timer = default(DispatcherTimer);
        List<DateTime> time = new List<DateTime>();
        public GPSCode()
        {

        }

        /// <summary>
        /// initializes the GPSCode object
        /// </summary>
        /// <param name="frequency">frequency of collecting GPS data(in milliseconds) </param>
        public void InitializeGPS(int frequency)
        {
            gcw.Start();
            gcw.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(gcw_StatusChanged);
           // gcw.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(gcw_PositionChanged);
            log.WriteLog(gcw.Status.ToString());
            this.collectionFrequency = frequency;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(collectionFrequency);
            timer.Tick += new EventHandler(timer_Tick);
            readings = collectionDuration / (collectionFrequency / 1000);
            timer.Tick += (s, e) => 
            { 
                if (--readings <= 0) 
                {
                    timer.Stop();
                    log.WriteLog("Data Collection Stopped . . .Saving to file");
                    readings = collectionDuration / (collectionFrequency / 1000);
                //    SaveLocationToFile(fileName);
                }
            };
            timer.Start();
           // DoTimer(itCount, collectionFrequency, (s, e) => { });
        }

        /// <summary>
        /// reads the current GPS position
        /// </summary>
        public void ReadPosition()
        {
            DoTimer(timer_Tick);
           // SaveLocationToFile(fileName);
        }

        /// <summary>
        /// save the location coordinate to a file
        /// </summary>
        /// <param name="fileName">the filename</param>
       public void SaveLocationToFile(string fileName)
        {
            using (var gpsData = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (StreamWriter wt = new StreamWriter(new IsolatedStorageFileStream(fileName, FileMode.Append, FileAccess.Write, gpsData)))
                {
                    int timeStamps = 0;
                    foreach (GeoCoordinate g in gpsValues)
                    {
                        wt.Write(time[timeStamps].Hour.ToString() + ":" + time[timeStamps].Minute.ToString() + ":" + time[timeStamps].Second.ToString() + "\t");
                        wt.Write(g.Latitude.ToString("0.000") + "\t" + g.Longitude.ToString("0.000") + "\t" + g.Speed.ToString() + "\r\n");
                        timeStamps++;
                    }
                    wt.Close();
                }
            }
            gpsValues.Clear();
            time.Clear();
        }


        /// <summary>
        /// stops the geoCoordinateWatcher object
        /// </summary>
        public void StopGPS()
        {
            try
            {
                if (gcw.Status == GeoPositionStatus.Ready)
                {
                    gcw.Stop();
                    log.WriteLog("GPS stopped");
                }
            }
            catch (Exception e)
            {
                log.WriteLog(e.Message);
            }
        }

        /// <summary>
        /// release all resources
        /// </summary>
        public void ReleaseGPSResources()
        {
            gcw.Dispose();
            gcw.PositionChanged -= gcw_PositionChanged;
            gcw.StatusChanged -= gcw_StatusChanged;
            log.WriteLog("GPS Resources Released");
        }

        void gcw_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            log.WriteLog("Locating your Position");
            gpsValues.Add(e.Position.Location);
            //throw new NotImplementedException();
        }

        void gcw_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            //throw new NotImplementedException();
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    // location is unsupported on this device
                    log.WriteLog("Location not supported on this device");
                    break;
                case GeoPositionStatus.NoData:
                    // data unavailable
                    log.WriteLog("Location data currently unavailable");
                    break;
                case GeoPositionStatus.Initializing:
                    //initializing data acquisition
                    log.WriteLog("Initializing data collection");
                    break;
            }
        }

        void DoTimer(EventHandler handler)
        {            
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            gpsValues.Add(gcw.Position.Location);
            time.Add(DateTime.Now);
        }

        /// <summary>
        /// collects multiple GPS readings after particular time intervals
        /// </summary>
        /// <param name="samplingFrequency">time between data collection(in minutes)</param>
        /// <param name="collectionDuration">duration of data collection(in minutes)</param>
        /// <param name="count">number of times data is to be collected in chunks</param>
        /// <param name="collectionFrequency">frequency of collecting GPS data(in milliseconds)</param>
        /// <param name="fileName">filename in which GPS readings are to be stored</param>
        public void GetMultipleGPSReadings(int samplingFrequency, int collectionDuration, int count, int collectionFrequency, string fileName)
        {
            this.fileName = fileName;
            this.count = count;
            this.collectionDuration = collectionDuration;
            InitializeGPS(collectionFrequency);
            source = new EventSource(samplingFrequency, collectionDuration);
            source.OnEvent += new EventSource.TickEventHandler(Source_OnEvent);
            source.Start();
            itCount = 0;
          source.OffEvent += new EventSource.TickEventHandler(Source_OffEvent);

        }

        public Stream ReadGPSReadings(string fileName)
        {
            Stream s = File.OpenRead(fileName);
            return s;
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
                ReadPosition();
            }           
            itCount++;
        }

         void Source_OffEvent()
        {          
            SaveLocationToFile(fileName);
            StopGPS();
        }
    }
}

