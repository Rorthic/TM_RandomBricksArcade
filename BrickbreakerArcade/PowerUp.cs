using Microsoft.Xna.Framework;
using StudioForge.TotalMiner.API;
using System;

namespace RandomBricksArcade
{
    class PowerUp
    {
        public enum PowerUpType
        {
            NONE,
            WIDE_PADDLE,
            STICKY_BALL,
            SLOW_BALL,
            BIG_BALL,
            //no timed ones
            HEAVY_BALL,
            TRIPLE_BALLS,
            RESET_SPEED,
            BULLETS,
            EXTRA_LIFE,
            //power downs timed
            NARROW_PADDLE,
            INVERT_CONTROLS,
            RANDOM_BOUNCE,
            CURVE_BALL,
            //non timed
            INCREASE_SPEED,




        }
        public float Timer { get; private set; }
        public Rectangle Rectangle { get; private set; }
        public PowerUpType Type { get; }
        public Color Color { get; private set; }

        public bool DestroyMe { get; private set; }
        public int CurAnimationFrame = 0;

        int GlobalScale = 1;
        int defHeight = 8;
        int defWidth = 12;
        int Height { get { return defHeight * GlobalScale; } } //4
        int Width { get { return defWidth * GlobalScale; } } //8
        int frameChange = 5;
        bool grow = true;




        Vector2 Loc = Vector2.Zero;
        Vector2 myVel = new Vector2(0, 1);

        public PowerUp(Vector2 pos, int scale)
        {
            GlobalScale = scale;
            Type = RandomType();
            Loc = pos;
            Timer = 0;

            SetDefaults();
        }

        private void SetDefaults()
        {
            myVel = new Vector2(0, 1 * GlobalScale);
            Rectangle = MovePowerUp(Vector2.Zero);
            //SetColorByType();
            DestroyMe = false;

        }

        public void Update()
        {
            MovePowerUp(myVel);
            frameChange--;
            if (frameChange < 0)
            {
                if (grow)
                {
                    defWidth++;
                    defHeight++;
                }
                else
                {
                    defWidth--;
                    defHeight--;
                }
                if (defHeight >= 8)
                {
                    grow = false;
                }
                if (defHeight <= 7)
                {
                    grow = true;
                }
                frameChange = 10;
            }


        }

        public PowerUpType PowerUpCollected()
        {
            //Call down to the timer method to do the right stuff
            this.SetToDestroy();
            return Type;
        }

        public Vector2 Location()
        {
            return Loc;
        }

        private Rectangle MovePowerUp(Vector2 vel)
        {
            Loc.X = Loc.X + vel.X;
            Loc.Y = Loc.Y + vel.Y;
            Rectangle = new Rectangle((int)Loc.X, (int)Loc.Y, Width, Height);
            return Rectangle;
        }

        public void SetToDestroy()
        {
            DestroyMe = true;
        }

        PowerUpType RandomType()
        {
            //spawn a random power up
            var rnd = BrickbreakerGame.Random;
            PowerUpType type = PowerUpType.NONE;
            switch (rnd.Next(0, 17))
            {
                
                case 0: type = PowerUpType.WIDE_PADDLE; break;
                case 1: type = PowerUpType.STICKY_BALL; break;
                case 2: type = PowerUpType.SLOW_BALL; break;
                case 3: type = PowerUpType.BIG_BALL; break;
                case 4: type = PowerUpType.HEAVY_BALL; break;
                case 5: type = PowerUpType.TRIPLE_BALLS; break;
                case 6: type = PowerUpType.RESET_SPEED; break;
                case 7: type = PowerUpType.BULLETS; break;
                case 8: type = PowerUpType.EXTRA_LIFE; break;
                case 9: type = PowerUpType.NARROW_PADDLE; break;
                case 10: type = PowerUpType.INVERT_CONTROLS; break;
                case 11: type = PowerUpType.RANDOM_BOUNCE; break;
                case 12: type = PowerUpType.INCREASE_SPEED; break;
                case 13: type = PowerUpType.BULLETS; break;
                case 14: type = PowerUpType.TRIPLE_BALLS; break;
                case 15: type = PowerUpType.HEAVY_BALL; break;
                case 16: type = PowerUpType.CURVE_BALL; break;
                default: type = PowerUpType.NONE; break;

            }
            
            return type;
             //return (PowerUpType)rnd.Next(Enum.GetNames(typeof(PowerUpType)).Length);
            //return PowerUpType.STICKY_BALL;
        }


    }

