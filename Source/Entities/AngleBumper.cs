using Microsoft.Xna.Framework;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using System;
using Celeste.Mod.Helpers;
using Celeste.Mod;
using Celeste.Mod.ScugHelper;
using System.Collections;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/AngleBumper")]
public class AngleBumper : Bumper
{
    private float Angle;
    public AngleBumper(EntityData data, Vector2 offset) : base(data, offset)
    {
        Angle = data.Float("Angle", 0f) * 180 / MathF.PI;
        sprite.RemoveSelf();
        spriteEvil.RemoveSelf();
        hitWiggler.RemoveSelf();
        Add(sprite = GFX.SpriteBank.Create(data.String("ColdSprite", "bumper")));
        Add(spriteEvil = GFX.SpriteBank.Create(data.String("HotSprite", "bumper_evil")));
        Add(hitWiggler = Wiggler.Create(1.2f, 2f, (v) => { spriteEvil.Position = hitDir * hitWiggler.Value * 8f; }));
        Components.RemoveAll<PlayerCollider>();
        Add(new PlayerCollider(OnPlayer));
    }

    public override void Update() {
        base.Update();
        Position = anchor;
    }
    
    private void OnPlayer(Player player)
    {
        if (fireMode)
        {
            if (!SaveData.Instance.Assists.Invincible)
            {
                Vector2 vector = (player.Center - Center).SafeNormalize();
                hitDir = -vector;
                hitWiggler.Start();
                Audio.Play("event:/game/09_core/hotpinball_activate", Position);
                respawnTimer = 0.6f;
                player.Die(vector);
                SceneAs<Level>().Particles.Emit(P_FireHit, 12, Center + vector * 12f, Vector2.One * 3f, vector.Angle());
            }
        }
        else if (respawnTimer <= 0f)
        {
            if ((Scene as Level).Session.Area.ID == 9)
                Audio.Play("event:/game/09_core/pinballbumper_hit", Position);
            else
                Audio.Play("event:/game/06_reflection/pinballbumper_hit", Position);

            respawnTimer = 0.6f;
            Vector2 delta = Center - player.Center;
            float length = delta.Length();
            Vector2 unitAngle = new(MathF.Cos(Angle), MathF.Sin(Angle));
            Vector2 explodePos = player.Center - length * unitAngle;
            Vector2 vector2 = player.ExplodeLaunch(explodePos, snapUp: false, sidesOnly: false);
            sprite.Play("hit", restart: true);
            spriteEvil.Play("hit", restart: true);
            light.Visible = false;
            bloom.Visible = false;
            SceneAs<Level>().DirectionalShake(vector2, 0.15f);
            SceneAs<Level>().Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);
            SceneAs<Level>().Particles.Emit(P_Launch, 12, Center + vector2 * 12f, Vector2.One * 3f, vector2.Angle());
        }
    }
}