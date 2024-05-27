using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked]
    public class TileInterceptor : Component
    {
        public Action<MTexture, Vector2, Point> Intercepter;
        public bool HighPriority;

        public TileInterceptor(Action<MTexture, Vector2, Point> intercepter, bool highPriority)
            : base(false, false)
        {
            Intercepter = intercepter;
            HighPriority = highPriority;
        }

        public TileInterceptor(TileGrid applyToGrid, bool highPriority)
            : base(false, false)
        {
            Intercepter = (t, v, p) => applyToGrid.Tiles[p.X, p.Y] = t;
            HighPriority = highPriority;
        }

        public static bool TileCheck(Scene scene, MTexture tile, Vector2 at)
        {
            at += Vector2.One * 4f;
            TileInterceptor tileInterceptor1 = null;
            List<Component> components = scene.Tracker.GetComponents<TileInterceptor>();
            for (int index = components.Count - 1; index >= 0; --index)
            {
                TileInterceptor tileInterceptor2 = (TileInterceptor) components[index];
                if ((tileInterceptor1 == null || tileInterceptor2.HighPriority) && tileInterceptor2.Entity.CollidePoint(at))
                {
                    tileInterceptor1 = tileInterceptor2;
                    if (tileInterceptor2.HighPriority)
                        break;
                }
            }
            if (tileInterceptor1 == null)
                return false;
            Point point = new Point((int) ((at.X - (double) tileInterceptor1.Entity.X) / 8.0), (int) ((at.Y - (double) tileInterceptor1.Entity.Y) / 8.0));
            tileInterceptor1.Intercepter(tile, at, point);
            return true;
        }
    }
}
