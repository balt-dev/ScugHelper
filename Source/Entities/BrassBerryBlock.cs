using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste.Mod;
using Celeste;
using Celeste.Mod.ScugHelper;
namespace Celeste.Mod.ScugHelper.Entities;

[Tracked]
[CustomEntity("ScugHelper/BrassBerryBlock")]
public class BrassBerryBlock : Solid
{
	private MTexture[,] nineSlice;

	private Image berry;

	private float startY;

	private float yLerp;

	private float sinkTimer;

	private float renderLerp;

	public BrassBerryBlock(Vector2 position, float width, float height)
		: base(position, width, height, safe: false)
	{
		startY = Y;
		berry = new Image(GFX.Game["objects/ScugHelper/brassBerry/idle00"]);
		berry.CenterOrigin();
		berry.Position = new Vector2(width / 2f, height / 2f);
        MTexture mTexture = GFX.Game["objects/ScugHelper/brassBerry/brassBerryBlock00"];
		nineSlice = new MTexture[3, 3];
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				nineSlice[i, j] = mTexture.GetSubtexture(new Rectangle(i * 8, j * 8, 8, 8));
			}
		}
        Depth = -10000;
		Add(new LightOcclude());
		Add(new MirrorSurface());
		SurfaceSoundIndex = 32;
	}

	public BrassBerryBlock(EntityData data, Vector2 offset)
		: this(data.Position + offset, data.Width, data.Height)
	{
	}

	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		DisableStaticMovers();
		Visible = false;
		Collidable = false;
		renderLerp = 1f;
		bool flag = ScugHelperModule.Session.BrassBerryFollowing != null;
		if (!flag)
		{
			DestroyStaticMovers();
			RemoveSelf();
		}
	}

	private void DrawBlock(Vector2 offset, Color color)
	{
		float num = Collider.Width / 8f - 1f;
		float num2 = Collider.Height / 8f - 1f;
		for (int i = 0; i <= num; i++)
			for (int j = 0; j <= num2; j++)
			{
				int num3 = (i < num) ? Math.Min(i, 1) : 2;
				int num4 = (j < num2) ? Math.Min(j, 1) : 2;
				nineSlice[num3, num4].Draw(Position + offset + Shake + new Vector2(i * 8, j * 8), Vector2.Zero, color);
			}
	}

	public override void Render()
	{
		Level level = Scene as Level;
		Vector2 vector = new(0f, (level.Bounds.Bottom - startY + 32f) * Ease.CubeIn(renderLerp));
		Vector2 position = Position;
		Position += vector;
		DrawBlock(new Vector2(-1f, 0f), Color.Black);
		DrawBlock(new Vector2(1f, 0f), Color.Black);
		DrawBlock(new Vector2(0f, -1f), Color.Black);
		DrawBlock(new Vector2(0f, 1f), Color.Black);
		DrawBlock(Vector2.Zero, Color.White);
		berry.Color = Color.White;
		berry.RenderPosition = Center;
		berry.Render();
		Position = position;
	}

	public override void Update()
	{
		base.Update();
		if (!Visible)
		{
			Player entity = Scene.Tracker.GetEntity<Player>();
			if (entity != null && entity.X > X - 80f)
			{
				Visible = true;
				Collidable = true;
				renderLerp = 1f;
			}
		}
		if (Visible)
			renderLerp = Calc.Approach(renderLerp, 0f, Engine.DeltaTime * 3f);
		if (HasPlayerRider())
			sinkTimer = 0.1f;
		else if (sinkTimer > 0f)
			sinkTimer -= Engine.DeltaTime;
		yLerp = Calc.Approach(yLerp, (sinkTimer > 0f) ? 1f : 0f, 1f * Engine.DeltaTime);
		float y = MathHelper.Lerp(startY, startY + 12f, Ease.SineInOut(yLerp));
        MoveToY(y);

        if (renderLerp == 0f)
			EnableStaticMovers();
		else
			DisableStaticMovers();
	}
}
