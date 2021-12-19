using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Xml;
using System.Windows.Forms;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Game
{
    public enum DIRECTION { UP, DOWN, RIGHT, LEFT, COUNT };
    public enum TYPE { HEART, COIN, BEER, CAR, COUNT };
    public enum STATE { OPENING, OPEN, CLOSING, CLOSED };

    static class Program
    {
        static public bool loadResorces = false;
        static public RenderWindow window;
        static public Resources resources;
        static public Engine engine;

        static private Stopwatch stopwatch;
        static private float deltaTime = 0f;

        static private void GlobalInitialization()
        {
            RenderWindow initWin = new RenderWindow(new VideoMode(800, 600), "", Styles.None);
            RectangleShape progressBar = new RectangleShape(new Vector2f(20f, 5f));
            Text text = new Text();
            RectangleShape initBackground = new RectangleShape(new Vector2f(initWin.Size.X, initWin.Size.Y));

            try
            {
                InitDeltaTime();
                InitResources();
                text.CharacterSize = 32;
                text.Font = resources.font;
                text.FillColor = new Color(Color.Red);
                progressBar.Position = new Vector2f(0f, initWin.Size.Y - progressBar.Size.Y);
                progressBar.FillColor = new Color(Color.Red);
                initBackground.Texture = new Texture("..\\..\\resource\\images\\background_init.jpg");
            }
            catch (Exception exception)
            {
                ShowError(exception);
            }
            
            while (true)
            {
                initWin.Clear();

                List<string> tList = resources.LoadingStatus();
                tList.Add("Engine");
                progressBar.Size = new Vector2f((4 - tList.Count) * 145f + 20f, 5f);

                text.DisplayedString = "Loading... " + string.Join(", ", tList);
                text.Origin = new Vector2f(text.GetGlobalBounds().Width / 2f, text.GetGlobalBounds().Height / 2f);
                text.Position = new Vector2f(initWin.Size.X / 2f, initWin.Size.Y - 35f);

                initWin.Draw(initBackground);
                initWin.Draw(text);
                initWin.Draw(progressBar);

                initWin.Display();

                if (tList.Count == 1)
                {
                    initWin.Close();
                    CalcElapsedTime();
                    InitWindow();
                    InitEngine();
                    InitWindowEvents();
                    
                    return;
                }
            }
        }

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
            VideoMode mode = new VideoMode(resources.options.winWidth, resources.options.winHeight);
            string title = resources.options.winTitle;

            window = new RenderWindow(mode, title);
            window.SetVerticalSyncEnabled(true);
            window.SetActive(true);
        }

        static private void InitWindowEvents()
        {
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
            GlobalInitialization();

            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear();

                CalcElapsedTime();

                engine.Update(deltaTime, ref window);
                engine.Render(ref window);

                window.Display();
            }
        }
    }
}

