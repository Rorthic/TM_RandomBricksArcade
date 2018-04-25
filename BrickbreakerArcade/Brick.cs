using Microsoft.Xna.Framework;
using StudioForge.TotalMiner;

namespace RandomBricksArcade
{
    class Brick
    {
        #region Feilds
        public Vector2 pos;//
        public Rectangle rectangle;//
        public int health;//
        public Color color;
        int defWidth = 16;
        int defHeight = 8;
        public int Height { get { return (defHeight * GlobalScale) / TPSizeModifier; } }
        public int Width { get { return (defWidth * GlobalScale) / TPSizeModifier; } }
        public bool breakable;//

        public int ItemToMimic = (int) Item.DiamondPickaxe;     
        int border = 2;
        int GlobalScale = 1;
        int TPSizeModifier = 1;

        #endregion
        public Brick(int scale, int modifier)
        {
            GlobalScale = scale;
            TPSizeModifier = modifier;
            pos = Vector2.Zero;
            rectangle = new Rectangle((int)pos.X, (int)pos.Y, Width - border, Height - border);
            health = 1;
            color = Color.White;
            ItemToMimic = (int)Item.DiamondPickaxe;
            breakable = true;
        }
        public Brick(float x, float y, int hitsToBreak, bool canBreak, Color c, int scale, int sizeModifier)
        {
            GlobalScale = scale;
            TPSizeModifier = sizeModifier;
            pos = new Vector2(x, y);
            health = hitsToBreak;
            breakable = canBreak;
            color = c;
            rectangle = new Rectangle((int)pos.X, (int)pos.Y, Width - border, Height - border);
            
            SetDefaultItemMimic();

        }

        public Brick(float x, float y, int hitsToBreak, bool canBreak, int r, int scale)
        {
            GlobalScale = scale;
            pos = new Vector2(x, y);
            health = hitsToBreak;
            breakable = canBreak;
            color = Color.White;

            ItemToMimic = GetItemToMimic(r);

            rectangle = new Rectangle((int)pos.X, (int)pos.Y, Width - border, Height - border);
        }

        public void ResizeBrick(int width)
        {
            defWidth = width;
            rectangle = new Rectangle((int)pos.X, (int)pos.Y, Width - border, Height - border);
        }

        public void SetDefaultItemMimic()
        {
            if (breakable && health == 1)
            {
                ItemToMimic = (int)Item.Basalt;
            }
            else if (breakable && health == 2)
            {
                ItemToMimic = (int)Item.Iron;
            }
            else if (breakable && health == 3)
            {
                ItemToMimic = (int)Item.Diamond;
            }
            else
            {
                ItemToMimic = (int)Item.DiamondPickaxe; 
            }
        }


        private int GetItemToMimic(int row)
        {
            int mimic;

            if (!breakable)
            {
                row = 16; //bedrock
            }
            else if (breakable && health == 2)
            {
                row = 17; //iron

            }
            else if (breakable && health == 3)
            {
                row = 18; //diamond
            }

            switch (row)
            {
                case 0: mimic = (int)Item.Grass; break;
                case 1: mimic = (int)Item.Dirt; break;
                case 2: mimic = (int)Item.Clay; break;
                case 3: mimic = (int)Item.Sandstone; break;
                case 4: mimic = (int)Item.Limestone; break;
                case 5: mimic = (int)Item.Basalt; break;
                case 6: mimic = (int)Item.Andesite; break;
                case 7: mimic = (int)Item.Dacite; break;
                case 8: mimic = (int)Item.Diorite; break;
                case 9: mimic = (int)Item.Tuff; break;
                case 10: mimic = (int)Item.Serpentine; break;
                case 11: mimic = (int)Item.Gabbro; break;
                case 12: mimic = (int)Item.Granite; break;
                case 13: mimic = (int)Item.Komatiite; break;
                case 14: mimic = (int)Item.Marble; break;
                case 15: mimic = (int)Item.Rhyolite; break;
                case 16: mimic = (int)Item.Bedrock; break;
                case 17: mimic = (int)Item.Iron; break;
                case 18: mimic = (int)Item.Diamond; break;
                default: mimic = (int)Item.Gold; break;

            }

            return mimic;

        }
    }
}
