using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Celeste
{
    public class PlayerSprite : Sprite
    {
        public const string Idle = "idle";
        public const string Shaking = "shaking";
        public const string FrontEdge = "edge";
        public const string LookUp = "lookUp";
        public const string Walk = "walk";
        public const string RunSlow = "runSlow";
        public const string RunFast = "runFast";
        public const string RunWind = "runWind";
        public const string RunStumble = "runStumble";
        public const string JumpSlow = "jumpSlow";
        public const string FallSlow = "fallSlow";
        public const string Fall = "fall";
        public const string JumpFast = "jumpFast";
        public const string FallFast = "fallFast";
        public const string FallBig = "bigFall";
        public const string LandInPose = "fallPose";
        public const string Tired = "tired";
        public const string TiredStill = "tiredStill";
        public const string WallSlide = "wallslide";
        public const string ClimbUp = "climbUp";
        public const string ClimbDown = "climbDown";
        public const string ClimbLookBackStart = "climbLookBackStart";
        public const string ClimbLookBack = "climbLookBack";
        public const string Dangling = "dangling";
        public const string Duck = "duck";
        public const string Dash = "dash";
        public const string Sleep = "sleep";
        public const string Sleeping = "asleep";
        public const string Flip = "flip";
        public const string Skid = "skid";
        public const string DreamDashIn = "dreamDashIn";
        public const string DreamDashLoop = "dreamDashLoop";
        public const string DreamDashOut = "dreamDashOut";
        public const string SwimIdle = "swimIdle";
        public const string SwimUp = "swimUp";
        public const string SwimDown = "swimDown";
        public const string StartStarFly = "startStarFly";
        public const string StarFly = "starFly";
        public const string StarMorph = "starMorph";
        public const string IdleCarry = "idle_carry";
        public const string RunCarry = "runSlow_carry";
        public const string JumpCarry = "jumpSlow_carry";
        public const string FallCarry = "fallSlow_carry";
        public const string PickUp = "pickup";
        public const string Throw = "throw";
        public const string Launch = "launch";
        public const string TentacleGrab = "tentacle_grab";
        public const string TentacleGrabbed = "tentacle_grabbed";
        public const string TentaclePull = "tentacle_pull";
        public const string TentacleDangling = "tentacle_dangling";
        public const string SitDown = "sitDown";
        private readonly string spriteName;
        public int HairCount = 4;
        private static readonly Dictionary<string, PlayerAnimMetadata> FrameMetadata = new(StringComparer.OrdinalIgnoreCase);

        public PlayerSpriteMode Mode { get; private set; }

        public PlayerSprite(PlayerSpriteMode mode)
            : base(null, null)
        {
            Mode = mode;
            string id = "";
            switch (mode)
            {
                case PlayerSpriteMode.Madeline:
                    id = "player";
                    break;
                case PlayerSpriteMode.MadelineNoBackpack:
                    id = "player_no_backpack";
                    break;
                case PlayerSpriteMode.Badeline:
                    id = "badeline";
                    break;
                case PlayerSpriteMode.MadelineAsBadeline:
                    id = "player_badeline";
                    break;
                case PlayerSpriteMode.Playback:
                    id = "player_playback";
                    break;
            }
            spriteName = id;
            GFX.SpriteBank.CreateOn(this, id);
        }

        public Vector2 HairOffset
        {
            get
            {
                return Texture != null && FrameMetadata.TryGetValue(Texture.AtlasPath, out PlayerAnimMetadata playerAnimMetadata) ? playerAnimMetadata.HairOffset : Vector2.Zero;
            }
        }

        public float CarryYOffset
        {
            get
            {
                return Texture != null && FrameMetadata.TryGetValue(Texture.AtlasPath, out PlayerAnimMetadata playerAnimMetadata) ? playerAnimMetadata.CarryYOffset * Scale.Y : 0.0f;
            }
        }

        public int HairFrame
        {
            get
            {
                return Texture != null && FrameMetadata.TryGetValue(Texture.AtlasPath, out PlayerAnimMetadata playerAnimMetadata) ? playerAnimMetadata.Frame : 0;
            }
        }

        public bool HasHair
        {
            get
            {
                return Texture != null && FrameMetadata.TryGetValue(Texture.AtlasPath, out PlayerAnimMetadata playerAnimMetadata) && playerAnimMetadata.HasHair;
            }
        }

        public bool Running
        {
            get
            {
                if (LastAnimationID == null)
                    return false;
                return LastAnimationID == "flip" || LastAnimationID.StartsWith("run");
            }
        }

        public bool DreamDashing => LastAnimationID != null && LastAnimationID.StartsWith("dreamDash");

        public override void Render()
        {
            Vector2 renderPosition = RenderPosition;
            RenderPosition = RenderPosition.Floor();
            base.Render();
            RenderPosition = renderPosition;
        }

        public static void CreateFramesMetadata(string sprite)
        {
            foreach (SpriteDataSource source in GFX.SpriteBank.SpriteData[sprite].Sources)
            {
                XmlElement xmlElement = source.XML["Metadata"];
                string str1 = source.Path;
                if (xmlElement != null)
                {
                    if (!string.IsNullOrEmpty(source.OverridePath))
                        str1 = source.OverridePath;
                    foreach (XmlElement xml in xmlElement.GetElementsByTagName("Frames"))
                    {
                        string str2 = str1 + xml.Attr("path", "");
                        string[] strArray1 = xml.Attr("hair").Split('|');
                        string[] strArray2 = xml.Attr("carry", "").Split(',');
                        for (int index = 0; index < Math.Max(strArray1.Length, strArray2.Length); ++index)
                        {
                            PlayerAnimMetadata playerAnimMetadata = new();
                            string str3 = str2 + (index < 10 ? "0" : (object) "") + index;
                            if (index == 0 && !GFX.Game.Has(str3))
                                str3 = str2;
                            FrameMetadata[str3] = playerAnimMetadata;
                            if (index < strArray1.Length)
                            {
                                if (strArray1[index].Equals("x", StringComparison.OrdinalIgnoreCase) || strArray1[index].Length <= 0)
                                {
                                    playerAnimMetadata.HasHair = false;
                                }
                                else
                                {
                                    string[] strArray3 = strArray1[index].Split(':');
                                    string[] strArray4 = strArray3[0].Split(',');
                                    playerAnimMetadata.HasHair = true;
                                    playerAnimMetadata.HairOffset = new Vector2(Convert.ToInt32(strArray4[0]), Convert.ToInt32(strArray4[1]));
                                    playerAnimMetadata.Frame = strArray3.Length >= 2 ? Convert.ToInt32(strArray3[1]) : 0;
                                }
                            }
                            if (index < strArray2.Length && strArray2[index].Length > 0)
                                playerAnimMetadata.CarryYOffset = int.Parse(strArray2[index]);
                        }
                    }
                }
            }
        }

        public static void ClearFramesMetadata() => FrameMetadata.Clear();
    }
}
