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
            spriteBatch.Draw(CoreGlobals.BlankTexture, game.BallRect, Color.SaddleBrown);
            spriteBatch.Draw(CoreGlobals.BlankTexture, game.PlayerPaddleRect, Color.White);
        }

        private void DrawBricks()
        {
            Color color;
            for (int i = 0; i < game.Bricks.Count; i++)
            {

                switch (game.Bricks[i].health)
                {
                    case 1:
                        color = Color.Green;
                        break;
                    case 2:
                        color = Color.Blue;
                        break;
                    case 3:
                        color = Color.Purple;
                        break;
                    case 10:
                        color = Color.Gray;
                        break;
                    default:
                        color = Color.White;
                        break;
                }
                spriteBatch.Draw(CoreGlobals.BlankTexture, game.Bricks[i].rectangle, color);
            }
        }

        void DrawHud()
        {
            spriteBatch.DrawString(font, game.ScoreText1, new Vector2(4f, 0f), Color.White, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font, game.LivesText, new Vector2(220, 0f), Color.White, 0f, Vector2.Zero, 0.4f, SpriteEffects.None, 0f);

        }

        void DrawGameOver()
        {
            spriteBatch.DrawStringCentered(font, "HighScore: " + game.highScore.LoadHighScore().ToString(), 20, Color.White, 0.4f);
            spriteBatch.DrawStringCentered(font, "Random", 35, Color.White, 1.2f);
            spriteBatch.DrawStringCentered(font, "Bricks", 65, Color.White, 1.2f);
            spriteBatch.DrawStringCentered(font, "Game Over", 120, Color.Red, 0.8f);
            spriteBatch.DrawStringCentered(font, game.CreditText, 160, Color.White, 0.6f);
            spriteBatch.DrawStringCentered(font, "Move with left and right arrows", 200, Color.Green, .4f);
            spriteBatch.DrawStringCentered(font, "Up arrow releases ball.", 215, Color.Green, .4f);

        }


        #endregion

    }
}
