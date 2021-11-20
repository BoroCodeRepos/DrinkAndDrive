using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.Windows.Forms;

using SFML.Graphics;
using SFML.Window;

namespace Game
{
    public enum DIRECTION { UP, DOWN, RIGHT, LEFT, COUNT };
    public enum TYPE { HEART, COIN, BEER, CAR, COUNT };
    public enum STATE { OPENING, OPEN, CLOSING, CLOSED };

    static class Program
    {
        static private float deltaTime = 0f;
        static private RenderWindow window;
        static private Engine engine;
        static public  Resources resources;
        static private Stopwatch stopwatch;

        static private void InitDeltaTime()
        {
            stopwatch = new Stopwatch(); // deltaTime
            stopwatch.Start();
        }

        static private void InitResources()
        {
            resources = new Resources();
        }

        static private void InitEngine()
        {
            engine = new Engine(resources);
        }

        static private void InitWindow()
        {
            VideoMode mode = new VideoMode(resources.options.winWidth, resources.options.winHeight);
            string title = resources.options.winTitle;

            window = new RenderWindow(mode, title);
            window.SetVerticalSyncEnabled(true);
            
            window.Closed += engine.OnClose;
            window.KeyPressed += engine.OnKeyPressed;
            window.KeyReleased += engine.OnKeyReleased;
        }

        static private void CalcElapsedTime()
        {
            deltaTime = stopwatch.ElapsedMilliseconds;
            stopwatch.Restart();
        }

        static public void ShowError(Exception exception)
        {
            string Message = exception.Message;
            string Source = exception.Source;
            MessageBoxButtons Buttons = MessageBoxButtons.OK;
            MessageBox.Show(Message, Source, Buttons);
            Environment.Exit(1);
        }

        static void Main()
        {
            InitDeltaTime();
            InitResources();
            InitEngine();
            InitWindow();
            
            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear();

                CalcElapsedTime();

                engine.Update(deltaTime);
                engine.Render(ref window);

                window.Display();
            } 
        } 
    } 
}

