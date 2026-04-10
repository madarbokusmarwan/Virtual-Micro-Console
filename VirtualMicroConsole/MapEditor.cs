using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace VirtualMicroConsole
{
    public class MapEditor : GraphicInterface
    {
        // fields && properties ---------
        const int TILESIZE = 28;
        int currentMap = 0;
        int currentMapHoover = 0;
        int CurrentID = 0;
        List<int[,]> data;
        Button[] UI;
        string brush = "pencil";
        Texture2D SelectionTexture;
        Texture2D GridTexture;
        Texture2D NoGridTexture;
        Texture2D VoidIconTexture;
        Vector2 SelectionPos;
        Vector2 GridOffset;
        Vector2 Camera;
        bool GridVisible = true;
        private Vector2 LevelsPosition;
        private const int NBLEVELS = 10;
        private const int MAPSIZE = 256;
        private Vector2 MapGap;
        Rectangle TileGridRect;
        MouseState oldMS;
        Vector2 MouseDrag;
        Vector2 OldCamera;
        int IDToCopy;
        bool ToggleBin = false;
        Button YesButton;
        Button NoButton;
        Vector2 Center = new Vector2(800, 480) / 2f;

        // BASE --------------------------
        public override void Load(SpriteBatch spriteBatch, GraphicsDevice graphics, ContentManager content, int screenWidth, int screenHeight, MousePosition mp)
        {
            base.Load(spriteBatch, graphics, content, screenWidth, screenHeight, mp);
         
            Camera = Vector2.Zero;
            MouseDrag = Vector2.Zero;
            IDToCopy = -1;

            SaveSystem.LoadMaps(ref data);
            if (data == null)
            {
                reset();
            }          
            int offx = (ScreenWidth - 16 * TILESIZE) / 2;
            int offy = (ScreenHeight - 16 * TILESIZE) / 2;
            GridOffset = new Vector2(offx, offy);
            MapGap = Vector2.Zero;

            LevelsPosition = new Vector2(10, 50);

            // textures
            SelectionTexture = content.Load<Texture2D>("assets/selection");
            NoGridTexture = content.Load<Texture2D>("assets/no grid icon");
            GridTexture = content.Load<Texture2D>("assets/grid icon");
            VoidIconTexture = content.Load<Texture2D>("assets/void icon");

            // UI
            UI = new Button[7];
            UI[0] = new Button(content.Load<Texture2D>("assets/pencil icon"), new Vector2(690, 30), BtnPencil, mp);
            UI[1] = new Button(content.Load<Texture2D>("assets/copy icon"), new Vector2(722, 30), BtnCopy, mp);
            UI[2] = new Button(content.Load<Texture2D>("assets/paste icon"), new Vector2(690, 62), BtnPaste, mp);
            UI[3] = new Button(GridTexture, new Vector2(722, 62), BtnGrid, mp);
            UI[4] = new Button(content.Load<Texture2D>("assets/paint icon"), new Vector2(658, 30), BtnFloodFill, mp);
            UI[5] = new Button(content.Load<Texture2D>("assets/drag icon"), new Vector2(658, 62), BtnDrag, mp);
            UI[6] = new Button(content.Load<Texture2D>("assets/bin icon"), new Vector2(754, 62), BtnBin, mp);
           //UI[7] = new Button(content.Load<Texture2D>("assets/earaser icon"), new Vector2(754, 30), BtnEaraser);


            YesButton = new Button(content.Load<Texture2D>("assets/yes button"), Center + new Vector2(-114, -30), BtnYes, mp);
            NoButton = new Button(content.Load<Texture2D>("assets/no button"), Center + new Vector2(5, -30), BtnNo, mp);

            SelectionPos = UI[0].Position;

            // tiles
            TileGridRect = new Rectangle(650, 120, ScreenWidth - 650 - 10, ScreenHeight - 120 - 10);
        }
        public override void Draw(GameTime gt)
        {
            spriteBatch.Draw(SelectionTexture, SelectionPos, Color.White);

            // camera
            spriteBatch.DrawString(systemFont, $"camera\n(X:{(int)Camera.X}, Y:{(int)Camera.Y})", new Vector2(10, 380), Color.White);

            // levels
            for (int i = 0; i < NBLEVELS; i++)
            {
                var pos = LevelsPosition + new Vector2(0, 24 * i);
                string txt = "level #0" + i;
                if (i == currentMapHoover)
                {                    
                    geo.draw_rect(pos.X, pos.Y, 100, 20, Color.White);
                    spriteBatch.DrawString(systemFont, txt, pos, Color.Black);
                }
                else if (i == currentMap)
                {
                    spriteBatch.DrawString(systemFont, txt, pos, Color.Yellow);
                }
                else spriteBatch.DrawString(systemFont, txt, pos, Color.White);
            }

            // grid
            var map = data[currentMap];
            for (int l = 0; l < 16; l++)
            {
                for (int c = 0; c < 16; c++)
                {
                    if (map[l + (int)Camera.Y, c + (int)Camera.X] == -1)
                    {
                        int gap = 1;
                        float alpha = 0.8f;
                        Color color = Color.CornflowerBlue;
                        Rectangle tileArea = new Rectangle(c*TILESIZE + (int)GridOffset.X, l*TILESIZE + (int)GridOffset.Y, TILESIZE, TILESIZE);
                        if (tileArea.Contains(mp())) color = Color.Yellow;
                        if (GridVisible) geo.draw_rect(c * TILESIZE + GridOffset.X, l * TILESIZE + GridOffset.Y, TILESIZE, TILESIZE, color * alpha);
                        geo.draw_rect(c * TILESIZE + GridOffset.X + gap, l * TILESIZE + GridOffset.Y + gap, TILESIZE - gap * 2, TILESIZE - gap * 2, Color.Black);
                    }
                    else
                    {
                        int index = map[l + (int)Camera.Y, c + (int)Camera.X];
                        Vector2 pos = new Vector2(c * TILESIZE + GridOffset.X, l * TILESIZE + GridOffset.Y);
                        float scale = TILESIZE / 8f;
                        spriteBatch.Draw(VirtualMicroConsole.Textures[index], pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }
                }
            }
            

            // tiles          
            int nbCols = TileGridRect.Width / 32;
            int xGap = (TileGridRect.Width % 32) / 2;
            geo.draw_rect(TileGridRect.X, TileGridRect.Y, TileGridRect.Width, TileGridRect.Height, Color.Black);
            for (int i = 0; i < VirtualMicroConsole.Textures.Count; i++)
            {
                int line = i / nbCols;
                int col = i % nbCols;
                float size = 4f;
                Vector2 pos = new Vector2(xGap + TileGridRect.X + col * 32, TileGridRect.Y + line * 32);
                if (i>0 && VirtualMicroConsole.Textures[i-1] != null)
                {                   
                    Texture2D texture = VirtualMicroConsole.Textures[i-1];
                    if (texture.Width > texture.Height) size = 8f / texture.Width * 4;
                    else size = 8 / texture.Height * 4;             
                    spriteBatch.Draw(texture, pos, null, Color.White, 0f, Vector2.Zero, size, SpriteEffects.None, 0f);
                   
                }
                else if (i == 0) spriteBatch.Draw(VoidIconTexture, pos, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);

                // sélection
                if (i == CurrentID) spriteBatch.Draw(SelectionTexture, pos, Color.White);
            }

            // buttonds
            foreach (var b in UI)
            {
                b.Draw(spriteBatch);
            }


            // check window
            if (ToggleBin)
            {
                geo.draw_rect(Center.X - 150, Center.Y - 100, 300, 120, Color.White);
                geo.draw_rect(Center.X - 150 + 5, Center.Y - 100 + 5, 300 - 10, 120 - 10, Color.Black);
                spriteBatch.DrawString(systemFont, $"Proceed to delete Level{currentMap} ?", Center + new Vector2(-130, -80), Color.White);
                YesButton.Draw(spriteBatch);
                NoButton.Draw(spriteBatch);
            }
        }
        public override void Update(GameTime gt)
        {
            var ms = Mouse.GetState();

            if (ToggleBin == false)
            {
                foreach (var b in UI)
                {
                    b.Update();
                }

                // tile selection
                if (TileGridRect.Contains(mp()) && ms.LeftButton == ButtonState.Pressed && oldMS.LeftButton == ButtonState.Released)
                {
                    int col = (int)(mp().X - TileGridRect.X) / 32;
                    int line = (int)(mp().Y - TileGridRect.Y) / 32;
                    int oldID = CurrentID;
                    CurrentID = col + line * 4;
                    if (VirtualMicroConsole.Textures[CurrentID-1] == null) CurrentID = oldID;
                }

                // draw in the grid
                Rectangle mapRect = new Rectangle((int)GridOffset.X, (int)GridOffset.Y, 16 * TILESIZE, 16 * TILESIZE);
                if (brush == "pencil" && mapRect.Contains(mp()) && ms.LeftButton == ButtonState.Pressed)
                {
                    int col = (int)(mp().X - mapRect.X) / TILESIZE;
                    int line = (int)(mp().Y - mapRect.Y) / TILESIZE;
                    data[currentMap][line + (int)Camera.Y, col + (int)Camera.X] = CurrentID-1;
                }
                if (brush == "floodfill" && mapRect.Contains(mp()) && ms.LeftButton == ButtonState.Pressed)
                {
                    int col = (int)(mp().X - mapRect.X) / TILESIZE + (int)Camera.X;
                    int line = (int)(mp().Y - mapRect.Y) / TILESIZE + (int)Camera.Y;
                    FloodFill(data[currentMap], col, line);
                }

                // level selection
                bool isHooverSomething = false;
                for (int i = 0; i < NBLEVELS; i++)
                {
                    Rectangle levelRectHandler = new Rectangle((int)LevelsPosition.X, (int)LevelsPosition.Y + i * 24, 100, 24);
                    if (levelRectHandler.Contains(mp()))
                    {
                        currentMapHoover = i;
                        isHooverSomething = true;
                        if (Mouse.GetState().LeftButton == ButtonState.Pressed && oldMS.LeftButton == ButtonState.Released)
                        {
                            currentMap = i;
                            Camera = Vector2.Zero;
                        }
                    }
                }
                if (!isHooverSomething) currentMapHoover = -1;


                // map drag
                if (brush == "drag")
                {
                    if (Mouse.GetState().LeftButton == ButtonState.Pressed && oldMS.LeftButton == ButtonState.Released)
                    {
                        OldCamera = Camera;
                    }
                    if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                    {
                        MouseDrag += (ms.Position - oldMS.Position).ToVector2();
                        float coeff = 32;
                        Camera = OldCamera - new Vector2(MouseDrag.X / coeff, MouseDrag.Y / coeff);
                        Camera.X = Utils.clamp(Camera.X, 0, MAPSIZE);
                        Camera.Y = Utils.clamp(Camera.Y, 0, MAPSIZE);
                    }
                    else if (Mouse.GetState().LeftButton == ButtonState.Released)
                    {
                        MouseDrag = Vector2.Zero;
                    }
                }
            }
            else
            {
                YesButton.Update();
                NoButton.Update();
            }

            oldMS = ms;
        }
        public override void EndRun()
        {
        }

        public List<int[,]> GetMaps()
        {
            List<int[,]> copy = new List<int[,]>();
            foreach (var item in data)
            {
                copy.Add((int[,])item.Clone());
            }
            return copy;
        }

        // self methods
        private void Fill(int mapIndex, int tileID)
        {
            var map = data[mapIndex];
            for (int l = 0; l < map.GetLength(0); l++)
            {
                for (int c = 0; c < map.GetLength(1); c++)map[l, c] = tileID;
            }
        }
        private void BtnPencil(Button b)
        {
            brush = "pencil";
            SelectionPos = UI[0].Position;
            VirtualMicroConsole.ChangeMouseCursorTexture(1);
        }
        private void BtnPaste(Button b)
        {
            if (IDToCopy != -1)
            {
                data[currentMap] = (int[,])data[IDToCopy].Clone();
            }
            SelectionPos = UI[1].Position;
        }
        private void BtnCopy(Button b)
        {
            IDToCopy = currentMap;
            SelectionPos = UI[2].Position;
        }
        private void BtnGrid(Button b)
        {
            GridVisible = !GridVisible;
            if (GridVisible) b.Texture = GridTexture;
            else b.Texture = NoGridTexture;
        }
        private void BtnDrag(Button b)
        {
            brush = "drag";
            VirtualMicroConsole.ChangeMouseCursorTexture(3);
            SelectionPos = UI[5].Position;
        }
        private void BtnBin(Button b)
        {
            ToggleBin = true;
        }
        private void BtnYes(Button b)
        {
            ToggleBin = false;
            Fill(currentMap, -1);
        }
        private void BtnFloodFill(Button b)
        {
            brush = "floodfill";
            SelectionPos = UI[4].Position;
            VirtualMicroConsole.ChangeMouseCursorTexture(2);
        }
        private void BtnNo(Button b)
        {
            ToggleBin = false;
        }
        private int GetMapWidth()
        {
            return data[currentMap].GetLength(1);
        }
        private int GetMapHeight()
        {
            return data[currentMap].GetLength(0);
        }
        private void reset()
        {
            data = new List<int[,]>();
            for (int i = 0; i < NBLEVELS; i++)
            {
                data.Add(new int[MAPSIZE, MAPSIZE]);
                Fill(i, -1);
            }
        }
        private void FloodFill(int[,] grid, int startX, int startY)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);

            // On ne remplit que les zones noires (valeur 0)
            if (grid[startY, startX] == CurrentID-1)
                return;

            var startColor = grid[startY, startX];

            var queue = new Queue<(int, int)>();
            queue.Enqueue((startY, startX));
            grid[startY, startX] = CurrentID-1; // On le colore en blanc

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
                            grid[newY, newX] = CurrentID-1; // Devient blanc
                            queue.Enqueue((newY, newX));
                        }
                    }
                }
            }
        }
    }
}
