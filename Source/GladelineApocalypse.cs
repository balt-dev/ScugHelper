using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.ScugHelper;

internal static class GladelineApocalypse
{
    [OnLoad]
    internal static void LoadHooks() {
        static MTexture DoCheck(MTexture self)
        {
            if (ScugHelperModuleSettings.GladelineApocalypse)
            {
                var im = GFX.Portraits["madeline/normal00"];
                im.ScaleFix = Math.Max((float) self.Width, (float) self.Height) / 160f;
                return im;
            }
            else
            {
                return self;
            }
        }

        On.Monocle.MTexture.DrawCentered_Vector2 += static (orig, self, a) => { orig(DoCheck(self),  a); };
        On.Monocle.MTexture.DrawCentered_Vector2_Color += static (orig, self, a, b) => { orig(DoCheck(self),  a, b); };
        On.Monocle.MTexture.DrawCentered_Vector2_Color_Vector2 += static (orig, self, a, b, c) => { orig(DoCheck(self),  a, b, c); };
        On.Monocle.MTexture.DrawCentered_Vector2_Color_Vector2_float += static (orig, self, a, b, c, d) => { orig(DoCheck(self),  a, b, c, d); };
        On.Monocle.MTexture.DrawCentered_Vector2_Color_Vector2_float_SpriteEffects += static (orig, self, a, b, c, d, e) => { orig(DoCheck(self),  a, b, c, d, e); };
        On.Monocle.MTexture.DrawCentered_Vector2_Color_float += static (orig, self, a, b, c) => { orig(DoCheck(self),  a, b, c); };
        On.Monocle.MTexture.DrawCentered_Vector2_Color_float_float += static (orig, self, a, b, c, d) => { orig(DoCheck(self),  a, b, c, d); };
        On.Monocle.MTexture.DrawCentered_Vector2_Color_float_float_SpriteEffects += static (orig, self, a, b, c, d, e) => { orig(DoCheck(self),  a, b, c, d, e); };
        
        On.Monocle.MTexture.DrawOutlineCentered_Vector2 += static (orig, self, a) => { orig(DoCheck(self),  a); };
        On.Monocle.MTexture.DrawOutlineCentered_Vector2_Color += static (orig, self, a, b) => { orig(DoCheck(self),  a, b); };
        On.Monocle.MTexture.DrawOutlineCentered_Vector2_Color_Vector2 += static (orig, self, a, b, c) => { orig(DoCheck(self),  a, b, c); };
        On.Monocle.MTexture.DrawOutlineCentered_Vector2_Color_Vector2_float += static (orig, self, a, b, c, d) => { orig(DoCheck(self),  a, b, c, d); };
        On.Monocle.MTexture.DrawOutlineCentered_Vector2_Color_Vector2_float_SpriteEffects += static (orig, self, a, b, c, d, e) => { orig(DoCheck(self),  a, b, c, d, e); };
        On.Monocle.MTexture.DrawOutlineCentered_Vector2_Color_float += static (orig, self, a, b, c) => { orig(DoCheck(self),  a, b, c); };
        On.Monocle.MTexture.DrawOutlineCentered_Vector2_Color_float_float += static (orig, self, a, b, c, d) => { orig(DoCheck(self),  a, b, c, d); };
        On.Monocle.MTexture.DrawOutlineCentered_Vector2_Color_float_float_SpriteEffects += static (orig, self, a, b, c, d, e) => { orig(DoCheck(self),  a, b, c, d, e); };
        
        On.Monocle.MTexture.Draw_Vector2 += static (orig, self, a) => { orig(DoCheck(self),  a); };
        On.Monocle.MTexture.Draw_Vector2_Vector2 += static (orig, self, a, b) => { orig(DoCheck(self),  a, b); };
        On.Monocle.MTexture.Draw_Vector2_Vector2_Color += static (orig, self, a, b, z) => { orig(DoCheck(self),  a, b, z); };
        On.Monocle.MTexture.Draw_Vector2_Vector2_Color_Vector2 += static (orig, self, a, b, c, z) => { orig(DoCheck(self),  a, b, c, z); };
        On.Monocle.MTexture.Draw_Vector2_Vector2_Color_Vector2_float += static (orig, self, a, b, c, d, z) => { orig(DoCheck(self),  a, b, c, d, z); };
        On.Monocle.MTexture.Draw_Vector2_Vector2_Color_Vector2_float_SpriteEffects += static (orig, self, a, b, c, d, e, z) => { orig(DoCheck(self),  a, b, c, d, e, z); };
        On.Monocle.MTexture.Draw_Vector2_Vector2_Color_float += static (orig, self, a, b, c, z) => { orig(DoCheck(self),  a, b, c, z); };
        On.Monocle.MTexture.Draw_Vector2_Vector2_Color_float_float += static (orig, self, a, b, c, d, z) => { orig(DoCheck(self),  a, b, c, d, z); };
        On.Monocle.MTexture.Draw_Vector2_Vector2_Color_float_float_SpriteEffects += static (orig, self, a, b, c, d, e, z) => { orig(DoCheck(self),  a, b, c, d, e, z); };
        
        On.Monocle.MTexture.DrawJustified_Vector2_Vector2 += static (orig, self, a, b) => { orig(DoCheck(self),  a, b); };
        On.Monocle.MTexture.DrawJustified_Vector2_Vector2_Color += static (orig, self, a, b, z) => { orig(DoCheck(self),  a, b, z); };
        On.Monocle.MTexture.DrawJustified_Vector2_Vector2_Color_Vector2 += static (orig, self, a, b, c, z) => { orig(DoCheck(self),  a, b, c, z); };
        On.Monocle.MTexture.DrawJustified_Vector2_Vector2_Color_Vector2_float += static (orig, self, a, b, c, d, z) => { orig(DoCheck(self),  a, b, c, d, z); };
        On.Monocle.MTexture.DrawJustified_Vector2_Vector2_Color_Vector2_float_SpriteEffects += static (orig, self, a, b, c, d, e, z) => { orig(DoCheck(self),  a, b, c, d, e, z); };
        On.Monocle.MTexture.DrawJustified_Vector2_Vector2_Color_float += static (orig, self, a, b, c, z) => { orig(DoCheck(self),  a, b, c, z); };
        On.Monocle.MTexture.DrawJustified_Vector2_Vector2_Color_float_float += static (orig, self, a, b, c, d, z) => { orig(DoCheck(self),  a, b, c, d, z); };
        On.Monocle.MTexture.DrawJustified_Vector2_Vector2_Color_float_float_SpriteEffects += static (orig, self, a, b, c, d, e, z) => { orig(DoCheck(self),  a, b, c, d, e, z); };
        
        On.Monocle.MTexture.DrawOutlineJustified_Vector2_Vector2 += static (orig, self, a, b) => { orig(DoCheck(self),  a, b); };
        On.Monocle.MTexture.DrawOutlineJustified_Vector2_Vector2_Color += static (orig, self, a, b, z) => { orig(DoCheck(self),  a, b, z); };
        On.Monocle.MTexture.DrawOutlineJustified_Vector2_Vector2_Color_Vector2 += static (orig, self, a, b, c, z) => { orig(DoCheck(self),  a, b, c, z); };
        On.Monocle.MTexture.DrawOutlineJustified_Vector2_Vector2_Color_Vector2_float += static (orig, self, a, b, c, d, z) => { orig(DoCheck(self),  a, b, c, d, z); };
        On.Monocle.MTexture.DrawOutlineJustified_Vector2_Vector2_Color_Vector2_float_SpriteEffects += static (orig, self, a, b, c, d, e, z) => { orig(DoCheck(self),  a, b, c, d, e, z); };
        On.Monocle.MTexture.DrawOutlineJustified_Vector2_Vector2_Color_float += static (orig, self, a, b, c, z) => { orig(DoCheck(self),  a, b, c, z); };
        On.Monocle.MTexture.DrawOutlineJustified_Vector2_Vector2_Color_float_float += static (orig, self, a, b, c, d, z) => { orig(DoCheck(self),  a, b, c, d, z); };
        On.Monocle.MTexture.DrawOutlineJustified_Vector2_Vector2_Color_float_float_SpriteEffects += static (orig, self, a, b, c, d, e, z) => { orig(DoCheck(self),  a, b, c, d, e, z); };
    }
}
