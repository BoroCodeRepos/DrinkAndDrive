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
    public class AnimationState
    {
        public int currentFrameId;
        public float currentFrameTime;

        public int framesNr;
        public float maxFrameTime;

        public IntRect currentFrame;

        public Vector2f currPosition;
        public Vector2f primPosition;

        public AnimationState() { }

        public AnimationState(
            int framesNr,
            float maxFrameTime,
            Vector2f position
        )
        {
            this.framesNr = framesNr;
            this.maxFrameTime = maxFrameTime;

            currentFrameId = 0;
            currentFrameTime = 0f;
            currentFrame = new IntRect(0, 0, Animation.tSize, Animation.tSize);

            currPosition = position;
            primPosition = position;
        }
    }
}
