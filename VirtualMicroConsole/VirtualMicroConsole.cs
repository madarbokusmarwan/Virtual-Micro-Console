using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace VirtualMicroConsole
{
    public abstract class VirtualMicroConsole : GraphicInterface
    {
        public float GameWidth;
        public float GameHeight;
        private RenderTarget2D Screen;
        private RenderTarget2D GameRender;
        protected const int WIDTH = 96;
        protected const int HEIGHT = 96;
        private bool FullScreen = false;
        private SpriteFont MainFont;
        private SpriteFont TitleFont;
        private Color MainColor = Color.White;
        private Color TitleColor = Color.White;
        private VirtualKeyboard vKeyboard;
        private Glitch glitch;
        public static List<Texture2D> Textures;
        private string State = "gameplay";
        private MapEditor mapEditor;
        private SpriteEditor spriteEditor;
        private MusicTest musicTest;
        private Texture2D[] ConsoleTextures;
        protected Inputs inputs;
        protected Camera camera;
        private float Timer = 0;
        private bool KeyDisplay = false;
        private Texture2D KeysTexture;
        private Texture2D[] MouseTextures;
        private SoundManager soundManager;
        private EditionMenu editionMenu;
        private KeyboardState oldKState;
        private MouseState oldMsState;
        private SoundEffect ClickSound;
        public static List<int[,]> Maps;
        public static Dictionary<int, int[]> Flags;
        private static int mouseStatus = 0;
        private string FileCodePath;
        private string error = "";
        public float LoadingTimer = 0;
        public Button ReloadBtn;

        // game loop ----------------------------------------------------------
        public override void Load(SpriteBatch spriteBatch, GraphicsDevice graphics, ContentManager content, int screenWidth, int screenHeight, MousePosition mp)
        {
            base.Load(spriteBatch, graphics, content, screenWidth, screenHeight, mousePosition);
            GameWidth = screenWidth;
            GameHeight = screenHeight;

            SaveSystem.LoadFileCodePath(ref FileCodePath);
            if (FileCodePath != "") SaveSystem.UserPath = FileCodePath.Substring(0, FileCodePath.Length - 9) + "save";
            Utils.debug(SaveSystem.UserPath);
            SaveSystem.LoadFlags(ref Flags);
            if (Flags == null) Flags = new Dictionary<int, int[]>();
            Screen = new RenderTarget2D(graphics, WIDTH, HEIGHT);
            GameRender = new RenderTarget2D(graphics, screenWidth, screenHeight);
            Textures = new List<Texture2D>();
            mapEditor = new MapEditor();
            spriteEditor = new SpriteEditor();
            spriteEditor.Load(spriteBatch, graphics, content, screenWidth, screenHeight, mousePosition);
            mapEditor.Load(spriteBatch, graphics, content, screenWidth, screenHeight, mousePosition);
            inputs = new Inputs();
            camera = new Camera();
            editionMenu = new EditionMenu(content, new Vector2((ScreenWidth - 400) / 2, 150));

            // data structs
            vKeyboard = new VirtualKeyboard(inputs, WIDTH, HEIGHT, content);

            glitch = new Glitch()
            {
                Height = 25,
                Y = 0
            };

            // graphics
            MainFont = content.Load<SpriteFont>("assets/fonts/mainfont");
            TitleFont = content.Load<SpriteFont>("assets/fonts/title");
            KeysTexture = content.Load<Texture2D>("assets/keys2");
            MouseTextures = new Texture2D[6];
            for (int i = 0; i < 6; i++)
            {
                MouseTextures[i] = content.Load<Texture2D>($"assets/mouse{i+1}");
            }
            ConsoleTextures = new Texture2D[5];
            for (int i = 0; i < 5; i++)
            {
                ConsoleTextures[i] = content.Load<Texture2D>($"assets/console{i+1}");
            }

            foreach (var texture in spriteEditor.GetTextures())
            {
                RegisterTexture(texture);
            }

            // map data
            Maps = mapEditor.GetMaps();

            // sounds & musics
            ClickSound = content.Load<SoundEffect>("assets/sounds/click");
            soundManager = new SoundManager(content);

            ReloadBtn = new Button(content.Load<Texture2D>("assets/reload"), new Vector2(GameWidth - 100, 30), Reload, mousePosition);
            musicTest = new MusicTest(soundManager, content);
            musicTest.Load(spriteBatch, graphics, content, screenWidth, screenHeight, mousePosition);
            LoadingTimer = 1f;
            Init();
        }
        public override void Update(GameTime gt)
        {
            inputs.Update();
            camera.Update(gt);

            // game
            vKeyboard.Update();
            if (LoadingTimer >= 0) LoadingTimer -= (float)gt.ElapsedGameTime.TotalSeconds;
            else SafeExectue(() => Update30((float)gt.ElapsedGameTime.TotalSeconds, (float)gt.TotalGameTime.TotalSeconds), "Update");
            

            // interfaces
            if (State == "gameplay")
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed && oldMsState.LeftButton == ButtonState.Released) ClickSound.Play();

                if (Keyboard.GetState().IsKeyDown(Keys.Escape)) FullScreen = false;
                if (Keyboard.GetState().IsKeyDown(Keys.F1))
                {
                    FullScreen = true;
                    Timer = 1f;
                }
                if (Keyboard.GetState().IsKeyDown(Keys.F3) && FullScreen == false)
                {
                    editionMenu.IsActive = true;
                    editionMenu.Update(gt);
                    if (Keyboard.GetState().IsKeyDown(Keys.RightShift) && oldKState.IsKeyUp(Keys.RightShift))
                    {
                        editionMenu.Advance();
                    }
                }
                if (Keyboard.GetState().IsKeyUp(Keys.F3) && oldKState.IsKeyDown(Keys.F3) && FullScreen == false)
                {
                    editionMenu.IsActive = false;
                    State = editionMenu.GetChoice();
                    Timer = 2f;
                    MediaPlayer.Pause();
                }

                if (Keyboard.GetState().IsKeyDown(Keys.F4) && FullScreen == false)
                {
                    var fileDialog = new OpenFileDialog()
                    {
                        Title = "Link a code file",
                        Multiselect = false,
                        Filter = "C# Source File (*.cs)|*.cs"

                    };
                    if (fileDialog.ShowDialog() == DialogResult.OK && Path.GetExtension(fileDialog.FileName) == ".cs")
                    {
                        FileCodePath = fileDialog.FileName;
                        SaveSystem.SaveGame(FileCodePath);
                        Game1.ResetRequest = true;
                        LoadingTimer = 1.5f;
                    }
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.F5) && FullScreen == false)
                {
                    string sourcePath = Directory.GetCurrentDirectory() + "/DEFAULT_PROJECT";
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.Title = "Créer un nouveau script";

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        CopyDirectory(sourcePath, dialog.FileName);
                        FileCodePath = dialog.FileName + "\\SCRIPT.cs";
                        SaveSystem.SaveGame(FileCodePath);
                        Game1.ResetRequest = true;
                        LoadingTimer = 1.5f;
                    }
                }                

                if (Keyboard.GetState().IsKeyDown(Keys.F2) && FullScreen == false) KeyDisplay = true;
                else KeyDisplay = false;

                if (FullScreen == false) ReloadBtn.Update();
            }
            else
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    State = "gameplay";
                    ChangeMouseCursorTexture(0);
                    MediaPlayer.Stop();
                }
                else if (State == "map") mapEditor.Update(gt);
                else if (State == "sprite") spriteEditor.Update(gt);
                else if (State == "music") musicTest.Update(gt);
            }
            Timer -= (float)gt.ElapsedGameTime.TotalSeconds;

            // glitch
            //glitch.Y += 0.9f;
            glitch.Y += Utils.rnd(5, 10)/10f;
            if (glitch.Y > HEIGHT) glitch.Y = 0 - glitch.Height;

            oldKState = Keyboard.GetState();
            oldMsState = Mouse.GetState();
        }
        public override void Draw(GameTime gt)
        {
            DrawOnFakeScreen(gt);

            graphics.SetRenderTarget(GameRender);
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            graphics.Clear(new Color(15, 15, 25));
            DrawInterfaces(gt);
            DrawInstructions();
            DrawFPS(gt);
            DrawKeys();
            //DrawMouseCoords();
            editionMenu.Draw(spriteBatch, systemFont, geo);
            spriteBatch.End();
            graphics.SetRenderTarget(null);

            graphics.Clear(new Color(15, 15, 25));

            float scaleX = GameWidth / ScreenWidth;
            float scaleY = GameHeight / ScreenHeight;
            float scale = (float)Math.Min(scaleX, scaleY);
            int newWidth = (int)(800 * scale);
            int newHeight = (int)(480 * scale);            

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            spriteBatch.Draw(GameRender, new Rectangle((int)(GameWidth - newWidth) / 2, (int)(GameHeight - newHeight) / 2, newWidth, newHeight), Color.White);
            DrawMouse(scale);
            spriteBatch.End();

        }
        public override void EndRun()
        {
            spriteEditor.EndRun();
            mapEditor.EndRun();
            SaveSystem.SaveUser(spriteEditor.AllBinaryTexture, mapEditor.GetMaps(), Flags);
        }

        // Mini Task methods ----------------------------------------------------------
        private void DrawInstructions()
        {
            if (State == "gameplay" && FullScreen == false)
            {
                spriteBatch.DrawString(systemFont, ">> press F1 to put the game on full screen", new Vector2(20, 400), Color.White);
                spriteBatch.DrawString(systemFont, ">> press F2 to see controls", new Vector2(20, 400+20), Color.White);
                spriteBatch.DrawString(systemFont, ">> press F3 to open edition menu and R-shift to naviguate", new Vector2(20, 400 + 40), Color.White);
                spriteBatch.DrawString(systemFont, ">> press F4/F5 to create/open file code", new Vector2(20, 400 + 60), Color.White);
                // draw file associated
                if (FileCodePath != "") spriteBatch.DrawString(systemFont, "[" + FileCodePath.Split('\\')[FileCodePath.Split('\\').Length - 2] + "]", new Vector2(20, 50), Color.Yellow);
                else spriteBatch.DrawString(systemFont, "[no code associated]", new Vector2(20, 50), Color.Yellow);
            }
            else spriteBatch.DrawString(systemFont, "(press ESCAPE to get back)", new Vector2(250, 380 + 60), Color.Yellow * Timer);
            spriteBatch.DrawString(systemFont, error, new Vector2(20, 70), Color.Red);

        }
        private void DrawMouse(float scale)
        {
            if (!(State == "gameplay" && FullScreen))
            {
                if (Mouse.GetState().LeftButton == ButtonState.Pressed && mouseStatus == 0)
                {
                    spriteBatch.Draw(MouseTextures[4], Mouse.GetState().Position.ToVector2() - new Vector2(8, 4), null, Color.White, 0f, Vector2.Zero, 2*scale, SpriteEffects.None, 0f);
                }
                else if (Mouse.GetState().LeftButton == ButtonState.Pressed && mouseStatus == 3)
                {
                    spriteBatch.Draw(MouseTextures[5], Mouse.GetState().Position.ToVector2() - new Vector2(8, 4), null, Color.White, 0f, Vector2.Zero, 2 * scale, SpriteEffects.None, 0f);
                }
                else if (mouseStatus==1)
                {
                    spriteBatch.Draw(MouseTextures[mouseStatus], Mouse.GetState().Position.ToVector2() - new Vector2(8, 4), null, Color.White, 0f, new Vector2(2,14), 2 * scale, SpriteEffects.None, 0f);
                }
                else spriteBatch.Draw(MouseTextures[mouseStatus], Mouse.GetState().Position.ToVector2() - new Vector2(8, 4), null, Color.White, 0f, Vector2.Zero, 2 * scale, SpriteEffects.None, 0f);
            }                
        }
        private void DrawMouseCoords()
        {
            var ms = Mouse.GetState();
            if (ms.MiddleButton == ButtonState.Pressed && !FullScreen)
                spriteBatch.DrawString(systemFont, ms.Position.ToString(), ms.Position.ToVector2() - new Vector2(30), Color.White);
        }
        private void DrawFPS(GameTime gt)
        {
            spriteBatch.DrawString(systemFont, "FPS:" + (int)(1 / gt.ElapsedGameTime.TotalSeconds), new Vector2(10), Color.White);
        }
        private void DrawKeys()
        {
            if (KeyDisplay) spriteBatch.Draw(KeysTexture, new Vector2(ScreenWidth, ScreenHeight)/2f + new Vector2(0, -30), null, Color.White, 0f, KeysTexture.Bounds.Center.ToVector2(), 0.6f, SpriteEffects.None, 0f);
        }
        private void DrawOnFakeScreen(GameTime gt)
        {
            graphics.SetRenderTarget(Screen);
            graphics.Clear(Color.Black);
            spriteBatch.Begin(transformMatrix: camera.ToMatrix(), samplerState: SamplerState.PointClamp);
            if (vKeyboard.IsActive) vKeyboard.Draw(gt, txt, setColor, rect, line, spriteBatch, MainFont);
            else if (LoadingTimer >= 0)
            {
                int x = (WIDTH - txtw("Loading ") - 20) / 2;
                int y = (HEIGHT - 8) / 2;
                float speed = 0.1f;
                if (LoadingTimer <= 0.5) speed *= 0.5f;
                int n = (int)(((float)gt.TotalGameTime.TotalSeconds / speed) % 2);
                txt("Loading ", x, y);
                rect(x + txtw("Loading ") + n * 10, y, 10, 10);
            }
            else SafeExectue(()=> DrawGame((float)gt.ElapsedGameTime.TotalSeconds, (float)gt.TotalGameTime.TotalSeconds), "Draw");
            spriteBatch.End();
            spriteBatch.Begin();
            if (LoadingTimer <= 0) SafeExectue(() => DrawUI((float)gt.ElapsedGameTime.TotalSeconds, (float)gt.TotalGameTime.TotalSeconds), "DrawUI");
            DrawGlitch();
            spriteBatch.End();
            graphics.SetRenderTarget(null);
        }
        private void DrawInterfaces(GameTime gt)
        {
            // interfaces
            if (State == "gameplay")
            {
                ReloadBtn.Draw(spriteBatch);

                float scale = 2f;
                float gap = 50;
                if (FullScreen)
                {
                    scale = GetScreenScale();
                    gap = 0;
                }
                int id = 0;
                int Yangle = 90;
                float scaleX = scale;
                float rotation = 0;
                if (!FullScreen)
                {                   
                    if (mousePosition().X > 650)
                    {
                        id = 2;
                        Yangle = 110;
                        rotation = 1;
                    }
                    else if (mousePosition().X > 600)
                    {
                        id = 3;
                        Yangle = 80;
                        rotation = 2;

                    }
                    else if (mousePosition().X < 150)
                    {
                        id = 4;
                        Yangle = 100;
                        rotation = -2;

                    }
                    else if (mousePosition().X < 200)
                    {
                        id = 1;
                        Yangle = 70;
                        rotation = -1;

                    }                 
                    rotation = MathHelper.ToRadians(rotation);
                    scaleX = scale * (float)Math.Sin(MathHelper.ToRadians(Yangle)); // false perspective
                }
                spriteBatch.Draw(ConsoleTextures[id], new Vector2(ScreenWidth, ScreenHeight - gap) / 2f, null, Color.White, rotation, ConsoleTextures[id].Bounds.Center.ToVector2(), 0.6f*scale/2f, SpriteEffects.None, 0f);
                spriteBatch.Draw(Screen, new Vector2(ScreenWidth, ScreenHeight - gap) / 2f, null, Color.White, rotation, new Vector2(WIDTH, HEIGHT) / 2f, new Vector2(scaleX, scale), SpriteEffects.None, 0f);
            }
            else if (State == "sprite") spriteEditor.Draw(gt);
            else if (State == "map") mapEditor.Draw(gt);
            else if (State == "music") musicTest.Draw(gt);
        }
        private Vector2 mousePosition()
        {
            float scaleX = GameWidth / ScreenWidth;
            float scaleY = GameHeight / ScreenHeight;
            float scale = (float)Math.Min(scaleX, scaleY);
            int newWidth = (int)(800 * scale);
            int newHeight = (int)(480 * scale);
            Vector2 ms = Mouse.GetState().Position.ToVector2();
            Vector2 offset = new Vector2((GameWidth - newWidth) / 2, (GameHeight - newHeight) / 2);
            ms -= offset;
            ms /= scale;
            return ms;
        }
        private void Reload(Button b)
        {
            Game1.ResetRequest = true;
            LoadingTimer = 1.5f;
        }
        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            // Crée le dossier de destination s'il n'existe pas
            if (!Directory.Exists(destinationDir))
                Directory.CreateDirectory(destinationDir);

            // Copie tous les fichiers
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                File.Copy(file, destFile, true); // true = écrase si déjà existant
            }

            // Copie tous les sous-dossiers récursivement
            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destinationDir, Path.GetFileName(dir));
                CopyDirectory(dir, destSubDir);
            }
        }


        // abstract ---------------------------------------------------------
        public abstract void Init();
        public abstract void Update30(float dt, float total_gametime);
        public abstract void DrawGame(float dt, float total_gametime);
        public abstract void DrawUI(float dt, float total_gametime);

        // system ------------------------------------------------
        private float GetScreenScale()
        {
            float scaleX = ScreenWidth / (float)WIDTH;
            float scaleY = ScreenHeight / (float)HEIGHT;
            return (int)Math.Min(scaleX, scaleY);
        }
        /// <summary>
        /// 0 -> black / 1 -> white
        /// </summary>
        /// <param name="color"></param>       
        public void RegisterTexture(Texture2D texture)
        {
            Textures.Add(texture);
        }


        /// <summary>
        /// 0->mouse, 1->pencil, 2->paint pot, 3->hand
        /// </summary>
        /// <param name="id"></param>
        public static void ChangeMouseCursorTexture(int id)
        {
            if (id >= 0 && id<4) mouseStatus = id;

        }        
        private void SafeExectue(Action action, string context)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                error = $"Erreur '{context}' : {e.Message}";
            }
        }


        // Interfaces -----------------------------------------------
        private void keyboard(string title, string ID, int limit = 16)
        {
            vKeyboard.IsActive = true;
            vKeyboard.Title = title;
            vKeyboard.Limit = limit;
            vKeyboard.keyIndex = 0;
            vKeyboard.keyboardCursor = 0;
            vKeyboard.Outputs[ID] = "";
        }
        private void DrawGlitch()
        {
            //spriteBatch.Draw(PixelTexture, new Rectangle(0 + (int)camera.pos.X, (int)(glitch.Y + camera.pos.Y), WIDTH, glitch.Height), null, Color.Gray * 0.05f);
            geo.draw_rect(0, glitch.Y, WIDTH, glitch.Height-10, Color.Gray * 0.015f);
            geo.draw_rect(0, glitch.Y - 5, WIDTH, glitch.Height, Color.Gray * 0.015f);
        }

        // protected ----------------------------------------------------------
        public void snd(int id)
        {
            soundManager.PlaySound(id);
        }
        public void music(int id, bool loop = false)
        {
            soundManager.PlayMusic(id, loop);
        }
        public void pixel(float x, float y)
        {
            geo.pixel(x, y, MainColor);
        }
        public void rect(float x, float y, int w, int h)
        {
            geo.rect(x, y, w, h, MainColor);
        }
        public void rect(float x, float y, int w, int h, int thickness)
        {
            geo.rect(x, y, w, h, thickness);
        }
        public void circ(float x, float y, int r)
        {
            geo.circ(x, y, r, MainColor);
        }
        public void circ(float x, float y, int r, int thickness)
        {
            geo.circ(x, y, r, Color.White);
            geo.circ(x, y, r-thickness, Color.Black);
        }
        public void line(float x1, float y1, float x2, float y2, int thickness)
        {
            geo.line(x1, y1, x2, y2, thickness, MainColor);
        }
        public bool k_pressed(params buttons[] buttons)
        {
            return inputs.pressed(buttons);
        }
        public bool k_released(params buttons[] buttons)
        {
            return inputs.released(buttons);
        }
        public bool k_down(params buttons[] buttons)
        {
            return inputs.down(buttons);
        }
        public bool k_up(params buttons[] buttons)
        {
            return inputs.up(buttons);
        }
        public void cam(float vx, float vy)
        {
            camera.move(vx, vy);
        }
        public void camto(float x, float y)
        {
            camera.moveAt(x, y);
        }
        public void cam_shake(int force, float duration)
        {
            camera.shake(force, duration);
        }
        public float angle(float x1, float y1, float x2, float y2)
        {
            return Utils.angle(x1, y1, x2, y2);
        }
        public float dist(float x1, float y1, float x2, float y2)
        {
            return Utils.dist(x1, y1, x2, y2);
        }
        public float clamp(float value, float min, float max)
        {
            return Utils.clamp(value, min, max);
        }
        public bool collide_rect(float x1, float y1, float w1, float h1, float x2, float y2, float w2, float h2)
        {
            Rectangle r1 = new Rectangle((int)x1, (int)y1, (int)w1, (int)h1);
            Rectangle r2 = new Rectangle((int)x2, (int)y2, (int)w2, (int)h2);
            return r1.Intersects(r2);
        }
        public bool collide_rect(float x1, float y1, int id1, float x2, float y2, int id2)
        {
            Rectangle r1 = new Rectangle((int)x1, (int)y1, Textures[id1].Width, Textures[id1].Height);
            Rectangle r2 = new Rectangle((int)x2, (int)y2, Textures[id2].Width, Textures[id2].Height);
            return r1.Intersects(r2);
        }
        public int rnd(int min, int max)
        {
            return Utils.rnd(min, max);
        }
        public bool flag(int tile_id, int flag)
        {
            if (tile_id == -1) return false;
            return Flags[tile_id][flag] == 1;
        }
        public int mget(int line, int col, int lvl = 0)
        {
            return Maps[lvl][line, col];
        }
        public void mset(int newID, int line, int col, int lvl = 0)
        {
            Maps[lvl][line, col] = newID;
        }
        public void img(int id, float x, float y, int scale = 1, float rotation = 0, bool flipX = false, bool flipY = false)
        {
            SpriteEffects flip = SpriteEffects.None;
            if (flipX) flip = SpriteEffects.FlipHorizontally;
            else if (flipY) flip = SpriteEffects.FlipVertically;
            spriteBatch.Draw(Textures[id], new Vector2(x, y), null, Color.White, rotation, Vector2.Zero, scale, flip, 0f);
        }
        public void img(int id, float x, float y, int nbFrames, float speed, float total_gametime, int scale = 1, float rotation = 0, bool flipX = false, bool flipY = false)
        {
            int n = (int)(total_gametime / speed % nbFrames);
            img(id + n, x, y, scale, rotation, flipX, flipY);
        }
        public void txt(string text, float x, float y, float size = 1)
        {
            float scale = 1;
            if (size == 2) scale = 1.5f;
            else if (size >= 3) scale = 2;
            spriteBatch.DrawString(MainFont, text, new Vector2(x, y), MainColor, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        }
        public void map(int lvl = 0, float x = 0, float y = 0)
        {
            for (int l = 0; l < Maps[lvl].GetLength(0); l++)
            {
                for (int c = 0; c < Maps[lvl].GetLength(1); c++)
                {
                    int id = Maps[lvl][l, c];
                    if (id != -1) img(id, x + c * 8, y + l * 8);
                }
            }
        }
        public void setColor(int color)
        {
            if (color == 0) this.MainColor = Color.Black;
            else this.MainColor = Color.White;
        }
        protected int txtw(string text)
        {
            return (int)MainFont.MeasureString(text).X;
        }
    }
    public struct Glitch
    {
        public float Y;
        public int Height;
    }   
    public class VirtualKeyboard
    {
        public bool IsActive;
        public int keyboardCursor;
        public int keyIndex;
        public int Limit;
        public string Title;
        public string Output;
        public string Letters;
        public Dictionary<string, string> Outputs;
        public Inputs inputs;
        public int WIDTH;
        public int HEIGHT;
        Texture2D AgreeButton;
        Texture2D AgreeButton2;

        public VirtualKeyboard(Inputs inputs, int screenWidth, int screenHeight, ContentManager content)
        {
            IsActive = false;
            keyboardCursor = 0;
            keyIndex = 0;
            Letters = "azertyuiopqsdfghjklmwxcvbn";
            Output = "";
            Outputs = new Dictionary<string, string>();
            this.inputs = inputs;
            WIDTH = screenWidth;
            HEIGHT = screenHeight;
            AgreeButton = content.Load<Texture2D>("assets/agree");
            AgreeButton2 = content.Load<Texture2D>("assets/agree2");
        }
        
        public string koutput(string ID)
        {
            if (Outputs.ContainsKey(ID)) return Outputs[ID];
            return "";
        }
        public string GetData(string name)
        {
            if (false) ;
            return "";
        }
        public void Draw(GameTime gt, txt txt, setColor setColor, rect rect, line line, SpriteBatch spriteBatch, SpriteFont MainFont)
        {
            // title
            rect(0, 0, WIDTH, 16);
            setColor(0);
            txt(Title, 20, 1);
            setColor(1);

            // cursor && input
            txt(Output, 10 + Utils.rnd(0, 1), 30 + Utils.rnd(0, 1));
            if ((int)gt.TotalGameTime.TotalSeconds % 2 == 0) rect(12 + MainFont.MeasureString(Output).X, 48, 8, 2);

            // keyboard
            line(0, 64, WIDTH, 64, 2);
            string[] keyboard = { "azertyuiop", "qsdfghjklm", "wxcvbn" };

            int x = 0;
            int y = 0;
            foreach (var l in keyboard)
            {
                foreach (var c in l)
                {
                    if (keyIndex < 26 && c == Letters[keyIndex])
                    {
                        rect(25 + x * 14 - 1, 70 + y * 20 - 1, 10, 16);
                        setColor(0);
                        txt(c.ToString(), 25 + x * 14, 70 + y * 20);
                        setColor(1);
                    }
                    else txt(c.ToString(), 25 + x * 14, 70 + y * 20);
                    x++;
                }
                y++;
                if (y == 2) x = 2;
                else x = 0;
            }

            // agree btn
            if (keyIndex < 26)
                spriteBatch.Draw(AgreeButton, new Vector2(WIDTH / 2, HEIGHT * 3 / 4), null, Color.White, 0f, AgreeButton.Bounds.Center.ToVector2(), 1f, SpriteEffects.None, 0f);
            else spriteBatch.Draw(AgreeButton2, new Vector2(WIDTH / 2, HEIGHT * 3 / 4), null, Color.White, 0f, AgreeButton.Bounds.Center.ToVector2(), 1f, SpriteEffects.None, 0f);
        }
        public void Update()
        {
            if (IsActive)
            {
                // choix de la touche
                if (inputs.pressed(buttons.right)) keyIndex++;
                if (inputs.pressed(buttons.left)) keyIndex--;
                if (inputs.pressed(buttons.up))
                {
                    if (keyIndex >= 20) keyIndex -= 8;
                    else keyIndex -= 10;
                }
                if (inputs.pressed(buttons.down))
                {
                    if (keyIndex >= 18) keyIndex += 6;
                    else if (keyIndex >= 12) keyIndex += 8;
                    else keyIndex += 10;
                }
                keyIndex = Math.Clamp(keyIndex, 0, 26);

                if (inputs.pressed(buttons.A))
                {
                    if (keyIndex < 26) Output += Letters[keyIndex];
                    else
                    {
                        IsActive = false;
                        //snd(); agree
                    }
                }
                else if (inputs.pressed(buttons.B) && Output.Length > 0)
                {
                    Output = Output.Substring(0, Output.Length - 1);
                }
            }
        }

        public delegate void txt(string text, float x, float y, float size =1);
        public delegate void setColor(int color);
        public delegate void rect(float x, float y, int w, int h);
        public delegate void line(float x1, float y1, float x2, float y2, int thickness);
    }

    public delegate Vector2 MousePosition();

}
