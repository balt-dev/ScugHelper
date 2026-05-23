using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.Helpers;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[TrackedAs(typeof(WallBooster))]
[CustomEntity("ScugHelper/NeutrallessWallBooster")]
public class NeutrallessWallBooster : WallBooster
{
    private Vector2 imageOffset = Vector2.Zero;

    private static ILHook? wallJumpHook;

    public NeutrallessWallBooster(EntityData data, Vector2 offset) : base(data, offset) {
        tiles = BuildNeutrallessSprite(Facing == Facings.Left);
        Add(new StaticMover {
            SolidChecker = IsRiding,
            OnShake = OnShake,
            OnEnable = OnEnable,
            OnDisable = OnDisable
        });
        Collider = Facing == Facings.Left ? new Hitbox(4f, data.Height) : new Hitbox(4f, data.Height, 4f);
    }
    public bool IsRiding(Solid solid) {
        return Facing switch {
            Facings.Left => CollideCheckOutside(solid, Position - Vector2.UnitX),
            Facings.Right => CollideCheckOutside(solid, Position + Vector2.UnitX * 7),
            _ => false,
        };
    }

    public void OnEnable() {
        Active = Visible = Collidable = true;
    }

    public void OnDisable() {
        Active = Collidable = false;
        Visible = false;
    }
    public void OnShake(Vector2 amount) {
        imageOffset += amount;
    }
    public override void Render() {
        Vector2 position = Position;
        Position += imageOffset;
        base.Render();
        Position = position;
    }

    public List<Sprite> BuildNeutrallessSprite(bool left) {
        foreach (var tile in tiles)
            tile.RemoveSelf();
        List<Sprite> list = [];
        for (int i = 0; i < Height; i += 8) {
            string id;
            if (i == 0) id = "neutrallessWallBoosterTop";
            else if (!(i + 16 > Height)) id = "neutrallessWallBoosterMid";
            else id = "neutrallessWallBoosterBottom";

            Sprite sprite = GFX.SpriteBank.Create(id);
            if (sprite?.Texture?.Texture?.Texture is null) throw new Exception($"Sprite of ID {id} was null.");
            if (!left)
                sprite.FlipX = true;
            sprite.Position = new Vector2(0f, i);

            list.Add(sprite);
            Add(sprite);
        }

        return list;
    }
    [OnLoad]
    public static void LoadHooks() {
        wallJumpHook = new ILHook(typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic)!, ModWallJump);
    }
    [OnUnload]
    public static void UnloadHooks() {
        wallJumpHook?.Dispose();
    }


    private static void ModWallJump(ILContext il) {
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNextBestFit(MoveType.After, 16,
            instr => instr.OpCode == OpCodes.Ldarg_0,
            instr => instr.MatchLdfld<Player>("moveX"))
        ) throw new Exception("Neutralless wall boosters failed to match IL for Player orig_WallJump hook.");
        ILCursor cursorAfterBranch = cursor.Clone();
        if (!cursorAfterBranch.TryGotoNextBestFit(MoveType.After, instr => instr.OpCode == OpCodes.Brfalse_S))
            throw new Exception("Neutralless wall boosters failed to match IL for Player orig_WallJump hook.");
        cursor.Emit(OpCodes.Pop);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(NeutralJumpCheck);
        cursor.Emit(OpCodes.Brfalse_S, cursorAfterBranch.Next!);
        cursor.Emit(OpCodes.Ldarg_0);
        cursor.EmitDelegate(static (Player player) => player.moveX); // Do it this way so that other hooks don't explode
    }

    private static bool NeutralJumpCheck(Player self) {
        foreach (WallBooster entity in self.Scene.Tracker.GetEntities<WallBooster>())
            if (entity is NeutrallessWallBooster && entity.Facing == self.Facing && self.CollideCheck(entity))
                return false;
        return true;
    }
}
#nullable restore
