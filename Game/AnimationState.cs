using SFML.Graphics;
using SFML.System;

namespace Game
{
    public class AnimationState
    {
        public int currentFrameId;
        public float currentFrameTime;

        public int framesNr;
        public float maxFrameTime;

        public IntRect currentFrame;

        public Vector2f position;

        public AnimationState() 
        {
            currentFrameId = 0;
            currentFrameTime = 0f;
        }

        public AnimationState(int framesNr, float maxFrameTime, Vector2f position)
        {
            this.framesNr = framesNr;
            this.maxFrameTime = maxFrameTime;
            this.position = position;

            currentFrameId = 0;
            currentFrameTime = 0f;
            currentFrame = new IntRect(0, 0, Animation.tSize, Animation.tSize);
        }
    }
}
