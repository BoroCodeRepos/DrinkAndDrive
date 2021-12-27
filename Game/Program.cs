using SFML.Graphics;
using SFML.System;
using SFML.Window;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Game
{
    /// <summary>
    /// Typ wyliczeniowy dotyczący kierunków.
    /// </summary>
    public enum DIRECTION { UP, DOWN, RIGHT, LEFT, COUNT };
    /// <summary>
    /// Typ wyliczeniowy dotyczący wyświetlanych obiektów.
    /// </summary>
    public enum TYPE { HEART, COIN, BEER, CAR, COUNT };
    /// <summary>
    /// Typ wyliczeniowy stanów shadera.
    /// </summary>
    public enum STATE { OPENING, OPEN, CLOSING, CLOSED };

    /// <summary>
    /// Główna klasa gry.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// Obiekt z oknem głównym gry.
        /// </summary>
        static public RenderWindow window;
        /// <summary>
        /// Obiekt ze wszystkimi zasobami gry.
        /// </summary>
        static public Resources resources;
        /// <summary>
        /// Obiekt z logiką gry.
        /// </summary>
        static public Engine engine;

        /// <summary>
        /// Obiekt liczący czas do ponownego wywołania.
        /// </summary>
        static private Stopwatch stopwatch;
        /// <summary>
        /// Zmienna przechowująca czas (tick time).
        /// </summary>
        static private float deltaTime = 0f;

        /// <summary>
        /// Metoda inicjalizująca wszystkie obiekty gry.
        /// </summary>
        static private void GlobalInitialization()
        {
            // utworzenie okna inicjalizacyjnego
            RenderWindow initWin = new RenderWindow(new VideoMode(800, 600), "", Styles.None);
            // utworzenie obiektu progres baru
            RectangleShape progressBar = new RectangleShape(new Vector2f(20f, 5f));
            // utworzenie obiektu wyświetlanej informacji o inicjalizowanych zasobach
            Text text = new Text();
            // utworzenie obiektu tła okna inicjalizującego
            RectangleShape initBackground = new RectangleShape(new Vector2f(initWin.Size.X, initWin.Size.Y));

            try
            {
                // próba inicjalizacji obiektów
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
                // bład podczas inicjalizacji jest ukazywany użytkownikowi
                ShowError(exception);
            }
            
            while (true)
            {
                // czyszczenie okna
                initWin.Clear();
                // ładowanie inicjalizujących zasobów
                var tList = resources.LoadingStatus();
                tList.Add("Engine");
                // aktualizacja wypełnienia progresbaru
                progressBar.Size = new Vector2f((4 - tList.Count) * 145f + 20f, 5f);
                // aktualizacja wyświetlanego tekstu
                text.DisplayedString = "Loading... " + string.Join(", ", tList);
                text.Origin = new Vector2f(text.GetGlobalBounds().Width / 2f, text.GetGlobalBounds().Height / 2f);
                text.Position = new Vector2f(initWin.Size.X / 2f, initWin.Size.Y - 35f);
                // wyświetlenie obiektów na ekranie
                initWin.Draw(initBackground);
                initWin.Draw(text);
                initWin.Draw(progressBar);

                initWin.Display();

                if (tList.Count == 1)
                {
                    // inicjalizacja silnika gry i okna głównego
                    initWin.Close();
                    CalcElapsedTime();
                    InitWindow();
                    InitEngine();
                    InitWindowEvents();
                    
                    return;
                }
            }
        }

        /// <summary>
        /// Metoda inicjalizująca zegar do ponownego wywołania programu.
        /// </summary>
        static private void InitDeltaTime()
        {
            stopwatch = new Stopwatch(); // utworzenie nowego obiektu
            stopwatch.Start(); // start timera
        }

        /// <summary>
        /// Metoda inicjalizująca zasoby gry.
        /// </summary>
        static private void InitResources()
        {
            // utworzenie obiektu zasobów i inicjalizacja wątków
            resources = new Resources();
            resources.Init();
        }

        /// <summary>
        /// Metoda inicjalizująca silnik gry.
        /// </summary>
        static private void InitEngine()
        {
            // inicjalizacja silnika gry
            // konieczne wcześniejsze utworzenie zasobów gry
            engine = new Engine(resources);
        }

        /// <summary>
        /// Metoda inicjalizująca okno gry.
        /// </summary>
        static private void InitWindow()
        {
            // utworzenie wielkości ekranu (pobrane z dokumentu XML)
            VideoMode mode = new VideoMode(resources.options.winWidth, resources.options.winHeight);
            // pobranie nazwy okna z dokumentu XML
            string title = resources.options.winTitle;
            // utworzenie okna gry
            window = new RenderWindow(mode, title);
            // inicjalizacja
            window.SetVerticalSyncEnabled(true);
            window.SetActive(true);
        }

        /// <summary>
        /// Metoda inicjalizująca zdarzenia głownego okna gry.
        /// </summary>
        static private void InitWindowEvents()
        {
            // przpisanie metod silnika do zdarzeń okna głównego
            window.Closed += engine.OnClose;
            window.KeyPressed += engine.OnKeyPressed;
            window.KeyReleased += engine.OnKeyReleased;
        }

        /// <summary>
        /// Metoda wyznaczająca czas ponownego wywołania.
        /// </summary>
        static private void CalcElapsedTime()
        {
            // pobranie czasu, jaki upłynął od poprzedniego wywołania
            deltaTime = stopwatch.ElapsedMilliseconds;
            // restart licznika
            stopwatch.Restart();
        }

        /// <summary>
        /// Metoda komunikująca błąd w programie.
        /// </summary>
        /// <param name="exception">Obiekt wskazujący meijsce błędu.</param>
        static public void ShowError(Exception exception)
        {
            // pobranie wiadomości
            string Message = exception.Message;
            // pobranie źródła błędu
            string Source = exception.Source;
            // ustawienie przycisków okna błędu
            MessageBoxButtons Buttons = MessageBoxButtons.OK;
            // pokazanie okna błędu
            MessageBox.Show(Message, Source, Buttons);
            // zamknięcie aplikacji
            Environment.Exit(1);
        }

        /// <summary>
        /// Metoda główna, z pętlą programu i globalną inicjalizacją zasobów.
        /// </summary>
        static void Main()
        {
            // inicjalizacja zasobów gry
            GlobalInitialization();

            // pętla główna programu
            while (window.IsOpen)
            {
                // czyszczenie okna
                window.DispatchEvents();
                window.Clear();

                // wyznaczenie upłyniętego czasu
                CalcElapsedTime();

                // aktualizacja silnika gry
                engine.Update(deltaTime, ref window);
                // rendering elementów w oknie
                engine.Render(ref window);

                // wyświetlenie okna
                window.Display();
            }
        }
    }
}

