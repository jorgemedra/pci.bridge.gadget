using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace OD_Finesse_Bridge.utils
{
    class Cronos
    {
        public delegate void Finish();
        public event Finish onFinished;
        private int seconds;
        public bool alive;
        
        public Cronos(int seconds)
        {
            this.seconds = seconds;
            alive = false;
        }

        protected virtual void onFinish()
        {
            alive = false;
            onFinished();
        }

        public void start()
        {
            int count = 0;
            alive = true;
            while (count < seconds)
            {
                if (!alive)
                {
                    count = seconds - 1;
                }
                Thread.Sleep(1000);
                count++;
            }
            onFinish();
        }

        public void stop()
        {
            alive = false;
        }


    }
}