    class PowerUpTracker
    {
        ITMGame game;
        BrickbreakerGame brickbreaker;

        public PowerUpTracker(ITMGame g, BrickbreakerGame bk)
        {
            game = g;
            brickbreaker = bk;
        }

        float Wide_Paddle_Timer = 0;
        float Wide_Paddle_Default_Time = 25;
        float Sticky_Ball_Timer = 0;
        float Sticky_Ball_Default_Time = 25;
        float Slow_Ball_Timer = 0;
        float Slow_Ball_Default_Time = 25;
        float Big_Ball_Timer = 0;
        float Big_Ball_Default_Time = 25;
        //power downs
        float Narrow_Paddle_Timer = 0;
        float Narrow_Paddles_Default_Time = 25;
        float Invert_Controls_Timer = 0;
        float Invert_Controls_Default_Time = 25;
        float Random_Bounce_Timer = 0;
        float Random_Bounce_Default_Time = 25;
        float Curve_Ball_Timer = 0;
        float Curve_Ball_Default_Time = 25;

        bool Wide_Paddle_Timer_Active = false;
        bool Sticky_Ball_Timer_Active = false;
        bool Slow_Ball_Timer_Active = false;
        bool Big_Ball_Timer_Active = false;
        bool Curve_Ball_Timer_Active = false;
        //power downs
        bool Narrow_Paddle_Timer_Active = false;
        bool Invert_Controls_Timer_Active = false;
        bool Random_Bounce_Timer_Active = false;

        int defaultBulletAmount = 20;
        float defaultSpeedIncrease = .2f;

        public void PowerUpCollected(PowerUp.PowerUpType type)
        {
            //do the collection code here
            switch (type)
            {
                case PowerUp.PowerUpType.WIDE_PADDLE: WidePaddleCollected(); break;
                case PowerUp.PowerUpType.STICKY_BALL: StickyBallCollected(); break;
                case PowerUp.PowerUpType.SLOW_BALL: SlowBallCollected(); break;
                case PowerUp.PowerUpType.BIG_BALL: BigBallCollected(); break;
                //non timed ones
                case PowerUp.PowerUpType.HEAVY_BALL: HeavyBallCollected(); break;
                case PowerUp.PowerUpType.EXTRA_LIFE: ExtraLifeCollected(); break;
                case PowerUp.PowerUpType.TRIPLE_BALLS: TrippleBallsCollected(); break;
                case PowerUp.PowerUpType.RESET_SPEED: ResetSpeedCollected(); break;
                case PowerUp.PowerUpType.BULLETS: BulletsCollected(); break;
                //power downs timed
                case PowerUp.PowerUpType.NARROW_PADDLE: NarrowPaddlesCollected(); break;
                case PowerUp.PowerUpType.INVERT_CONTROLS: InvertControlsCollected(); break;
                case PowerUp.PowerUpType.RANDOM_BOUNCE: RandomBounceCollected(); break;
                case PowerUp.PowerUpType.CURVE_BALL: CurveBallCollected(); break;
                //non timed
                case PowerUp.PowerUpType.INCREASE_SPEED: IncreaseSpeedCollected(); break;


            }
        }


