using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SFML.Graphics;
using SFML.Window;
using SFML.System;

namespace Game
{
    static public class Animation
    {
        static public Texture texture;
        static public float primaryTime;
        static public int tSize;

        static public List<AnimationState> states;

        static public void Create(Vector2f position)
        {
            states.Add(new AnimationState(12, 65f, position));
        }

        static public void Update(float dt, float offsetX)
        {
            foreach (var state in states)
            {
                state.currentFrameTime += dt;
                if (state.currentFrameTime > state.maxFrameTime)
                {
                    state.currentFrameTime -= state.maxFrameTime;
                    state.currentFrameId = (state.currentFrameId + 1) % state.framesNr;
                    state.currentFrame = new IntRect(tSize * state.currentFrameId, 0, tSize, tSize);
                    state.currPosition.X = state.primPosition.X + offsetX;
                }
            }
        }

        static public void Render(ref RenderWindow window)
        {
            Sprite s = new Sprite(texture);
            foreach (var state in states)
            {
                s.Position = new Vector2f(state.currPosition.X, state.currPosition.Y);
                s.TextureRect = state.currentFrame;
                s.Origin = new Vector2f(tSize / 2f, tSize / 2f);
                window.Draw(s);
            }
        }
    }
}
