﻿// Decompiled with JetBrains decompiler
// Type: Celeste.Credits
// Assembly: Celeste, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FAF6CA25-5C06-43EB-A08F-9CCF291FE6A3
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Celeste\orig\Celeste.exe

using Microsoft.Xna.Framework;
using Monocle;
using System.Collections.Generic;

namespace Celeste
{
    public class Credits
    {
        public static string[] Remixers = new string[8]
        {
            "Maxo",
            "Ben Prunty",
            "Christa Lee",
            "in love with a ghost",
            "2 Mello",
            "Jukio Kallio",
            "Kuraine",
            "image:matthewseiji"
        };
        public static Color BorderColor = Color.Black;
        public const float CreditSpacing = 64f;
        public const float AutoScrollSpeed = 100f;
        public const float InputScrollSpeed = 600f;
        public const float ScrollResumeDelay = 1f;
        public const float ScrollAcceleration = 1800f;
        private readonly List<Credits.CreditNode> credits;
        public float AutoScrollSpeedMultiplier = 1f;
        private float scrollSpeed = 100f;
        private float scroll;
        private readonly float height;
        private float scrollDelay;
        private float scrollbarAlpha;
        private readonly float alignment;
        private readonly float scale;
        public float BottomTimer;
        public bool Enabled = true;
        public bool AllowInput = true;
        public static PixelFont Font;
        public static float FontSize;
        public static float LineHeight;

