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
        private bool run;

        public TimeCounter()
        {
            currentTime = 0d;
            eventTime = 0d;
            run = false;
            isEvent = false;
        }

        public TimeCounter(double evTime)
        {
            currentTime = 0d;
            eventTime = evTime;
            run = false;
            isEvent = false;
        }

        public void SetEventTime(double eventTime)
        {
            this.eventTime = eventTime;
        }

        public void Start()
        {
            run = true;
        }

        public void Stop()
        {
            run = false;
        }

        public void Update(float dt)
        {
            if (run)
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

        public bool IsRun()
        {
            return run;
        }
    }
}
