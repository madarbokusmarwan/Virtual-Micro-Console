using System.Collections.Generic;
using System;

public class MainGame : VirtualMicroConsole
{
    Player player;
    Ball ball;
    List<Brick> AllBricks;
    float camx;
    float camy;

    public override void Init()
    {
        camx = 0;
        camy = 0;

        // player
        player = new Player();
        player.x = (WIDTH - Player.w) / 2;
        player.y = HEIGHT - Player.h - 5;

        // ball
        ball = new Ball();
        ball.Activated = false;

        // bricks
        AllBricks = new List<Brick>();
        int offsetx = (WIDTH - 5 * Brick.w) / 2;
        int offsety = 12;
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var b = new Brick();
                b.x = offsetx + i * Brick.w;
                b.y = offsety + j * Brick.h;
                AllBricks.Add(b);
            }
        }
    }

    public override void Update30(float dt, float total_gametime)
    {
        if (player.life > 0)
        {
            // camera
            camx = camx * 0.5f;
            camy = camy * 0.5f;
            camto(camx, camy);

            // player
            int margin = 5;
            if (k_down(buttons.right)) player.x += Player.SPEED;
            if (k_down(buttons.left)) player.x -= Player.SPEED;
            player.x = clamp(player.x, margin, WIDTH - Player.w - margin);

            // ball
            if (!ball.Activated)
            {
                ball.x = player.x + Player.w / 2 - 4;
                ball.y = player.y - 8;
                if (k_pressed(buttons.A))
                {
                    ball.Activated = true;
                    ball.vy = -Ball.SPEED;
                }
            }
            else
            {
                ball.x += ball.vx;
                ball.y += ball.vy;
                if (collide_rect(ball.x, ball.y, Ball.ID, player.x, player.y, Player.ID))
                {
                    float a = angle(player.x + Player.w / 2, player.y + Player.h / 2, ball.x + 4, ball.y + 4);
                    ball.vx = (float)Math.Cos(a) * Ball.SPEED;
                    ball.vy = (float)Math.Sin(a) * Ball.SPEED;
                }
                if (ball.y > HEIGHT)
                {
                    snd(7);
                    ball.Activated = false;
                    player.life--;
                    if (player.life == 0) snd(16);
                    cam_shake(1, 0.5f);
                    ball.vx = 0;
                    ball.vy = 0;
                }
                if (ball.x <= 0 || ball.x + 8 >= WIDTH) ball.vx = -ball.vx;
                if (ball.y <= 0) ball.vy = -ball.vy;
                ball.x = clamp(0, ball.x, WIDTH - 9);
                ball.y = clamp(0, ball.y, HEIGHT - 9);

                // bricks
                foreach (var b in AllBricks)
                {
                    if (collide_rect(b.x, b.y, Brick.w, Brick.h, ball.x, ball.y, 8, 8))
                    {
                        float a = angle(b.x + Brick.w / 2, b.y + Brick.h / 2, ball.x + 4, ball.y + 4);
                        ball.vx = (float)Math.Cos(a) * Ball.SPEED;
                        ball.vy = (float)Math.Sin(a) * Ball.SPEED;
                        snd(19);
                        b.ToRemove = true;
                        break;
                    }
                }
                AllBricks.RemoveAll(b => b.ToRemove);
            }
        }
        else if (k_pressed(buttons.A)) Init();
    }
    public override void DrawGame(float dt, float total_gametime)
    {
        img(Player.ID, player.x, player.y);
        img(Ball.ID, ball.x, ball.y);
        foreach (var b in AllBricks)
        {
            rect(b.x, b.y, Brick.w-1, Brick.h-1);
        }      
    }
    public override void DrawUI(float dt, float total_gametime)
    {
        // gameover
        if (player.life == 0)
        {
            txt("GAME OVER", (WIDTH - txtw("GAME OVER")) / 2 + rnd(-1, 1), player.y - 40 + rnd(-1, 1));
            txt("\npress (A)", (WIDTH - txtw("press (A)")) / 2, player.y - 30);
        }
        for (int i = 0; i < 3; i++)
        {
            img(3, 10 + i * 8, 2);
            if (i <= player.life - 1) img(2, 10 + i * 8, 2);
        }
    }
}

public abstract class Sprite
{
    public float x;
    public float y;
}
public class Player : Sprite
{
    public const int w = 24;
    public const int h = 5;
    public int life = 3;
    public const float SPEED = 3;
    public const int ID = 0;
}
public class Ball : Sprite
{
    public float vx;
    public float vy;
    public bool Activated;
    public const float SPEED = 3.5f;
    public const int ID = 1;
}
public class Brick : Sprite
{
    public const int w = 15;
    public const int h = 5;
    public bool ToRemove = false;
}