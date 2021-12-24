using SFML.Graphics;
using SFML.System;
using SFML.Window;

using System.Collections.Generic;
using System.Linq;

namespace Game
{
    /// <summary>
    /// Klasa - kontener na obiekty GUI.
    /// </summary>
    public class GUI
    {
        /// <summary>
        /// Główna klasa komponentu.
        /// </summary>
        public class Component
        {
            /// <summary>Typ wyliczeniowy stany komponentu.</summary>
            public enum STATE { IDLE, HOVER, ACTIVE };
            /// <summary>Definicja delegaty funkcji.</summary>
            public delegate void Function();
            /// <summary>Delegata do wystąpienia zdarzenia kliknięcia.</summary>
            public Function onClick;
            /// <summary>Delegata do wystąpienia zdarzenia najechania myszką.</summary>
            public Function onMouseOver;

            /// <summary>Zmienna przechowująca aktualny stan komponentu.</summary>
            public STATE state;
            /// <summary>Zmienna przechowująca poprzedni odczyt klawisza myszy.</summary>
            public bool oldButtonState;
            /// <summary>Wyświetlany tekst komponentu.</summary>
            public SFML.Graphics.Text text;
            /// <summary>Obiekt - kształt komponentu.</summary>
            public RectangleShape shape;
            /// <summary>Zmienna przechowująca aktualny kolor tekstu.</summary>
            public Color textColor;
            /// <summary>Zmienna przechowująca aktualny kolor tła komponentu.</summary>
            public Color shapeColor;

            /// <summary>
            /// Konstruktor - inicjalizacja podstawowych parametrów.
            /// </summary>
            public Component()
            {
                oldButtonState = Mouse.IsButtonPressed(Mouse.Button.Left);
                state = STATE.IDLE;
            }

            /// <summary>
            /// Konstruktor - sparametryzowana inicjalizacja komponentu.
            /// </summary>
            /// <param name="characterSize">Rozmiar wyświetlanego tekstu.</param>
            /// <param name="text">Treść wyświetlanego tekstu.</param>
            /// <param name="font">Referencja do obiektu fontu.</param>
            /// <param name="textColor">Kolor wyświetlanego tekstu.</param>
            /// <param name="shapeColor">Kolor tła komponentu.</param>
            public Component(uint characterSize, string text, Font font, Color textColor, Color shapeColor)
            {
                state = STATE.IDLE;
                oldButtonState = Mouse.IsButtonPressed(Mouse.Button.Left);
                this.textColor = textColor;
                this.shapeColor = shapeColor;
                this.text = new SFML.Graphics.Text(text, font, characterSize);
            }

            /// <summary>
            /// Metoda wirtualna - aktualizacja stanu komponentu, implementacja w rozszerzeniach.
            /// </summary>
            /// <param name="window">Referencja do głównego okna gry.</param>
            public virtual void Update(ref RenderWindow window) { }
            /// <summary>
            /// Metoda wirtualna - rendering komponentu, implementacja w rozszerzeniach.
            /// </summary>
            /// <param name="window">Referencja do głównego okna gry</param>
            public virtual void Render(ref RenderWindow window) { }
        }

        /// <summary>
        /// Klasa przycisku, rozszerzenie możliwości komponentu.
        /// </summary>
        public class Button : Component
        {
            /// <summary>Zmienna stanu odtwarzanego dźwięku przy najechaniu na komponent.</summary>
            bool gainSound;
            /// <summary>
            /// Zmienne kolorów tekstu w zależności od stanu komponentu.
            /// </summary>
            public Color
                textIdleColor,
                textHoverColor,
                textActiveColor;

