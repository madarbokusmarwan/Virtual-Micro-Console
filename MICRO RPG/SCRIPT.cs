using System;

class MainGame : VirtualMicroConsole
{
    float x;
    float y;
    bool flip = false;
    bool avancing = false;
    float Timer;
    string message = "";

    public override void Init()
    {
        x = 10;
        y = 10;
        Timer = -0.1f;
        music(7, true);
    }

    public override void Update30(float dt, float total_gametime)
    {
        float speed = 3f;
        float oldX = x;
        float oldY = y;

        // movements
        if (k_down(buttons.right))
        {
            x += speed;
            flip = false;
        }
        if (k_down(buttons.left))
        {
            x -= speed;
            flip = true;
        }
        if (k_down(buttons.down)) y += speed;
        if (k_down(buttons.up)) y -= speed;

        if (x != oldX || y != oldY) avancing = true;
        else avancing = false;

        // map interactions
        int pcol = (int)x / 8;
        int pline = (int)y / 8;
        pline = Math.Max(pline, 2);
        pcol = Math.Max(pcol, 2);
        bool interact = false;
        for (int i = pline-2; i < pline+4; i++)
        {
            for (int j = pcol-2; j < pcol+4; j++)
            {
                int tile_id = mget(i, j, 9);
                int tx = j * 8;
                int ty = i * 8;
                if (collide_rect(x + 2, y + 4, 4, 16 - 4, tx, ty, 8, 8))
                {
                    // collisions
                    if (flag(tile_id, 0))
                    {
                        x = oldX;
                        y = oldY;
                    }
                    // open door
                    if (tile_id == 3)
                    {
                        snd(17);
                        mset(4, i, j, 9);
                    }
                }

                // pannel           
                if (tile_id == 6 && dist(x + 4, y + 8, tx + 4, ty + 4) < 12)
                {
                    interact = true;
                    message = "Welcome to\nDornwich !";
                }
                if (tile_id == 10 && dist(x + 4, y + 8, tx + 4, ty + 4) < 12)
                {
                    interact = true;
                    message = "What's up";
                }
                
                // close door
                if (tile_id == 4 && dist(x + 4, y + 8, tx + 4, ty + 4) > 20)
                {
                    snd(17);
                    mset(3, i, j, 9);
                }
            }
        }
        if (interact) Timer += 3*dt;
        else Timer -= 4*dt;
        Timer = clamp(Timer, 0, 1);

        // world border
        x = clamp(x, 0, WIDTH * 2 - 8);
        y = clamp(y, 0, HEIGHT * 2 - 16);

        // camera
        camto(WIDTH * (int)(x / WIDTH), HEIGHT * (int)((y+14) / HEIGHT));
    }

    public override void DrawGame(float dt, float total_gametime)
    {
        map(9);
        img(11, 30, HEIGHT + 60, 2, 1, total_gametime);
        // player
        if (avancing)img(7, x, y, 2, 1/8f, total_gametime, flipX:flip);
        else img(9, x, y, flipX: flip);
    }
    public override void DrawUI(float dt, float total_gametime)
    {
        // pannel message
        if (Timer >= 0)
        {
            int pw = (int)((WIDTH - 20) * Timer);
            int ph = (int)(28 * Timer);
            int px = (WIDTH - pw) / 2;
            int py = HEIGHT - ph - 5;
            rect(px, py, pw, ph, 2);
            if (Timer == 1) txt(message, px + 3, py + 3); 
        }
    }
}