        private static List<Credits.CreditNode> CreateCredits(bool title, bool polaroids)
        {
            List<Credits.CreditNode> credits = new();
            if (title)
            {
                credits.Add(new Credits.Image(nameof(title), 320f));
            }

            credits.AddRange(new List<Credits.CreditNode>()
            {
                 new Credits.Role("Maddy Thorson", new string[4]
                {
                    "Director",
                    "Designer",
                    "Writer",
                    "Gameplay Coder"
                }),
                 new Credits.Role("Noel Berry", new string[3]
                {
                    "Co-Creator",
                    "Programmer",
                    "Artist"
                }),
                 new Credits.Role("Amora B.", new string[2]
                {
                    "Concept Artist",
                    "High Res Artist"
                }),
                 new Credits.Role("Pedro Medeiros", new string[2]
                {
                    "Pixel Artist",
                    "UI Artist"
                }),
                 new Credits.Role("Lena Raine", new string[1]
                {
                    "Composer"
                }),
                 new Credits.Team("Power Up Audio", new string[4]
                {
                    "Kevin Regamey",
                    "Jeff Tangsoc",
                    "Joey Godard",
                    "Cole Verderber"
                }, new string[1]{ "Sound Designers" })
            });
            if (polaroids)
            {
                credits.Add(new Credits.Image(GFX.Portraits, "credits/a", 64f, -0.05f));
            }

            credits.AddRange(new List<Credits.CreditNode>()
            {
                 new Credits.Role("Gabby DaRienzo", new string[1]
                {
                    "3D Artist"
                }),
                 new Credits.Role("Sven Bergström", new string[1]
                {
                    "3D Lighting Artist"
                })
            });
            credits.AddRange(new List<Credits.CreditNode>()
            {
                 new Credits.Thanks("Writing Assistance", new string[5]
                {
                    "Noel Berry",
                    "Amora B.",
                    "Greg Lobanov",
                    "Lena Raine",
                    "Nick Suttner"
                }),
                 new Credits.Thanks("Script Editor", new string[1]
                {
                    "Nick Suttner"
                }),
                 new Credits.Thanks("Narrative Consulting", new string[4]
                {
                    "Silverstring Media",
                    "Claris Cyarron",
                    "with Lucas JW Johnson",
                    "and Tanya Kan"
                })
            });
            credits.Add(new Credits.Thanks("Remixers", Credits.Remixers));
            credits.Add(new Credits.MultiCredit("Musical Performances", new Credits.MultiCredit.Section[3]
            {
                new Credits.MultiCredit.Section("Violin, Viola", new string[1]
                {
                    "Michaela Nachtigall"
                }),
                new Credits.MultiCredit.Section("Cello", new string[1]
                {
                    "SungHa Hong"
                }),
                new Credits.MultiCredit.Section("Drums", new string[1]
                {
                    "Doug Perry"
                })
            }));
            if (polaroids)
            {
                credits.Add(new Credits.Image(GFX.Portraits, "credits/b", 64f, 0.05f));
            }

            credits.Add(new Credits.Thanks("Operations Manager", new string[1]
            {
                "Heidy Motta"
            }));
            credits.Add(new Credits.Thanks("Porting", new string[2]
            {
                "Sickhead Games, LLC",
                "Ethan Lee"
            }));
            credits.Add(new Credits.MultiCredit("Localization", new Credits.MultiCredit.Section[4]
            {
                new Credits.MultiCredit.Section("French, Italian, Korean, Russian,\nSimplified Chinese, Spanish,\nBrazilian Portuguese, German", new string[1]
                {
                    "EDS Wordland, Ltd."
                }),
                new Credits.MultiCredit.Section("Japanese", new string[4]
                {
                    "8-4, Ltd",
                    "Keiko Fukuichi",
                    "Graeme Howard",
                    "John Ricciardi"
                }),
                new Credits.MultiCredit.Section("German, Additional Brazilian Portuguese", new string[4]
                {
                    "Shloc, Ltd.",
                    "Oli Chance",
                    "Isabel Sterner",
                    "Nadine Leonhardt"
                }),
                new Credits.MultiCredit.Section("Additional Brazilian Portuguese", new string[1]
                {
                    "Amora B."
                })
            }));
            credits.Add(new Credits.Thanks("Contributors & Playtesters", new string[32]
            {
                "Nels Anderson",
                "Liam & Graeme Berry",
                "Tamara Bruketta",
                "Allan Defensor",
                "Grayson Evans",
                "Jada Gibbs",
                "Em Halberstadt",
                "Justin Jaffray",
                "Chevy Ray Johnston",
                "Will Lacerda",
                "Myriame Lachapelle",
                "Greg Lobanov",
                "Rafinha Martinelli",
                "Shane Neville",
                "Kyle Pulver",
                "Murphy Pyan",
                "Garret Randell",
                "Kevin Regamey",
                "Atlas Regaudie",
                "Stefano Strapazzon",
                "Nick Suttner",
                "Ryan Thorson",
                "Greg Wohlwend",
                "Justin Yngelmo",
                "baldjared",
                "zep",
                "DevilSquirrel",
                "Covert_Muffin",
                "buhbai",
                "Chaikitty",
                "Msushi",
                "TGH"
            }));
            credits.Add(new Credits.Thanks("Community", new string[3]
            {
                "Speedrunners & Tool Assisted Speedrunners",
                "Everest Modding Community",
                "The Celeste Discord"
            }));
            credits.Add(new Credits.Thanks("Special Thanks", new string[15]
            {
                "Fe Angelo",
                "Bruce Berry & Marilyn Firth",
                "Josephine Baird",
                "Liliane Carpinski",
                "Yvonne Hanson",
                "Katherine Elaine Jones",
                "Clint 'halfcoordinated' Lexa",
                "Greg Lobanov",
                "Gabi 'Platy' Madureira",
                "Rodrigo Monteiro",
                "Fernando Piovesan",
                "Paulo Szyszko Pita",
                "Zoe Si",
                "Julie, Richard, & Ryan Thorson",
                "Davey Wreden"
            }));
            if (polaroids)
            {
                credits.Add(new Credits.Image(GFX.Portraits, "credits/c", 64f, -0.05f));
            }

            credits.Add(new Credits.Thanks("Production Kitties", new string[4]
            {
                "Jiji, Mr Satan, Peridot",
                "Azzy, Phil, Furiosa",
                "Fred, Bastion, Meredith",
                "Bobbin, Finn"
            }));
            credits.AddRange(new List<Credits.CreditNode>()
            {
                 new Credits.Image(GFX.Misc, "fmod"),
                 new Credits.Image(GFX.Misc, "monogame"),
                 new Credits.ImageRow(new Credits.Image[2]
                {
                    new Credits.Image(GFX.Misc, "fna"),
                    new Credits.Image(GFX.Misc, "xna")
                })
            });
            credits.Add(new Credits.Break(540f));
            if (polaroids)
            {
                credits.Add(new Credits.Image(GFX.Portraits, "credits/d", rotation: 0.05f, screenCenter: true));
            }

            credits.Add(new Credits.Ending(Dialog.Clean("CREDITS_THANKYOU"), !polaroids));
            return credits;
        }

