using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VirtualMicroConsole
{
    public abstract class GraphicInterface
    {
        protected SpriteBatch spriteBatch;
        protected int ScreenWidth;
        protected int ScreenHeight;
        protected SpriteFont systemFont;
        protected GraphicsDevice graphics;
        protected Geometry geo;
        protected MousePosition mp;

        public virtual void Load(SpriteBatch spriteBatch, GraphicsDevice graphics, ContentManager content, int screenWidth, int screenHeight, MousePosition mp)
        {
            this.spriteBatch = spriteBatch;
            this.graphics = graphics;
            geo = new Geometry(spriteBatch, graphics);
            ScreenWidth = screenWidth;
            ScreenHeight = screenHeight;
            systemFont = content.Load<SpriteFont>("assets/fonts/system");
            this.mp = mp;
        }
        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
        public abstract void EndRun();

        
    }

    public class Button
    {
        public Texture2D Texture;
        public Vector2 Position { get; private set; }
        private OnClick onClick;
        private MouseState old;
        private bool Hoover;
        public float scale = 1f;
        public bool centered = false;
        private MousePosition mp;
        public Button(Texture2D texture, Vector2 pos, OnClick o, MousePosition mp)
        {
            Texture = texture;
            Position = pos;
            onClick = o;
            old = Mouse.GetState();
            this.mp = mp;
        }

        public void Update()
        {
            Rectangle bounds = new Rectangle((int)Position.X, (int)Position.Y, (int)(Texture.Width*scale), (int)(Texture.Height*scale));
            MouseState ms = Mouse.GetState();           
            if (bounds.Contains(mp()))
            {
                Hoover = true;
                if (ms.LeftButton == ButtonState.Pressed && old.LeftButton == ButtonState.Released) onClick(this);
            }
            else Hoover = false;

            old = ms;
        }
        public void Draw(SpriteBatch sb)
        {
            Color color = Color.White;
            if (Hoover) color = Color.Gray;
            Vector2 origin = Vector2.Zero;
            if (centered) origin = Texture.Bounds.Center.ToVector2();
            sb.Draw(Texture, Position, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        public delegate void OnClick(Button b);
    }
    public class Entry
    {
        public delegate void Alert(string data);

        // fields && properties -------------
        public Alert alert;
        private string Data;
        private bool IsActive = false;
        private Texture2D Texture;
        private Vector2 Position;
        private Vector2 Gap;
        private MouseState oldMS;
        private bool Hoover;
        private SpriteFont Font;
        private Keys oldKeyPressed;
        private int Max;
        private MousePosition mp;

        // constructor ----------------------
        public Entry(Texture2D texture, Vector2 pos, SpriteFont font, MousePosition mp, int max=999)
        {
            Texture = texture;
            Position = pos;
            Font = font;
            Gap = new Vector2(5);
            Data = "";
            Max = max;
            this.mp = mp;
        }

        // methods --------------------------
        public void Update(GameTime gt)
        {
            // activation
            Rectangle bounds = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            MouseState ms = Mouse.GetState();
            if (bounds.Contains(mp()))
            {
                Hoover = true;
                if (ms.LeftButton == ButtonState.Pressed && oldMS.LeftButton == ButtonState.Released) IsActive = !IsActive;
            }
            else
            {
                Hoover = false;
                if (ms.LeftButton == ButtonState.Pressed) IsActive = false;
            }

            // keyboard writing
            var kState = Keyboard.GetState();
            if (IsActive)
            {
                
                if (kState.GetPressedKeys().Length > 0)
                {
                    string k = kState.GetPressedKeys()[0].ToString();
                    if (Data.Length < Max && k != oldKeyPressed.ToString())
                    {
                        if (k.Length == 1)
                        {
                            Data += k.ToLower();
                            oldKeyPressed = kState.GetPressedKeys()[0];
                        }
                        else if (k.Contains("NumPad"))
                        {
                            Data += k[k.Length-1];
                            oldKeyPressed = kState.GetPressedKeys()[0];
                        }
                    }
                    
                }
                if (kState.IsKeyUp(oldKeyPressed)) oldKeyPressed = Keys.None;

                // entry
                if (kState.IsKeyDown(Keys.Enter))
                {
                    alert(Data);
                    IsActive = false;
                }

                // backspace
                if (kState.IsKeyDown(Keys.Back) && oldKeyPressed != Keys.Back)
                {
                    if (Data.Length > 0) Data = Data.Remove(Data.Length - 1);
                    oldKeyPressed = Keys.Back;
                }
            }

            oldMS = ms;
        }
        public void Draw(SpriteBatch sb, GameTime gt)
        {
            // BASE
            Color color = Color.White;
            if (Hoover) color = Color.DarkGray;
            sb.Draw(Texture, Position, color);

            // text
            sb.DrawString(Font, Data, Position + Gap, Color.Black);

            // cursor wich apear and disapear at regular time intervall
            Vector2 textGap = new Vector2(Font.MeasureString(Data).X + 2, 0);
            if (IsActive && (int)gt.TotalGameTime.TotalSeconds%2 == 0) sb.DrawString(Font, "|", Position + Gap + textGap, Color.Black); 
        }
    }
}
