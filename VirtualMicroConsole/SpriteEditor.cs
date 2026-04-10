using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace VirtualMicroConsole
{
    public class SpriteEditor : GraphicInterface
    {
        // fields && properties -------------
        private Button[] buttons;
        private Vector2 gridPos;
        private const int DEFAULTPIXELSIZE = 32;
        private int pixelSize = DEFAULTPIXELSIZE;
        private int gap = 2;
        private int[,] grid;
        private string brush;
        private bool gridVisible = true;
        private Texture2D SelectionTexture;
        private Texture2D NoGridTexture;
        private Texture2D GridTexture;
        private Texture2D IndicationsTexture;
        private Texture2D FlagTexture;
        private Texture2D FlagEnableTexture;
        private Vector2 SelectionPos;
        public BinaryTexture[] AllBinaryTexture;
        private Texture2D[] AllTexture;
        private int CurrentID;
        private int IDToCopy;
        private MouseState oldMsState;
        private int SpriteGridY;
        private int FILESIZE = 32;
        private int BrushSize = 1;
        private int BrushColor = 0;
        private Rectangle SizeRectangle;
        private Vector2 SizeCoordinates;
        private Texture2D ColorGaugeTexture;
        private Texture2D ColorCursorTexture;
        private Vector2 MiddleGrid;

        // BASE methods -----------------
        public override void Load(SpriteBatch spriteBatch, GraphicsDevice graphics, ContentManager content, int screenWidth, int screenHeight, MousePosition mp)
        {
            base.Load(spriteBatch, graphics, content, screenWidth, screenHeight, mp);

            // textures
            SelectionTexture = content.Load<Texture2D>("assets/selection");
            IndicationsTexture = content.Load<Texture2D>("assets/computer mouse");
            NoGridTexture = content.Load<Texture2D>("assets/no grid icon");
            GridTexture = content.Load<Texture2D>("assets/grid icon");
            FlagTexture = content.Load<Texture2D>("assets/flag1");
            FlagEnableTexture = content.Load<Texture2D>("assets/flag2");
            ColorGaugeTexture = content.Load<Texture2D>("assets/color gauge");
            ColorCursorTexture = content.Load<Texture2D>("assets/color selector");

            // buttons
            int gap1 = 600;
            buttons = new Button[19];
            buttons[0] = new Button(content.Load<Texture2D>("assets/pencil icon"), new Vector2(gap1, 130), BtnPencil, mp);
            buttons[1] = new Button(content.Load<Texture2D>("assets/paint icon"), new Vector2(gap1 + 32, 130), BtnPaint, mp);
            buttons[2] = new Button(content.Load<Texture2D>("assets/framing"), new Vector2(gap1, 162), BtnFraming, mp);
            buttons[3] = new Button(content.Load<Texture2D>("assets/grid icon"), new Vector2(gap1 + 32, 162), BtnGrid, mp);
            buttons[4] = new Button(content.Load<Texture2D>("assets/copy icon"), new Vector2(gap1, 194), BtnCopy, mp);
            buttons[5] = new Button(content.Load<Texture2D>("assets/paste icon"), new Vector2(gap1 + 32, 194), BtnPaste, mp);
            buttons[6] = new Button(content.Load<Texture2D>("assets/bin icon2"), new Vector2(150, 140), Clear, mp);
            buttons[7] = new Button(content.Load<Texture2D>("assets/save icon"), new Vector2(50, 140), BtnSave, mp);
            for (int i = 0; i < 8; i++)
            {
                buttons[8 + i] = new Button(FlagTexture, new Vector2(50 + i*20, 100), BtnFlag, mp);  
            }
            for (int i = 0; i < 3; i++)
            {
                buttons[16 + i] = new Button(content.Load<Texture2D>($"assets/pixel{3-i}"), new Vector2(gap1 + i * 22, 250), BtnPixel, mp);
            }

            // entries
            Texture2D entryTexture = geo.GetRectTexture(64, 25, Color.Gray);
            SizeRectangle = new Rectangle(100, 210, 96, 96);
            SizeCoordinates = Vector2.One;

            // grid
            AllBinaryTexture = new BinaryTexture[96];
            AllTexture = new Texture2D[96];
            grid = new int[8, 8];
            gridPos = new Vector2((ScreenWidth - grid.GetLength(0) * pixelSize) / 2f, ScreenHeight / 3f - (grid.GetLength(1) * pixelSize) / 2f - 10);
            MiddleGrid = gridPos + Vector2.One*8*pixelSize/2;
            Clear(buttons[7]);
            SpriteGridY = ScreenHeight / 3 * 2 + 15;

            // load all textures
            BinaryTexture[] all = null;
            SaveSystem.LoadTextures(ref all);
            for (int i = 0; i < all.Length; i++)
            {
                var b = all[i];
                if (b != null)
                {
                    var t = CreateTexture(b);
                    AllBinaryTexture[i] = b;
                    AllTexture[i] = t;
                }
            }


            // start confing
            brush = "pencil";
            SelectionPos = buttons[0].Position;

            // load texture by default
            IDToCopy = -1;
            CurrentID = 0;
            LoadGridTexture();

            // flags
            for (int i = 0; i < AllBinaryTexture.Length; i++)
            {
                if (!VirtualMicroConsole.Flags.ContainsKey(i)) VirtualMicroConsole.Flags[i] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            }
        }
        public override void Draw(GameTime gt)
        {
            // UI
            foreach (var b in buttons)
            {
                b.Draw(spriteBatch);
            }
            Vector2 msCoords = ToDrawGrid();
            msCoords = new Vector2(Utils.clamp(msCoords.X, 0, grid.GetLength(1)), Utils.clamp(msCoords.Y, 0, grid.GetLength(0)));
            spriteBatch.Draw(IndicationsTexture, new Vector2(650, 0), null, Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(SelectionTexture, SelectionPos, Color.White);
            spriteBatch.DrawString(systemFont, "COLOR", new Vector2(610, 30), Color.CornflowerBlue);
            spriteBatch.DrawString(systemFont, "coords => " + msCoords.ToString(), new Vector2(580, 290), Color.Yellow);

            // size control
            geo.draw_rect(20, 290, 64, 24, Color.Black);
            geo.draw_rect(20, 290, 60, 20, Color.Gray);
            spriteBatch.DrawString(systemFont, "ID #" + CurrentID, new Vector2(23, 293), Color.Yellow);

            // grid
            Color color;
            for (int l = 0; l < grid.GetLength(0); l++)
            {
                for (int c = 0; c < grid.GetLength(1); c++)
                {
                    color = Color.Gray;
                    if (grid[l, c] == 1) color = Color.Black;
                    else if (grid[l, c] == 2) color = Color.White;
                    if (grid[l, c] == 0)
                    {

                        geo.draw_rect(gridPos.X + c * (pixelSize + gap), gridPos.Y + l * (pixelSize + gap), pixelSize/2, pixelSize/2, Color.Gray);
                        geo.draw_rect(gridPos.X + c * (pixelSize + gap) + pixelSize/2, gridPos.Y + l * (pixelSize + gap), pixelSize/2, pixelSize/2, Color.DimGray);
                        geo.draw_rect(gridPos.X + c * (pixelSize + gap), gridPos.Y + l * (pixelSize + gap) + pixelSize / 2, pixelSize/2, pixelSize/2, Color.DimGray);
                        geo.draw_rect(gridPos.X + c * (pixelSize + gap) + pixelSize/2, gridPos.Y + l * (pixelSize + gap) + pixelSize / 2, pixelSize/2, pixelSize/2, Color.Gray);

                    }
                    else geo.draw_rect(gridPos.X + c * (pixelSize + gap), gridPos.Y + l * (pixelSize + gap), pixelSize, pixelSize, color);

                    // when hoover
                    Vector2 pixel = ToDrawGrid();
                    bool fisrtCondition = (l == pixel.Y && c == pixel.X);
                    bool secondCondition = ((l == pixel.Y || l == pixel.Y+1) && (c == pixel.X || c==pixel.X+1));
                    bool thirdCondition = ((l == pixel.Y || l == pixel.Y + 1 || l == pixel.Y - 1) && (c == pixel.X || c == pixel.X + 1 || c==pixel.X-1));
                    if (BrushSize == 1 && fisrtCondition) 
                        geo.draw_rect(gridPos.X + c * (pixelSize + gap), gridPos.Y + l * (pixelSize + gap), pixelSize, pixelSize, Color.Black * 0.26f);
                    else if (BrushSize == 2 && secondCondition) 
                        geo.draw_rect(gridPos.X + c * (pixelSize + gap), gridPos.Y + l * (pixelSize + gap), pixelSize, pixelSize, Color.Black * 0.26f);
                    else if (BrushSize == 3 && thirdCondition) 
                        geo.draw_rect(gridPos.X + c * (pixelSize + gap), gridPos.Y + l * (pixelSize + gap), pixelSize, pixelSize, Color.Black * 0.26f);

                }
            }

            // color selection
            spriteBatch.Draw(ColorGaugeTexture, new Vector2(700, 130), Color.White);
            spriteBatch.Draw(ColorCursorTexture, new Vector2(725, 130 + 32 * BrushColor), Color.White);

            // size control
            float w = SizeRectangle.Width / 4 * SizeCoordinates.X;
            float h = SizeRectangle.Height / 4 * SizeCoordinates.Y;
            geo.draw_rect(SizeRectangle.X, SizeRectangle.Y, SizeRectangle.Width, SizeRectangle.Height, Color.Black);
            geo.draw_line_rect(SizeRectangle.X, SizeRectangle.Y, (int)w, (int)h, 3, Color.White, Color.Black);

            // files
            geo.draw_rect(0, SpriteGridY-15, ScreenWidth, 330, Color.Black);
            geo.draw_rect(0, SpriteGridY-15, ScreenWidth, 10, Color.White);
            int nbCols = 24;
            int nbLines = 5;
            float gapX = (800 - nbCols * FILESIZE) / 2f;
            for (int i = 0; i < nbCols* nbLines; i++)
            {
                int col = i % nbCols;
                int line = i / nbCols;

                if (i<AllTexture.Length && AllTexture[i] != null)
                {
                    var texture = AllTexture[i];
                    int scale = 32 / Math.Max(AllTexture[i].Width, AllTexture[i].Height);
                    spriteBatch.Draw(texture, new Vector2(gapX + col * FILESIZE, SpriteGridY + line * FILESIZE), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                }

                // selection
                if (i == CurrentID)
                {
                    spriteBatch.Draw(SelectionTexture, new Vector2(gapX + col * FILESIZE, SpriteGridY + line * FILESIZE), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }
        public override void Update(GameTime gt)
        {
            var ms = Mouse.GetState();

            // drawing grid
            if (ms.LeftButton == ButtonState.Pressed || ms.RightButton == ButtonState.Pressed)
            {
                int col = (int)ToDrawGrid().X;
                int line = (int)ToDrawGrid().Y;

                // vérifier que la souris se trouve dans la grille
                if (col >= 0 && col < grid.GetLength(1) && line >= 0 && line < grid.GetLength(0)) 
                {
                    // draw
                    if (ms.LeftButton == ButtonState.Pressed)
                    {
                        if (brush == "pencil")
                        {
                            if (BrushSize == 3)
                            {
                                for (int i = -1; i < BrushSize-1; i++)
                                {
                                    for (int j = -1; j < BrushSize-1; j++)
                                    {
                                        if (line+i < grid.GetLength(0) && col+j < grid.GetLength(1) && line+i>=0 && col+j>=0) grid[line + i, col + j] = BrushColor;
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < BrushSize; i++)
                                {
                                    for (int j = 0; j < BrushSize; j++)
                                    {
                                        if (line + i < grid.GetLength(0) && col + j < grid.GetLength(1)) grid[line + i, col + j] = BrushColor;
                                    }
                                }
                            }
                        }
                        else if (brush == "framing" && (line % 2 == 0 && col % 2 == 0) || (line % 2 == 1 && col % 2 == 1)) grid[line, col] = BrushColor;
                        else if (brush == "paint pot") FloodFill(grid, col, line);

                    }
                }
            }
            // sprite selection
            if (mp().Y > SpriteGridY)
            {
                float gapX = (800 - 24 * FILESIZE) / 2f;
                int col = (int)(mp().X - gapX)/FILESIZE;
                int line = (int)(mp().Y - SpriteGridY) / FILESIZE;

                if(ms.LeftButton == ButtonState.Pressed && oldMsState.LeftButton == ButtonState.Released)
                {
                    CurrentID = col + line * 24;
                    CurrentID = Math.Min(CurrentID, 96-1);
                    LoadGridTexture();
                }
            }

            // size control
            if (ms.LeftButton == ButtonState.Pressed && SizeRectangle.Contains(mp()))
            {
                int col = (int)(mp().X - SizeRectangle.X) / (SizeRectangle.Width / 4) + 1;
                int line = (int)(mp().Y - SizeRectangle.Y) / (SizeRectangle.Height / 4) + 1;
                SizeCoordinates = new Vector2(col, line);
                TransformGrid(8 * col, 8 * line);
            }

            // color selection
            BrushColor -= (ms.ScrollWheelValue - oldMsState.ScrollWheelValue) / 120;
            if (BrushColor > 2) BrushColor = 0;
            else if (BrushColor < 0) BrushColor = 2;
            if (ms.MiddleButton == ButtonState.Pressed && oldMsState.MiddleButton == ButtonState.Released)
            {
                int col = (int)ToDrawGrid().X;
                int line = (int)ToDrawGrid().Y;
                if (col >= 0 && line >= 0 && col < grid.GetLength(1) && line < grid.GetLength(0)) BrushColor = grid[line, col];
            }

            // buttons
            foreach (var b in buttons)
            {
                b.Update();
            }

            oldMsState = ms;
        }
        public override void EndRun()
        {
            
        }

        // other methods ------------------
        public Texture2D[] GetTextures()
        {
            return AllTexture;
        }
        private Vector2 ToDrawGrid()
        {
            var relativePos = mp() - gridPos;
            int col = (int)(relativePos.X / (pixelSize + gap));
            int line = (int)(relativePos.Y / (pixelSize + gap));
            return new Vector2(col, line);
        }
        private void TransformGrid(int newWidth, int newHeight)
        {
            grid = new int[newHeight, newWidth];
            pixelSize = (int)((8f / Math.Max(newWidth, newHeight)) * DEFAULTPIXELSIZE);
            if (newWidth > 16 || newHeight > 16) gap = 1;
            else gap = 2;
            Clear(buttons[7]);

            // new Grid Pos
            Vector2 Middle = new Vector2(newWidth, newHeight) * pixelSize / 2;
            gridPos = MiddleGrid - Middle;
        }
        public Texture2D CreateTexture(BinaryTexture t)
        {
            Texture2D texture;
            Color[] color = new Color[t.Width * t.Height];
            texture = new Texture2D(graphics, t.Width, t.Height);
            for (int i = 0; i < t.Height; i++)
            {
                for (int j = 0; j < t.Width; j++)
                {
                    if (t.data[i][j] == 0) color[j + i * t.Width] = Color.Transparent;
                    else if (t.data[i][j] == 1) color[j + i * t.Width] = Color.Black;
                    else if (t.data[i][j] == 2) color[j + i * t.Width] = Color.White;
                }
            }
            texture.SetData(color);
            return texture;
        }

        // UI methods --------------
        private void BtnPencil(Button b)
        {
            brush = "pencil";
            SelectionPos = buttons[0].Position;
            VirtualMicroConsole.ChangeMouseCursorTexture(1);
        }
        private void BtnPaint(Button b)
        {
            brush = "paint pot";
            SelectionPos = b.Position;
            VirtualMicroConsole.ChangeMouseCursorTexture(2);
        }
        private void BtnGrid(Button b)
        {
            gridVisible = !gridVisible;
            if (gridVisible == true)
            {
                b.Texture = GridTexture;
                gap = 2;
                pixelSize -= gap;
            }
            else
            {
                b.Texture = NoGridTexture;
                pixelSize += gap;
                gap = 0;
            }
        }
        private void BtnFraming(Button b)
        {
            brush = "framing";
            SelectionPos = b.Position;
            VirtualMicroConsole.ChangeMouseCursorTexture(1);
        }
        private void BtnSave(Button b)
        {
            var convert = new int[grid.GetLength(0)][];
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                convert[i] = new int[grid.GetLength(1)];
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    convert[i][j] = grid[i, j];
                }
            }

            var bi = new BinaryTexture()
            {
                Width = grid.GetLength(1),
                Height = grid.GetLength(0),
                data = convert
            };
            AllBinaryTexture[CurrentID] = bi;
            AllTexture[CurrentID] = CreateTexture(bi);
            VirtualMicroConsole.Textures.Add(CreateTexture(bi));
        }
        private void BtnFlag(Button b)
        {
            // image swap
            if (b.Texture == FlagTexture) b.Texture = FlagEnableTexture;
            else b.Texture = FlagTexture;

            RegisterFlags();
        }
        private void BtnCopy(Button b)
        {
            IDToCopy = CurrentID;
        }
        private void BtnPaste(Button b)
        {
            if (IDToCopy != -1)
            {
                int current = CurrentID;
                CurrentID = IDToCopy;
                LoadGridTexture();
                CurrentID = current;
            }
        }
        private void Clear(Button b)
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++) grid[i, j] = 0;
            }
        }
        private void BtnPixel(Button b)
        {
            int index = Array.IndexOf(buttons, b);
            if (index == 16) BrushSize = 1;
            else if (index == 17) BrushSize = 2;
            else if (index == 18) BrushSize = 3;
        }       
        private void FloodFill(int[,] grid, int startX, int startY)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // On ne remplit que les zones noires (valeur 0)
            if (grid[startY, startX] == BrushColor)
                return;

            var startColor = grid[startY, startX];

            var queue = new Queue<(int, int)>();
            queue.Enqueue((startY, startX));
            grid[startY, startX] = BrushColor; // On le colore en blanc

            // Directions : haut, bas, gauche, droite
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
           
            while (queue.Count > 0)
            {
                var (y, x) = queue.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int newX = x + dx[i];
                    int newY = y + dy[i];
                    // Vérifie qu'on est dans les limites de la grille
                    if (newX >= 0 && newX < cols && newY >= 0 && newY < rows)
                    {
                        // Si c’est encore noir, on continue la propagation
                        if (grid[newY, newX] == startColor)
                        {
                            grid[newY, newX] = BrushColor; // Devient blanc
                            queue.Enqueue((newY, newX));
                        }
                    }
                }
            }
        }
        private void LoadGridTexture()
        {
            SizeCoordinates = Vector2.One;
            if (AllBinaryTexture[CurrentID] != null)
            {
                var t = AllBinaryTexture[CurrentID];
                TransformGrid(t.Width, t.Height);
                for (int l = 0; l < t.Height; l++)
                {
                    for (int c = 0; c < t.Width; c++)
                    {
                        grid[l, c] = t.data[l][c];
                    }
                }
            }
            else
            {
                TransformGrid(8, 8);
                Clear(buttons[7]);
            }

            // flags
            LoadFlags();
        }
        private void RegisterFlags()
        {
            for (int i = 0; i < 8; i++)
            {
                var btn = buttons[8 + i];
                if (btn.Texture == FlagEnableTexture) VirtualMicroConsole.Flags[CurrentID][i] = 1;
                else VirtualMicroConsole.Flags[CurrentID][i] = 0;
            }
        }
        private void LoadFlags()
        {
            for (int i = 0; i < 8; i++)
            {
                var btn = buttons[8 + i];
                if (!VirtualMicroConsole.Flags.ContainsKey(CurrentID)) VirtualMicroConsole.Flags[CurrentID] = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };

                if (VirtualMicroConsole.Flags[CurrentID][i] == 1) btn.Texture = FlagEnableTexture;
                else btn.Texture = FlagTexture;
            }
        }
    }
}
