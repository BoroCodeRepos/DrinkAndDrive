using SFML.Graphics;
using SFML.System;

using System.Collections.Generic;

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

        static public void Update(float dt)
        {
            List<AnimationState> toDelete = new List<AnimationState>();
            foreach (var state in states)
            {
                state.currentFrameTime += dt;
                if (state.currentFrameTime > state.maxFrameTime)
                {
                    state.currentFrameTime -= state.maxFrameTime;
                    if (++state.currentFrameId >= state.framesNr)
                        toDelete.Add(state);
                    state.currentFrame = new IntRect(tSize * state.currentFrameId, 0, tSize, tSize);
                }
            }

            foreach (var state in toDelete)
                states.Remove(state);
        }

        static public void Render(ref RenderWindow window)
        {
            Sprite s = new Sprite(texture);
            foreach (var state in states)
            {
                s.Position = new Vector2f(state.position.X, state.position.Y);
                s.Origin = new Vector2f(tSize / 2f, tSize / 2f);
                s.TextureRect = state.currentFrame;
                window.Draw(s);
            }
        }
    }
}
