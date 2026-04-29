using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using System.Collections;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[TrackedAs(typeof(Seeker))]
[CustomEntity("ScugHelper/FragileSeeker")]
public class FragileSeeker : Seeker
{
    bool Crushing = false;

    public FragileSeeker(Vector2 position, Vector2[] patrolPoints) : base(position, patrolPoints)
    {
        sprite.RemoveSelf();
        Add(sprite = GFX.SpriteBank.Create("fragileSeeker"));
        sprite.OnLastFrame = f =>
        {
            if (flipAnimations.Contains(f) && spriteFacing != facing)
            {
                spriteFacing = facing;
                if (nextSprite != null)
                {
                    sprite.Play(nextSprite);
                    nextSprite = null;
                }
            }
        };
        sprite.OnChange = (last, next) =>
        {
            nextSprite = null;
            sprite.OnLastFrame(last);
        };
    }

    public FragileSeeker(EntityData data, Vector2 offset) : this(data.Position + offset, data.NodesOffset(offset)) {}

    [OnLoad]
    public static void LoadHooks() {
        On.Celeste.Actor.TrySquishWiggle_CollisionData_int_int += OnSquishWiggle;
        On.Celeste.Seeker.GotBouncedOn += OnBounce;
        On.Celeste.Seeker.CreateTrail += OnTrail;
    }
    [OnUnload]
    public static void UnloadHooks() {
        On.Celeste.Actor.TrySquishWiggle_CollisionData_int_int -= OnSquishWiggle;
        On.Celeste.Seeker.GotBouncedOn -= OnBounce;
        On.Celeste.Seeker.CreateTrail -= OnTrail;
    }

    static readonly Color NewTrailColor = Calc.HexToColor("c717d7");

    private static void OnTrail(On.Celeste.Seeker.orig_CreateTrail orig, Seeker self)
    {
        if (self is FragileSeeker)
        {
            Vector2 scale = self.sprite.Scale;
            self.sprite.Scale *= 1f - 0.3f * self.scaleWiggler.Value;
            self.sprite.Scale.X *= self.spriteFacing;
            TrailManager.Add(self, NewTrailColor, 0.5f, frozenUpdate: false, useRawDeltaTime: false);
            self.sprite.Scale = scale;
        }
        else orig(self);
    }

    private static bool OnSquishWiggle(On.Celeste.Actor.orig_TrySquishWiggle_CollisionData_int_int orig, Actor self, CollisionData data, int wiggleX, int wiggleY)
    {
        if (self is FragileSeeker fragSeeker && fragSeeker.Crushing) return false;
        return orig(self, data, wiggleX, wiggleY);
    }

    private static void OnBounce(On.Celeste.Seeker.orig_GotBouncedOn orig, Seeker self, Entity entity)
    {
        if (self is FragileSeeker fragSeeker) {
            IEnumerator Coro() {
                yield return 0.3f;
                fragSeeker.Crushing = true;
                self.SquishCallback(new CollisionData());
                fragSeeker.Crushing = false;
            }
            self.Add(new Coroutine(Coro()));
        }
        orig(self, entity);
    }
}
#nullable restore
