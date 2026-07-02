using Monocle;

namespace Celeste.Mod.ScugHelper;

internal abstract class AbstractEntityColliderList : ColliderList {
    public AbstractEntityColliderList(Collider self) => colliders = [self];
    public AbstractEntityColliderList(Entity self) => colliders = [self.Collider is AbstractEntityColliderList l ? l.OriginalCollider : self.Collider];
    public Collider OriginalCollider => colliders[0];

    public override bool Collide(Circle o) => base.Collide(o) && CheckEntity(o);
    public override bool Collide(Hitbox o) => base.Collide(o) && CheckEntity(o);
    public override bool Collide(ColliderList o) => base.Collide(o) && CheckEntity(o);
    public override bool Collide(Grid o) => base.Collide(o) && CheckEntity(o);

    protected virtual bool CheckEntity(Collider o) => CheckEntity(o.Entity);
    protected virtual bool CheckEntity(Entity o) => true;
}