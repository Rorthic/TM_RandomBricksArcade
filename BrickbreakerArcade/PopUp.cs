using StudioForge.Engine.Core;
using System;
using System.IO;
using StudioForge.TotalMiner;
using Microsoft.Xna.Framework.Graphics;
using StudioForge.Engine;
using Microsoft.Xna.Framework;

namespace BrickbreakerArcade
{
    class PopUp
    {
        SpriteBatchSafe spriteBatch = CoreGlobals.SpriteBatch;
        SpriteFont font = CoreGlobals.GameFont;

        float drawTime;
        float drawScale;
        Color drawColor;
        string drawMsg;

        PopUp instance;

        void DrawPopup()
        {
            drawTime -= Services.ElapsedTime;
            if (drawTime > 0)
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, DepthStencilState.None, null);
                spriteBatch.DrawStringCentered(font, drawMsg, 60, drawColor, drawScale);
                spriteBatch.End();
            }

        }
        public PopUp(string msg, Color color, float scale, float time)
        {
            drawMsg = msg;
            drawColor = color;
            drawScale = scale;
            drawTime = time;

            instance = this;
            DrawPopup();
        }

    }
}