        public int ScoreValue(PowerUp.PowerUpType type)
        {
            switch (type)
            {
                case PowerUp.PowerUpType.WIDE_PADDLE: return 1;
                case PowerUp.PowerUpType.STICKY_BALL: return 1;
                case PowerUp.PowerUpType.SLOW_BALL: return 1; ;
                case PowerUp.PowerUpType.BIG_BALL: return 1;
                //non timed ones
                case PowerUp.PowerUpType.HEAVY_BALL: return 1;
                case PowerUp.PowerUpType.EXTRA_LIFE: return 1;
                case PowerUp.PowerUpType.TRIPLE_BALLS: return 1;
                case PowerUp.PowerUpType.RESET_SPEED: return 1;
                case PowerUp.PowerUpType.BULLETS: return 1;
                //power downs timed
                case PowerUp.PowerUpType.NARROW_PADDLE: return 10;
                case PowerUp.PowerUpType.INVERT_CONTROLS: return 20;
                case PowerUp.PowerUpType.RANDOM_BOUNCE: return 5;
                case PowerUp.PowerUpType.CURVE_BALL: return 25;
                //non timed
                case PowerUp.PowerUpType.INCREASE_SPEED: return 10;

            }
            return 0;
        }





        #region PowerUpCollected()

        private void IncreaseSpeedCollected()
        {
            brickbreaker.IncreaseAllBallSpeed(defaultSpeedIncrease);
        }

        private void RandomBounceCollected()
        {
            brickbreaker.RandomBounce = true;
            Random_Bounce_Timer_Active = true;
            Random_Bounce_Timer += Random_Bounce_Default_Time;
        }

        private void CurveBallCollected()
        {
            brickbreaker.CurveBall = true;
            Curve_Ball_Timer_Active = true;
            Curve_Ball_Timer += Curve_Ball_Default_Time;
        }

        private void InvertControlsCollected()
        {
            brickbreaker.InvertControls = true;
            Invert_Controls_Timer_Active = true;
            Invert_Controls_Timer += Invert_Controls_Default_Time;
        }

        private void NarrowPaddlesCollected()
        {

            Narrow_Paddle_Timer_Active = true;
            Narrow_Paddle_Timer += Narrow_Paddles_Default_Time;
            brickbreaker.Paddle.WidthMultiplier /= 2;
        }

        void BulletsCollected()
        {
            brickbreaker.bulletsRemain += defaultBulletAmount;
        }

        void ResetSpeedCollected()
        {
            brickbreaker.resetBallVel = true;
        }

        void ExtraLifeCollected()
        {
            brickbreaker.AddExtraLife();
        }

        void HeavyBallCollected()
        {
            brickbreaker.AddToHeavyBallCount(3);
        }

        void SlowBallCollected()
        {
            Slow_Ball_Timer_Active = true;
            Slow_Ball_Timer += Slow_Ball_Default_Time;
            brickbreaker.slowBall = true;
        }
        void StickyBallCollected()
        {
            Sticky_Ball_Timer_Active = true;
            Sticky_Ball_Timer += Sticky_Ball_Default_Time;
            brickbreaker.stickToPaddle = true;
        }

        void WidePaddleCollected()
        {
            Wide_Paddle_Timer_Active = true;
            Wide_Paddle_Timer += Wide_Paddle_Default_Time;
            brickbreaker.Paddle.WidthMultiplier += 1;
        }
        private void BigBallCollected()
        {
            Big_Ball_Timer_Active = true;
            Big_Ball_Timer += Big_Ball_Default_Time;
            for (int i = 0; i < brickbreaker.Balls.Count; i++)
            {
                brickbreaker.Balls[i].SizeMultiplier += 1;
            }
        }

        private void TrippleBallsCollected()
        {
            brickbreaker.SpawnNewBalls(3);
        }

        #endregion

