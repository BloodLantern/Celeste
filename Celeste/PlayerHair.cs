// Decompiled with JetBrains decompiler
// Type: Celeste.PlayerHair
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    [Tracked(false)]
    public class PlayerHair : Component
    {
        public const string Hair = "characters/player/hair00";
        public Color Color = Player.NormalHairColor;
        public Color Border = Color.Black;
        public float Alpha = 1f;
        public Facings Facing;
        public bool DrawPlayerSpriteOutline;
        public bool SimulateMotion = true;
        public Vector2 StepPerSegment = new(0.0f, 2f);
        public float StepInFacingPerSegment = 0.5f;
        public float StepApproach = 64f;
        public float StepYSinePerSegment;
        public PlayerSprite Sprite;
        public List<Vector2> Nodes = new();
        private readonly List<MTexture> bangs = GFX.Game.GetAtlasSubtextures("characters/player/bangs");
        private float wave;

        public PlayerHair(PlayerSprite sprite)
            : base(true, true)
        {
            Sprite = sprite;
            for (int index = 0; index < sprite.HairCount; ++index)
            {
                Nodes.Add(Vector2.Zero);
            }
        }

        public void Start()
        {
            Vector2 vector2 = Entity.Position + new Vector2(-(int)Facing * 200, 200f);
            for (int index = 0; index < Nodes.Count; ++index)
            {
                Nodes[index] = vector2;
            }
        }

        public void AfterUpdate()
        {
            Nodes[0] = Sprite.RenderPosition + new Vector2(0.0f, -9f * Sprite.Scale.Y) + (Sprite.HairOffset * new Vector2((float)Facing, 1f));
            Vector2 target = Nodes[0] + new Vector2((float)(-(int)Facing * (double)StepInFacingPerSegment * 2.0), (float)Math.Sin(wave) * StepYSinePerSegment) + StepPerSegment;
            Vector2 node = Nodes[0];
            float num1 = 3f;
            for (int index = 1; index < Sprite.HairCount; ++index)
            {
                if (index >= Nodes.Count)
                {
                    Nodes.Add(Nodes[index - 1]);
                }

                if (SimulateMotion)
                {
                    float num2 = (float)(1.0 - (index / (double)Sprite.HairCount * 0.5)) * StepApproach;
                    Nodes[index] = Calc.Approach(Nodes[index], target, num2 * Engine.DeltaTime);
                }
                if ((double)(Nodes[index] - node).Length() > (double)num1)
                {
                    Nodes[index] = node + ((Nodes[index] - node).SafeNormalize() * num1);
                }

                target = Nodes[index] + new Vector2(-(int)Facing * StepInFacingPerSegment, (float)Math.Sin(wave + (index * 0.800000011920929)) * StepYSinePerSegment) + StepPerSegment;
                node = Nodes[index];
            }
        }

        public override void Update()
        {
            wave += Engine.DeltaTime * 4f;
            base.Update();
        }

        public void MoveHairBy(Vector2 amount)
        {
            for (int index = 0; index < Nodes.Count; ++index)
            {
                Nodes[index] += amount;
            }
        }

        public override void Render()
        {
            if (!Sprite.HasHair)
            {
                return;
            }

            Vector2 origin = new(5f, 5f);
            Color color1 = Border * Alpha;
            Color color2 = Color * Alpha;
            if (DrawPlayerSpriteOutline)
            {
                Color color3 = Sprite.Color;
                Vector2 position = Sprite.Position;
                Sprite.Color = color1;
                Sprite.Position = position + new Vector2(0.0f, -1f);
                Sprite.Render();
                Sprite.Position = position + new Vector2(0.0f, 1f);
                Sprite.Render();
                Sprite.Position = position + new Vector2(-1f, 0.0f);
                Sprite.Render();
                Sprite.Position = position + new Vector2(1f, 0.0f);
                Sprite.Render();
                Sprite.Color = color3;
                Sprite.Position = position;
            }
            Nodes[0] = Nodes[0].Floor();
            if (color1.A > 0)
            {
                for (int index = 0; index < Sprite.HairCount; ++index)
                {
                    int hairFrame = Sprite.HairFrame;
                    MTexture mtexture = index == 0 ? bangs[hairFrame] : GFX.Game["characters/player/hair00"];
                    Vector2 hairScale = GetHairScale(index);
                    mtexture.Draw(Nodes[index] + new Vector2(-1f, 0.0f), origin, color1, hairScale);
                    mtexture.Draw(Nodes[index] + new Vector2(1f, 0.0f), origin, color1, hairScale);
                    mtexture.Draw(Nodes[index] + new Vector2(0.0f, -1f), origin, color1, hairScale);
                    mtexture.Draw(Nodes[index] + new Vector2(0.0f, 1f), origin, color1, hairScale);
                }
            }
            for (int index = Sprite.HairCount - 1; index >= 0; --index)
            {
                int hairFrame = Sprite.HairFrame;
                (index == 0 ? bangs[hairFrame] : GFX.Game["characters/player/hair00"]).Draw(Nodes[index], origin, color2, GetHairScale(index));
            }
        }

        private Vector2 GetHairScale(int index)
        {
            float y = (float)(0.25 + ((1.0 - (index / (double)Sprite.HairCount)) * 0.75));
            return new Vector2((index == 0 ? (float)Facing : y) * Math.Abs(Sprite.Scale.X), y);
        }
    }
}
