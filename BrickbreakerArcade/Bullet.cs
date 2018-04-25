using Microsoft.Xna.Framework;

namespace RandomBricksArcade
{
    class Bullet
    {
        public Color color { get { return Color.OrangeRed; } private set { color = value; } }
        public Rectangle rectangle { get; private set; }
        public Vector2 pos { get; private set; }
        public Vector2 speed { get { return new Vector2(0, -4); } private set {; } }
        int GlobalScale = 1;


        int Width { get { return 2 * GlobalScale; } }
        int Height  { get { return 3 * GlobalScale; } }
    


        public float RoF = 0.25f;

       
        public Bullet()
        {
            pos = Vector2.Zero;
            rectangle = CreateRectangle(pos);
            GlobalScale = 1;
        }

        public Bullet(Vector2 loc, int scale)
        {
            pos = loc;
            rectangle = CreateRectangle(pos);
            GlobalScale = scale;
        }

        Rectangle CreateRectangle(Vector2 pos)
        {
            return new Rectangle((int)pos.X, (int)pos.Y, Width, Height);
        }

        public void Update()
        {
            pos += speed * GlobalScale;
            rectangle = CreateRectangle(pos);
        }
    

    }
}
