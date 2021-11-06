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
    class Animation
    {
        Texture texture;
        Sprite  sprite;
        IntRect rect;
        float primaryTime;
        uint frameId, framesNr;
        bool loop;

        public Animation(
            Texture texture, 
            Sprite  sprite,
            IntRect firstRect,
            float primaryTime,
            uint framesNr,
            bool loop
        )
        {
            frameId = 0;
            this.texture = texture;
            this.sprite = sprite;
            this.primaryTime = primaryTime;
            this.framesNr = framesNr;
            this.loop = loop;
        }

        public void Update(float dt)
        {

        }
    }
}
