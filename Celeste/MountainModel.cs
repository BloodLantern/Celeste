// Decompiled with JetBrains decompiler
// Type: Celeste.MountainModel
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste
{
    public class MountainModel : IDisposable
    {
        public MountainCamera Camera;
        public Vector3 Forward;
        public float SkyboxOffset;
        public bool LockBufferResizing;
        private VirtualRenderTarget buffer;
        private VirtualRenderTarget blurA;
        private VirtualRenderTarget blurB;
        public static RasterizerState MountainRasterizer = new()
        {
            CullMode = CullMode.CullClockwiseFace,
            MultiSampleAntiAlias = true
        };
        public static RasterizerState CullNoneRasterizer = new()
        {
            CullMode = CullMode.None,
            MultiSampleAntiAlias = false
        };
        public static RasterizerState CullCCRasterizer = new()
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            MultiSampleAntiAlias = false
        };
        public static RasterizerState CullCRasterizer = new()
        {
            CullMode = CullMode.CullClockwiseFace,
            MultiSampleAntiAlias = false
        };
        private int currState;
        private int nextState;
        private int targetState;
        private float easeState = 1f;
        private readonly MountainState[] mountainStates = new MountainState[4];
        public Vector3 CoreWallPosition = Vector3.Zero;
        private VertexBuffer billboardVertices;
        private IndexBuffer billboardIndices;
        private VertexPositionColorTexture[] billboardInfo = new VertexPositionColorTexture[2048];
        private Texture2D[] billboardTextures = new Texture2D[512];
        private readonly Ring fog;
        private readonly Ring fog2;
        public float NearFogAlpha;
        public float StarEase;
        public float SnowStretch;
        public float SnowSpeedAddition = 1f;
        public float SnowForceFloat;
        private readonly Ring starsky;
        private readonly Ring starfog;
        private readonly Ring stardots0;
        private readonly Ring starstream0;
        private readonly Ring starstream1;
        private readonly Ring starstream2;
        private bool ignoreCameraRotation;
        private Quaternion lastCameraRotation;
        private Vector3 starCenter = new(0.0f, 32f, 0.0f);
        private float birdTimer;
        public List<VertexPositionColor> DebugPoints = new();
        public bool DrawDebugPoints;

        public MountainModel()
        {
            mountainStates[0] = new MountainState(MTN.MountainTerrainTextures[0], MTN.MountainBuildingTextures[0], MTN.MountainSkyboxTextures[0], Calc.HexToColor("010817"));
            mountainStates[1] = new MountainState(MTN.MountainTerrainTextures[1], MTN.MountainBuildingTextures[1], MTN.MountainSkyboxTextures[1], Calc.HexToColor("13203E"));
            mountainStates[2] = new MountainState(MTN.MountainTerrainTextures[2], MTN.MountainBuildingTextures[2], MTN.MountainSkyboxTextures[2], Calc.HexToColor("281A35"));
            mountainStates[3] = new MountainState(MTN.MountainTerrainTextures[0], MTN.MountainBuildingTextures[0], MTN.MountainSkyboxTextures[0], Calc.HexToColor("010817"));
            fog = new Ring(6f, -1f, 20f, 0.0f, 24, Color.White, MTN.MountainFogTexture);
            fog2 = new Ring(6f, -4f, 10f, 0.0f, 24, Color.White, MTN.MountainFogTexture);
            starsky = new Ring(18f, -18f, 20f, 0.0f, 24, Color.White, Color.Transparent, MTN.MountainStarSky);
            starfog = new Ring(10f, -18f, 19.5f, 0.0f, 24, Calc.HexToColor("020915"), Color.Transparent, MTN.MountainFogTexture);
            stardots0 = new Ring(16f, -18f, 19f, 0.0f, 24, Color.White, Color.Transparent, MTN.MountainStars, 4f);
            starstream0 = new Ring(5f, -8f, 18.5f, 0.2f, 80, Color.Black, MTN.MountainStarStream);
            starstream1 = new Ring(4f, -6f, 18f, 1f, 80, Calc.HexToColor("9228e2") * 0.5f, MTN.MountainStarStream);
            starstream2 = new Ring(3f, -4f, 17.9f, 1.4f, 80, Calc.HexToColor("30ffff") * 0.5f, MTN.MountainStarStream);
            ResetRenderTargets();
            ResetBillboardBuffers();
        }

        public void SnapState(int state)
        {
            currState = nextState = targetState = state % mountainStates.Length;
            easeState = 1f;
            if (state != 3)
            {
                return;
            }

            StarEase = 1f;
        }

        public void EaseState(int state)
        {
            targetState = state % mountainStates.Length;
            lastCameraRotation = Camera.Rotation;
        }

        public void Update()
        {
            if (currState != nextState)
            {
                easeState = Calc.Approach(easeState, 1f, (nextState == targetState ? 1f : 4f) * Engine.DeltaTime);
                if (easeState >= 1.0)
                {
                    currState = nextState;
                }
            }
            else if (nextState != targetState)
            {
                nextState = targetState;
                easeState = 0.0f;
            }
            StarEase = Calc.Approach(StarEase, nextState == 3 ? 1f : 0.0f, (nextState == 3 ? 1.5f : 1f) * Engine.DeltaTime);
            SnowForceFloat = Calc.ClampedMap(StarEase, 0.95f, 1f);
            ignoreCameraRotation = (nextState == 3 && currState != 3 && StarEase < 0.5) || (nextState != 3 && currState == 3 && StarEase > 0.5);
            if (nextState == 3)
            {
                SnowStretch = Calc.ClampedMap(StarEase, 0.0f, 0.25f) * 50f;
                SnowSpeedAddition = SnowStretch * 4f;
            }
            else
            {
                SnowStretch = Calc.ClampedMap(StarEase, 0.25f, 1f) * 50f;
                SnowSpeedAddition = (float)(-(double)SnowStretch * 4.0);
            }
            starfog.Rotate((float)(-(double)Engine.DeltaTime * 0.0099999997764825821));
            fog.Rotate((float)(-(double)Engine.DeltaTime * 0.0099999997764825821));
            fog.TopColor = fog.BotColor = Color.Lerp(mountainStates[currState].FogColor, mountainStates[nextState].FogColor, easeState);
            fog2.Rotate((float)(-(double)Engine.DeltaTime * 0.0099999997764825821));
            fog2.TopColor = fog2.BotColor = Color.White * 0.3f * NearFogAlpha;
            starstream1.Rotate(Engine.DeltaTime * 0.01f);
            starstream2.Rotate(Engine.DeltaTime * 0.02f);
            birdTimer += Engine.DeltaTime;
        }

        public void ResetRenderTargets()
        {
            int width = Math.Min(1920, Engine.ViewWidth);
            int height = Math.Min(1080, Engine.ViewHeight);
            if (buffer != null && !buffer.IsDisposed && (buffer.Width == width || LockBufferResizing))
            {
                return;
            }

            DisposeTargets();
            buffer = VirtualContent.CreateRenderTarget("mountain-a", width, height, true, false);
            blurA = VirtualContent.CreateRenderTarget("mountain-blur-a", width / 2, height / 2);
            blurB = VirtualContent.CreateRenderTarget("mountain-blur-b", width / 2, height / 2);
        }

        public void ResetBillboardBuffers()
        {
            if (billboardVertices != null && !billboardIndices.IsDisposed && !billboardIndices.GraphicsDevice.IsDisposed && !billboardVertices.IsDisposed && !billboardVertices.GraphicsDevice.IsDisposed && billboardInfo.Length <= billboardVertices.VertexCount)
            {
                return;
            }

            DisposeBillboardBuffers();
            billboardVertices = new VertexBuffer(Engine.Graphics.GraphicsDevice, typeof(VertexPositionColorTexture), billboardInfo.Length, BufferUsage.None);
            billboardIndices = new IndexBuffer(Engine.Graphics.GraphicsDevice, typeof(short), billboardInfo.Length / 4 * 6, BufferUsage.None);
            short[] data = new short[billboardIndices.IndexCount];
            int index = 0;
            int num = 0;
            while (index < data.Length)
            {
                data[index] = (short)num;
                data[index + 1] = (short)(num + 1);
                data[index + 2] = (short)(num + 2);
                data[index + 3] = (short)num;
                data[index + 4] = (short)(num + 2);
                data[index + 5] = (short)(num + 3);
                index += 6;
                num += 4;
            }
            billboardIndices.SetData<short>(data);
        }

        public void Dispose()
        {
            DisposeTargets();
            DisposeBillboardBuffers();
        }

        public void DisposeTargets()
        {
            if (buffer == null || buffer.IsDisposed)
            {
                return;
            }

            buffer.Dispose();
            blurA.Dispose();
            blurB.Dispose();
        }

        public void DisposeBillboardBuffers()
        {
            if (billboardVertices != null && !billboardVertices.IsDisposed)
            {
                billboardVertices.Dispose();
            }

            if (billboardIndices == null || billboardIndices.IsDisposed)
            {
                return;
            }

            billboardIndices.Dispose();
        }

        public void BeforeRender(Scene scene)
        {
            ResetRenderTargets();
            Quaternion quaternion = Camera.Rotation;
            if (ignoreCameraRotation)
            {
                quaternion = lastCameraRotation;
            }

            Matrix perspectiveFieldOfView = Matrix.CreatePerspectiveFieldOfView(0.7853982f, Engine.Width / (float)Engine.Height, 0.25f, 50f);
            Matrix matrix1 = Matrix.CreateTranslation(-Camera.Position) * Matrix.CreateFromQuaternion(quaternion);
            Matrix matrix2 = matrix1 * perspectiveFieldOfView;
            Forward = Vector3.Transform(Vector3.Forward, Camera.Rotation.Conjugated());
            Engine.Graphics.GraphicsDevice.SetRenderTarget((RenderTarget2D)buffer);
            if (StarEase < 1.0)
            {
                Matrix matrix3 = Matrix.CreateTranslation(0.0f, (float)(5.0 - (Camera.Position.Y * 1.1000000238418579)), 0.0f) * Matrix.CreateFromQuaternion(quaternion) * perspectiveFieldOfView;
                if (currState == nextState)
                {
                    mountainStates[currState].Skybox.Draw(matrix3, Color.White);
                }
                else
                {
                    mountainStates[currState].Skybox.Draw(matrix3, Color.White);
                    mountainStates[nextState].Skybox.Draw(matrix3, Color.White * easeState);
                }
                if (currState != nextState)
                {
                    GFX.FxMountain.Parameters["ease"].SetValue(easeState);
                    GFX.FxMountain.CurrentTechnique = GFX.FxMountain.Techniques["Easing"];
                }
                else
                {
                    GFX.FxMountain.CurrentTechnique = GFX.FxMountain.Techniques["Single"];
                }

                Engine.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                Engine.Graphics.GraphicsDevice.RasterizerState = MountainModel.MountainRasterizer;
                GFX.FxMountain.Parameters["WorldViewProj"].SetValue(matrix2);
                GFX.FxMountain.Parameters["fog"].SetValue(fog.TopColor.ToVector3());
                Engine.Graphics.GraphicsDevice.Textures[0] = mountainStates[currState].TerrainTexture.Texture;
                Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                if (currState != nextState)
                {
                    Engine.Graphics.GraphicsDevice.Textures[1] = mountainStates[nextState].TerrainTexture.Texture;
                    Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
                }
                MTN.MountainTerrain.Draw(GFX.FxMountain);
                GFX.FxMountain.Parameters["WorldViewProj"].SetValue(Matrix.CreateTranslation(CoreWallPosition) * matrix2);
                MTN.MountainCoreWall.Draw(GFX.FxMountain);
                GFX.FxMountain.Parameters["WorldViewProj"].SetValue(matrix2);
                Engine.Graphics.GraphicsDevice.Textures[0] = mountainStates[currState].BuildingsTexture.Texture;
                Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                if (currState != nextState)
                {
                    Engine.Graphics.GraphicsDevice.Textures[1] = mountainStates[nextState].BuildingsTexture.Texture;
                    Engine.Graphics.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
                }
                MTN.MountainBuildings.Draw(GFX.FxMountain);
                fog.Draw(matrix2);
            }
            if (StarEase > 0.0)
            {
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
                Draw.Rect(0.0f, 0.0f, buffer.Width, buffer.Height, Color.Black * Ease.CubeInOut(Calc.ClampedMap(StarEase, 0.0f, 0.6f)));
                Draw.SpriteBatch.End();
                Matrix matrix4 = Matrix.CreateTranslation(starCenter - Camera.Position) * Matrix.CreateFromQuaternion(quaternion) * perspectiveFieldOfView;
                float alpha = Calc.ClampedMap(StarEase, 0.8f, 1f);
                starsky.Draw(matrix4, MountainModel.CullCCRasterizer, alpha);
                starfog.Draw(matrix4, MountainModel.CullCCRasterizer, alpha);
                stardots0.Draw(matrix4, MountainModel.CullCCRasterizer, alpha);
                starstream0.Draw(matrix4, MountainModel.CullCCRasterizer, alpha);
                starstream1.Draw(matrix4, MountainModel.CullCCRasterizer, alpha);
                starstream2.Draw(matrix4, MountainModel.CullCCRasterizer, alpha);
                Engine.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                Engine.Graphics.GraphicsDevice.RasterizerState = MountainModel.CullCRasterizer;
                Engine.Graphics.GraphicsDevice.Textures[0] = MTN.MountainMoonTexture.Texture;
                Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
                GFX.FxMountain.CurrentTechnique = GFX.FxMountain.Techniques["Single"];
                GFX.FxMountain.Parameters["WorldViewProj"].SetValue(matrix2);
                GFX.FxMountain.Parameters["fog"].SetValue(fog.TopColor.ToVector3());
                MTN.MountainMoon.Draw(GFX.FxMountain);
                float num = birdTimer * 0.2f;
                Matrix matrix5 = Matrix.CreateScale(0.25f) * Matrix.CreateRotationZ((float)Math.Cos((double)num * 2.0) * 0.5f) * Matrix.CreateRotationX((float)(0.40000000596046448 + (Math.Sin((double)num) * 0.05000000074505806))) * Matrix.CreateRotationY((float)(-(double)num - 1.5707963705062866)) * Matrix.CreateTranslation((float)Math.Cos((double)num) * 2.2f, (float)(31.0 + (Math.Sin((double)num * 2.0) * 0.800000011920929)), (float)Math.Sin((double)num) * 2.2f);
                GFX.FxMountain.Parameters["WorldViewProj"].SetValue(matrix5 * matrix2);
                GFX.FxMountain.Parameters["fog"].SetValue(fog.TopColor.ToVector3());
                MTN.MountainBird.Draw(GFX.FxMountain);
            }
            DrawBillboards(matrix2, scene.Tracker.GetComponents<Billboard>());
            if (StarEase < 1.0)
            {
                fog2.Draw(matrix2, MountainModel.CullCRasterizer);
            }

            if (DrawDebugPoints && DebugPoints.Count > 0)
            {
                GFX.FxDebug.World = Matrix.Identity;
                GFX.FxDebug.View = matrix1;
                GFX.FxDebug.Projection = perspectiveFieldOfView;
                GFX.FxDebug.TextureEnabled = false;
                GFX.FxDebug.VertexColorEnabled = true;
                VertexPositionColor[] array = DebugPoints.ToArray();
                foreach (EffectPass pass in GFX.FxDebug.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    Engine.Graphics.GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, array, 0, array.Length / 3);
                }
            }
            _ = GaussianBlur.Blur((RenderTarget2D)buffer, blurA, blurB, 0.75f, samples: GaussianBlur.Samples.Five);
        }

        private void DrawBillboards(Matrix matrix, List<Component> billboards)
        {
            int val1 = 0;
            int num1 = billboardInfo.Length / 4;
            Vector3 vector3_1 = Vector3.Transform(Vector3.Left, Camera.Rotation.LookAt(Vector3.Zero, Forward, Vector3.Up).Conjugated());
            Vector3 vector3_2 = Vector3.Transform(Vector3.Up, Camera.Rotation.LookAt(Vector3.Zero, Forward, Vector3.Up).Conjugated());
            foreach (Billboard billboard in billboards)
            {
                if (billboard.Entity.Visible && billboard.Visible)
                {
                    billboard.BeforeRender?.Invoke();
                    if (billboard.Color.A >= 0 && billboard.Size.X != 0.0 && billboard.Size.Y != 0.0 && billboard.Scale.X != 0.0 && billboard.Scale.Y != 0.0 && billboard.Texture != null)
                    {
                        if (val1 < num1)
                        {
                            Vector3 position = billboard.Position;
                            Vector3 vector3_3 = vector3_1 * billboard.Size.X * billboard.Scale.X;
                            Vector3 vector3_4 = vector3_2 * billboard.Size.Y * billboard.Scale.Y;
                            Vector3 vector3_5 = -vector3_3;
                            Vector3 vector3_6 = -vector3_4;
                            int index1 = val1 * 4;
                            int index2 = (val1 * 4) + 1;
                            int index3 = (val1 * 4) + 2;
                            int index4 = (val1 * 4) + 3;
                            billboardInfo[index1].Color = billboard.Color;
                            billboardInfo[index1].TextureCoordinate.X = billboard.Texture.LeftUV;
                            billboardInfo[index1].TextureCoordinate.Y = billboard.Texture.BottomUV;
                            billboardInfo[index1].Position = position + vector3_3 + vector3_6;
                            billboardInfo[index2].Color = billboard.Color;
                            billboardInfo[index2].TextureCoordinate.X = billboard.Texture.LeftUV;
                            billboardInfo[index2].TextureCoordinate.Y = billboard.Texture.TopUV;
                            billboardInfo[index2].Position = position + vector3_3 + vector3_4;
                            billboardInfo[index3].Color = billboard.Color;
                            billboardInfo[index3].TextureCoordinate.X = billboard.Texture.RightUV;
                            billboardInfo[index3].TextureCoordinate.Y = billboard.Texture.TopUV;
                            billboardInfo[index3].Position = position + vector3_5 + vector3_4;
                            billboardInfo[index4].Color = billboard.Color;
                            billboardInfo[index4].TextureCoordinate.X = billboard.Texture.RightUV;
                            billboardInfo[index4].TextureCoordinate.Y = billboard.Texture.BottomUV;
                            billboardInfo[index4].Position = position + vector3_5 + vector3_6;
                            billboardTextures[val1] = billboard.Texture.Texture.Texture;
                        }
                        ++val1;
                    }
                }
            }
            ResetBillboardBuffers();
            if (val1 <= 0)
            {
                return;
            }

            billboardVertices.SetData<VertexPositionColorTexture>(billboardInfo);
            Engine.Graphics.GraphicsDevice.SetVertexBuffer(billboardVertices);
            Engine.Graphics.GraphicsDevice.Indices = billboardIndices;
            Engine.Graphics.GraphicsDevice.RasterizerState = MountainModel.CullNoneRasterizer;
            Engine.Graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            Engine.Graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            Engine.Graphics.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            GFX.FxTexture.Parameters["World"].SetValue(matrix);
            int num2 = Math.Min(val1, billboardInfo.Length / 4);
            Texture2D billboardTexture = billboardTextures[0];
            int offset = 0;
            for (int index = 1; index < num2; ++index)
            {
                if (billboardTextures[index] != billboardTexture)
                {
                    DrawBillboardBatch(billboardTexture, offset, index - offset);
                    billboardTexture = billboardTextures[index];
                    offset = index;
                }
            }
            DrawBillboardBatch(billboardTexture, offset, num2 - offset);
            if (val1 * 4 <= billboardInfo.Length)
            {
                return;
            }

            billboardInfo = new VertexPositionColorTexture[billboardInfo.Length * 2];
            billboardTextures = new Texture2D[billboardInfo.Length / 4];
        }

        private void DrawBillboardBatch(Texture2D texture, int offset, int sprites)
        {
            Engine.Graphics.GraphicsDevice.Textures[0] = texture;
            foreach (EffectPass pass in GFX.FxTexture.CurrentTechnique.Passes)
            {
                pass.Apply();
                Engine.Graphics.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, offset * 4, 0, sprites * 4, 0, sprites * 2);
            }
        }

        public void Render()
        {
            float scale = Engine.ViewWidth / (float)buffer.Width;
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, null, null);
            Draw.SpriteBatch.Draw((RenderTarget2D)buffer, Vector2.Zero, new Rectangle?(buffer.Bounds), Color.White * 1f, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
            Draw.SpriteBatch.Draw((RenderTarget2D)blurB, Vector2.Zero, new Rectangle?(blurB.Bounds), Color.White, 0.0f, Vector2.Zero, scale * 2f, SpriteEffects.None, 0.0f);
            Draw.SpriteBatch.End();
        }
    }
}