        public Credits(float alignment = 0.5f, float scale = 1f, bool haveTitle = true, bool havePolaroids = false)
        {
            this.alignment = alignment;
            this.scale = scale;
            credits = Credits.CreateCredits(haveTitle, havePolaroids);
            Credits.Font = Dialog.Languages["english"].Font;
            Credits.FontSize = Dialog.Languages["english"].FontFaceSize;
            Credits.LineHeight = Font.Get(FontSize).LineHeight;
            height = 0.0f;
            foreach (Credits.CreditNode credit in credits)
            {
                height += credit.Height(scale) + (64f * scale);
            }

            height += 476f;
            if (!havePolaroids)
            {
                return;
            }

            height -= 280f;
        }

        public void Update()
        {
            if (Enabled)
            {
                scroll += scrollSpeed * Engine.DeltaTime * scale;
                if (scrollDelay <= 0.0)
                {
                    scrollSpeed = Calc.Approach(scrollSpeed, 100f * AutoScrollSpeedMultiplier, 1800f * Engine.DeltaTime);
                }
                else
                {
                    scrollDelay -= Engine.DeltaTime;
                }

                if (AllowInput)
                {
                    if (Input.MenuDown.Check)
                    {
                        scrollDelay = 1f;
                        scrollSpeed = Calc.Approach(scrollSpeed, 600f, 1800f * Engine.DeltaTime);
                    }
                    else if (Input.MenuUp.Check)
                    {
                        scrollDelay = 1f;
                        scrollSpeed = Calc.Approach(scrollSpeed, -600f, 1800f * Engine.DeltaTime);
                    }
                    else if (scrollDelay > 0.0)
                    {
                        scrollSpeed = Calc.Approach(scrollSpeed, 0.0f, 1800f * Engine.DeltaTime);
                    }
                }
                if (scroll < 0.0 || scroll > (double)height)
                {
                    scrollSpeed = 0.0f;
                }

                scroll = Calc.Clamp(scroll, 0.0f, height);
                if (scroll >= (double)height)
                {
                    BottomTimer += Engine.DeltaTime;
                }
                else
                {
                    BottomTimer = 0.0f;
                }
            }
            scrollbarAlpha = Calc.Approach(scrollbarAlpha, !Enabled || scrollDelay <= 0.0 ? 0.0f : 1f, Engine.DeltaTime * 2f);
        }

        public void Render(Vector2 position)
        {
            Vector2 position1 = position + new Vector2(0.0f, 1080f - scroll).Floor();
            foreach (Credits.CreditNode credit in credits)
            {
                float num = credit.Height(scale);
                if (position1.Y > -(double)num && position1.Y < 1080.0)
                {
                    credit.Render(position1, alignment, scale);
                }

                position1.Y += num + (64f * scale);
            }
            if (scrollbarAlpha <= 0.0)
            {
                return;
            }

            int y = 64;
            int height1 = 1080 - (y * 2);
            float height2 = height1 * (height1 / height);
            float num1 = (float)(scroll / (double)height * (height1 - (double)height2));
            Draw.Rect(1844f, y, 12f, height1, Color.White * 0.2f * scrollbarAlpha);
            Draw.Rect(1844f, y + num1, 12f, height2, Color.White * 0.5f * scrollbarAlpha);
        }

