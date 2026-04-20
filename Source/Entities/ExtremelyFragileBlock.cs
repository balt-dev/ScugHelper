using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
namespace Celeste.Mod.ScugHelper.Entities;

#nullable enable
[Tracked]
[CustomEntity("ScugHelper/ExtremelyFragileBlock")]
public class ExtremelyFragileBlock : AbstractBreakableBlock
{
    
    internal class ExtremelyFragileBlockColliderList : ColliderList
    {
        private readonly ExtremelyFragileBlock block;
        public ExtremelyFragileBlockColliderList(ExtremelyFragileBlock block) {
            colliders = [block.Collider];
            this.block = block;
        }
        public override bool Collide(Circle o) => base.Collide(o) && block.DoBreak();
        public override bool Collide(Hitbox o) => base.Collide(o) && block.DoBreak();
        public override bool Collide(ColliderList o) => base.Collide(o) && block.DoBreak();
        public override bool Collide(Grid o) => base.Collide(o) && block.DoBreak();
        public override bool Collide(Rectangle o) => base.Collide(o) && block.DoBreak();
        public override bool Collide(Vector2 o) => base.Collide(o) && block.DoBreak();
        public override bool Collide(Vector2 a, Vector2 b) => base.Collide(a, b) && block.DoBreak();
     }

    public ExtremelyFragileBlock(EntityData data, Vector2 offset, EntityID id) : base(data, offset, id)
    {
        Collider = new ExtremelyFragileBlockColliderList(this);
    }
    private bool DoBreak() {
        Break(Vector2.Zero);
        return false;
    }
}
#nullable restore
