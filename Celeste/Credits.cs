// Decompiled with JetBrains decompiler
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
    private List<Credits.CreditNode> credits;
    public float AutoScrollSpeedMultiplier = 1f;
    private float scrollSpeed = 100f;
    private float scroll;
    private float height;
    private float scrollDelay;
    private float scrollbarAlpha;
    private float alignment;
    private float scale;
    public float BottomTimer;
    public bool Enabled = true;
    public bool AllowInput = true;
    public static PixelFont Font;
    public static float FontSize;
    public static float LineHeight;

    private static List<Credits.CreditNode> CreateCredits(bool title, bool polaroids)
    {
      List<Credits.CreditNode> credits = new List<Credits.CreditNode>();
      if (title)
        credits.Add((Credits.CreditNode) new Credits.Image(nameof (title), 320f));
      credits.AddRange((IEnumerable<Credits.CreditNode>) new List<Credits.CreditNode>()
      {
        (Credits.CreditNode) new Credits.Role("Maddy Thorson", new string[4]
        {
          "Director",
          "Designer",
          "Writer",
          "Gameplay Coder"
        }),
        (Credits.CreditNode) new Credits.Role("Noel Berry", new string[3]
        {
          "Co-Creator",
          "Programmer",
          "Artist"
        }),
        (Credits.CreditNode) new Credits.Role("Amora B.", new string[2]
        {
          "Concept Artist",
          "High Res Artist"
        }),
        (Credits.CreditNode) new Credits.Role("Pedro Medeiros", new string[2]
        {
          "Pixel Artist",
          "UI Artist"
        }),
        (Credits.CreditNode) new Credits.Role("Lena Raine", new string[1]
        {
          "Composer"
        }),
        (Credits.CreditNode) new Credits.Team("Power Up Audio", new string[4]
        {
          "Kevin Regamey",
          "Jeff Tangsoc",
          "Joey Godard",
          "Cole Verderber"
        }, new string[1]{ "Sound Designers" })
      });
      if (polaroids)
        credits.Add((Credits.CreditNode) new Credits.Image(GFX.Portraits, "credits/a", 64f, -0.05f));
      credits.AddRange((IEnumerable<Credits.CreditNode>) new List<Credits.CreditNode>()
      {
        (Credits.CreditNode) new Credits.Role("Gabby DaRienzo", new string[1]
        {
          "3D Artist"
        }),
        (Credits.CreditNode) new Credits.Role("Sven Bergström", new string[1]
        {
          "3D Lighting Artist"
        })
      });
      credits.AddRange((IEnumerable<Credits.CreditNode>) new List<Credits.CreditNode>()
      {
        (Credits.CreditNode) new Credits.Thanks("Writing Assistance", new string[5]
        {
          "Noel Berry",
          "Amora B.",
          "Greg Lobanov",
          "Lena Raine",
          "Nick Suttner"
        }),
        (Credits.CreditNode) new Credits.Thanks("Script Editor", new string[1]
        {
          "Nick Suttner"
        }),
        (Credits.CreditNode) new Credits.Thanks("Narrative Consulting", new string[4]
        {
          "Silverstring Media",
          "Claris Cyarron",
          "with Lucas JW Johnson",
          "and Tanya Kan"
        })
      });
      credits.Add((Credits.CreditNode) new Credits.Thanks("Remixers", Credits.Remixers));
      credits.Add((Credits.CreditNode) new Credits.MultiCredit("Musical Performances", new Credits.MultiCredit.Section[3]
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
        credits.Add((Credits.CreditNode) new Credits.Image(GFX.Portraits, "credits/b", 64f, 0.05f));
      credits.Add((Credits.CreditNode) new Credits.Thanks("Operations Manager", new string[1]
      {
        "Heidy Motta"
      }));
      credits.Add((Credits.CreditNode) new Credits.Thanks("Porting", new string[2]
      {
        "Sickhead Games, LLC",
        "Ethan Lee"
      }));
      credits.Add((Credits.CreditNode) new Credits.MultiCredit("Localization", new Credits.MultiCredit.Section[4]
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
      credits.Add((Credits.CreditNode) new Credits.Thanks("Contributors & Playtesters", new string[32]
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
      credits.Add((Credits.CreditNode) new Credits.Thanks("Community", new string[3]
      {
        "Speedrunners & Tool Assisted Speedrunners",
        "Everest Modding Community",
        "The Celeste Discord"
      }));
      credits.Add((Credits.CreditNode) new Credits.Thanks("Special Thanks", new string[15]
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
        credits.Add((Credits.CreditNode) new Credits.Image(GFX.Portraits, "credits/c", 64f, -0.05f));
      credits.Add((Credits.CreditNode) new Credits.Thanks("Production Kitties", new string[4]
      {
        "Jiji, Mr Satan, Peridot",
        "Azzy, Phil, Furiosa",
        "Fred, Bastion, Meredith",
        "Bobbin, Finn"
      }));
      credits.AddRange((IEnumerable<Credits.CreditNode>) new List<Credits.CreditNode>()
      {
        (Credits.CreditNode) new Credits.Image(GFX.Misc, "fmod"),
        (Credits.CreditNode) new Credits.Image(GFX.Misc, "monogame"),
        (Credits.CreditNode) new Credits.ImageRow(new Credits.Image[2]
        {
          new Credits.Image(GFX.Misc, "fna"),
          new Credits.Image(GFX.Misc, "xna")
        })
      });
      credits.Add((Credits.CreditNode) new Credits.Break(540f));
      if (polaroids)
        credits.Add((Credits.CreditNode) new Credits.Image(GFX.Portraits, "credits/d", rotation: 0.05f, screenCenter: true));
      credits.Add((Credits.CreditNode) new Credits.Ending(Dialog.Clean("CREDITS_THANKYOU"), !polaroids));
      return credits;
    }

    public Credits(float alignment = 0.5f, float scale = 1f, bool haveTitle = true, bool havePolaroids = false)
    {
      this.alignment = alignment;
      this.scale = scale;
      this.credits = Credits.CreateCredits(haveTitle, havePolaroids);
      Credits.Font = Dialog.Languages["english"].Font;
      Credits.FontSize = Dialog.Languages["english"].FontFaceSize;
      Credits.LineHeight = (float) Credits.Font.Get(Credits.FontSize).LineHeight;
      this.height = 0.0f;
      foreach (Credits.CreditNode credit in this.credits)
        this.height += credit.Height(scale) + 64f * scale;
      this.height += 476f;
      if (!havePolaroids)
        return;
      this.height -= 280f;
    }

    public void Update()
    {
      if (this.Enabled)
      {
        this.scroll += this.scrollSpeed * Engine.DeltaTime * this.scale;
        if ((double) this.scrollDelay <= 0.0)
          this.scrollSpeed = Calc.Approach(this.scrollSpeed, 100f * this.AutoScrollSpeedMultiplier, 1800f * Engine.DeltaTime);
        else
          this.scrollDelay -= Engine.DeltaTime;
        if (this.AllowInput)
        {
          if (Input.MenuDown.Check)
          {
            this.scrollDelay = 1f;
            this.scrollSpeed = Calc.Approach(this.scrollSpeed, 600f, 1800f * Engine.DeltaTime);
          }
          else if (Input.MenuUp.Check)
          {
            this.scrollDelay = 1f;
            this.scrollSpeed = Calc.Approach(this.scrollSpeed, -600f, 1800f * Engine.DeltaTime);
          }
          else if ((double) this.scrollDelay > 0.0)
            this.scrollSpeed = Calc.Approach(this.scrollSpeed, 0.0f, 1800f * Engine.DeltaTime);
        }
        if ((double) this.scroll < 0.0 || (double) this.scroll > (double) this.height)
          this.scrollSpeed = 0.0f;
        this.scroll = Calc.Clamp(this.scroll, 0.0f, this.height);
        if ((double) this.scroll >= (double) this.height)
          this.BottomTimer += Engine.DeltaTime;
        else
          this.BottomTimer = 0.0f;
      }
      this.scrollbarAlpha = Calc.Approach(this.scrollbarAlpha, !this.Enabled || (double) this.scrollDelay <= 0.0 ? 0.0f : 1f, Engine.DeltaTime * 2f);
    }

    public void Render(Vector2 position)
    {
      Vector2 position1 = position + new Vector2(0.0f, 1080f - this.scroll).Floor();
      foreach (Credits.CreditNode credit in this.credits)
      {
        float num = credit.Height(this.scale);
        if ((double) position1.Y > -(double) num && (double) position1.Y < 1080.0)
          credit.Render(position1, this.alignment, this.scale);
        position1.Y += num + 64f * this.scale;
      }
      if ((double) this.scrollbarAlpha <= 0.0)
        return;
      int y = 64;
      int height1 = 1080 - y * 2;
      float height2 = (float) height1 * ((float) height1 / this.height);
      float num1 = (float) ((double) this.scroll / (double) this.height * ((double) height1 - (double) height2));
      Draw.Rect(1844f, (float) y, 12f, (float) height1, Color.White * 0.2f * this.scrollbarAlpha);
      Draw.Rect(1844f, (float) y + num1, 12f, height2, Color.White * 0.5f * this.scrollbarAlpha);
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
        this.Name = name;
        this.Roles = string.Join(", ", roles);
      }

      public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
      {
        Credits.Font.DrawOutline(Credits.FontSize, this.Name, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.8f * scale, Credits.Role.NameColor, 2f, Credits.BorderColor);
        position.Y += (float) ((double) Credits.LineHeight * 1.7999999523162842 + 8.0) * scale;
        Credits.Font.DrawOutline(Credits.FontSize, this.Roles, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1f * scale, Credits.Role.RolesColor, 2f, Credits.BorderColor);
      }

      public override float Height(float scale = 1f) => (float) ((double) Credits.LineHeight * 2.7999999523162842 + 8.0 + 64.0) * scale;
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
        this.Name = name;
        this.Members = members;
        this.Roles = string.Join(", ", roles);
      }

      public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
      {
        Credits.Font.DrawOutline(Credits.FontSize, this.Name, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.8f * scale, Credits.Role.NameColor, 2f, Credits.BorderColor);
        position.Y += (float) ((double) Credits.LineHeight * 1.7999999523162842 + 8.0) * scale;
        for (int index = 0; index < this.Members.Length; ++index)
        {
          Credits.Font.DrawOutline(Credits.FontSize, this.Members[index], position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.4f * scale, Credits.Team.TeamColor, 2f, Credits.BorderColor);
          position.Y += Credits.LineHeight * 1.4f * scale;
        }
        Credits.Font.DrawOutline(Credits.FontSize, this.Roles, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1f * scale, Credits.Role.RolesColor, 2f, Credits.BorderColor);
      }

      public override float Height(float scale = 1f) => (float) ((double) Credits.LineHeight * (1.7999999523162842 + (double) this.Members.Length * 1.3999999761581421 + 1.0) + 8.0 + 64.0) * scale;
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
      private string[] linkedImages;

      public Thanks(string title, params string[] to)
        : this(0, title, to)
      {
      }

      public Thanks(int topPadding, string title, params string[] to)
      {
        this.TopPadding = topPadding;
        this.Title = title;
        this.Credits = to;
        this.linkedImages = new string[this.Credits.Length];
        for (int index = 0; index < this.linkedImages.Length; ++index)
        {
          this.linkedImages[index] = (string) null;
          if (this.Credits[index].StartsWith("image:"))
            this.linkedImages[index] = this.Credits[index].Substring(6);
        }
      }

      public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
      {
        position.Y += (float) this.TopPadding * scale;
        Credits.Font.DrawOutline(Credits.FontSize, this.Title, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.4f * scale, this.TitleColor, 2f, Credits.BorderColor);
        position.Y += (float) ((double) Credits.LineHeight * 1.3999999761581421 + 8.0) * scale;
        for (int index = 0; index < this.Credits.Length; ++index)
        {
          if (this.linkedImages[index] != null)
          {
            MTexture mtexture = GFX.Gui[this.linkedImages[index]];
            mtexture.DrawJustified(position.Floor() + new Vector2(0.0f, -2f), new Vector2(alignment, 0.0f), Credits.BorderColor, 1.15f * scale, 0.0f);
            mtexture.DrawJustified(position.Floor() + new Vector2(0.0f, 2f), new Vector2(alignment, 0.0f), Credits.BorderColor, 1.15f * scale, 0.0f);
            mtexture.DrawJustified(position.Floor() + new Vector2(-2f, 0.0f), new Vector2(alignment, 0.0f), Credits.BorderColor, 1.15f * scale, 0.0f);
            mtexture.DrawJustified(position.Floor() + new Vector2(2f, 0.0f), new Vector2(alignment, 0.0f), Credits.BorderColor, 1.15f * scale, 0.0f);
            mtexture.DrawJustified(position.Floor(), new Vector2(alignment, 0.0f), this.CreditsColor, 1.15f * scale, 0.0f);
            position.Y += (float) mtexture.Height * 1.15f * scale;
          }
          else
          {
            Credits.Font.DrawOutline(Credits.FontSize, this.Credits[index], position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.15f * scale, this.CreditsColor, 2f, Credits.BorderColor);
            position.Y += Credits.LineHeight * 1.15f * scale;
          }
        }
      }

      public override float Height(float scale = 1f) => ((float) ((double) Credits.LineHeight * (1.3999999761581421 + (double) this.Credits.Length * 1.1499999761581421) + (this.Credits.Length != 0 ? 8.0 : 0.0)) + (float) this.TopPadding) * scale;
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
        this.TopPadding = topPadding;
        this.Title = title;
        this.Sections = to;
      }

      public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
      {
        position.Y += (float) this.TopPadding * scale;
        Credits.Font.DrawOutline(Credits.FontSize, this.Title, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.4f * scale, this.TitleColor, 2f, Credits.BorderColor);
        position.Y += (float) ((double) Credits.LineHeight * 1.3999999761581421 + 8.0) * scale;
        for (int index1 = 0; index1 < this.Sections.Length; ++index1)
        {
          Credits.MultiCredit.Section section = this.Sections[index1];
          string subtitle = section.Subtitle;
          Credits.Font.DrawOutline(Credits.FontSize, section.Subtitle, position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 0.7f * scale, this.SubtitleColor, 2f, Credits.BorderColor);
          position.Y += (float) ((double) section.SubtitleLines * (double) Credits.LineHeight * 0.699999988079071) * scale;
          for (int index2 = 0; index2 < section.Credits.Length; ++index2)
          {
            Credits.Font.DrawOutline(Credits.FontSize, section.Credits[index2], position.Floor(), new Vector2(alignment, 0.0f), Vector2.One * 1.15f * scale, this.CreditsColor, 2f, Credits.BorderColor);
            position.Y += Credits.LineHeight * 1.15f * scale;
          }
          position.Y += 32f * scale;
        }
      }

      public override float Height(float scale = 1f)
      {
        float num = 0.0f + (float) this.TopPadding + (float) ((double) Credits.LineHeight * 1.3999999761581421 + 8.0);
        for (int index = 0; index < this.Sections.Length; ++index)
          num = num + (float) ((double) this.Sections[index].SubtitleLines * (double) Credits.LineHeight * 0.699999988079071) + Credits.LineHeight * 1.15f * (float) this.Sections[index].Credits.Length;
        return (num + 32f * (float) (this.Sections.Length - 1)) * scale;
      }

      public class Section
      {
        public string Subtitle;
        public int SubtitleLines;
        public string[] Credits;

        public Section(string subtitle, params string[] credits)
        {
          this.Subtitle = subtitle.ToUpper();
          this.SubtitleLines = subtitle.Split('\n').Length;
          this.Credits = credits;
        }
      }
    }

    private class Ending : Credits.CreditNode
    {
      public string Text;
      public bool Spacing;

      public Ending(string text, bool spacing)
      {
        this.Text = text;
        this.Spacing = spacing;
      }

      public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
      {
        if (this.Spacing)
          position.Y += 540f;
        else
          position.Y += (float) ((double) ActiveFont.LineHeight * 1.5 * (double) scale * 0.5);
        ActiveFont.DrawOutline(this.Text, new Vector2(960f, position.Y), new Vector2(0.5f, 0.5f), Vector2.One * 1.5f * scale, Color.White, 2f, Credits.BorderColor);
      }

      public override float Height(float scale = 1f) => this.Spacing ? 540f : ActiveFont.LineHeight * 1.5f * scale;
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
        this.Atlas = atlas;
        this.ImagePath = path;
        this.BottomPadding = bottomPadding;
        this.Rotation = rotation;
        this.ScreenCenter = screenCenter;
      }

      public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
      {
        MTexture atla = this.Atlas[this.ImagePath];
        Vector2 position1 = position + new Vector2((float) atla.Width * (0.5f - alignment), (float) atla.Height * 0.5f) * scale;
        if (this.ScreenCenter)
          position1.X = 960f;
        atla.DrawCentered(position1, Color.White, scale, this.Rotation);
      }

      public override float Height(float scale = 1f) => ((float) this.Atlas[this.ImagePath].Height + this.BottomPadding) * scale;
    }

    private class ImageRow : Credits.CreditNode
    {
      private Credits.Image[] images;

      public ImageRow(params Credits.Image[] images) => this.images = images;

      public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
      {
        float num1 = this.Height(scale);
        float num2 = 0.0f;
        foreach (Credits.Image image in this.images)
          num2 += (float) (image.Atlas[image.ImagePath].Width + 32) * scale;
        float num3 = num2 - 32f * scale;
        Vector2 vector2 = position - new Vector2(alignment * num3, 0.0f);
        foreach (Credits.Image image in this.images)
        {
          image.Render(vector2 + new Vector2(0.0f, (float) (((double) num1 - (double) image.Height(scale)) / 2.0)), 0.0f, scale);
          vector2.X += (float) (image.Atlas[image.ImagePath].Width + 32) * scale;
        }
      }

      public override float Height(float scale = 1f)
      {
        float num = 0.0f;
        foreach (Credits.Image image in this.images)
        {
          if ((double) image.Height(scale) > (double) num)
            num = image.Height(scale);
        }
        return num;
      }
    }

    private class Break : Credits.CreditNode
    {
      public float Size;

      public Break(float size = 64f) => this.Size = size;

      public override void Render(Vector2 position, float alignment = 0.5f, float scale = 1f)
      {
      }

      public override float Height(float scale = 1f) => this.Size * scale;
    }
  }
}