        private abstract class CreditNode
        {
            public abstract void Render(Vector2 position, float alignment = 0.5f, float scale = 1f);

            public abstract float Height(float scale = 1f);
        }

        private class Role : Credits.CreditNode
        {
            public const float NameScale = 1.8f;
            public const float RolesScale = 1f;
            public const float Spacing = 8f;
            public const float BottomSpacing = 64f;
            public static readonly Color NameColor = Color.White;
            public static readonly Color RolesColor = Color.White * 0.8f;
            public string Name;
            public string Roles;

            public Role(string name, params string[] roles)
            {
                Name = name;
                Roles = string.Join(", ", roles);
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                Credits.Font.DrawOutline(Credits.FontSize, Name, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.8f * scale, Credits.Role.NameColor, 2f, Credits.BorderColor);
                position.Y += (float)((LineHeight * 1.7999999523162842) + 8.0) * scale;
                Credits.Font.DrawOutline(Credits.FontSize, Roles, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1f * scale, Credits.Role.RolesColor, 2f, Credits.BorderColor);
            }

            public override float Height(float scale = 1f)
            {
                return (float)((LineHeight * 2.7999999523162842) + 8.0 + 64.0) * scale;
            }
        }

        private class Team : Credits.CreditNode
        {
            public const float TeamScale = 1.4f;
            public static readonly Color TeamColor = Color.White;
            public string Name;
            public string[] Members;
            public string Roles;

            public Team(string name, string[] members, params string[] roles)
            {
                Name = name;
                Members = members;
                Roles = string.Join(", ", roles);
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                Credits.Font.DrawOutline(Credits.FontSize, Name, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.8f * scale, Credits.Role.NameColor, 2f, Credits.BorderColor);
                position.Y += (float)((LineHeight * 1.7999999523162842) + 8.0) * scale;
                for (int index = 0; index < Members.Length; ++index)
                {
                    Credits.Font.DrawOutline(Credits.FontSize, Members[index], position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.4f * scale, Credits.Team.TeamColor, 2f, Credits.BorderColor);
                    position.Y += Credits.LineHeight * 1.4f * scale;
                }
                Credits.Font.DrawOutline(Credits.FontSize, Roles, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1f * scale, Credits.Role.RolesColor, 2f, Credits.BorderColor);
            }

            public override float Height(float scale = 1f)
            {
                return (float)((LineHeight * (1.7999999523162842 + (Members.Length * 1.3999999761581421) + 1.0)) + 8.0 + 64.0) * scale;
            }
        }

        private class Thanks : Credits.CreditNode
        {
            public const float TitleScale = 1.4f;
            public const float CreditsScale = 1.15f;
            public const float Spacing = 8f;
            public readonly Color TitleColor = Color.White;
            public readonly Color CreditsColor = Color.White * 0.8f;
            public int TopPadding;
            public string Title;
            public string[] Credits;
            private readonly string[] linkedImages;

            public Thanks(string title, params string[] to)
                : this(0, title, to)
            {
            }

