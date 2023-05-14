// Decompiled with JetBrains decompiler
// Type: Celeste.Pico8.Classic
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Celeste.Pico8
{
    public class Classic
    {
        public Emulator E;
        private Point room;
        private List<Classic.ClassicObject> objects;
        public int freeze;
        private int shake;
        private bool will_restart;
        private int delay_restart;
        private HashSet<int> got_fruit;
        private bool has_dashed;
        private int sfx_timer;
        private bool has_key;
        private bool pause_player;
        private bool flash_bg;
        private int music_timer;
        private bool new_bg;
        private readonly int k_left;
        private readonly int k_right = 1;
        private readonly int k_up = 2;
        private readonly int k_down = 3;
        private readonly int k_jump = 4;
        private readonly int k_dash = 5;
        private int frames;
        private int seconds;
        private int minutes;
        private int deaths;
        private int max_djump;
        private bool start_game;
        private int start_game_flash;
        private bool room_just_loaded;
        private List<Classic.Cloud> clouds;
        private List<Classic.Particle> particles;
        private List<Classic.DeadParticle> dead_particles;

        public void Init(Emulator emulator)
        {
            E = emulator;
            room = new Point(0, 0);
            objects = new List<Classic.ClassicObject>();
            freeze = 0;
            will_restart = false;
            delay_restart = 0;
            got_fruit = new HashSet<int>();
            has_dashed = false;
            sfx_timer = 0;
            has_key = false;
            pause_player = false;
            flash_bg = false;
            music_timer = 0;
            new_bg = false;
            room_just_loaded = false;
            frames = 0;
            seconds = 0;
            minutes = 0;
            deaths = 0;
            max_djump = 1;
            start_game = false;
            start_game_flash = 0;
            clouds = new List<Classic.Cloud>();
            for (int index = 0; index <= 16; ++index)
            {
                clouds.Add(new Classic.Cloud()
                {
                    x = E.rnd(128f),
                    y = E.rnd(128f),
                    spd = 1f + E.rnd(4f),
                    w = 32f + E.rnd(32f)
                });
            }

            particles = new List<Classic.Particle>();
            for (int index = 0; index <= 32; ++index)
            {
                particles.Add(new Classic.Particle()
                {
                    x = E.rnd(128f),
                    y = E.rnd(128f),
                    s = E.flr(E.rnd(5f) / 4f),
                    spd = 0.25f + E.rnd(5f),
                    off = E.rnd(1f),
                    c = 6 + E.flr(0.5f + E.rnd(1f))
                });
            }

            dead_particles = new List<Classic.DeadParticle>();
            title_screen();
        }

        private void title_screen()
        {
            got_fruit = new HashSet<int>();
            frames = 0;
            deaths = 0;
            max_djump = 1;
            start_game = false;
            start_game_flash = 0;
            E.music(40, 0, 7);
            load_room(7, 3);
        }

        private void begin_game()
        {
            frames = 0;
            seconds = 0;
            minutes = 0;
            music_timer = 0;
            start_game = false;
            E.music(0, 0, 7);
            load_room(0, 0);
        }

        private int level_index()
        {
            return (room.X % 8) + (room.Y * 8);
        }

        private bool is_title()
        {
            return level_index() == 31;
        }

        private void psfx(int num)
        {
            if (sfx_timer > 0)
            {
                return;
            }

            E.sfx(num);
        }

        private void draw_player(Classic.ClassicObject obj, int djump)
        {
            int num = 0;
            switch (djump)
            {
                case 0:
                    num = 128;
                    break;
                case 2:
                    num = E.flr(frames / 3 % 2) != 0 ? 144 : 160;
                    break;
            }
            E.spr(obj.spr + num, obj.x, obj.y, flipX: obj.flipX, flipY: obj.flipY);
        }

        private void break_spring(Classic.spring obj)
        {
            obj.hide_in = 15;
        }

        private void break_fall_floor(Classic.fall_floor obj)
        {
            if (obj.state != 0)
            {
                return;
            }

            psfx(15);
            obj.state = 1;
            obj.delay = 15;
            _ = init_object<Classic.smoke>(new Classic.smoke(), obj.x, obj.y);
            Classic.spring spring = obj.collide<Classic.spring>(0, -1);
            if (spring == null)
            {
                return;
            }

            break_spring(spring);
        }

        private T init_object<T>(T obj, float x, float y, int? tile = null) where T : Classic.ClassicObject
        {
            objects.Add(obj);
            if (tile.HasValue)
            {
                obj.spr = tile.Value;
            }

            obj.x = (int)x;
            obj.y = (int)y;
            obj.init(this, E);
            return obj;
        }

        private void destroy_object(Classic.ClassicObject obj)
        {
            int index = objects.IndexOf(obj);
            if (index < 0)
            {
                return;
            }

            objects[index] = null;
        }

        private void kill_player(Classic.player obj)
        {
            sfx_timer = 12;
            E.sfx(0);
            ++deaths;
            shake = 10;
            destroy_object(obj);
            //Stats.Increment(Stat.PICO_DEATHS);
            dead_particles.Clear();
            for (int index = 0; index <= 7; ++index)
            {
                float a = index / 8f;
                dead_particles.Add(new Classic.DeadParticle()
                {
                    x = obj.x + 4f,
                    y = obj.y + 4f,
                    t = 10,
                    spd = new Vector2(E.cos(a) * 3f, E.sin(a + 0.5f) * 3f)
                });
            }
            restart_room();
        }

        private void restart_room()
        {
            will_restart = true;
            delay_restart = 15;
        }

        private void next_room()
        {
            if (room.X == 2 && room.Y == 1)
            {
                E.music(30, 500, 7);
            }
            else if (room.X == 3 && room.Y == 1)
            {
                E.music(20, 500, 7);
            }
            else if (room.X == 4 && room.Y == 2)
            {
                E.music(30, 500, 7);
            }
            else if (room.X == 5 && room.Y == 3)
            {
                E.music(30, 500, 7);
            }

            if (room.X == 7)
            {
                load_room(0, room.Y + 1);
            }
            else
            {
                load_room(room.X + 1, room.Y);
            }
        }

        public void load_room(int x, int y)
        {
            room_just_loaded = true;
            has_dashed = false;
            has_key = false;
            for (int index = 0; index < objects.Count; ++index)
            {
                objects[index] = null;
            }

            room.X = x;
            room.Y = y;
            for (int index1 = 0; index1 <= 15; ++index1)
            {
                for (int index2 = 0; index2 <= 15; ++index2)
                {
                    int num = E.mget((room.X * 16) + index1, (room.Y * 16) + index2);
                    switch (num)
                    {
                        case 11:
                            init_object<Classic.platform>(new Classic.platform(), index1 * 8, index2 * 8).dir = -1f;
                            break;
                        case 12:
                            init_object<Classic.platform>(new Classic.platform(), index1 * 8, index2 * 8).dir = 1f;
                            break;
                        default:
                            Classic.ClassicObject classicObject = null;
                            if (num == 1)
                            {
                                classicObject = new Classic.player_spawn();
                            }
                            else if (num == 18)
                            {
                                classicObject = new Classic.spring();
                            }
                            else if (num == 22)
                            {
                                classicObject = new Classic.balloon();
                            }
                            else if (num == 23)
                            {
                                classicObject = new Classic.fall_floor();
                            }
                            else if (num == 86)
                            {
                                classicObject = new Classic.message();
                            }
                            else if (num == 96)
                            {
                                classicObject = new Classic.big_chest();
                            }
                            else if (num == 118)
                            {
                                classicObject = new Classic.flag();
                            }
                            else if (!got_fruit.Contains(1 + level_index()))
                            {
                                switch (num)
                                {
                                    case 8:
                                        classicObject = new Classic.key();
                                        break;
                                    case 20:
                                        classicObject = new Classic.chest();
                                        break;
                                    case 26:
                                        classicObject = new Classic.fruit();
                                        break;
                                    case 28:
                                        classicObject = new Classic.fly_fruit();
                                        break;
                                    case 64:
                                        classicObject = new Classic.fake_wall();
                                        break;
                                }
                            }
                            if (classicObject != null)
                            {
                                _ = init_object<Classic.ClassicObject>(classicObject, index1 * 8, index2 * 8, new int?(num));
                                break;
                            }
                            break;
                    }
                }
            }
            if (is_title())
            {
                return;
            }

            _ = init_object<Classic.room_title>(new Classic.room_title(), 0.0f, 0.0f);
        }

        public void Update()
        {
            frames = (frames + 1) % 30;
            if (frames == 0 && level_index() < 30)
            {
                seconds = (seconds + 1) % 60;
                if (seconds == 0)
                {
                    ++minutes;
                }
            }
            if (music_timer > 0)
            {
                --music_timer;
                if (music_timer <= 0)
                {
                    E.music(10, 0, 7);
                }
            }
            if (sfx_timer > 0)
            {
                --sfx_timer;
            }

            if (freeze > 0)
            {
                --freeze;
            }
            else
            {
                if (shake > 0 && Settings.Instance.ScreenShake != ScreenshakeAmount.Off)
                {
                    --shake;
                    E.camera();
                    if (shake > 0)
                    {
                        if (Settings.Instance.ScreenShake == ScreenshakeAmount.On)
                        {
                            E.camera(E.rnd(5f) - 2f, E.rnd(5f) - 2f);
                        }
                        else
                        {
                            E.camera(E.rnd(3f) - 1f, E.rnd(3f) - 1f);
                        }
                    }
                }
                if (will_restart && delay_restart > 0)
                {
                    --delay_restart;
                    if (delay_restart <= 0)
                    {
                        will_restart = true;
                        load_room(room.X, room.Y);
                    }
                }
                room_just_loaded = false;
                int num = 0;
                while (num != -1)
                {
                    int index = num;
                    num = -1;
                    for (; index < objects.Count; ++index)
                    {
                        Classic.ClassicObject classicObject = objects[index];
                        if (classicObject != null)
                        {
                            classicObject.move(classicObject.spd.X, classicObject.spd.Y);
                            classicObject.update();
                            if (room_just_loaded)
                            {
                                room_just_loaded = false;
                                num = index;
                                break;
                            }
                        }
                    }
                    while (objects.IndexOf(null) >= 0)
                    {
                        _ = objects.Remove(null);
                    }
                }
                if (!is_title())
                {
                    return;
                }

                if (!start_game && (E.btn(k_jump) || E.btn(k_dash)))
                {
                    E.music(-1, 0, 0);
                    start_game_flash = 50;
                    start_game = true;
                    E.sfx(38);
                }
                if (!start_game)
                {
                    return;
                }

                --start_game_flash;
                if (start_game_flash > -30)
                {
                    return;
                }

                begin_game();
            }
        }

        public void Draw()
        {
            E.pal();
            if (start_game)
            {
                int b = 10;
                if (start_game_flash > 10)
                {
                    if (frames % 10 < 5)
                    {
                        b = 7;
                    }
                }
                else
                {
                    b = start_game_flash <= 5 ? (start_game_flash <= 0 ? 0 : 1) : 2;
                }

                if (b < 10)
                {
                    E.pal(6, b);
                    E.pal(12, b);
                    E.pal(13, b);
                    E.pal(5, b);
                    E.pal(1, b);
                    E.pal(7, b);
                }
            }
            int c = 0;
            if (flash_bg)
            {
                c = frames / 5;
            }
            else if (new_bg)
            {
                c = 2;
            }

            E.rectfill(0.0f, 0.0f, 128f, 128f, c);
            if (!is_title())
            {
                foreach (Classic.Cloud cloud in clouds)
                {
                    cloud.x += cloud.spd;
                    E.rectfill(cloud.x, cloud.y, cloud.x + cloud.w, (float)(cloud.y + 4.0 + ((1.0 - (cloud.w / 64.0)) * 12.0)), new_bg ? 14f : 1f);
                    if (cloud.x > 128.0)
                    {
                        cloud.x = -cloud.w;
                        cloud.y = E.rnd(120f);
                    }
                }
            }
            E.map(room.X * 16, room.Y * 16, 0, 0, 16, 16, 2);
            for (int index = 0; index < objects.Count; ++index)
            {
                Classic.ClassicObject classicObject = objects[index];
                switch (classicObject)
                {
                    case Classic.platform _:
                    case Classic.big_chest _:
                        draw_object(classicObject);
                        break;
                }
            }
            E.map(room.X * 16, room.Y * 16, is_title() ? -4 : 0, 0, 16, 16, 1);
            for (int index = 0; index < objects.Count; ++index)
            {
                Classic.ClassicObject classicObject = objects[index];
                switch (classicObject)
                {
                    case null:
                    case Classic.platform _:
                    case Classic.big_chest _:
                        continue;
                    default:
                        draw_object(classicObject);
                        continue;
                }
            }
            E.map(room.X * 16, room.Y * 16, 0, 0, 16, 16, 3);
            foreach (Classic.Particle particle in particles)
            {
                particle.x += particle.spd;
                particle.y += E.sin(particle.off);
                particle.off += E.min(0.05f, particle.spd / 32f);
                E.rectfill(particle.x, particle.y, particle.x + particle.s, particle.y + particle.s, particle.c);
                if (particle.x > 132.0)
                {
                    particle.x = -4f;
                    particle.y = E.rnd(128f);
                }
            }
            for (int index = dead_particles.Count - 1; index >= 0; --index)
            {
                Classic.DeadParticle deadParticle = dead_particles[index];
                deadParticle.x += deadParticle.spd.X;
                deadParticle.y += deadParticle.spd.Y;
                --deadParticle.t;
                if (deadParticle.t <= 0)
                {
                    dead_particles.RemoveAt(index);
                }

                E.rectfill(deadParticle.x - (deadParticle.t / 5), deadParticle.y - (deadParticle.t / 5), deadParticle.x + (deadParticle.t / 5), deadParticle.y + (deadParticle.t / 5), 14 + (deadParticle.t % 2));
            }
            E.rectfill(-5f, -5f, -1f, 133f, 0.0f);
            E.rectfill(-5f, -5f, 133f, -1f, 0.0f);
            E.rectfill(-5f, 128f, 133f, 133f, 0.0f);
            E.rectfill(128f, -5f, 133f, 133f, 0.0f);
            if (is_title())
            {
                E.print("press button", 42f, 96f, 5f);
            }

            if (level_index() != 30)
            {
                return;
            }

            Classic.ClassicObject classicObject1 = null;
            foreach (Classic.ClassicObject classicObject2 in objects)
            {
                if (classicObject2 is Classic.player)
                {
                    classicObject1 = classicObject2;
                    break;
                }
            }
            if (classicObject1 == null)
            {
                return;
            }

            float x2 = E.min(24f, 40f - E.abs((float)(classicObject1.x + 4.0 - 64.0)));
            E.rectfill(0.0f, 0.0f, x2, 128f, 0.0f);
            E.rectfill(128f - x2, 0.0f, 128f, 128f, 0.0f);
        }

        private void draw_object(Classic.ClassicObject obj)
        {
            obj.draw();
        }

        private void draw_time(int x, int y)
        {
            int seconds = this.seconds;
            int num1 = minutes % 60;
            int num2 = E.flr(minutes / 60);
            E.rectfill(x, y, x + 32, y + 6, 0.0f);
            E.print((num2 < 10 ?
                "0" : (object)
                "").ToString() + num2 + ":" + (num1 < 10 ?
                "0" : (object)
                "") + num1 + ":" + (seconds < 10 ?
                "0" : (object)
                "") + seconds, x + 1, y + 1, 7f);
        }

        private float clamp(float val, float a, float b)
        {
            return E.max(a, E.min(b, val));
        }

        private float appr(float val, float target, float amount)
        {
            return (double)val <= (double)target ? E.min(val + amount, target) : E.max(val - amount, target);
        }

        private int sign(float v)
        {
            return (double)v > 0.0 ? 1 : (double)v >= 0.0 ? 0 : -1;
        }

        private bool maybe()
        {
            return (double)E.rnd(1f) < 0.5;
        }

        private bool solid_at(float x, float y, float w, float h)
        {
            return tile_flag_at(x, y, w, h, 0);
        }

        private bool ice_at(float x, float y, float w, float h)
        {
            return tile_flag_at(x, y, w, h, 4);
        }

        private bool tile_flag_at(float x, float y, float w, float h, int flag)
        {
            for (int x1 = (int)E.max(0.0f, E.flr(x / 8f));
                x1 <= (double)E.min(15f, (float)(((double)x + (double)w - 1.0) / 8.0)); ++x1)
            {
                for (int y1 = (int)E.max(0.0f, E.flr(y / 8f));
                    y1 <= (double)E.min(15f, (float)(((double)y + (double)h - 1.0) / 8.0)); ++y1)
                {
                    if (E.fget(tile_at(x1, y1), flag))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int tile_at(int x, int y)
        {
            return E.mget((room.X * 16) + x, (room.Y * 16) + y);
        }

        private bool spikes_at(float x, float y, int w, int h, float xspd, float yspd)
        {
            for (int x1 = (int)E.max(0.0f, E.flr(x / 8f));
                x1 <= (double)E.min(15f, (float)(((double)x + w - 1.0) / 8.0)); ++x1)
            {
                for (int y1 = (int)E.max(0.0f, E.flr(y / 8f));
                    y1 <= (double)E.min(15f, (float)(((double)y + h - 1.0) / 8.0)); ++y1)
                {
                    int num = tile_at(x1, y1);
                    if ((num == 17 && ((double)E.mod((float)((double)y + h - 1.0), 8f) >= 6.0 || (double)y + h == (y1 * 8) + 8) && (double)yspd >= 0.0) || (num == 27 && (double)E.mod(y, 8f) <= 2.0 && (double)yspd <= 0.0) || (num == 43 && (double)E.mod(x, 8f) <= 2.0 && (double)xspd <= 0.0) || (num == 59 && (((double)x + w - 1.0) % 8.0 >= 6.0 || (double)x + w == (x1 * 8) + 8) && (double)xspd >= 0.0))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private class Cloud
        {
            public float x;
            public float y;
            public float spd;
            public float w;
        }

        private class Particle
        {
            public float x;
            public float y;
            public int s;
            public float spd;
            public float off;
            public int c;
        }

        private class DeadParticle
        {
            public float x;
            public float y;
            public int t;
            public Vector2 spd;
        }

        public class player : Classic.ClassicObject
        {
            public bool p_jump;
            public bool p_dash;
            public int grace;
            public int jbuffer;
            public int djump;
            public int dash_time;
            public int dash_effect_time;
            public Vector2 dash_target = new(0.0f, 0.0f);
            public Vector2 dash_accel = new(0.0f, 0.0f);
            public float spr_off;
            public bool was_on_ground;
            public Classic.player_hair hair;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                spr = 1f;
                djump = g.max_djump;
                hitbox = new Rectangle(1, 3, 6, 5);
            }

            public override void update()
            {
                if (G.pause_player)
                {
                    return;
                }

                int ox = E.btn(G.k_right) ? 1 : (E.btn(G.k_left) ? -1 : 0);
                if (G.spikes_at(x + hitbox.X, y + hitbox.Y, hitbox.Width, hitbox.Height, spd.X, spd.Y))
                {
                    G.kill_player(this);
                }

                if (y > 128.0)
                {
                    G.kill_player(this);
                }

                bool flag1 = is_solid(0, 1);
                bool flag2 = is_ice(0, 1);
                if (flag1 && !was_on_ground)
                {
                    _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y + 4f);
                }

                int num1 = !E.btn(G.k_jump) ? 0 : (!p_jump ? 1 : 0);
                p_jump = E.btn(G.k_jump);
                if (num1 != 0)
                {
                    jbuffer = 4;
                }
                else if (jbuffer > 0)
                {
                    --jbuffer;
                }

                bool flag3 = E.btn(G.k_dash) && !p_dash;
                p_dash = E.btn(G.k_dash);
                if (flag1)
                {
                    grace = 6;
                    if (djump < G.max_djump)
                    {
                        G.psfx(54);
                        djump = G.max_djump;
                    }
                }
                else if (grace > 0)
                {
                    --grace;
                }

                --dash_effect_time;
                if (dash_time > 0)
                {
                    _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y);
                    --dash_time;
                    spd.X = G.appr(spd.X, dash_target.X, dash_accel.X);
                    spd.Y = G.appr(spd.Y, dash_target.Y, dash_accel.Y);
                }
                else
                {
                    int num2 = 1;
                    float amount1 = 0.6f;
                    float amount2 = 0.15f;
                    if (!flag1)
                    {
                        amount1 = 0.4f;
                    }
                    else if (flag2)
                    {
                        amount1 = 0.05f;
                        if (ox == (flipX ? -1 : 1))
                        {
                            amount1 = 0.05f;
                        }
                    }
                    spd.X = (double)E.abs(spd.X) > num2
                        ? G.appr(spd.X, E.sign(spd.X) * num2, amount2)
                        : G.appr(spd.X, ox * num2, amount1);
                    if (spd.X != 0.0)
                    {
                        flipX = spd.X < 0.0;
                    }

                    float target = 2f;
                    float amount3 = 0.21f;
                    if ((double)E.abs(spd.Y) <= 0.15000000596046448)
                    {
                        amount3 *= 0.5f;
                    }

                    if (ox != 0 && is_solid(ox, 0) && !is_ice(ox, 0))
                    {
                        target = 0.4f;
                        if ((double)E.rnd(10f) < 2.0)
                        {
                            _ = G.init_object<Classic.smoke>(new Classic.smoke(), x + (ox * 6), y);
                        }
                    }
                    if (!flag1)
                    {
                        spd.Y = G.appr(spd.Y, target, amount3);
                    }

                    if (jbuffer > 0)
                    {
                        if (grace > 0)
                        {
                            G.psfx(1);
                            jbuffer = 0;
                            grace = 0;
                            spd.Y = -2f;
                            _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y + 4f);
                        }
                        else
                        {
                            int num3 = is_solid(-3, 0) ? -1 : (is_solid(3, 0) ? 1 : 0);
                            if (num3 != 0)
                            {
                                G.psfx(2);
                                jbuffer = 0;
                                spd.Y = -2f;
                                spd.X = -num3 * (num2 + 1);
                                if (!is_ice(num3 * 3, 0))
                                {
                                    _ = G.init_object<Classic.smoke>(new Classic.smoke(), x + (num3 * 6), y);
                                }
                            }
                        }
                    }
                    int num4 = 5;
                    float num5 = num4 * 0.707106769f;
                    if (djump > 0 & flag3)
                    {
                        _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y);
                        --djump;
                        dash_time = 4;
                        G.has_dashed = true;
                        dash_effect_time = 10;
                        int num6 = E.dashDirectionX(flipX ? -1 : 1);
                        int num7 = E.dashDirectionY(flipX ? -1 : 1);
                        if (num6 != 0 && num7 != 0)
                        {
                            spd.X = num6 * num5;
                            spd.Y = num7 * num5;
                        }
                        else if (num6 != 0)
                        {
                            spd.X = num6 * num4;
                            spd.Y = 0.0f;
                        }
                        else if (num7 != 0)
                        {
                            spd.X = 0.0f;
                            spd.Y = num7 * num4;
                        }
                        else
                        {
                            spd.X = flipX ? -1f : 1f;
                            spd.Y = 0.0f;
                        }
                        G.psfx(3);
                        G.freeze = 2;
                        G.shake = 6;
                        dash_target.X = 2 * E.sign(spd.X);
                        dash_target.Y = 2 * E.sign(spd.Y);
                        dash_accel.X = 1.5f;
                        dash_accel.Y = 1.5f;
                        if (spd.Y < 0.0)
                        {
                            dash_target.Y *= 0.75f;
                        }

                        if (spd.Y != 0.0)
                        {
                            dash_accel.X *= 0.707106769f;
                        }

                        if (spd.X != 0.0)
                        {
                            dash_accel.Y *= 0.707106769f;
                        }
                    }
                    else if (flag3 && djump <= 0)
                    {
                        G.psfx(9);
                        _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y);
                    }
                }
                spr_off += 0.25f;
                spr = !flag1
                    ? is_solid(ox, 0) ? 5f : 3f
                    : E.btn(G.k_down)
                        ? 6f
                        : E.btn(G.k_up) ? 7f : spd.X == 0.0 || (!E.btn(G.k_left) && !E.btn(G.k_right)) ? 1f : (float)(1.0 + (spr_off % 4.0));

                if (y < -4.0 && G.level_index() < 30)
                {
                    G.next_room();
                }

                was_on_ground = flag1;
            }

            public override void draw()
            {
                if ((double)x is < (-1.0) or > 121.0)
                {
                    x = G.clamp(x, -1f, 121f);
                    spd.X = 0.0f;
                }
                hair.draw_hair(this, flipX ? -1 : 1, djump);
                G.draw_player(this, djump);
            }
        }

        public class player_hair
        {
            private readonly Classic.player_hair.node[] hair = new Classic.player_hair.node[5];
            private readonly Emulator E;
            private readonly Classic G;

            public player_hair(Classic.ClassicObject obj)
            {
                E = obj.E;
                G = obj.G;
                for (int index = 0; index <= 4; ++index)
                {
                    hair[index] = new Classic.player_hair.node()
                    {
                        x = obj.x,
                        y = obj.y,
                        size = E.max(1f, E.min(2f, 3 - index))
                    };
                }
            }

            public void draw_hair(Classic.ClassicObject obj, int facing, int djump)
            {
                int num = djump switch
                {
                    1 => 8,
                    2 => 7 + (E.flr(G.frames / 3 % 2) * 4),
                    _ => 12,
                };
                int c = num;
                Vector2 vector2 = new(obj.x + 4f - (facing * 2), obj.y + (E.btn(G.k_down) ? 4f : 3f));
                foreach (Classic.player_hair.node node in hair)
                {
                    node.x += (float)((vector2.X - (double)node.x) / 1.5);
                    node.y += (float)((vector2.Y + 0.5 - node.y) / 1.5);
                    E.circfill(node.x, node.y, node.size, c);
                    vector2 = new Vector2(node.x, node.y);
                }
            }

            private class node
            {
                public float x;
                public float y;
                public float size;
            }
        }

        public class player_spawn : Classic.ClassicObject
        {
            private Vector2 target;
            private int state;
            private int delay;
            private Classic.player_hair hair;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                spr = 3f;
                target = new Vector2(x, y);
                y = 128f;
                spd.Y = -4f;
                state = 0;
                delay = 0;
                solids = false;
                hair = new Classic.player_hair(this);
                E.sfx(4);
            }

            public override void update()
            {
                if (state == 0)
                {
                    if (y >= target.Y + 16.0)
                    {
                        return;
                    }

                    state = 1;
                    delay = 3;
                }
                else if (state == 1)
                {
                    spd.Y += 0.5f;
                    if (spd.Y > 0.0 && delay > 0)
                    {
                        spd.Y = 0.0f;
                        --delay;
                    }
                    if (spd.Y <= 0.0 || y <= (double)target.Y)
                    {
                        return;
                    }

                    y = target.Y;
                    spd = new Vector2(0.0f, 0.0f);
                    state = 2;
                    delay = 5;
                    G.shake = 5;
                    _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y + 4f);
                    E.sfx(5);
                }
                else
                {
                    if (state != 2)
                    {
                        return;
                    }

                    --delay;
                    spr = 6f;
                    if (delay >= 0)
                    {
                        return;
                    }

                    G.destroy_object(this);
                    G.init_object<Classic.player>(new Classic.player(), x, y).hair = hair;
                }
            }

            public override void draw()
            {
                hair.draw_hair(this, 1, G.max_djump);
                G.draw_player(this, G.max_djump);
            }
        }

        public class spring : Classic.ClassicObject
        {
            public int hide_in;
            private int hide_for;
            private int delay;

            public override void update()
            {
                if (hide_for > 0)
                {
                    --hide_for;
                    if (hide_for <= 0)
                    {
                        spr = 18f;
                        delay = 0;
                    }
                }
                else if (spr == 18.0)
                {
                    Classic.player player = collide<Classic.player>(0, 0);
                    if (player != null && player.spd.Y >= 0.0)
                    {
                        spr = 19f;
                        player.y = y - 4f;
                        player.spd.X *= 0.2f;
                        player.spd.Y = -3f;
                        player.djump = G.max_djump;
                        delay = 10;
                        _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y);
                        Classic.fall_floor fallFloor = collide<Classic.fall_floor>(0, 1);
                        if (fallFloor != null)
                        {
                            G.break_fall_floor(fallFloor);
                        }

                        G.psfx(8);
                    }
                }
                else if (delay > 0)
                {
                    --delay;
                    if (delay <= 0)
                    {
                        spr = 18f;
                    }
                }
                if (hide_in <= 0)
                {
                    return;
                }

                --hide_in;
                if (hide_in > 0)
                {
                    return;
                }

                hide_for = 60;
                spr = 0.0f;
            }
        }

        public class balloon : Classic.ClassicObject
        {
            private float offset;
            private float start;
            private float timer;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                offset = E.rnd(1f);
                start = y;
                hitbox = new Rectangle(-1, -1, 10, 10);
            }

            public override void update()
            {
                if (spr == 22.0)
                {
                    offset += 0.01f;
                    y = start + (E.sin(offset) * 2f);
                    Classic.player player = collide<Classic.player>(0, 0);
                    if (player == null || player.djump >= G.max_djump)
                    {
                        return;
                    }

                    G.psfx(6);
                    _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y);
                    player.djump = G.max_djump;
                    spr = 0.0f;
                    timer = 60f;
                }
                else if (timer > 0.0)
                {
                    --timer;
                }
                else
                {
                    G.psfx(7);
                    _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y);
                    spr = 22f;
                }
            }

            public override void draw()
            {
                if (spr != 22.0)
                {
                    return;
                }

                E.spr((float)(13.0 + (offset * 8.0 % 3.0)), x, y + 6f);
                E.spr(spr, x, y);
            }
        }

        public class fall_floor : Classic.ClassicObject
        {
            public int state;
            public bool solid = true;
            public int delay;

            public override void update()
            {
                if (state == 0)
                {
                    if (!check<Classic.player>(0, -1) && !check<Classic.player>(-1, 0) && !check<Classic.player>(1, 0))
                    {
                        return;
                    }

                    G.break_fall_floor(this);
                }
                else if (state == 1)
                {
                    --delay;
                    if (delay > 0)
                    {
                        return;
                    }

                    state = 2;
                    delay = 60;
                    collideable = false;
                }
                else
                {
                    if (state != 2)
                    {
                        return;
                    }

                    --delay;
                    if (delay > 0 || check<Classic.player>(0, 0))
                    {
                        return;
                    }

                    G.psfx(7);
                    state = 0;
                    collideable = true;
                    _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y);
                }
            }

            public override void draw()
            {
                if (state == 2)
                {
                    return;
                }

                if (state != 1)
                {
                    E.spr(23f, x, y);
                }
                else
                {
                    E.spr(23 + ((15 - delay) / 5), x, y);
                }
            }
        }

        public class smoke : Classic.ClassicObject
        {
            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                spr = 29f;
                spd.Y = -0.1f;
                spd.X = 0.3f + E.rnd(0.2f);
                x += E.rnd(2f) - 1f;
                y += E.rnd(2f) - 1f;
                flipX = G.maybe();
                flipY = G.maybe();
                solids = false;
            }

            public override void update()
            {
                spr += 0.2f;
                if (spr < 32.0)
                {
                    return;
                }

                G.destroy_object(this);
            }
        }

        public class fruit : Classic.ClassicObject
        {
            private float start;
            private float off;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                spr = 26f;
                start = y;
                off = 0.0f;
            }

            public override void update()
            {
                Classic.player player = collide<Classic.player>(0, 0);
                if (player != null)
                {
                    player.djump = G.max_djump;
                    G.sfx_timer = 20;
                    E.sfx(13);
                    _ = G.got_fruit.Add(1 + G.level_index());
                    _ = G.init_object<Classic.lifeup>(new Classic.lifeup(), x, y);
                    G.destroy_object(this);
                    //Stats.Increment(Stat.PICO_BERRIES);
                }
                ++off;
                y = start + (E.sin(off / 40f) * 2.5f);
            }
        }

        public class fly_fruit : Classic.ClassicObject
        {
            private float start;
            private bool fly;
            private float step = 0.5f;
            private float sfx_delay = 8f;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                start = y;
                solids = false;
            }

            public override void update()
            {
                if (fly)
                {
                    if (sfx_delay > 0.0)
                    {
                        --sfx_delay;
                        if (sfx_delay <= 0.0)
                        {
                            G.sfx_timer = 20;
                            E.sfx(14);
                        }
                    }
                    spd.Y = G.appr(spd.Y, -3.5f, 0.25f);
                    if (y < -16.0)
                    {
                        G.destroy_object(this);
                    }
                }
                else
                {
                    if (G.has_dashed)
                    {
                        fly = true;
                    }

                    step += 0.05f;
                    spd.Y = E.sin(step) * 0.5f;
                }
                Classic.player player = collide<Classic.player>(0, 0);
                if (player == null)
                {
                    return;
                }

                player.djump = G.max_djump;
                G.sfx_timer = 20;
                E.sfx(13);
                _ = G.got_fruit.Add(1 + G.level_index());
                _ = G.init_object<Classic.lifeup>(new Classic.lifeup(), x, y);
                G.destroy_object(this);
                //Stats.Increment(Stat.PICO_BERRIES);
            }

            public override void draw()
            {
                float num = 0.0f;
                if (!fly)
                {
                    if ((double)E.sin(step) < 0.0)
                    {
                        num = 1f + E.max(0.0f, G.sign(y - start));
                    }
                }
                else
                {
                    num = (float)(((double)num + 0.25) % 3.0);
                }

                E.spr(45f + num, x - 6f, y - 2f, flipX: true);
                E.spr(spr, x, y);
                E.spr(45f + num, x + 6f, y - 2f);
            }
        }

        public class lifeup : Classic.ClassicObject
        {
            private int duration;
            private float flash;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                spd.Y = -0.25f;
                duration = 30;
                x -= 2f;
                y -= 4f;
                flash = 0.0f;
                solids = false;
            }

            public override void update()
            {
                --duration;
                if (duration > 0)
                {
                    return;
                }

                G.destroy_object(this);
            }

            public override void draw()
            {
                flash += 0.5f;
                E.print("1000", x - 2f, y, (float)(7.0 + (flash % 2.0)));
            }
        }

        public class fake_wall : Classic.ClassicObject
        {
            public override void update()
            {
                hitbox = new Rectangle(-1, -1, 18, 18);
                Classic.player player = collide<Classic.player>(0, 0);
                if (player != null && player.dash_effect_time > 0)
                {
                    player.spd.X = -G.sign(player.spd.X) * 1.5f;
                    player.spd.Y = -1.5f;
                    player.dash_time = -1;
                    G.sfx_timer = 20;
                    E.sfx(16);
                    G.destroy_object(this);
                    _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y);
                    _ = G.init_object<Classic.smoke>(new Classic.smoke(), x + 8f, y);
                    _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y + 8f);
                    _ = G.init_object<Classic.smoke>(new Classic.smoke(), x + 8f, y + 8f);
                    _ = G.init_object<Classic.fruit>(new Classic.fruit(), x + 4f, y + 4f);
                }
                hitbox = new Rectangle(0, 0, 16, 16);
            }

            public override void draw()
            {
                E.spr(64f, x, y);
                E.spr(65f, x + 8f, y);
                E.spr(80f, x, y + 8f);
                E.spr(81f, x + 8f, y + 8f);
            }
        }

        public class key : Classic.ClassicObject
        {
            public override void update()
            {
                int num1 = E.flr(spr);
                spr = (float)(9.0 + (((double)E.sin(G.frames / 30f) + 0.5) * 1.0));
                int num2 = E.flr(spr);
                if (num2 == 10 && num2 != num1)
                {
                    flipX = !flipX;
                }

                if (!check<Classic.player>(0, 0))
                {
                    return;
                }

                E.sfx(23);
                G.sfx_timer = 20;
                G.destroy_object(this);
                G.has_key = true;
            }
        }

        public class chest : Classic.ClassicObject
        {
            private float start;
            private float timer;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                x -= 4f;
                start = x;
                timer = 20f;
            }

            public override void update()
            {
                if (!G.has_key)
                {
                    return;
                }

                --timer;
                x = start - 1f + E.rnd(3f);
                if (timer > 0.0)
                {
                    return;
                }

                G.sfx_timer = 20;
                E.sfx(16);
                _ = G.init_object<Classic.fruit>(new Classic.fruit(), x, y - 4f);
                G.destroy_object(this);
            }
        }

        public class platform : Classic.ClassicObject
        {
            public float dir;
            private float last;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                x -= 4f;
                solids = false;
                hitbox.Width = 16;
                last = x;
            }

            public override void update()
            {
                spd.X = dir * 0.65f;
                if (x < -16.0)
                {
                    x = 128f;
                }

                if (x > 128.0)
                {
                    x = -16f;
                }

                if (!check<Classic.player>(0, 0))
                {
                    collide<Classic.player>(0, -1)?.move_x((int)(x - (double)last), 1);
                }

                last = x;
            }

            public override void draw()
            {
                E.spr(11f, x, y - 1f);
                E.spr(12f, x + 8f, y - 1f);
            }
        }

        public class message : Classic.ClassicObject
        {
            private float last;
            private float index;

            public override void draw()
            {
                string str = "-- celeste mountain --#this memorial to those# perished on the climb";
                if (check<Classic.player>(4, 0))
                {
                    if (index < (double)str.Length)
                    {
                        index += 0.5f;
                        if (index >= last + 1.0)
                        {
                            ++last;
                            E.sfx(35);
                        }
                    }
                    Vector2 vector2 = new(8f, 96f);
                    for (int index = 0;
                        index < (double)this.index; ++index)
                    {
                        if (str[index] != '#')
                        {
                            E.rectfill(vector2.X - 2f, vector2.Y - 2f, vector2.X + 7f, vector2.Y + 6f, 7f);
                            E.print(str[index].ToString() ?? "", vector2.X, vector2.Y, 0.0f);
                            vector2.X += 5f;
                        }
                        else
                        {
                            vector2.X = 8f;
                            vector2.Y += 7f;
                        }
                    }
                }
                else
                {
                    index = 0.0f;
                    last = 0.0f;
                }
            }
        }

        public class big_chest : Classic.ClassicObject
        {
            private int state;
            private float timer;
            private List<Classic.big_chest.particle> particles;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                hitbox.Width = 16;
            }

            public override void draw()
            {
                if (state == 0)
                {
                    Classic.player player = collide<Classic.player>(0, 8);
                    if (player != null && player.is_solid(0, 1))
                    {
                        E.music(-1, 500, 7);
                        E.sfx(37);
                        G.pause_player = true;
                        player.spd.X = 0.0f;
                        player.spd.Y = 0.0f;
                        state = 1;
                        _ = G.init_object<Classic.smoke>(new Classic.smoke(), x, y);
                        _ = G.init_object<Classic.smoke>(new Classic.smoke(), x + 8f, y);
                        timer = 60f;
                        particles = new List<Classic.big_chest.particle>();
                    }
                    E.spr(96f, x, y);
                    E.spr(97f, x + 8f, y);
                }
                else if (state == 1)
                {
                    --timer;
                    G.shake = 5;
                    G.flash_bg = true;
                    if (timer <= 45.0 && particles.Count < 50)
                    {
                        particles.Add(new Classic.big_chest.particle()
                        {
                            x = 1f + E.rnd(14f),
                            y = 0.0f,
                            h = 32f + E.rnd(32f),
                            spd = 8f + E.rnd(8f)
                        });
                    }

                    if (timer < 0.0)
                    {
                        state = 2;
                        particles.Clear();
                        G.flash_bg = false;
                        G.new_bg = true;
                        _ = G.init_object<Classic.orb>(new Classic.orb(), x + 4f, y + 4f);
                        G.pause_player = false;
                    }
                    foreach (Classic.big_chest.particle particle in particles)
                    {
                        particle.y += particle.spd;
                        E.rectfill(x + particle.x, y + 8f - particle.y, x + particle.x, E.min(y + 8f - particle.y + particle.h, y + 8f), 7f);
                    }
                }
                E.spr(112f, x, y + 8f);
                E.spr(113f, x + 8f, y + 8f);
            }

            private class particle
            {
                public float x;
                public float y;
                public float h;
                public float spd;
            }
        }

        public class orb : Classic.ClassicObject
        {
            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                spd.Y = -4f;
                solids = false;
            }

            public override void draw()
            {
                spd.Y = G.appr(spd.Y, 0.0f, 0.5f);
                Classic.player player = collide<Classic.player>(0, 0);
                if (spd.Y == 0.0 && player != null)
                {
                    G.music_timer = 45;
                    E.sfx(51);
                    G.freeze = 10;
                    G.shake = 10;
                    G.destroy_object(this);
                    G.max_djump = 2;
                    player.djump = 2;
                }
                E.spr(102f, x, y);
                float num = G.frames / 30f;
                for (int index = 0; index <= 7; ++index)
                {
                    E.circfill((float)(x + 4.0 + ((double)E.cos(num + (index / 8f)) * 8.0)), (float)(y + 4.0 + ((double)E.sin(num + (index / 8f)) * 8.0)), 1f, 7f);
                }
            }
        }

        public class flag : Classic.ClassicObject
        {
            private float score;
            private bool show;

            public override void init(Classic g, Emulator e)
            {
                base.init(g, e);
                x += 5f;
                score = G.got_fruit.Count;
                //Stats.Increment(Stat.PICO_COMPLETES);
                //Achievements.Register(Achievement.PICO8);
            }

            public override void draw()
            {
                spr = (float)(118.0 + (G.frames / 5.0 % 3.0));
                E.spr(spr, x, y);
                if (show)
                {
                    E.rectfill(32f, 2f, 96f, 31f, 0.0f);
                    E.spr(26f, 55f, 6f);
                    E.print("x" + score, 64f, 9f, 7f);
                    G.draw_time(49, 16);
                    E.print("deaths:" + G.deaths, 48f, 24f, 7f);
                }
                else
                {
                    if (!check<Classic.player>(0, 0))
                    {
                        return;
                    }

                    E.sfx(55);
                    G.sfx_timer = 30;
                    show = true;
                }
            }
        }

        public class room_title : Classic.ClassicObject
        {
            private float delay = 5f;

            public override void draw()
            {
                --delay;
                if (delay < -30.0)
                {
                    G.destroy_object(this);
                }
                else
                {
                    if (delay >= 0.0)
                    {
                        return;
                    }

                    E.rectfill(24f, 58f, 104f, 70f, 0.0f);
                    if (G.room.X == 3 && G.room.Y == 1)
                    {
                        E.print("old site", 48f, 62f, 7f);
                    }
                    else if (G.level_index() == 30)
                    {
                        E.print("summit", 52f, 62f, 7f);
                    }
                    else
                    {
                        int num = (1 + G.level_index()) * 100;
                        E.print(num.ToString() + "m", 52 + (num < 1000 ? 2 : 0), 62f, 7f);
                    }
                    G.draw_time(4, 4);
                }
            }
        }

        public class ClassicObject
        {
            public Classic G;
            public Emulator E;
            public int type;
            public bool collideable = true;
            public bool solids = true;
            public float spr;
            public bool flipX;
            public bool flipY;
            public float x;
            public float y;
            public Rectangle hitbox = new(0, 0, 8, 8);
            public Vector2 spd = new(0.0f, 0.0f);
            public Vector2 rem = new(0.0f, 0.0f);

            public virtual void init(Classic g, Emulator e)
            {
                G = g;
                E = e;
            }

            public virtual void update() { }

            public virtual void draw()
            {
                if (spr <= 0.0)
                {
                    return;
                }

                E.spr(spr, x, y, flipX: flipX, flipY: flipY);
            }

            public bool is_solid(int ox, int oy)
            {
                return (oy > 0 && !check<Classic.platform>(ox, 0) && check<Classic.platform>(ox, oy)) || G.solid_at(x + hitbox.X + ox, y + hitbox.Y + oy, hitbox.Width, hitbox.Height) || check<Classic.fall_floor>(ox, oy) || check<Classic.fake_wall>(ox, oy);
            }

            public bool is_ice(int ox, int oy)
            {
                return G.ice_at(x + hitbox.X + ox, y + hitbox.Y + oy, hitbox.Width, hitbox.Height);
            }

            public T collide<T>(int ox, int oy) where T : Classic.ClassicObject
            {
                Type type = typeof(T);
                foreach (Classic.ClassicObject classicObject in G.objects)
                {
                    if (classicObject != null && classicObject.GetType() == type && classicObject != this && classicObject.collideable && classicObject.x + (double)classicObject.hitbox.X + classicObject.hitbox.Width > x + (double)hitbox.X + ox && classicObject.y + (double)classicObject.hitbox.Y + classicObject.hitbox.Height > y + (double)hitbox.Y + oy && classicObject.x + (double)classicObject.hitbox.X < x + (double)hitbox.X + hitbox.Width + ox && classicObject.y + (double)classicObject.hitbox.Y < y + (double)hitbox.Y + hitbox.Height + oy)
                    {
                        return classicObject as T;
                    }
                }
                return default;
            }

            public bool check<T>(int ox, int oy) where T : Classic.ClassicObject
            {
                return collide<T>(ox, oy) != null;
            }

            public void move(float ox, float oy)
            {
                rem.X += ox;
                int amount1 = E.flr(rem.X + 0.5f);
                rem.X -= amount1;
                move_x(amount1, 0);
                rem.Y += oy;
                int amount2 = E.flr(rem.Y + 0.5f);
                rem.Y -= amount2;
                move_y(amount2);
            }

            public void move_x(int amount, int start)
            {
                if (solids)
                {
                    int ox = G.sign(amount);
                    for (int index = start;
                        index <= (double)E.abs(amount); ++index)
                    {
                        if (!is_solid(ox, 0))
                        {
                            x += ox;
                        }
                        else
                        {
                            spd.X = 0.0f;
                            rem.X = 0.0f;
                            break;
                        }
                    }
                }
                else
                {
                    x += amount;
                }
            }

            public void move_y(int amount)
            {
                if (solids)
                {
                    int oy = G.sign(amount);
                    for (int index = 0;
                        index <= (double)E.abs(amount); ++index)
                    {
                        if (!is_solid(0, oy))
                        {
                            y += oy;
                        }
                        else
                        {
                            spd.Y = 0.0f;
                            rem.Y = 0.0f;
                            break;
                        }
                    }
                }
                else
                {
                    y += amount;
                }
            }
        }
    }
}