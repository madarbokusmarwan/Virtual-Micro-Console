using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VirtualMicroConsole
{
    public class Inputs
    {
        // fields && properties ------------------
        private KeyboardState newKB, oldKB;
        private Dictionary<buttons, Keys> ButtonToKey;

        // constructor ----------------------
        public Inputs()
        {
            newKB = Keyboard.GetState();
            ButtonToKey = new Dictionary<buttons, Keys>();
            ButtonToKey[buttons.A] = Keys.F;
            ButtonToKey[buttons.B] = Keys.S;
            ButtonToKey[buttons.right] = Keys.Right;
            ButtonToKey[buttons.left] = Keys.Left;
            ButtonToKey[buttons.down] = Keys.Down;
            ButtonToKey[buttons.up] = Keys.Up;
        }

        // methods --------------------------
        public void Update()
        {
            oldKB = newKB;
            newKB = Keyboard.GetState();
        }
        public bool pressed(params buttons[] buttons)
        {
            foreach (var b in buttons)
            {
                Keys key = ButtonToKey[b];
                if (newKB.IsKeyDown(key) && oldKB.IsKeyUp(key))
                {
                    return true;
                }
            }
            return false;
        }
        public bool released(params buttons[] buttons)
        {
            foreach (var b in buttons)
            {
                Keys key = ButtonToKey[b];
                if (oldKB.IsKeyDown(key) && newKB.IsKeyUp(key))
                {
                    return true;
                }
            }
            return false;
        }
        public bool down(params buttons[] buttons)
        {
            foreach (var b in buttons)
            {
                Keys key = ButtonToKey[b];
                if (newKB.IsKeyDown(key))
                {
                    return true;
                }
            }
            return false;
        }
        public bool up(params buttons[] buttons)
        {
            foreach (var b in buttons)
            {
                Keys key = ButtonToKey[b];
                if (newKB.IsKeyUp(key))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public enum buttons
    {
        A,
        B,
        right,
        left,
        up,
        down
    }

}