            /// <summary>
            /// Konstruktor - sparametryzowana inicjalizacja komponentu. 
            /// </summary>
            /// <param name="shapeSize">Wielkość kształtu komponentu.</param>
            /// <param name="centrePos">Pozycja środkowa komponentu.</param>
            /// <param name="characterSize">Rozmiar wyświetlanego tekstu.</param>
            /// <param name="text">Treść wyświetlanego tekstu.</param>
            /// <param name="font">Referencja do obiektu fontu.</param>
            /// <param name="idle">Kolor tekstu w stanie bezczynności.</param>
            /// <param name="hover">Kolor tekstu w stanie najechania myszką.</param>
            /// <param name="active">Kolor tekstu w stanie aktywnym.</param>
            public Button(
                Vector2f shapeSize,
                Vector2f centrePos,
                uint characterSize,
                string text,
                Font font,
                Color idle,
                Color hover,
                Color active
            ) : base(characterSize, text, font, idle, new Color(Color.Transparent))
            {
                gainSound = false;
                textIdleColor = idle;
                textHoverColor = hover;
                textActiveColor = active;
                shape = new RectangleShape(shapeSize)
                {
                    FillColor = new Color(0, 0, 0, 0),
                    Origin = new Vector2f(shapeSize.X / 2f, shapeSize.Y / 2f),
                    Position = new Vector2f(centrePos.X, centrePos.Y)
                };

                this.text = new SFML.Graphics.Text(text, font)
                {
                    CharacterSize = characterSize,
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    FillColor = new Color(idle),
                    OutlineColor = new Color(Color.Black),
                    OutlineThickness = 1f
                };

                Vector2f origin = new Vector2f(0f, 0f)
                {
                    X = this.text.GetGlobalBounds().Width / 2f,
                    Y = shapeSize.Y / 2f
                };
                this.text.Origin = new Vector2f(origin.X, origin.Y);
            }

            /// <summary>
            /// Metoda aktualizująca stan przycisku - implementacja metody z klasy bazowej.
            /// </summary>
            /// <param name="window">Referencja do głównego okna gry.</param>
            public override void Update(ref RenderWindow window)
            {
                Vector2i mousePos = Mouse.GetPosition(window);
                if (shape.GetGlobalBounds().Contains(mousePos.X, mousePos.Y))
                {
                    text.FillColor = new Color(textHoverColor);
                    onMouseOver?.Invoke();
                    if (!gainSound)
                    {
                        Program.resources.sounds["btn_hover"].Play();
                        gainSound = true;
                    }

                    if (Mouse.IsButtonPressed(Mouse.Button.Left))
                    {
                        if (onClick != null)
                            Program.resources.sounds["btn_click"].Play();
                        text.FillColor = new Color(textActiveColor);
                        onClick?.Invoke();
                    }
                }
                else
                {
                    text.FillColor = new Color(textIdleColor);
                    gainSound = false;
                }
            }
            /// <summary>
            /// Metoda renderująca elementy przycisku - implementacja metody z klasy bazowej.
            /// </summary>
            /// <param name="window">Referencja do głównego okna gry.</param>
            public override void Render(ref RenderWindow window)
            {
                window.Draw(shape);
                window.Draw(text);
            }
        }

        /// <summary>
        /// Klasa tekstu, rozszerzenie możliwości komponentu.
        /// </summary>
        public class Text : Component
        {
            /// <summary>
            /// Konstruktor - sprametryzowana inicjalizacja komponentu.
            /// </summary>
            /// <param name="centrePos">Wskazanie centralnej pozycji tekstu.</param>
            /// <param name="characterSize">Rozmiar wyświetlanego tekstu.</param>
            /// <param name="text">Treść wyświetlanego tekstu.</param>
            /// <param name="font">Referencja do obiektu fontu.</param>
            /// <param name="color">Kolor wyświetlanego tekstu.</param>
            public Text(
                Vector2f centrePos,
                uint characterSize,
                string text,
                Font font,
                Color color
            ) : base(characterSize, text, font, color, new Color(Color.Transparent))
            {
                this.text = new SFML.Graphics.Text(text, font)
                {
                    CharacterSize = characterSize,
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    FillColor = new Color(color),
                    OutlineColor = new Color(Color.Black),
                    OutlineThickness = 1f,
                };

                this.shape = new RectangleShape()
                {
                    Size = new Vector2f(this.text.GetGlobalBounds().Width, this.text.GetGlobalBounds().Height),
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    FillColor = new Color(Color.Transparent),
                    Origin = new Vector2f(
                        this.text.GetGlobalBounds().Width / 2f,
                        this.text.GetGlobalBounds().Height / 2f
                    )
                };

                this.text.Origin = new Vector2f(
                    (this.text.GetGlobalBounds().Width + characterSize / 8f) / 2f,
                    (this.text.GetGlobalBounds().Height + characterSize / 2f) / 2f
                );
            }

            /// <summary>
            /// Metoda aktualizująca stan tekstu - wymagana implementacja metody z klasy bazowej.
            /// </summary>
            /// <param name="window">Referencja do głównego okna gry.</param>
            public override void Update(ref RenderWindow window) 
            {
                return;
            }
            /// <summary>
            /// Metoda renderująca elementy komponentu - implementacja metody z klasy bazowej.
            /// </summary>
            /// <param name="window">Referencja do głównego okna gry.</param>
            public override void Render(ref RenderWindow window)
            {
                window.Draw(shape);
                window.Draw(text);
            }
        }