        #region TimerCompleted()
        void Wide_Paddle_Timer_Completed()
        {
            Wide_Paddle_Timer = 0;
            Wide_Paddle_Timer_Active = false;
            if (brickbreaker.Paddle.WidthMultiplier > 1) //paddle hasnt been shrunk
            {
                brickbreaker.Paddle.WidthMultiplier = 1;
            }
        }
        void Sticky_Ball_Timer_Completed()
        {
            Sticky_Ball_Timer = 0;
            Sticky_Ball_Timer_Active = false;
            brickbreaker.stickToPaddle = false;
        }
        void Slow_Ball_Timer_Completed()
        {
            Slow_Ball_Timer = 0;
            Slow_Ball_Timer_Active = false;
            brickbreaker.slowBall = false;
        }
        void Big_Ball_Timer_Completed()
        {
            Big_Ball_Timer = 0;
            Big_Ball_Timer_Active = false;
            for (int i = 0; i < brickbreaker.Balls.Count; i++)
            {
                brickbreaker.Balls[i].SizeMultiplier = 1;
            }
        }
        //power downs
        private void Random_Bounce_Timer_Completed()
        {
            Random_Bounce_Timer = 0;
            Random_Bounce_Timer_Active = false;
            brickbreaker.RandomBounce = false;
        }

        private void Invert_Controls_Timer_Completed()
        {
            Invert_Controls_Timer = 0;
            Invert_Controls_Timer_Active = false;
            brickbreaker.InvertControls = false;
        }

        private void Narrow_Paddle_Timer_Completed()
        {
            Narrow_Paddle_Timer = 0;
            Narrow_Paddle_Timer_Active = false;
            if (brickbreaker.Paddle.WidthMultiplier < 1) //no size increase so set to default
            {
                brickbreaker.Paddle.WidthMultiplier = 1;
            }

        }

        private void Curve_Ball_Timer_Completed()
        {
            Curve_Ball_Timer = 0;
            Curve_Ball_Timer_Active = false;
            brickbreaker.CurveBall = false;
            brickbreaker.resetBallVel = true;

        }

        #endregion

        public void DecreaseTimers(float time)
        {
            if (Wide_Paddle_Timer_Active)
            {
                Wide_Paddle_Timer -= time;
            }
            if (Sticky_Ball_Timer_Active)
            {
                Sticky_Ball_Timer -= time;
            }
            if (Slow_Ball_Timer_Active)
            {
                Slow_Ball_Timer -= time;
            }
            if (Big_Ball_Timer_Active)
            {
                Big_Ball_Timer -= time;
            }
            //power downs
            if (Narrow_Paddle_Timer_Active)
            {
                Narrow_Paddle_Timer -= time;
            }
            if (Invert_Controls_Timer_Active)
            {
                Invert_Controls_Timer -= time;
            }
            if (Random_Bounce_Timer_Active)
            {
                Random_Bounce_Timer -= time;
            }
            if (Curve_Ball_Timer_Active)
            {
                Curve_Ball_Timer -= time;
            }



            if (Wide_Paddle_Timer < 0)
            {
                Wide_Paddle_Timer_Completed();
            }
            if (Sticky_Ball_Timer < 0)
            {
                Sticky_Ball_Timer_Completed();
            }
            if (Slow_Ball_Timer < 0)
            {
                Slow_Ball_Timer_Completed();
            }
            if (Big_Ball_Timer < 0)
            {
                Big_Ball_Timer_Completed();
            }
            //power downs
            if (Narrow_Paddle_Timer < 0)
            {
                Narrow_Paddle_Timer_Completed();
            }
            if (Invert_Controls_Timer < 0)
            {
                Invert_Controls_Timer_Completed();
            }
            if (Random_Bounce_Timer < 0)
            {
                Random_Bounce_Timer_Completed();
            }
            if (Curve_Ball_Timer < 0)
            {
                Curve_Ball_Timer_Completed();
            }

        }


        public void ResetPowerUpTimers()
        {

            Slow_Ball_Timer_Completed();
            Sticky_Ball_Timer_Completed();
            Wide_Paddle_Timer_Completed();
            Big_Ball_Timer_Completed();
            Narrow_Paddle_Timer_Completed();
            Invert_Controls_Timer_Completed();
            Random_Bounce_Timer_Completed();
            Curve_Ball_Timer_Completed();
        }
    }
}
