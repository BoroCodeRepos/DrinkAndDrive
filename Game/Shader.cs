using SFML.Graphics;
using SFML.System;

namespace Game
{
    /// <summary>
    /// Klasa filtru przyciemniającego ekran (do wyświetlenia menu).
    /// </summary>
    class Shader
    {
        /// <summary>Obiekt filtru wyświetlany na ekranie.</summary>
        RectangleShape shader;
        /// <summary>Zmienna określająca aktualny stan filtru.</summary>
        STATE state;
        /// <summary>Zmienna określająca przezroczystość filtru.</summary>
        byte  alpha;
        /// <summary>Zmienna określająca krok przezroczystości filtru.</summary>
        byte  alphaStep;
        /// <summary>Zmienna określająca zadaną przezroczystość filtru.</summary>
        byte  alphaTarget;
        /// <summary>Zmienna określająca czas do wykonania kroku przezroczystości.</summary>
        float timeToAlphaStep;
        /// <summary>Zmienna określająca czas trwania danego stopnia przezroczystości.</summary>
        float time;

        /// <summary>
        /// Konstruktor - inicjalizacja parametrów licznika.
        /// </summary>
        /// <param name="width">Szerokość filtru.</param>
        /// <param name="height">Wysokość filtru.</param>
        /// <param name="alphaTarget">Zadana przezroczystość.</param>
        /// <param name="state">Stan początkowy filtru.</param>
        public Shader(uint width, uint height, byte alphaTarget, bool state = false)
        {
            shader = new RectangleShape(new Vector2f(width, height));
            timeToAlphaStep = 1f;
            alphaStep = 1;
            time = 0f;
            if (state)
            {
                this.state = STATE.OPEN;
                alpha = alphaTarget;
            }
            else
            {
                this.state = STATE.CLOSED;
                alpha = byte.MinValue;
            }
            shader.FillColor = new Color(0, 0, 0, alpha);
        }

        /// <summary>
        /// Metoda wywołująca stan filtru.
        /// </summary>
        /// <param name="state">Wywoływany stan filtru.</param>
        public void SetState(STATE state)
        {
            this.state = state;
        }

        /// <summary>
        /// Metoda ustawiająca parametry filtru.
        /// </summary>
        /// <param name="alphaTarget">Zadana przezroczystość.</param>
        /// <param name="alphaStep">Krok przezroczystości filtru.</param>
        /// <param name="timeToAlphaStep">Czas wykonania kroku przezroczystości.</param>
        public void SetUp(byte alphaTarget, byte alphaStep, float timeToAlphaStep)
        {
            this.alphaTarget = alphaTarget;
            this.alphaStep = alphaStep;
            this.timeToAlphaStep = timeToAlphaStep;
        }

        /// <summary>
        /// Metoda zwracająca wyświetlany obiekt filtru w oknie.
        /// </summary>
        /// <returns>Obiekt filtru.</returns>
        public Shape GetShape()
        {
            return shader;
        }

        /// <summary>
        /// Metoda aktualizująca stan filtru.
        /// </summary>
        /// <param name="dt">Czas od poprzedniego wywołania.</param>
        public void Update(float dt)
        {
            if (state == STATE.OPENING || state == STATE.CLOSING)
            {
                time += dt / 1000f;
                if (time >= timeToAlphaStep)
                {
                    time -= timeToAlphaStep;
                    if (state == STATE.OPENING)
                    {
                        if (alpha >= alphaTarget)
                        {
                            alpha = alphaTarget;
                            state = STATE.OPEN;
                            time = 0f;
                            return;
                        }
                        else
                        {
                            int i = (int)alpha + (int)alphaStep;
                            if (i > alphaTarget) alpha = alphaTarget;
                            else alpha = (byte)i;
                        }
                    }

                    if (state == STATE.CLOSING)
                    {
                        if (alpha == byte.MinValue)
                        {
                            state = STATE.CLOSED;
                            time = 0f;
                            return;
                        }
                        else
                        {
                            int i = (int)alpha - (int)alphaStep;
                            if (i < 0) alpha = 0;
                            else alpha = (byte)i;
                        }
                    }
                    shader.FillColor = new Color(0, 0, 0, alpha);
                }
            }
        }
    }
}
