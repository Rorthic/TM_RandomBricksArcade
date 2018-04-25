using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StudioForge.Engine;
using StudioForge.Engine.Core;
using StudioForge.Engine.Integration;
using StudioForge.TotalMiner;
using System;
using System.IO;

namespace RandomBricksArcade
{
    struct AnimationFrame
    {
        public Rectangle[] Rect;
        // public SpriteEffects Effects;
    }

    class BrickbreakerRenderer : IArcadeMachineRenderer
    {
        #region Fields

        BrickbreakerGame brickBreaker;
        SpriteFont font;
        SpriteBatchSafe spriteBatch;

        //animations
        AnimationFrame[] SourceRect;
        Texture2D SpriteSheet;
        Rectangle ballSprite;

        int count = 0;

        int drawScale;

        #endregion

        #region Initialization

        public void LoadContent(InitState state)
        {
            spriteBatch = CoreGlobals.SpriteBatch;
            font = CoreGlobals.GameFont;


            using (var stream = File.OpenRead(FileSystem.RootPath + BrickbreakerMod.Path + "BrickBreakerTP.png")) SpriteSheet = Texture2D.FromStream(CoreGlobals.GraphicsDevice, stream);

            //24x16 images
            SourceRect = new AnimationFrame[15];
            SourceRect[(int)PowerUp.PowerUpType.WIDE_PADDLE] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(0, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.STICKY_BALL] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(24, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.BIG_BALL] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(48, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.HEAVY_BALL] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(72, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.TRIPLE_BALLS] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(96, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.RESET_SPEED] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(120, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.BULLETS] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(144, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.EXTRA_LIFE] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(168, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.SLOW_BALL] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(192, 0, 24, 16) } };

