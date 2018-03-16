using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomBricksArcade
{
    class Brick
    {
        #region Feilds
        public Vector2 pos;//
        public Rectangle rectangle;//
        public int health;//
        public static int height { get { return 8; } }//original 8
        public static int width { get { return 16; } }//original 32
        public bool breakable;//


        

        #endregion
        public Brick(float x, float y, int hitsToBreak, bool canBreak)
        {
            pos = new Vector2(x, y);
            health = hitsToBreak;
            breakable = canBreak;
            rectangle = new Rectangle((int)pos.X, (int)pos.Y, width - 1, height - 1);
        }
    }
}
