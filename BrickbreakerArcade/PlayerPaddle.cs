using Microsoft.Xna.Framework;

namespace RandomBricksArcade
{

    public class PlayerPaddle
    {
        //exact middle of paddle --|--
        public Vector2 CenterPos { get; set; }
        public float CenterPosX { get { return CenterPos.X; } set { CenterPos = new Vector2(value, CenterPos.Y); } }
        public float CenterPosY { get { return CenterPos.Y; } set { CenterPos = new Vector2(CenterPos.X, value); } }

        public float WidthMultiplier = 1;
        //public Texture2D texture;

        Point screenSize;
        int GlobalScale = 1;

        public int Width { get { return (int)((32f * GlobalScale) * WidthMultiplier); } }
        public int HalfWidth { get { return Width / 2; } }

        int Height { get { return 8 * GlobalScale; } }
        public int HalfHeight {get {return Height / 2; } }

        public int Indent { get { return 4 * GlobalScale; } }
        public int Speed { get { return 4 * GlobalScale; } }
        public Rectangle Rectangle
        {
            get
            {
                Vector2 TopLeftCorner = new Vector2(CenterPos.X - HalfWidth, CenterPos.Y - HalfHeight);
                return new Rectangle((int)TopLeftCorner.X, (int)TopLeftCorner.Y, Width, Height);
            }
        }

        public PlayerPaddle(Point screenSize, int scale)
        {
            this.screenSize = screenSize;
            GlobalScale = scale;

            ResetPositionToDefault();

        }

        public void ResetPositionToDefault()
        {
            CenterPosX = screenSize.X / 2;
            CenterPosY = screenSize.Y - Height;

        }


    }
}
