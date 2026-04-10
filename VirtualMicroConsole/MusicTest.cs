using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualMicroConsole
{
    public class MusicTest : GraphicInterface
    {
        private SoundManager soundManager;
        private Entry IDEntry;
        private int ID;
        private Button TrackChoice; // allow to choice to listen between sound effects & musics
        private Button PlayButton; 
        private Texture2D MusicBtn;
        private Texture2D SoundBtn;
        private Texture2D PlayTexture;
        private Texture2D PauseTexture;
        private int Seed;
        private FastNoise fastNoise;
        private double[] Line;
        private int Width;
        private string AudioTrack;
        private bool IsPlaying = false;
        private float PlayTimer;
        private float DurationTimer = 0;
        private string[] MusicNames;
        private string[] SoundNames;

        public MusicTest(SoundManager soundManager, ContentManager content)
        {
            this.soundManager = soundManager;
            AudioTrack = "sound";

            // perlin noise
            Seed = Utils.rnd(1, 10000);
            fastNoise = new FastNoise(Seed);
            Width = 300;
            Line = new double[Width];
            GenerateLine();

            // names
            MusicNames = new string[9];
            MusicNames[0] = "Adventure begins !";
            MusicNames[1] = "Battle";
            MusicNames[2] = "rush";
            MusicNames[3] = "Quiet Walk";
            MusicNames[4] = "Arcade";
            MusicNames[5] = "Spooky";
            MusicNames[6] = "lonely tree";
            MusicNames[7] = "No Destination";
            MusicNames[8] = "power of the past";

            SoundNames = new string[31];
            SoundNames[0] = "coin 1";
            SoundNames[1] = "coin 2";
            SoundNames[2] = "coin 3";
            SoundNames[3] = "laser 1";
            SoundNames[4] = "laser 2";
            SoundNames[5] = "laser 3";
            SoundNames[6] = "explosion 1";
            SoundNames[7] = "explosion 2";
            SoundNames[8] = "explosion 3";
            SoundNames[9] = "jump 1";
            SoundNames[10] = "jump 2";
            SoundNames[11] = "jump 3";
            SoundNames[12] = "power-up 1";
            SoundNames[13] = "power-up 2";
            SoundNames[14] = "lose 1";
            SoundNames[15] = "lose 2";
            SoundNames[16] = "lose 3";
            SoundNames[17] = "hurt 1";
            SoundNames[18] = "hurt 2";
            SoundNames[19] = "hurt 3";
            SoundNames[20] = "bip 1";
            SoundNames[21] = "bip 2";
            SoundNames[22] = "bip 3";
            SoundNames[23] = "bip 4";
            SoundNames[24] = "power-up 3";
        }

        // Interface methods
        public override void Load(SpriteBatch spriteBatch, GraphicsDevice graphics, ContentManager content, int screenWidth, int screenHeight, MousePosition mp)
        {
            base.Load(spriteBatch, graphics, content, screenWidth, screenHeight, mp);

            // textures
            MusicBtn = content.Load<Texture2D>("assets/btn_music");
            SoundBtn = content.Load<Texture2D>("assets/btn_sound");
            PlayTexture = content.Load<Texture2D>("assets/play_btn");
            PauseTexture = content.Load<Texture2D>("assets/pause_btn");

            // ui
            TrackChoice = new Button(SoundBtn, new Vector2(600, 100), ChangeAudioTrack, mp);
            PlayButton = new Button(PlayTexture, new Vector2((800 - PlayTexture.Width*2) / 2f, 300), Play, mp)
            {
                scale = 2
            };
            float x = 600 + systemFont.MeasureString("#ID : ").X + 8;
            IDEntry = new Entry(geo.GetRectTexture(45, 22, Color.Gray * 0.9f), new Vector2(x, 50-2), systemFont, mp, 2);
            IDEntry.alert = ChangeID;
        }
        public override void Draw(GameTime gameTime)
        {
            spriteBatch.DrawString(systemFont, "#ID : ", new Vector2(600, 50), Color.LightYellow);
            //spriteBatch.DrawString(systemFont, "#ID : ", new Vector2(20), Color.White);

            //ui
            TrackChoice.Draw(spriteBatch);
            IDEntry.Draw(spriteBatch, gameTime);
            PlayButton.Draw(spriteBatch);

            // sound waves
            int w = 10;
            int gap = 2;
            int realWidth = (Width + Width / w * gap);
            int offsetX = (800 - realWidth) / 2;
            int offsetY = 250;

            for (int i = 0; i < Width / w; i++)
            {
                float h = (int)(Line[i * 10] * 100) * PlayTimer;
                if (DurationTimer <= 0) h = 0;
                h = Math.Max(15, h);
                geo.draw_rect(offsetX + i * (w + gap), offsetY - h, w, (int)h, Color.White);
            }

            // music info (title + duration)
            string title = "";
            Color blueGray = new Color(173, 183, 204);
            if (AudioTrack == "sound") title = SoundNames[ID];
            else if (AudioTrack == "music")
            {
                title = MusicNames[ID];
                string duration = soundManager.GetMusic(ID).Duration.ToString().Substring(3, 5);
                string playPosition = MediaPlayer.PlayPosition.ToString().Substring(3, 5);
                spriteBatch.DrawString(systemFont, duration, new Vector2(offsetX + realWidth + 20, offsetY-15), blueGray);
                spriteBatch.DrawString(systemFont, playPosition, new Vector2(offsetX - 55, offsetY-15), blueGray);
            }
            spriteBatch.DrawString(systemFont, title, new Vector2((800 - systemFont.MeasureString(title).X) / 2, offsetY + 10), blueGray);

        }       
        public override void Update(GameTime gameTime)
        {
            // sound waves
            Seed += 5;
            GenerateLine();
            if (IsPlaying) PlayTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            PlayTimer = Math.Min(1, PlayTimer);
            DurationTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            // UI
            IDEntry.Update(gameTime);
            TrackChoice.Update();
            PlayButton.Update();
        }
        public override void EndRun()
        {
            soundManager.Stop();
        }

        // UI
        private void ChangeAudioTrack(Button b)
        {
            if (AudioTrack == "music")
            {
                AudioTrack = "sound";
                TrackChoice.Texture = SoundBtn;
            }
            else
            {
                AudioTrack = "music";
                TrackChoice.Texture = MusicBtn;
            }
            ID = 0;
            DurationTimer = 0;
            PlayTimer = 0;
            soundManager.Stop();
        }
        private void ChangeID(string data)
        {
            int.TryParse(data, out ID);
            if (AudioTrack == "sound") ID = Math.Clamp(ID, 0, 23);
            else ID = Math.Clamp(ID, 0, 7);
        }
        private void Play(Button b)
        {
            IsPlaying = !IsPlaying;
            if (IsPlaying == true)
            {
                PlayButton.Texture = PauseTexture;
                if (AudioTrack == "sound")
                {
                    PlayTimer = 0.8f;
                    soundManager.PlaySound(ID);
                    DurationTimer = (float)soundManager.GetSound(ID).Duration.TotalSeconds;
                }
                else
                {
                    soundManager.PlayMusic(ID);
                    DurationTimer = (float)soundManager.GetMusic(ID).Duration.TotalSeconds;
                }              
            }
            else
            {
                PlayButton.Texture = PlayTexture;
                PlayTimer = 0;
                DurationTimer = 0;
                if (AudioTrack == "music") soundManager.Pause();
            }
        }

        // other
        private string GetNameByID(int id)
        {
            return "error";
        }
        private void GenerateLine()
        {
            float min = -0.71277815f;
            float max = 0.71277815f;

            // déplacement de la courbe en fonction de la seed qui augmente
            for (int c = 0 + (int)Seed; c < Width + (int)Seed; c++)
            {
                var noise = fastNoise.GetNoise(c, Seed);
                var normalize = Normalise(noise, min, max); // mettre les valeurs de la courbe entre 0 et 1
                Line[c - (int)Seed] = normalize;
            }
        }
        private float Normalise(float value, float min, float max)
        {
            return (value - min) / (max - min); // give back a value between 0 and 1
        }
    }
}
