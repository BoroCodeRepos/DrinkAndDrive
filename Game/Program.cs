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
            resources.Init();
        }

        static private void InitEngine()
        {
            engine = new Engine(resources);
        }

        static private void InitWindow()
        {
            try
            {
                XmlElement root = resources.document.DocumentElement;
                uint width = Convert.ToUInt16(root["window"].Attributes["width"].Value);
                uint height = Convert.ToUInt16(root["window"].Attributes["height"].Value);
                string title = root["window"].Attributes["title"].Value;
                window = new RenderWindow(new VideoMode(width, height), title);
            }
            catch(Exception exception)
            {
                ShowError(exception);
            }

            window.Closed += engine.OnClose;
            window.KeyPressed += engine.OnKeyPressed;
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
                engine.Render(window);

                window.Display();
            } 
        } 
    } 
}

