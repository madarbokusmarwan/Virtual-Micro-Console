using Microsoft.Xna.Framework;

namespace VirtualMicroConsole
{
    public class Camera
    {
        // fields & properties ------------------------
        public Vector2 pos { get; private set; }
        private int _shakingForce;
        private float _shakingDuration;
        private Vector2 _shaking;

        // constructor ----------------------
        public Camera()
        {
            pos = Vector2.Zero;
            _shaking = Vector2.Zero;
        }

        // methods ----------------------------        
        public void Update(GameTime gameTime)
        {
            if (_shakingDuration > 0)
            {
                _shakingDuration -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                _shaking = new Vector2(Utils.rnd(-_shakingForce, _shakingForce),
                    Utils.rnd(-_shakingForce, _shakingForce));               
            }
            else
            {
                _shaking = Vector2.Zero;
            }

            pos.Floor();
        }
        public Matrix ToMatrix()
        {
            return Matrix.CreateTranslation(new Vector3(-pos + _shaking, 0)) * Matrix.CreateRotationZ(0) * 
                Matrix.CreateScale(Vector3.One);
        }

        public void move(float vx, float vy)
        {
            pos += new Vector2(vx, vy);
        }
        public void moveAt(float x, float y)
        {
            pos = new Vector2(x, y);
        }
        public void shake(int force, float duration)
        {
            _shakingForce = force;
            _shakingDuration = duration;
        }
    }
}
