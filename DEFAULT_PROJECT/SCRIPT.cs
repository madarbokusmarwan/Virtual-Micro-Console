class MainGame : VirtualMicroConsole
{
    float x;
    float y;

    public override void Init()
    {
        x = (WIDTH - 16)/2f;
        y = 20;
    }

    public override void Update30(float dt, float total_gametime)
    {
        float speed = 2f;
        if (k_down(buttons.right)) x+=speed;
        if (k_down(buttons.left)) x-=speed;
        if (k_down(buttons.down)) y+=speed;
        if (k_down(buttons.up)) y-=speed;
        x = clamp(x, 0, WIDTH - 16);
        y = clamp(y, 0, HEIGHT - 16);
    }

    public override void DrawGame(float dt, float total_gametime)
    {
        string t = "Hello World !";
        img(0, x, y, 2, 1/4f, total_gametime, 2);
        txt(t, (WIDTH - txtw(t)) / 2f, 50);
    }
    public override void DrawUI(float dt, float total_gametime)
    {
        // This function ignores camera offset
    }
}

/* ---------------- Advcanced Graphics ----------------------
 * img(int id, float x, float y, int scale = 1, float rotation=0, bool flipX = false, bool flipY = false)
 * img(int id, float x, float y, int nbFrames, float speed, GameTime gt, int scale=1, float rotation=0, bool flipX=false, bool flipY=false) => [for animations :D]
 * txt(string text, float x, float y, float size = 1)
 * map(int lvl=0, float x=0, float y=0) => [x and y refer to the offset]
 * 
 * ---------------- Basic geometry ----------------------
 * setColor(int color) => [0 for black, 1 for white, only active for primitives]
 * pixel(float x, float y)
 * rect(float x, float y, int w, int h)
 * rect(float x, float y, int w, int h, int thickness) => [outline rect]
 * circ(float x, float y, int r)
 * circ(float x, float y, int r, int thickness)
 * line(float x1, float y1, float x2, float y2, int thickness)
 * 
 * ---------------- Inputs ----------------------
 * k_pressed(params buttons[] buttons)
 * k_released(params buttons[] buttons)
 * k_down(params buttons[] buttons)
 * k_up(params buttons[] buttons)
 * buttons = {A, B, right, left, down, up} => 'buttons' refer to an enum, exemple : k_pressed(buttons.A)
 * 
 * ---------------- Audio ----------------------
 * snd(int id)
 * music(int id, bool loop = false)
 * 
 * ---------------- Camera ----------------------
 * cam(float vx, float vy)
 * camto(float x, float y)
 * cam_shake(int force, float duration)
 * 
 * ---------------- Map ----------------------
 * mget(int line, int col, int lvl = 0) => [get the id of a tile]
 * mset(int newID, int line, int col, int lvl = 0) => [change a tile in any map]
 * flag(int tile_id, int flag) => [check if a tile contains a specific flag]
 * 
 * ---------------- Maths ----------------------
 * rnd(int min, int max) => [random number]
 * clamp(float value, float min, float max) => [restrict a value between two extremums]
 * dist(float x1, float y1, float x2, float y2) => [return the distance between two points]
 * angle(float x1, float y1, float x2, float y2) => [return the angle between two points]
 * collide_rect(float x1, float y1, float w1, float h1, float x2, float y2, float w2, float h2)
 * collide_rect(float x1, float y1, int ID1, float h1, float x2, float y2, int ID2)
 * 
 * ---------------- Other ----------------------
 * txtw(string text) => [return the width of a text]
 * WIDTH => [const wich refet to screen width]
 * HEIGHT => [const wich refet to screen height]
 */

