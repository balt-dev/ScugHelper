using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using Celeste;
using MonoMod.Cil;
using System;
using System.Reflection;
using MonoMod.RuntimeDetour;
using System.Collections;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[Tracked]
[CustomEntity("ScugHelper/ControllableFallingBlock")]
public class ControllableFallingBlock : FallingBlock
{
    public int FallDirection = 1;

    public ControllableFallingBlock(EntityData data, Vector2 offset) : base(data, offset)
    {
        Remove(Get<Coroutine>());
        Add(new Coroutine(MySequence()));
    }

    public override void Update()
    {
        base.Update();
        if (HasPlayerClimbing())
            FallDirection = Input.MoveY.Value == 0 ? FallDirection : Input.MoveY.Value;
    }
    
    public IEnumerator MySequence()
    {
        while (!Triggered && (!PlayerFallCheck()))
            yield return null;

        while (FallDelay > 0f)
            FallDelay -= Engine.DeltaTime;
            yield return null;

        HasStartedFalling = true;
        while (true)
        {
            ShakeSfx();
            StartShaking();
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);

            yield return 0.2f;
            float timer = 0.4f;
            if (finalBoss)
            {
                timer = 0.2f;
            }

            while (timer > 0f && PlayerWaitCheck())
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }

            StopShaking();
            for (int i = 2; i < Width; i += 4)
            {
                if (Scene.CollideCheck<Solid>(TopLeft + new Vector2(i, -2f)))
                    SceneAs<Level>().Particles.Emit(P_FallDustA, 2, new Vector2(X + i, Y), Vector2.One * 4f, MathF.PI / 2f);

                SceneAs<Level>().Particles.Emit(P_FallDustB, 2, new Vector2(X + i, Y), Vector2.One * 4f);
            }

            float speed = 0f;
            while (true)
            {
                float maxSpeed = (finalBoss ? 130f : 160f) * FallDirection;
                Level level = SceneAs<Level>();
                speed = Calc.Approach(speed, maxSpeed, 500f * Engine.DeltaTime);
                if (MoveVCollideSolids(speed * Engine.DeltaTime, thruDashBlocks: true))
                    break;

                if (Top > level.Bounds.Bottom + 16 || (Top > level.Bounds.Bottom - 1 && CollideCheck<Solid>(Position + new Vector2(0f, FallDirection))))
                {
                    FallingBlock fallingBlock = this;
                    FallingBlock fallingBlock2 = this;
                    bool collidable = false;
                    fallingBlock2.Visible = false;
                    fallingBlock.Collidable = collidable;
                    yield return 0.2f;

                    RemoveSelf();
                    DestroyStaticMovers();
                    yield break;
                }

                yield return null;
            }

            ImpactSfx();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            SceneAs<Level>().DirectionalShake(Vector2.UnitY, finalBoss ? 0.2f : 0.3f);

            StartShaking();
            LandParticles();
            yield return 0.2f;
            StopShaking();
            if (CollideCheck<SolidTiles>(Position + new Vector2(0f, FallDirection)))
                Safe = true;

            while (CollideCheck<Platform>(Position + new Vector2(0f, FallDirection)))
                yield return 0.1f;
        }
    }
}
#nullable restore
