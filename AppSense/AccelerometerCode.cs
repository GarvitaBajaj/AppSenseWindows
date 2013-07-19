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
using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;
using System.IO.IsolatedStorage;
using System.IO;
using System.Collections.Generic;

namespace AppSense
{
    public class AccelerometerCode
    {
        Accelerometer a;
       // LogFormat log = new LogFormat();
        List<Vector3> xyz = new List<Vector3>();
        List<DateTime> time = new List<DateTime>();
        EventSource evs = default(EventSource);
        int count = 0;
        int itCount = 0;
        string fileName;
        
        public AccelerometerCode()
        {
        }

        /// <summary>
        /// initializes the accelerometer object
        /// </summary>
        /// <param name="frequency">sets the value of TimeBetweenUpdates property in milliseconds</param>
        public void InitializeAcc(int frequency)
        {
            if (!Accelerometer.IsSupported)
            {
       //         log.WriteLog("Your phone does not support Accelerometer Sensor");
            }
            else
            {
                if (a == null)
                {
                    a = new Accelerometer();
                    a.TimeBetweenUpdates = TimeSpan.FromMilliseconds(frequency);
                    a.CurrentValueChanged += (s, e) => { a_CurrentValueChanged(s, e); };
                }
            }
        }

        /// <summary>
        ///starts the accelerometer 
        /// </summary>
        public void StartAcc()
        {
            try
            {
                if (a != null)
                {
                    a.Start();
       //             log.WriteLog("Accelerometer started");
                    //time = DateTime.Now;
                }
            }
            catch (Exception e)
            {
       //         log.WriteLog(e.Message);
            }
        }

        /// <summary>
        /// saves the accelerometer readings and saves them to a file
        /// </summary>
        /// <param name="fileName">the filename</param>
        public void SaveAccReading(string fileName)
        {
            using (var accData = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (StreamWriter wt = new StreamWriter(new IsolatedStorageFileStream(fileName, FileMode.Append, FileAccess.ReadWrite, accData)))
                {
                    int timeStamps=0;
                    foreach (Vector3 accValue in xyz)
                    {
                        wt.Write(time[timeStamps].Hour.ToString() + ":" + time[timeStamps].Minute.ToString() + ":" + time[timeStamps].Second.ToString() + "\t");
                        wt.Write(accValue.X.ToString("0.00") + "\t" + accValue.Y.ToString("0.00") + "\t" + accValue.Z.ToString("0.00") + "\r\n");
                        timeStamps++;
                    }
                    wt.Close();
                }
            }
            xyz.Clear();
            time.Clear();
        }

        void a_CurrentValueChanged(Object sender, SensorReadingEventArgs<AccelerometerReading> sr)
        {
       //     log.WriteLog("Reading acc value . . .");
            xyz.Add(sr.SensorReading.Acceleration);
            time.Add(DateTime.Now);
        }

        /// <summary>
        /// stops the accelerometer
        /// </summary>
        public void StopAcc()
        {
            try
            {
                if (a != null)
                    a.Stop();
            }
            catch (Exception e)
            {
         //       log.WriteLog(e.Message);
            }
        }


        /// <summary>
        /// release all accelerometer resources 
        /// </summary>
        public void ReleaseAccResources()
        {
            if (a != null)
            {
                a.Dispose();
        //        log.WriteLog("Accelerometer Resources Released");
            }
        }


        /// <summary>
        /// gets accelerometer readings at continuous time intervals
        /// </summary>
        /// <param name="samplingFrequency">time between data collection(in minutes)</param>
        /// <param name="collectionDuration">duration of data collection(in minutes)</param>
        /// <param name="count">number of times data is to be collected in chunks</param>
        /// <param name="collectionFrequency">sets the TimeBetweenUpdates prop of accelerometer object(in milliseconds)</param>
        /// <param name="fileName">filename in which readings are to be stored</param>
        public void GetAccReading(int samplingFrequency, int collectionDuration, int count, int collectionFrequency,string fileName)
        {
            this.count = count;
            this.fileName = fileName;
            evs = new EventSource(samplingFrequency, collectionDuration);
            evs.OnEvent += new EventSource.TickEventHandler(Source_OnEvent);
            evs.Start();
            evs.OffEvent += new EventSource.TickEventHandler(Source_OffEvent);
            InitializeAcc(collectionFrequency);
        }

        public static Stream ReadAccReadings(string fileName)
        {
            Stream s = File.OpenRead(fileName);
            return s;
        }

        ////public Stream ReadAccReading(int samplingFrequency, int collectionDuration, int count, int collectionFrequency, string fileName)
        ////{
        ////    Stream s ;
        ////    this.count = count;
        ////    this.fileName = fileName;
        ////    evs = new EventSource(samplingFrequency, collectionDuration);
        ////    evs.OnEvent += new EventSource.TickEventHandler(Source_OnEvent);
        ////    evs.Start();
        ////    evs.OffEvent += new EventSource.TickEventHandler(Source_OffEvent);
        ////    //InitializeAcc(collectionFrequency);
        ////    return s;
        ////}

        void Source_OnEvent()
        {
     //       log.WriteLog("Collecting Data");
            if (itCount >= count)
            {
     //           log.WriteLog("Finished succesfully!");
                evs.Stop();
            }
            else
            {
                StartAcc();
            }
            itCount++;
        }
        
        void Source_OffEvent()
        {
     //       log.WriteLog("Waiting/Saving to file . . .");
            StopAcc();
            SaveAccReading(fileName);
        }
    }

}

