using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace VirtualMicroConsole
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private VirtualMicroConsole vmc;
        public static bool ResetRequest = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;

            // window resize
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;

            // fix framerate to 30FPS
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);
            Environment.Exit(0);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            ResetGame();
            vmc.LoadingTimer = 1.5f;
        }

        protected override void EndRun()
        {
            vmc.EndRun();

            base.EndRun();
        }

        protected override void Update(GameTime gameTime)
        {
            vmc.Update(gameTime);
            if (ResetRequest)
            {
                ResetGame();
                OnResize(this, null);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            vmc.Draw(gameTime);
            base.Draw(gameTime);
        }

        private void OnResize(object sender, EventArgs e)
        {
            vmc.GameWidth = GraphicsDevice.Viewport.Width;
            vmc.GameHeight = GraphicsDevice.Viewport.Height;
        }
        private void ResetGame()
        {
            vmc = ScriptReader.LoadScripts(this);
            vmc.Load(_spriteBatch, _graphics.GraphicsDevice, Content, 800, 480, null);
            Game1.ResetRequest = false;
        }
    }
}