            public Thanks(int topPadding, string title, params string[] to)
            {
                TopPadding = topPadding;
                Title = title;
                Credits = to;
                linkedImages = new string[Credits.Length];
                for (int index = 0; index < linkedImages.Length; ++index)
                {
                    linkedImages[index] = null;
                    if (Credits[index].StartsWith("image:"))
                    {
                        linkedImages[index] = Credits[index].Substring(6);
                    }
                }
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                position.Y += TopPadding * scale;
                Font.DrawOutline(FontSize, Title, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.4f * scale, TitleColor, 2f, BorderColor);
                position.Y += (float)((LineHeight * 1.3999999761581421) + 8.0) * scale;
                for (int index = 0; index < Credits.Length; ++index)
                {
                    if (linkedImages[index] != null)
                    {
                        MTexture mtexture = GFX.Gui[linkedImages[index]];
                        mtexture.DrawJustified(position.Floor() + new Vector2(0.0f, -2f), new Vector2(alignment, 0.0f), BorderColor, 1.15f * scale, 0.0f);
                        mtexture.DrawJustified(position.Floor() + new Vector2(0.0f, 2f), new Vector2(alignment, 0.0f), BorderColor, 1.15f * scale, 0.0f);
                        mtexture.DrawJustified(position.Floor() + new Vector2(-2f, 0.0f), new Vector2(alignment, 0.0f), BorderColor, 1.15f * scale, 0.0f);
                        mtexture.DrawJustified(position.Floor() + new Vector2(2f, 0.0f), new Vector2(alignment, 0.0f), BorderColor, 1.15f * scale, 0.0f);
                        mtexture.DrawJustified(position.Floor(), new Vector2(alignment, 0.0f), CreditsColor, 1.15f * scale, 0.0f);
                        position.Y += mtexture.Height * 1.15f * scale;
                    }
                    else
                    {
                        Font.DrawOutline(FontSize, Credits[index], position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.15f * scale, CreditsColor, 2f, BorderColor);
                        position.Y += LineHeight * 1.15f * scale;
                    }
                }
            }

            public override float Height(float scale = 1f)
            {
                return ((float)((LineHeight * (1.3999999761581421 + (Credits.Length * 1.1499999761581421))) + (Credits.Length != 0 ? 8.0 : 0.0)) + TopPadding) * scale;
            }
        }

        private class MultiCredit : Credits.CreditNode
        {
            public const float TitleScale = 1.4f;
            public const float SubtitleScale = 0.7f;
            public const float CreditsScale = 1.15f;
            public const float Spacing = 8f;
            public const float SectionSpacing = 32f;
            public readonly Color TitleColor = Color.White;
            public readonly Color SubtitleColor = Calc.HexToColor("a8a694");
            public readonly Color CreditsColor = Color.White * 0.8f;
            public int TopPadding;
            public string Title;
            public Credits.MultiCredit.Section[] Sections;

            public MultiCredit(string title, params Credits.MultiCredit.Section[] to)
                : this(0, title, to)
            {
            }

            public MultiCredit(int topPadding, string title, Credits.MultiCredit.Section[] to)
            {
                TopPadding = topPadding;
                Title = title;
                Sections = to;
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                position.Y += TopPadding * scale;
                Credits.Font.DrawOutline(Credits.FontSize, Title, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.4f * scale, TitleColor, 2f, Credits.BorderColor);
                position.Y += (float)((LineHeight * 1.3999999761581421) + 8.0) * scale;
                for (int index1 = 0; index1 < Sections.Length; ++index1)
                {
                    Credits.MultiCredit.Section section = Sections[index1];
                    _ = section.Subtitle;
                    Credits.Font.DrawOutline(Credits.FontSize, section.Subtitle, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 0.7f * scale, SubtitleColor, 2f, Credits.BorderColor);
                    position.Y += (float)(section.SubtitleLines * (double)Credits.LineHeight * 0.699999988079071) * scale;
                    for (int index2 = 0; index2 < section.Credits.Length; ++index2)
                    {
                        Credits.Font.DrawOutline(Credits.FontSize, section.Credits[index2], position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.15f * scale, CreditsColor, 2f, Credits.BorderColor);
                        position.Y += Credits.LineHeight * 1.15f * scale;
                    }
                    position.Y += 32f * scale;
                }
            }