            SourceRect[(int)PowerUp.PowerUpType.NARROW_PADDLE] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(216, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.INVERT_CONTROLS] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(240, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.RANDOM_BOUNCE] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(264, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.INCREASE_SPEED] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(288, 0, 24, 16) } };
            SourceRect[(int)PowerUp.PowerUpType.CURVE_BALL] = new AnimationFrame() { Rect = new Rectangle[] { new Rectangle(312, 0, 24, 16) } };

            //8x8 image
            ballSprite = new Rectangle(336, 0, 8, 8);

        }

        public void UnloadContent()
        {
        }

        void IArcadeMachineRenderer.LoadTexturePack()
        {
        }
        #endregion



        #region Draw


        public void Draw(ArcadeMachine baseGame)
        {
            brickBreaker = baseGame as BrickbreakerGame;
            if (brickBreaker == null) return;

            drawScale = brickBreaker.GlobalScale;
            CoreGlobals.GraphicsDevice.SetRenderTarget(brickBreaker.RenderTarget);
            CoreGlobals.GraphicsDevice.Clear(Color.Black);


            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, DepthStencilState.None, null);

            if (brickBreaker.State == BrickbreakerGame.GameState.GameOver)
            {
                DrawGameOver();
                DrawHud();
            }
            else
            {
                DrawPlay();
                DrawHud();
            }

            spriteBatch.End();
        }

        void DrawPlay()
        {
            //draw paddle
            spriteBatch.Draw(brickBreaker.texturePack.GetTexureForItem(Item.DiamondPickaxe), brickBreaker.Paddle.Rectangle, brickBreaker.texturePack.ItemSrcRect(Item.DiamondPickaxe), Color.White);
            //spriteBatch.Draw(CoreGlobals.BlankTexture, new Rectangle((int)brickBreaker.Paddle.CenterPos.X, (int)brickBreaker.Paddle.CenterPos.Y - brickBreaker.Paddle.HalfHeight / 2, 1, 1), Color.Red);
            //draw balls
            for (int i = 0; i < brickBreaker.Balls.Count; i++)
            {
                spriteBatch.Draw(SpriteSheet, brickBreaker.Balls[i].Rectangle, ballSprite, brickBreaker.Balls[i].Color);
              //  spriteBatch.Draw(CoreGlobals.BlankTexture, new Rectangle((int)brickBreaker.Balls[i].CenterPos.X, (int)brickBreaker.Balls[i].CenterPos.Y, 1, 1), Color.Red);

            }

            if (brickBreaker.useRandomLayout)
            {
                DrawBricksWithTexture();
            }
            else
            {
                DrawBricksNonTextured();
            }

            DrawPowerUps();
            DrawBullets();

            if (brickBreaker.messageToPlayer != string.Empty)
            {
                spriteBatch.DrawStringCentered(font, brickBreaker.messageToPlayer, 20 * drawScale, Color.Green, 0.4f);
            }

            //var rect = brickBreaker.Paddle.Rectangle;
            //rect.Width = (int)(brickBreaker.Paddle.Rectangle.Width * 0.2f);
            //spriteBatch.Draw(CoreGlobals.BlankTexture, rect, Color.White);
            //rect.X += rect.Width;
            //rect.Width = (int)(brickBreaker.Paddle.Rectangle.Width * 0.2f);
            //spriteBatch.Draw(CoreGlobals.BlankTexture, rect, Color.Red);
            //rect.X += rect.Width;
            //rect.Width = (int)(brickBreaker.Paddle.Rectangle.Width * 0.2f);
            //spriteBatch.Draw(CoreGlobals.BlankTexture, rect, Color.Green);
            //rect.X += rect.Width;
            //rect.Width = (int)(brickBreaker.Paddle.Rectangle.Width * 0.2f);
            //spriteBatch.Draw(CoreGlobals.BlankTexture, rect, Color.Blue);
            //rect.X += rect.Width;
            //rect.Width = (int)(brickBreaker.Paddle.Rectangle.Width * 0.2f);
            //spriteBatch.Draw(CoreGlobals.BlankTexture, rect, Color.Yellow);

            if (brickBreaker.State == BrickbreakerGame.GameState.GameOverTransition)
            {
                FlashScreen(Color.Black, new Color(15, 0, 0), 10);
                spriteBatch.DrawStringCentered(font, "GAME OVER ", 175, Color.Red, 1f);
                spriteBatch.DrawStringCentered(font, brickBreaker.ScoreText1, 225, Color.Green, 1f);

                if(brickBreaker.BeatHighScore())
                {
                    spriteBatch.DrawStringCentered(font, "Congrats New High Score" , 255, Color.Green, 1f);
                }

            }

            if (brickBreaker.pauseGame)
            {
                spriteBatch.DrawStringCentered(font, "PAUSE", 200, Color.Green, 2f);
            }

            if (!brickBreaker.playerAlive)
            {
                FlashScreen(Color.Black, new Color(15, 0, 0), 10);
                spriteBatch.DrawStringCentered(font, "Remaining " + brickBreaker.LivesText, 200, Color.Green, 1f);
            }

            if (brickBreaker.loadingNewLevel)
            {
                FlashScreen(Color.Black, new Color(0, 15, 0), 10);
                spriteBatch.DrawStringCentered(font, "Level complete.", 175, Color.AntiqueWhite, 1f);
                if (!brickBreaker.useRandomLayout) {
                    spriteBatch.DrawStringCentered(font, "Loading: " + brickBreaker.levelItem.ToString() , 225, Color.AntiqueWhite, 1f);
                }
            }
        }

        private void FlashScreen(Color colorA, Color colorB, int perColor)
        {
            //flash the screen
            if (count <= perColor)
            {
                CoreGlobals.GraphicsDevice.Clear(colorB);
                count++;
            }
            else if(count > perColor)
            {
                CoreGlobals.GraphicsDevice.Clear(colorA);
                count++;
                if(count == perColor * 2)
                {
                    count = 0;
                }
            }
        }

        //private void DrawSpriteRotated(Texture2D sprite, Rectangle destRect, Rectangle sourceRect, float rotation, Vector2 origin)
        //{
        //    //origin = new Vector2(destRect.X + (destRect.Width / 2), + destRect.Y - (destRect.Height / 2));
        //    origin = new Vector2(0, 0);
        //    SpriteEffects effects = SpriteEffects.None;
        //    float layerDepth = 1f;
        //    spriteBatch.Draw(sprite, destRect, sourceRect, Color.White, rotation, origin, effects, layerDepth);
        //}

        private void DrawBricksWithTexture()
        {

            for (int i = 0; i < brickBreaker.Bricks.Count; i++)
            {
                // int toMimic = GetItemToMimic(brickBreaker.Bricks[i].Row, brickBreaker.Bricks[i].health, brickBreaker.Bricks[i].breakable);
                int toMimic = brickBreaker.Bricks[i].ItemToMimic;
                spriteBatch.Draw(brickBreaker.texturePack.GetTexureForItem((Item)toMimic), brickBreaker.Bricks[i].rectangle, brickBreaker.texturePack.ItemSrcRect((Item)toMimic), Color.White);
                DrawBorderForTextured(brickBreaker.Bricks[i].rectangle);
            }


        }

        private void DrawBricksNonTextured()
        {
            for (int i = 0; i < brickBreaker.Bricks.Count; i++)
            {
                spriteBatch.Draw(CoreGlobals.BlankTexture, brickBreaker.Bricks[i].rectangle, brickBreaker.Bricks[i].color);
                DrawBorderForNonTextured(brickBreaker.Bricks[i].rectangle, brickBreaker.Bricks[i].color);
            }

        }

        private void DrawBorderForTextured(Rectangle r)
        {
            int bw = 1; // Border width
            Color c = new Color(0.1f, 0.1f, 0.1f, 0.05f);
            spriteBatch.Draw(CoreGlobals.BlankTexture, new Rectangle(r.Left + bw, r.Top, r.Width - (bw), r.Height - bw), c);
        }


        private void DrawBorderForNonTextured(Rectangle r, Color c)
        {
            int bw = 1; // Border width
            //c = new Color(0.1f, 0.1f, 0.1f, 0.05f);

            //c *= 1.25f; // 1.25f darken color 0.75f lightn color
            //c *= 0.75f;

            spriteBatch.Draw(CoreGlobals.BlankTexture, new Rectangle(r.Left - bw , r.Top, bw, r.Height), c * 1.25f); // Left
            //spriteBatch.Draw(CoreGlobals.BlankTexture, new Rectangle(r.Right, r.Top, bw, r.Height), c ); // Right
            //spriteBatch.Draw(CoreGlobals.BlankTexture, new Rectangle(r.Left, r.Top, r.Width, bw), c ); // Top
            spriteBatch.Draw(CoreGlobals.BlankTexture, new Rectangle(r.Left - bw, r.Bottom, r.Width, bw), c * .75f); // Bottom
        }

        private void DrawPowerUps()
        {
            for (int i = 0; i < brickBreaker.PowerUps.Count; i++)
            {
                //spriteBatch.Draw(CoreGlobals.BlankTexture, brickBreaker.PowerUps[i].Rectangle, brickBreaker.PowerUps[i].Color);

                Rectangle srcRect = SourceRect[(int)brickBreaker.PowerUps[i].Type].Rect[brickBreaker.PowerUps[i].CurAnimationFrame];
                spriteBatch.Draw(SpriteSheet, brickBreaker.PowerUps[i].Rectangle, srcRect, Color.White);
                //spriteBatch.Draw(SpriteSheet, brickBreaker.PowerUps[i].Location, srcRect, Color.White);

            }

        }

        private void DrawBullets()
        {
            for (int i = 0; i < brickBreaker.Bullets.Count; i++)
            {
                spriteBatch.Draw(CoreGlobals.BlankTexture, brickBreaker.Bullets[i].rectangle, brickBreaker.Bullets[i].color);
            }
            if (brickBreaker.bulletsRemain > 0)
            {
                spriteBatch.Draw(CoreGlobals.BlankTexture, brickBreaker.iconRect, Color.Red);
                spriteBatch.DrawString(font, brickBreaker.bulletsRemain.ToString(), new Vector2(brickBreaker.ScreenSize.X - 20, brickBreaker.ScreenSize.Y - 15), Color.White, 0f, Vector2.Zero, 0.3f, SpriteEffects.None, 0f);
            }
        }

        void DrawHud()
        {
            spriteBatch.DrawString(font, brickBreaker.ScoreText1, new Vector2(4f * drawScale, 0f), Color.White, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, brickBreaker.LivesText, new Vector2(220 * drawScale, 0f), Color.White, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0f);

            spriteBatch.Draw(CoreGlobals.BlankTexture, new Rectangle(0, brickBreaker.GameScreenY, brickBreaker.ScreenSize.X, 1), Color.White); //line below score/lives
            //spriteBatch.Draw(CoreGlobals.BlankTexture, new Rectangle(0, game.ScreenSize.Y - 1, game.ScreenSize.X, 1), Color.Red); //bottom of screen line

        }

        void DrawGameOver()
        {
            switch (brickBreaker.currentMenu)
            {
                case BrickbreakerGame.GameMenu.Controls:
                    DrawMainMenuControls();
                    break;
                default:
                    DrawMainMenu();
                    break;
            }
        }


        void DrawMainMenu()
        {
            int score = brickBreaker.useRandomLayout ? brickBreaker.highScoreRandom : brickBreaker.highScoreItem;
            spriteBatch.DrawStringCentered(font, "HighScore: " + score.ToString(), 20, Color.White, 0.4f);
            spriteBatch.DrawStringCentered(font, "Random", 35, Color.White, 1.2f);
            spriteBatch.DrawStringCentered(font, brickBreaker.useRandomLayout ? "Bricks" : "Items", 70, Color.White, 1.2f);
            spriteBatch.DrawStringCentered(font, "Game Over", 120, Color.Red, 0.8f);
            spriteBatch.DrawStringCentered(font, brickBreaker.CreditText, 160, Color.White, 0.6f);

            if (InputManager.IsUsingGamePad)
            {
                spriteBatch.DrawStringCentered(font, "Press A for Controls", 400, Color.White, 0.4f);
                spriteBatch.DrawStringCentered(font, "Press Y to toggle layout.", 420, Color.White, 0.4f);

            }
            else
            {
                spriteBatch.DrawStringCentered(font, "Press Enter for Controls", 400, Color.White, 0.4f);
                spriteBatch.DrawStringCentered(font, "Press Y to toggle layout.", 420, Color.White, 0.4f);
            }

        }

        void DrawMainMenuControls()
        {
            //dedication part of the screen

            spriteBatch.DrawStringCentered(font, "Special Thanks:", 30, Color.White, 0.5f);
            spriteBatch.DrawStringCentered(font, "Craig", 50, Color.Gold, 0.4f);
            spriteBatch.DrawStringCentered(font, "for the idea of using items for the levels.", 65, Color.White, 0.4f);

            DrawPowerUpList();

            //how to controll the game

            if (InputManager.IsUsingGamePad)
            {

                spriteBatch.DrawStringCentered(font, "Move with Left or Right Thumb stick.", 400, Color.Green, .4f);
                spriteBatch.DrawStringCentered(font, "A button releases ball.", 420, Color.Green, .4f);
                spriteBatch.DrawStringCentered(font, "B button to exit.", 440, Color.Green, .4f);
                spriteBatch.DrawStringCentered(font, "Back button to pause game.", 460, Color.Green, .4f);
            }
            else
            {
                spriteBatch.DrawStringCentered(font, "Move with mouse or A and D.", 400, Color.Green, .4f);
                spriteBatch.DrawStringCentered(font, "Left click or W to release ball.", 420, Color.Green, .4f);
                spriteBatch.DrawStringCentered(font, "Press Enter to exit.", 440, Color.Green, .4f);
                spriteBatch.DrawStringCentered(font, "Press P to pause game.", 460, Color.Green, .4f);
            }

        }

        private void DrawPowerUpList()
        {

            //list all powerups

            int x = 10;
            int x4 = 40;
            int y = 90;
            int width = 24;
            int height = 16;
            float scale = 0.4f;

            Rectangle srcRect = SourceRect[(int)PowerUp.PowerUpType.WIDE_PADDLE].Rect[0];
            string msg = "Wide Paddle: Increases width of player paddle temporarily. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.WIDE_PADDLE) + " pts";
            Rectangle rect = new Rectangle(x, y, width, height);
            Vector2 loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Green, scale);


            srcRect = SourceRect[(int)PowerUp.PowerUpType.STICKY_BALL].Rect[0];
            msg = "Sticky Paddle: Ball sticks to the paddle when it hits temporarily. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.STICKY_BALL) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Green, scale);

            srcRect = SourceRect[(int)PowerUp.PowerUpType.BIG_BALL].Rect[0];
            msg = "Big Ball: Ball grows in size temporarily. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.BIG_BALL) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Green, scale);

            srcRect = SourceRect[(int)PowerUp.PowerUpType.HEAVY_BALL].Rect[0];
            msg = "Heavy Ball: Ball has the ability to crush a brick without bouncing. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.HEAVY_BALL) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Green, scale);

            srcRect = SourceRect[(int)PowerUp.PowerUpType.TRIPLE_BALLS].Rect[0];
            msg = "Tripple Balls: Three more balls. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.TRIPLE_BALLS) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Green, scale);

            srcRect = SourceRect[(int)PowerUp.PowerUpType.RESET_SPEED].Rect[0];
            msg = "Reset Speed: Sets ball speed back to what it was at the begining of the level. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.RESET_SPEED) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Green, scale);

            srcRect = SourceRect[(int)PowerUp.PowerUpType.BULLETS].Rect[0];
            msg = "Bullets: Shoot bullets using same button that launches the ball. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.BULLETS) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Green, scale);

            srcRect = SourceRect[(int)PowerUp.PowerUpType.SLOW_BALL].Rect[0];
            msg = "Slow Ball: Slows the ball to half speed temporarily. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.SLOW_BALL) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Green, scale);

            srcRect = SourceRect[(int)PowerUp.PowerUpType.EXTRA_LIFE].Rect[0];
            msg = "Extra Life: Because more is better. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.EXTRA_LIFE) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Green, scale);

            msg = "Extra life every 500 pts.";
            y = y + 20;
            loc = new Vector2(x4, y);
            DrawStringAt(font, msg, loc, Color.Green, scale * .90f);
            //y = y + 20; //extra line to seperate bad and good

            srcRect = SourceRect[(int)PowerUp.PowerUpType.NARROW_PADDLE].Rect[0];
            msg = "Narrow Paddle: Decrease the size of the player paddle temporarily. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.NARROW_PADDLE) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Red, scale);

            srcRect = SourceRect[(int)PowerUp.PowerUpType.INVERT_CONTROLS].Rect[0];
            msg = "Invert Controls: Left is Right is Left temporarily. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.INVERT_CONTROLS) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Red, scale);

            srcRect = SourceRect[(int)PowerUp.PowerUpType.RANDOM_BOUNCE].Rect[0];
            msg = "Random Bounce: The ball temporarily bounces in random directions. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.RANDOM_BOUNCE) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Red, scale);

            srcRect = SourceRect[(int)PowerUp.PowerUpType.INCREASE_SPEED].Rect[0];
            msg = "Increase Speed: The ball doubles in speed. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.INCREASE_SPEED) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Red, scale);

            srcRect = SourceRect[(int)PowerUp.PowerUpType.CURVE_BALL].Rect[0];
            msg = "Curve Ball: The ball moves around erratically. " + brickBreaker.powerUpTracker.ScoreValue(PowerUp.PowerUpType.CURVE_BALL) + " pts";
            y = y + 20;
            rect = new Rectangle(x, y, width, height);
            loc = new Vector2(x4, y);
            spriteBatch.Draw(SpriteSheet, rect, srcRect, Color.White);
            DrawStringAt(font, msg, loc, Color.Red, scale);

        }

        private void DrawStringAt(SpriteFont font, string msg, Vector2 loc, Color color, float scale)
        {
            spriteBatch.DrawString(font, msg, loc, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }


        #endregion

    }
}
