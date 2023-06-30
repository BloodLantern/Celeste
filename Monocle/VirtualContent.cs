using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Monocle
{
    public static class VirtualContent
    {
        private static readonly List<VirtualAsset> assets = new();
        private static bool reloading;

        public static int Count => assets.Count;

        public static VirtualTexture CreateTexture(string path)
        {
            VirtualTexture texture = new(path);
            assets.Add(texture);
            return texture;
        }

        public static VirtualTexture CreateTexture(
            string name,
            int width,
            int height,
            Color color)
        {
            VirtualTexture texture = new(name, width, height, color);
            assets.Add(texture);
            return texture;
        }

        public static VirtualRenderTarget CreateRenderTarget(
            string name,
            int width,
            int height,
            bool depth = false,
            bool preserve = true,
            int multiSampleCount = 0)
        {
            VirtualRenderTarget renderTarget = new(name, width, height, multiSampleCount, depth, preserve);
            assets.Add(renderTarget);
            return renderTarget;
        }

        public static void BySize()
        {
            Dictionary<int, Dictionary<int, int>> dictionary = new();
            foreach (VirtualAsset asset in assets)
            {
                if (!dictionary.ContainsKey(asset.Width))
                    dictionary.Add(asset.Width, new Dictionary<int, int>());
                if (!dictionary[asset.Width].ContainsKey(asset.Height))
                    dictionary[asset.Width].Add(asset.Height, 0);
                dictionary[asset.Width][asset.Height]++;
            }
            foreach (KeyValuePair<int, Dictionary<int, int>> keyValuePair1 in dictionary)
            {
                foreach (KeyValuePair<int, int> keyValuePair2 in keyValuePair1.Value)
                    Console.WriteLine(keyValuePair1.Key.ToString() + "x" + keyValuePair2.Key + ": " + keyValuePair2.Value);
            }
        }

        public static void ByName()
        {
            foreach (VirtualAsset asset in assets)
                Console.WriteLine(asset.Name + "[" + asset.Width + "x" + asset.Height + "]");
        }

        internal static void Remove(VirtualAsset asset) => assets.Remove(asset);

        internal static void Reload()
        {
            if (reloading)
            {
                foreach (VirtualAsset asset in assets)
                    asset.Reload();
            }
            reloading = false;
        }

        internal static void Unload()
        {
            foreach (VirtualAsset asset in assets)
                asset.Unload();
            reloading = true;
        }
    }
}