            public override float Height(float scale = 1f)
            {
                float num = 0.0f + TopPadding + (float)((LineHeight * 1.3999999761581421) + 8.0);
                for (int index = 0; index < Sections.Length; ++index)
                {
                    num = num + (float)(Sections[index].SubtitleLines * (double)Credits.LineHeight * 0.699999988079071) + (Credits.LineHeight * 1.15f * Sections[index].Credits.Length);
                }

                return (num + (32f * (Sections.Length - 1))) * scale;
            }

            public class Section
            {
                public string Subtitle;
                public int SubtitleLines;
                public string[] Credits;

                public Section(string subtitle, params string[] credits)
                {
                    Subtitle = subtitle.ToUpper();
                    SubtitleLines = subtitle.Split('\n').Length;
                    Credits = credits;
                }
            }
        }

        private class Ending : Credits.CreditNode
        {
            public string Text;
            public bool Spacing;

            public Ending(string text, bool spacing)
            {
                Text = text;
                Spacing = spacing;
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                if (Spacing)
                {
                    position.Y += 540f;
                }
                else
                {
                    position.Y += (float)((double)ActiveFont.LineHeight * 1.5 * (double)scale * 0.5);
                }

                ActiveFont.DrawOutline(Text, new Vector2(960f, position.Y), new Vector2(0.5f, 0.5f), Vector2.One * 1.5f * scale, Color.White, 2f, Credits.BorderColor);
            }

            public override float Height(float scale = 1f)
            {
                return Spacing ? 540f : ActiveFont.LineHeight * 1.5f * scale;
            }
        }

        private class Image : Credits.CreditNode
        {
            public Atlas Atlas;
            public string ImagePath;
            public float BottomPadding;
            public float Rotation;
            public bool ScreenCenter;

            public Image(string path, float bottomPadding = 0.0f)
                : this(GFX.Gui, path, bottomPadding)
            {
            }

            public Image(
                Atlas atlas,
                string path,
                float bottomPadding = 0.0f,
                float rotation = 0.0f,
                bool screenCenter = false)
            {
                Atlas = atlas;
                ImagePath = path;
                BottomPadding = bottomPadding;
                Rotation = rotation;
                ScreenCenter = screenCenter;
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                MTexture atla = Atlas[ImagePath];
                Vector2 position1 = position + (new Vector2(atla.Width * (0.5f - alignment), atla.Height * 0.5f) * scale);
                if (ScreenCenter)
                {
                    position1.X = 960f;
                }

                atla.DrawCentered(position1, Color.White, scale, Rotation);
            }

            public override float Height(float scale = 1f)
            {
                return (Atlas[ImagePath].Height + BottomPadding) * scale;
            }
        }

        private class ImageRow : Credits.CreditNode
        {
            private readonly Credits.Image[] images;

            public ImageRow(params Credits.Image[] images)
            {
                this.images = images;
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
                float num1 = Height(scale);
                float num2 = 0.0f;
                foreach (Credits.Image image in images)
                {
                    num2 += (image.Atlas[image.ImagePath].Width + 32) * scale;
                }

                float num3 = num2 - (32f * scale);
                Vector2 vector2 = position - new Vector2(alignment * num3, 0.0f);
                foreach (Credits.Image image in images)
                {
                    image.Render(vector2 + new Vector2(0.0f, (float)(((double)num1 - (double)image.Height(scale)) / 2.0)), 0.0f, scale);
                    vector2.X += (image.Atlas[image.ImagePath].Width + 32) * scale;
                }
            }

            public override float Height(float scale = 1f)
            {
                float num = 0.0f;
                foreach (Credits.Image image in images)
                {
                    if ((double)image.Height(scale) > (double)num)
                    {
                        num = image.Height(scale);
                    }
                }
                return num;
            }
        }

        private class Break : Credits.CreditNode
        {
            public float Size;

            public Break(float size = 64f)
            {
                Size = size;
            }

            public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
            {
            }

            public override float Height(float scale = 1f)
            {
                return Size * scale;
            }
        }
    }
}
