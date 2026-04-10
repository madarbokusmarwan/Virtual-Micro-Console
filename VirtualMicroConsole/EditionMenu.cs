using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VirtualMicroConsole
{
    public class EditionMenu
    {
        public bool IsActive;
        private Texture2D Texture;
        private Texture2D Selector;
        private int Index;
        private string[] Names;
        private Vector2 Position;
        private float Timer = 0f;
        
        public EditionMenu(ContentManager content, Vector2 pos)
        {
            Index = 0;
            Names = new string[] { "sprite editor", "music test", "map editor" };
            Selector = content.Load<Texture2D>("assets/arrow_selection_texture2");
            Texture = content.Load<Texture2D>("assets/edition_menu2");
            Position = pos;
        }

        public void Advance()
        {
            Index++;
            if (Index > 2) Index = 0;
            Timer = 0.1f;
        }
        public void Update(GameTime gt)
        {
            if (Timer > 0) Timer -= (float)gt.ElapsedGameTime.TotalSeconds;
            else Timer = 0f;
        }
        public string GetChoice()
        {
            if (Index == 0) return "sprite";
            else if (Index == 1) return "music";
            return "map";
        }
        public void Draw(SpriteBatch spriteBatch, SpriteFont font, Geometry geo)
        {
            if (IsActive)
            {
                int cell_w = 133;
                int cell_h = 128;
                float alpha = 1 - (0.8f/0.1f) * Timer;
                spriteBatch.Draw(Texture, Position, Color.White);
                spriteBatch.Draw(Selector, Position + new Vector2(Index * cell_w + 10, -cell_h - 10 -(80*Timer)), Color.White);
                geo.draw_rect(150, Position.Y + cell_h + 10, 800-300, 20, Color.Black);
                spriteBatch.DrawString(font, Names[Index], Position + new Vector2(200 - font.MeasureString(Names[Index]).X/2, cell_h + 15), Color.Yellow * alpha);
            }
        }
    }
}
