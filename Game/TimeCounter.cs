using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class TimeCounter
    {
        private double currentTime;
        private double eventTime;
        private bool isEvent;
        private bool start;

        public TimeCounter()
        {
            currentTime = 0d;
            eventTime = 0d;
            start = false;
            isEvent = false;
        }

        public void SetEventTime(double eventTime)
        {
            this.eventTime = eventTime;
        }

        public void Start()
        {
            start = true;
        }

        public void Stop()
        {
            start = false;
        }

        public void Update(float dt)
        {
            if (start)
                currentTime += (double)dt;

            if (eventTime > 0d && currentTime >= eventTime)
            {
                currentTime -= eventTime;
                isEvent = true;
            }
        }

        public bool GetEventStatus()
        {
            return isEvent;
        }

        public void ClearEventStatus()
        {
            isEvent = false;
        }

        public void ClearTime()
        {
            currentTime = 0d;
        }

        public double GetCurrentTime()
        {
            return currentTime;
        }
    }
}
