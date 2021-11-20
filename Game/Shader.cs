using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Game
{
    class Shader
    {
        private RectangleShape shader;
        private STATE state;
        private byte  alpha;
        private byte  alphaStep;
        private byte  alphaTarget;
        private float timeToAlphaStep;
        private float time;

        public Shader(uint width, uint height, bool state = false)
        {
            shader = new RectangleShape(new Vector2f(width, height));
            this.timeToAlphaStep = timeToAlphaStep;
            this.alphaStep = alphaStep;
            time = 0f;
            if (state)
            {
                this.state = STATE.OPEN;
                alpha = byte.MaxValue;
            }
            else
            {
                this.state = STATE.CLOSED;
                alpha = byte.MinValue;
            }
            shader.FillColor = new Color(0, 0, 0, alpha);
        }

        public STATE GetState()
        {
            return state;
        }

        public void SetState(STATE state)
        {
            this.state = state;
        }

        public void SetUp(byte alphaTarget, byte alphaStep, float timeToAlphaStep)
        {
            this.alphaTarget = alphaTarget;
            this.alphaStep = alphaStep;
            this.timeToAlphaStep = timeToAlphaStep;
        }

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

        public Shape GetShape()
        {
            return shader;
        }
    }
}
