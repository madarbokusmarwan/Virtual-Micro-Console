using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualMicroConsole
{
    public class Geometry
    {
        protected Texture2D PixelTexture;
        protected SpriteBatch spriteBatch;
        protected GraphicsDevice graphics;

        public Geometry(SpriteBatch spriteBatch, GraphicsDevice graphics)
        {
            this.spriteBatch = spriteBatch;
            this.graphics = graphics;
            PixelTexture = new Texture2D(graphics, 1, 1);
            PixelTexture.SetData(new Color[] { Color.White });
        }

        public void pixel(float x, float y, Color color)
        {
            spriteBatch.Draw(PixelTexture, new Vector2(x, y), color);
        }
        public void rect(float x, float y, int w, int h, Color color)
        {
            draw_rect(x, y, w, h, color);
        }
        public void rect(float x, float y, int w, int h, int thickness)
        {
            spriteBatch.Draw(PixelTexture, new Rectangle((int)x, (int)y, w, h), null, Color.White);
            spriteBatch.Draw(PixelTexture, new Rectangle((int)x + thickness / 2, (int)y + thickness / 2, w - thickness, h - thickness), null, Color.Black);
        }
        public void circ(float x, float y, int r, Color color)
        {
            for (int i = (int)x - r; i < (int)x + r; i++)
            {
                for (int j = (int)y - r; j < (int)y + r; j++)
                {
                    if (Utils.dist(x, y, i, j) <= r) pixel(i, j, color);
                }
            }
        }
        public void circ(float x, float y, int r, int thickness, Color color)
        {

        }
        public void line(float x1, float y1, float x2, float y2, int thickness, Color color)
        {
            float distance = Utils.dist(x1, y1, x2, y2);
            Vector2 scale = new Vector2(distance, thickness);
            float angle = Utils.angle(x1, y1, x2, y2);
            spriteBatch.Draw(PixelTexture, new Vector2(x1, y1), null, color, angle, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
        public void draw_rect(float x, float y, int w, int h, Color color)
        {
            spriteBatch.Draw(PixelTexture, new Rectangle((int)x, (int)y, w, h), null, color);
        }
        public void draw_line_rect(float x, float y, int w, int h, int thickness, Color color1, Color color2)
        {
            spriteBatch.Draw(PixelTexture, new Rectangle((int)x, (int)y, w, h), null, color1);
            spriteBatch.Draw(PixelTexture, new Rectangle((int)x + thickness / 2, (int)y + thickness / 2, w - thickness, h - thickness), null, color2);
        }
        public Texture2D GetRectTexture(int w, int h, Color color)
        {
            Texture2D t = new Texture2D(graphics, w, h);
            Color[] data = new Color[w * h];
            for (int i = 0; i < w * h; i++) data[i] = color;
            t.SetData(data);

            return t;
        }
    }
}
