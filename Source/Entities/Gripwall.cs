using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using System.Collections.Generic;
using MonoMod.Cil;

[Tracked]
[CustomEntity("ScugHelper/Gripwall")]
public class Gripwall : Entity
{
    private static readonly Color RefillBothColor = new(0xd6, 0xf2, 0x64);
    private static readonly Color RefillDashColor = new(0x88, 0xea, 0xff);
    private static readonly Color RefillStaminaColor = new(0xf2, 0xe7, 0x9b);
    private static readonly Color RefillNoneColor = new(0xff, 0x6e, 0x54);

    public Facings Facing;
    private List<Sprite> tiles;

    public bool RefillDash { get; protected set; }
    public bool RefillStamina { get; protected set; }

    public Gripwall(EntityData data, Vector2 offset)
        : this(data.Position + offset, data.Height, data.Bool("left"), data.Bool("refillDash"), data.Bool("refillStamina"))
        { }


    public Gripwall(Vector2 position, float height, bool left, bool refillDash, bool refillStamina)
        : base(position)
    {
        RefillDash = refillDash;
        RefillStamina = refillStamina;
        Tag = Tags.TransitionUpdate;
        Depth = 1990;
        if (left)
        {
            Facing = Facings.Left;
            Collider = new Hitbox(2f, height);
        }
        else
        {
            Facing = Facings.Right;
            Collider = new Hitbox(2f, height, 6f);
        }
        tiles = BuildSprite(left);
    }


    private List<Sprite> BuildSprite(bool left)
    {
        List<Sprite> list = [];
        for (int i = 0; i < Height; i += 8)
        {
            string id;
            if (i == 0)
                id = "gripwallTop";
            else if (!(i + 16 > Height))
                id = "gripwallMid";
            else
                id = "gripwallBottom";

            Sprite sprite = GFX.SpriteBank.Create(id);
            if (left)
                sprite.Position = new Vector2(0f, i);
            else
            {
                sprite.FlipX = true;
                sprite.Position = new Vector2(4f, i);
            }
            if (RefillDash)
            {
                if (RefillStamina)
                {
                    sprite.Color = RefillBothColor;
                }
                else
                {
                    sprite.Color = RefillDashColor;
                }
            }
            else if (RefillStamina)
            {
                sprite.Color = RefillStaminaColor;
            }
            else
            {
                sprite.Color = RefillNoneColor;
            }

            Add(sprite);
        }
        return list;
    }

    internal static void LoadHooks()
    {
        IL.Celeste.Player.ClimbUpdate += ClimbUpdateHook;
    }


    internal static void UnloadHooks()
    {
        IL.Celeste.Player.ClimbUpdate -= ClimbUpdateHook;
    }

    private static void ClimbUpdateHook(ILContext il)
    {
        ILCursor cur = new(il);
        if (!cur.TryGotoNext(MoveType.Before,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchCallvirt<Player>("WallBoosterCheck")
        )) throw new InvalidOperationException("Gripwalls failed to match IL code for the ClimbUpdate hook.");
        static bool Delegate(Player self)
        {
            if (self.climbNoMoveTimer > 0)
            {
                return false;
            }
            if (ClimbBlocker.Check(self.Scene, self, self.Position + Vector2.UnitX * (int)self.Facing))
            {

                return false;
            }
            foreach (Gripwall gripwall in self.Scene.Tracker.GetEntities<Gripwall>())
                if (gripwall.Facing == self.Facing && self.CollideCheck(gripwall))
                {
                    if (gripwall.RefillDash)
                        self.RefillDash();
                    if (gripwall.RefillStamina)
                        self.RefillStamina();
                    self.Speed.Y = 0f;
                    return true;
                }
            return false;
        }
        cur.MoveAfterLabels();
        cur.EmitLdarg0();
        cur.EmitDelegate(Delegate);
        ILLabel label = cur.DefineLabel();
        cur.EmitBrfalse(label);
        cur.EmitLdcI4(Player.StClimb);
        cur.EmitRet();
        cur.MarkLabel(label);
    }
}