        /// <summary>
        /// Klasa komponentu tekstury, rozszerzenie możliwości komponentu.
        /// </summary>
        public class Texture : Component
        {
            /// <summary>Zmienna stanu odtwarzanego dźwięku przy najechaniu na komponent.</summary>
            bool gainSound;
            /// <summary>Zmienna referencyjna do obiektu tekstury.</summary>
            RectangleShape textureShape;
            /// <summary>
            /// Zmienne kolorów ramki w zależności od stanu komponentu.
            /// </summary>
            Color OutlineIdle, OutlineHover, OutlineActive;

            /// <summary>
            /// Konstruktor - sparametryzowana inicjalizacja komponentu (wyświetlanie pojazdów w menu).
            /// </summary>
            /// <param name="id">Identyfikator wyświetlanego pojazdu.</param>
            /// <param name="carCollection">Kolekcja wszystkich pojazdów.</param>
            /// <param name="shapeSize">Wielkość komponentu.</param>
            /// <param name="textureSize">Wielkość tekstury.</param>
            /// <param name="centrePos">Centralna pozycja komponentu na ekranie.</param>
            /// <param name="OutlineIdle">Kolor ramki w stanie bezczynności.</param>
            /// <param name="OutlineHover">Kolor ramki w stanie najechania myszką.</param>
            /// <param name="OutlineActive">Kolor ramki w stanie aktywnym.</param>
            /// <param name="rotation">Rotacja komponentu.</param>
            public Texture(
                int id,
                List<Entity> carCollection,
                Vector2f shapeSize,
                Vector2f textureSize,
                Vector2f centrePos,
                Color OutlineIdle,
                Color OutlineHover,
                Color OutlineActive,
                float rotation = 0f
            )
            {
                gainSound = false;
                this.OutlineIdle = OutlineIdle;
                this.OutlineHover = OutlineHover;
                this.OutlineActive = OutlineActive;
                shape = new RectangleShape(shapeSize)
                {
                    FillColor = new Color(0, 0, 0, 0),
                    Origin = new Vector2f(shapeSize.X / 2f, shapeSize.Y / 2f),
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    OutlineThickness = 10f,
                    OutlineColor = new Color(OutlineIdle),
                    Rotation = rotation,
                };
                Sprite car = carCollection[id].sprite;
                textureShape = new RectangleShape(textureSize)
                {
                    Origin = new Vector2f(textureSize.X / 2f, textureSize.Y / 2f),
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    Texture = new SFML.Graphics.Texture(car.Texture),
                    TextureRect = new IntRect(
                        car.TextureRect.Left,
                        car.TextureRect.Top,
                        car.TextureRect.Width,
                        car.TextureRect.Height
                    ),
                    Rotation = rotation,
                };
            }

            /// <summary>
            /// Konstruktor - sparametryzowana inicjalizacja komponentu (wyświetlanie napisu NEW HIGH SCORE).
            /// </summary>
            /// <param name="filename">Ścieżka do tekstury.</param>
            /// <param name="textureSize">Wielkość wyświetlanej tekstury.</param>
            /// <param name="centrePos">Pozycja centralna komponentu.</param>
            /// <param name="rotation">Rotacja komponentu.</param>
            public Texture(
                string filename,
                Vector2f textureSize,
                Vector2f centrePos,
                float rotation = 0f
            )
            {
                gainSound = false;
                shape = new RectangleShape();
                textureShape = new RectangleShape(textureSize)
                {
                    Texture = new SFML.Graphics.Texture(filename),
                    Origin = new Vector2f(textureSize.X / 2f, textureSize.Y / 2f),
                    Position = new Vector2f(centrePos.X, centrePos.Y),
                    Rotation = rotation,
                };
            }

