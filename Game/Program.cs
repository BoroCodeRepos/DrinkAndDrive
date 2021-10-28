using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

using SFML.Graphics;
using SFML.Window;

namespace Game
{
    static class Program
    {
        static float deltaTime = 0f;

        static GameWindow window;
        static GameLogic engine;
        static Stopwatch stopwatch;
        
        static void InitWindow()
        {
            GameWindow window = new GameWindow();
        }

        static void InitEngine()
        {
            engine = new GameLogic();
        }

        static void InitDeltaTime()
        {
            Stopwatch stopwatch = new Stopwatch(); // deltaTime
            stopwatch.Start();
        }

        static void CalcElapsedTime()
        {
            deltaTime = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
        }

        static void Main()
        {
            InitWindow();
            InitEngine();
            InitDeltaTime();
            

            while (window.IsOpen())
            {
                window.DispatchEvents();
                window.Clear();

                CalcElapsedTime();

                engine.Update(deltaTime);
                engine.Render(window);

                window.Display();
            } 
        } 
    } 
}

