using LittleWeebLibrary.Controllers;
using LittleWeebLibrary.GlobalInterfaces;
using System;
using System.Diagnostics;

namespace LittleWeebLibrary
{
    public class LittleWeeb
    {
        private readonly StartUp startUp;
        public LittleWeeb()
        {
            startUp = new StartUp();
            Debug.WriteLine("Starting littleweeb");
            startUp.Start();
        }

        public void Stop()
        {
            startUp.Stop();
        }
    }
}