            /// <summary>
            /// Metoda aktualizująca stan komponentu - implementacja metody z klasy bazowej.
            /// </summary>
            /// <param name="window">Referencja do głównego okna gry.</param>
            public override void Update(ref RenderWindow window)
            {
                Vector2i mousePos = Mouse.GetPosition(window);
                if (shape.GetGlobalBounds().Contains(mousePos.X, mousePos.Y))
                {
                    shape.OutlineColor = new Color(OutlineHover);
                    onMouseOver?.Invoke();

                    if (!gainSound)
                    {
                        Program.resources.sounds["btn_hover"].Play();
                        gainSound = true;
                    }

                    bool buttonState = Mouse.IsButtonPressed(Mouse.Button.Left);

                    if (buttonState && !oldButtonState)
                    {
                        Program.resources.sounds["btn_click"].Play();
                        shape.OutlineColor = new Color(OutlineActive);
                        onClick?.Invoke();
                    }

                    oldButtonState = buttonState;
                }
                else
                {
                    shape.OutlineColor = new Color(OutlineIdle);
                    gainSound = false;
                }
            }
            /// <summary>
            /// Metoda renderująca elementy komponentu - implementacja metody z klasy bazowej.
            /// </summary>
            /// <param name="window">Referencja do głównego okna gry.</param>
            public override void Render(ref RenderWindow window)
            {
                window.Draw(shape);
                window.Draw(textureShape);
            }
        }

        /// <summary>
        /// Klasa komponentu wpisywania tekstu, rozszerzenie możliwości komponentu.
        /// </summary>
        public class Input : Component
        {
            /// <summary>Zmienna przechowująca maksymalną długość wprowadzonego tekstu.</summary>
            public uint maxLen;
            /// <summary>Słownik z obsługiwanymi klawiszami.</summary>
            public Dictionary<Keyboard.Key, string> keys;
            /// <summary>Słownik ze stanami klawiszy.</summary>
            public Dictionary<Keyboard.Key, bool> keyStates;
            /// <summary>Delegata do wystąpienia zdarzenia potwierdzenia wpisania tekstu.</summary>
            public Function onEnter;

            /// <summary>
            /// Konstruktor - sparametryzowana inicjalizacja komponentu.
            /// </summary>
            /// <param name="position">Pozycja górnego lewego boku.</param>
            /// <param name="characterSize">Rozmiar wyświetlanego tekstu.</param>
            /// <param name="font">Referencja do obiektu fontu.</param>
            /// <param name="textColor">Kolor wyświetlanego tekstu.</param>
            /// <param name="keys">Referencja do słownika obsługiwanych klawiszy.</param>
            /// <param name="maxLen">Maksymalna długość wprowadzanego tekstu.</param>
            public Input(
                Vector2f position,
                uint characterSize, 
                Font font, 
                Color textColor, 
                Dictionary<Keyboard.Key, string> keys,
                uint maxLen = 21
            ) : base(characterSize, null, font, textColor, new Color(Color.Transparent))
            {
                this.maxLen = maxLen;
                keyStates = new Dictionary<Keyboard.Key, bool>();
                this.keys = keys;
                foreach (var key in keys.Keys.ToList())
                    keyStates[key] = true;

                text = new SFML.Graphics.Text(null, font)
                {
                    CharacterSize = characterSize,
                    Position = new Vector2f(position.X, position.Y),
                    FillColor = new Color(textColor),
                    OutlineColor = new Color(Color.Black),
                    OutlineThickness = 1f,
                };
            }

            /// <summary>
            /// Metoda aktualizująca stan komponentu - implementacja metody z klasy bazowej.
            /// </summary>
            /// <param name="window">Referencja do głównego okna gry.</param>
            public override void Update(ref RenderWindow window)
            {
                if (Keyboard.IsKeyPressed(Keyboard.Key.Return))
                    onEnter?.Invoke();

                var keyList = keys.Keys.ToList();

                foreach (var key in keyList)
                {
                    string value = keys[key];

                    bool currKeyState = Keyboard.IsKeyPressed(key);
                    bool oldKeyState = keyStates[key];

                    if (currKeyState && !oldKeyState)
                    {
                        string str = text.DisplayedString;

                        if (value == "BackSpace")
                        {
                            text.DisplayedString = str.Remove(str.Length - 1); ;
                        }
                        else
                        {
                            if (!Keyboard.IsKeyPressed(Keyboard.Key.LShift) &&
                                !Keyboard.IsKeyPressed(Keyboard.Key.RShift))
                                value = value.ToLower();

                            if (str.Length < maxLen)
                                text.DisplayedString = str + value;
                        }
                    }                    
                    keyStates[key] = currKeyState;
                }
            }
            /// <summary>
            /// Metoda renderująca elementy komponentu - implementacja metody z klasy bazowej.
            /// </summary>
            /// <param name="window">Referencja do głównego okna gry.</param>
            public override void Render(ref RenderWindow window)
            {
                window.Draw(text);
            }
        }
    }
}
