using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using System;
using System.Collections.Generic;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable

[Tracked]
[CustomEntity("ScugHelper/DeathEffectTrigger")]
public class DeathEffectTrigger(EntityData data, Vector2 offset) : Trigger(data, offset)
{
    private Vector2 EffectPosition = data.FirstNodeNullable(offset) ?? throw new NullReferenceException("Teleport gate does not have a teleport position node.");
    private readonly Color Color = data.HexColor("Color", Color.White);
    private readonly string SpritePath = data.String("SpritePath", "characters/player/hair00");
    private readonly int Amount = data.Int("Amount", 8);

    public override void OnEnter(Player player) {
        Level level = player.SceneAs<Level>();
        var ent = new Entity(EffectPosition) { new CustomDeathEffect(Color, SpritePath, Amount) };
        level.Add(ent);
    }
}

internal class CustomDeathEffect(Color color, string spritePath, int amount) : DeathEffect(color, Vector2.Zero)
{
    public override void Render() {
        if (Entity == null) return;
        Color flashColor = Settings.Instance.DisableFlashes || (Math.Floor(Percent * 10f) % 2 == 0) ? Color : Color.White;
        MTexture mTexture = GFX.Game[spritePath];
        float num = (Percent < 0.5f) ? (0.5f + Percent) : Ease.CubeOut(1f - (Percent - 0.5f) * 2f);
        for (int i = 0; i < amount; i++) {
            Vector2 offset = Calc.AngleToVector((i / (float) amount + Percent * 0.25f) * (MathF.PI * 2f), Ease.CubeOut(Percent) * 24f);
            mTexture.DrawCentered(Entity.Position + offset + new Vector2(-1f, 0f), Color.Black, new Vector2(num, num));
            mTexture.DrawCentered(Entity.Position + offset + new Vector2(1f, 0f), Color.Black, new Vector2(num, num));
            mTexture.DrawCentered(Entity.Position + offset + new Vector2(0f, -1f), Color.Black, new Vector2(num, num));
            mTexture.DrawCentered(Entity.Position + offset + new Vector2(0f, 1f), Color.Black, new Vector2(num, num));
        }

        for (int j = 0; j < amount; j++) {
            Vector2 offset = Calc.AngleToVector((j / (float) amount + Percent * 0.25f) * (MathF.PI * 2f), Ease.CubeOut(Percent) * 24f);
            mTexture.DrawCentered(Entity.Position + offset, flashColor, new Vector2(num, num));
        }
    }
}
