using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public static class DustStyles
    {
        public static Dictionary<int, DustStyle> Styles = new Dictionary<int, DustStyle>
        {
            {
                3,
                new DustStyle
                {
                    EdgeColors = new Vector3[3]
                    {
                        Calc.HexToColor("f25a10").ToVector3(),
                        Calc.HexToColor("ff0000").ToVector3(),
                        Calc.HexToColor("f21067").ToVector3()
                    },
                    EyeColor = Color.Red,
                    EyeTextures = "danger/dustcreature/eyes"
                }
            },
            {
                5,
                new DustStyle
                {
                    EdgeColors = new Vector3[3]
                    {
                        Calc.HexToColor("245ebb").ToVector3(),
                        Calc.HexToColor("17a0ff").ToVector3(),
                        Calc.HexToColor("17a0ff").ToVector3()
                    },
                    EyeColor = Calc.HexToColor("245ebb"),
                    EyeTextures = "danger/dustcreature/templeeyes"
                }
            }
        };

        public static DustStyle Get(Session session) => !DustStyles.Styles.ContainsKey(session.Area.ID) ? DustStyles.Styles[3] : DustStyles.Styles[session.Area.ID];

        public static DustStyle Get(Scene scene) => DustStyles.Get((scene as Level).Session);

        public struct DustStyle
        {
            public Vector3[] EdgeColors;
            public Color EyeColor;
            public string EyeTextures;
        }
    }
}
