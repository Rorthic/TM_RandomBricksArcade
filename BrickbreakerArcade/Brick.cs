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
        public Color color;
        public static int height { get { return 8; } }//original 8
        public static int width { get { return 16; } }//original 32
        public bool breakable;//
        //public int cHeight;//custom height
        //public int cWidth;//custom width


        

        #endregion
        public Brick()
        {

        }
        public Brick(float x, float y, int hitsToBreak, bool canBreak, Color c)
        {
            pos = new Vector2(x, y);
            health = hitsToBreak;
            breakable = canBreak;
            color = c;
            rectangle = new Rectangle((int)pos.X, (int)pos.Y, width - 1, height - 1);
        }

        public Brick(float x, float y, int hitsToBreak, bool canBreak, Color c, int ch, int cw)
        {
            pos = new Vector2(x, y);
            health = hitsToBreak;
            breakable = canBreak;
            color = c;
            rectangle = new Rectangle((int)pos.X, (int)pos.Y, cw - 1, ch - 1);
        }
    }
}
