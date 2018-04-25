using Microsoft.Xna.Framework;
using System;

namespace RandomBricksArcade
{
    public class PlayerBall
    {

        public Vector2 CenterPos = new Vector2(0, 0); //start pos
        public int SizeMultiplier = 1;
        public int HeavyBallPass = 0;
        public Vector2 OffSet = new Vector2(0, 7);
        public bool StuckToPaddle = false;
        public float Rotation = 0;
        public PlayerPaddle Paddle;

        int GlobalScale = 1;
        int maxSpeed = 6;

        public Color Color
        {
            get
            {
                if (HeavyBallPass <= 0)
                {
                    return Color.White;
                }
                else return Color.Teal;
            }
        }

        Point screenSize;

        public Vector2 CurrentVelocity { get; set; }
        public float CurrentVelocityX { get { return CurrentVelocity.X; } set { CurrentVelocity = new Vector2(value, CurrentVelocityY); } }
        public float CurrentVelocityY { get { return CurrentVelocity.Y; } set { CurrentVelocity = new Vector2(CurrentVelocity.X, value); } }

        public Vector2 DefaultLevelVelocity { get; private set; }

        float defVelInc = .2f;
        public float VelInc { get { return defVelInc * GlobalScale; } }

        public int Size { get { return (4 * GlobalScale) * SizeMultiplier; } } //its a square!
        public int HalfSize { get { return Size / 2; } }

        public Rectangle Rectangle
        {
            get
            {
                Vector2 TopLeftCorner = new Vector2(CenterPos.X - HalfSize, CenterPos.Y - HalfSize);
                return new Rectangle((int)TopLeftCorner.X, (int)TopLeftCorner.Y, Size, Size);
            }
        }

        public PlayerBall(Point screenSize, int scale, PlayerPaddle playerPaddle)
        {
            this.screenSize = screenSize;
            GlobalScale = scale;
            Paddle = playerPaddle;
        }

        int wobbleCount = 80;

        public void Update(bool testSlowBall, bool slowBall, bool CurveBall)
        {

                if (!testSlowBall && !slowBall)
                {

                    if (StuckToPaddle)
                    {
                        StickToPaddle();
                    }
                    else
                    {
                        CenterPos += CurrentVelocity;
                    //Rotation += .15f;
                    //if (Rotation > 360) Rotation = 0;
                    if (CurveBall)
                    {
                        wobbleCount--;
                        if (wobbleCount > 0)
                        {
                            //wobble ball
                            if (wobbleCount == 20) CurrentVelocity += new Vector2(0, +2);
                            if (wobbleCount == 40) CurrentVelocity += new Vector2(+2, 0);
                            if (wobbleCount == 60) CurrentVelocity += new Vector2(0, -2);
                            if (wobbleCount == 80) CurrentVelocity += new Vector2(-2, 0);

                        }
                        else
                        {
                            wobbleCount = 80;
                        }
                    }
                    
                }

                }
                else if (slowBall)
                {

                    if (StuckToPaddle)
                    {
                        StickToPaddle();
                    }
                    else
                    {
                        CenterPos += CurrentVelocity * 0.5f;
                    }

                }
                else
                {

                    if (StuckToPaddle)
                    {
                    StickToPaddle();
                }
                    else
                    {
                        CenterPos += CurrentVelocity * 0.05f;
                    }

                }
           


        }

        private void StickToPaddle()
        {
            CenterPos = Paddle.CenterPos - OffSet;
            CenterPos.Y -= HalfSize;
        }


        public void UpdateVelocity(int currentLevel)
        {

            //evey level starts with a steadly increasing speed but the speed increase of the ball durring play changes each level set
            DefaultLevelVelocity = new Vector2(GlobalScale + (currentLevel * .25f), -(GlobalScale + (currentLevel * .25f)));
            CurrentVelocity = DefaultLevelVelocity;

            if (currentLevel < 5)
            {
                defVelInc = 0.25f;
            }
            else if (currentLevel > 5 && currentLevel < 10)
            {
                defVelInc = .5f;
            }
            else
            {
                defVelInc = 1f;
            }

            if (CurrentVelocity.X > maxSpeed * GlobalScale)
            {
                DefaultLevelVelocity = new Vector2(maxSpeed * GlobalScale, -maxSpeed * GlobalScale); ;
                CurrentVelocity = DefaultLevelVelocity;
            }

            
        }

        public void ResetVelocityToDefault()
        {
            if (CurrentVelocityX > 0)
            {
                CurrentVelocityX = Math.Abs(DefaultLevelVelocity.X);
            }
            else
            {
                CurrentVelocityX = -Math.Abs(DefaultLevelVelocity.X);
            }

            if (CurrentVelocityY > 0)
            {
                CurrentVelocityY = Math.Abs(DefaultLevelVelocity.Y);
            }
            else
            {
                CurrentVelocityY = -Math.Abs(DefaultLevelVelocity.Y);
            }

        }


        public void IncreaseVelocity()
        {
            if (CurrentVelocityX > 0)
            {
                CurrentVelocityX += VelInc;
            }
            else
            {
                CurrentVelocityX += -VelInc;
            }

            if (CurrentVelocityY > 0)
            {
                CurrentVelocityY += VelInc;
            }
            else
            {
                CurrentVelocityY += -VelInc;
            }

        }
    }

}
