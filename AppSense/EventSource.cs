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
using System.Windows.Threading;
using System.Threading;

namespace AppSense
{
    public class EventSource
    {
        public delegate void TickEventHandler();
        public TickEventHandler OnEvent;
        public TickEventHandler OffEvent;

        int OnTime;
        int OffTime;

        DispatcherTimer dt;

        public void Start()
        {
            dt.Start();
        }

        public void Stop()
        {
            dt.Stop();
        }

        public EventSource(int OnTime, int OffTime)
        {
            dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 1, 0);
            dt.Tick += new System.EventHandler(dt_Tick);
            this.OnTime = OnTime;
            this.OffTime = OffTime;
        }
        
        bool isOn = true;
        int count = 0;

        void dt_Tick(object sender, EventArgs e)
        {
            if (isOn)
            {
                if (count >= OnTime)
                {
                    if (OnEvent != null)
                        OnEvent();
                    isOn = false;
                    count = 0;
                }
            }
            else
            {
                if (count >= OffTime)
                {
                    if (OffEvent != null)
                        OffEvent();
                    isOn = true;
                    count = 0;
                }
            }
            count++;

        }

    }
}