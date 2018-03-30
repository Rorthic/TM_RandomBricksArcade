using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StudioForge.Engine;
using StudioForge.Engine.Core;
using StudioForge.Engine.Integration;
using StudioForge.TotalMiner;

namespace RandomBricksArcade
{
    class BrickbreakerRenderer : IArcadeMachineRenderer
    {
        #region Fields

        BrickbreakerGame game;
        SpriteFont font;
        SpriteBatchSafe spriteBatch;

        #endregion

        #region Initialization

        public void LoadContent(InitState state)
        {
            spriteBatch = CoreGlobals.SpriteBatch;
            font = CoreGlobals.GameFont;
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
            game = baseGame as BrickbreakerGame;
            if (game == null) return;

            CoreGlobals.GraphicsDevice.SetRenderTarget(game.RenderTarget);
            CoreGlobals.GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, DepthStencilState.None, null);

            if (game.State == BrickbreakerGame.GameState.GameOver)
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
            DrawBricks();
            spriteBatch.Draw(CoreGlobals.BlankTexture, new Rectangle(0, game.GameScreenY, game.ScreenSize.X, 1), Color.White); //line below score/lives
            //spriteBatch.Draw(CoreGlobals.BlankTexture, new Rectangle(0, game.ScreenSize.Y - 1, game.ScreenSize.X, 1), Color.Red); //bottom of screen line
            spriteBatch.Draw(CoreGlobals.BlankTexture, game.BallRect, Color.White);
            spriteBatch.Draw(CoreGlobals.BlankTexture, game.PlayerPaddleRect, Color.White);

            if (game.messageToPlayer != string.Empty)
            {
                spriteBatch.DrawStringCentered(font, game.messageToPlayer, 20, Color.Green, 0.4f);
            }

            //var rect = game.PlayerPaddleRect;
            //rect.Width = (int)(game.PlayerPaddleRect.Width * 0.1f);
            //spriteBatch.Draw(CoreGlobals.BlankTexture, rect, Color.White);
            //rect.X += rect.Width;
            //rect.Width = (int)(game.PlayerPaddleRect.Width * 0.2f);
            //spriteBatch.Draw(CoreGlobals.BlankTexture, rect, Color.Red);
            //rect.X += rect.Width;
            //rect.Width = (int)(game.PlayerPaddleRect.Width * 0.4f);
            //spriteBatch.Draw(CoreGlobals.BlankTexture, rect, Color.Green);
            //rect.X += rect.Width;
            //rect.Width = (int)(game.PlayerPaddleRect.Width * 0.2f);
            //spriteBatch.Draw(CoreGlobals.BlankTexture, rect, Color.Blue);
            //rect.X += rect.Width;
            //rect.Width = (int)(game.PlayerPaddleRect.Width * 0.1f);
            //spriteBatch.Draw(CoreGlobals.BlankTexture, rect, Color.Yellow);
        }

        private void DrawBricks()
        {
            for (int i = 0; i < game.Bricks.Count; i++)
            {
                spriteBatch.Draw(CoreGlobals.BlankTexture, game.Bricks[i].rectangle, game.Bricks[i].color);
            }

        }

        void DrawHud()
        {
            spriteBatch.DrawString(font, game.ScoreText1, new Vector2(4f, 0f), Color.White, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, game.LivesText, new Vector2(220, 0f), Color.White, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0f);

        }

        void DrawGameOver()
        {
            switch (game.currentMenu)
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
            int score = game.useRandomLayout ? game.highScoreRandom : game.highScoreItem;
            spriteBatch.DrawStringCentered(font, "HighScore: " +  score.ToString(), 20, Color.White, 0.4f);
            spriteBatch.DrawStringCentered(font, "Random", 35, Color.White, 1.2f);
            spriteBatch.DrawStringCentered(font, game.useRandomLayout ? "Bricks" : "Items", 70, Color.White, 1.2f);
            spriteBatch.DrawStringCentered(font, "Game Over", 120, Color.Red, 0.8f);
            spriteBatch.DrawStringCentered(font, game.CreditText, 160, Color.White, 0.6f);

            if (InputManager.IsUsingGamePad)
            {
                spriteBatch.DrawStringCentered(font, "Press A for Controls", 190, Color.White, 0.4f);
                spriteBatch.DrawStringCentered(font, "Press Y to toggle layout.", 210, Color.White, 0.4f);
            }
            else
            {
                spriteBatch.DrawStringCentered(font, "Press Enter for Controls", 190, Color.White, 0.4f);
                spriteBatch.DrawStringCentered(font, "Press Y to toggle layout.", 210, Color.White, 0.4f);
            }
            //spriteBatch.DrawStringCentered(font, "---->", 215, Color.White, 0.4f);

        }

        void DrawMainMenuControls()
        {
            //dedication part of the screen

            spriteBatch.DrawStringCentered(font, "Special Thanks:", 35, Color.White, 0.5f);
            spriteBatch.DrawStringCentered(font, "Craig", 60, Color.Gold, 0.4f);
            spriteBatch.DrawStringCentered(font, "for the idea of using items for the levels.", 75, Color.White, 0.4f);

            //how to controll the game

            if (InputManager.IsUsingGamePad)
            {
                spriteBatch.DrawStringCentered(font, "Move with Left or Right Thumb stick", 125, Color.Green, .4f);
                spriteBatch.DrawStringCentered(font, "A button releases ball.", 140, Color.Green, .4f);
                spriteBatch.DrawStringCentered(font, "B button to exit", 165, Color.Green, .4f);
                //spriteBatch.DrawStringCentered(font, "Y at Main screen to toggle Layout", 180, Color.Green, .4f);
            }
            else
            {
                spriteBatch.DrawStringCentered(font, "Move with mouse", 125, Color.Green, .4f);
                spriteBatch.DrawStringCentered(font, "Left click to release ball.", 140, Color.Green, .4f);

                spriteBatch.DrawStringCentered(font, "Move with A and D", 165, Color.Green, .4f);
                spriteBatch.DrawStringCentered(font, "W releases ball.", 180, Color.Green, .4f);

                //spriteBatch.DrawStringCentered(font, "Y at Main screen to toggle Layout", 195, Color.Green, .4f);
                spriteBatch.DrawStringCentered(font, "Press Enter to exit", 210, Color.Green, .4f);
            }

            //spriteBatch.DrawStringCentered(font, "---->", 215, Color.White, 0.4f);
        }



        #endregion

    }
}
