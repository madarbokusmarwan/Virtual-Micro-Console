using Microsoft.Xna.Framework;

namespace VirtualMicroConsole
{
    class VirtualMicroConsole1 : VirtualMicroConsole
    {

        public override void Init()
        {
        }

        public override void Update30(float dt, float total_gametime)
        {
        }

        public override void DrawGame(float dt, float total_gametime)
        {
            int n = (int)(total_gametime / 0.5f) % 4;
            string t = "error" + new string('.', n);
            txt(t, (WIDTH - txtw(t))/2, HEIGHT/2);
        }

        public override void DrawUI(float dt, float total_gametime)
        {          
        }
    }
}
