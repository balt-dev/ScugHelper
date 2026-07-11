using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System;
using System.Collections;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
namespace Celeste.Mod.ScugHelper.Entities;

[CustomEntity("ScugHelper/Frog")]
public class Frog : Entity {
    internal readonly Vector2 Start;
    internal readonly Image Image;
    internal Vector2 SpriteScale = Vector2.One;
    
   	public Frog(EntityData data, Vector2 offset) : base(data.Position + offset) {
		Depth = -9999;
		Start = Position;
        Image = new(GFX.Game["objects/ScugHelper/frog"]) { Color = data.HexColor("Color", Color.White) };
        Image.JustifyOrigin(new(0.5f, 1.0f));
        Add(new Coroutine(IdleRoutine()));
        //Add(RibbitSFX = new SoundSource());
        // RibbitSFX.Play("event:/scughelper/objects/frog/ribbit_loop");
    }
    public override void Update() {
        Image.Scale.X = Calc.Approach(Image.Scale.X, Math.Sign(Image.Scale.X), 4f * Engine.DeltaTime);
        Image.Scale.Y = Calc.Approach(Image.Scale.Y, 1f, 4f * Engine.DeltaTime);
        base.Update();
    }
    
   	private IEnumerator IdleRoutine() {
		while (true) {
			float delay = 0.25f + Calc.Random.NextFloat(1f);
			for (float p = 0f; p < delay; p += Engine.DeltaTime)
			    yield return null;
			Audio.Play("event:/game/general/birdbaby_hop", Position);
			Vector2 target = Start + new Vector2(-4f + Calc.Random.NextFloat(8f), 0f);
			SpriteScale.X = Math.Sign(target.X - Position.X);
			SimpleCurve bezier = new(Position, target, (Position + target) / 2f - Vector2.UnitY * 14f);
			for (float p = 0f; p < 1f; p += Engine.DeltaTime * 4f) {
				Position = bezier.GetPoint(p);
				yield return null;
			}
			SpriteScale.X = Math.Sign(SpriteScale.X) * 1.4f;
			SpriteScale.Y = 0.6f;
			Position = target;
		}
	}
}
