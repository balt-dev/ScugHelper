using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;
using System.Collections.Generic;
using System.Linq;
namespace Celeste.Mod.ScugHelper.Entities.Actions;

#nullable enable


[Tracked(false)]
[CustomEntity("ScugHelper/ActionCassetteBlock")]
public class ActionCassetteBlock : Solid
{
    public class BoxSide(ActionCassetteBlock block, Color color) : Entity
    {
        public ActionCassetteBlock block = block;
        public Color color = color;

        public override void Render() => Draw.Rect(block.X, block.Y + block.Height - 8f, block.Width, 8 + block.BlockHeight, color);
    }

    readonly List<Image> PressedTextures = [];
    readonly List<Image> SolidTextures = [];
    readonly List<Image> AllTextures = [];
    readonly LightOcclude Occluder;
    readonly Color Color;
    readonly string[] Groups;
    readonly int GroupHash;
    readonly string? Flag;
    readonly bool FlagState;
    readonly string PressedTexture;
    readonly string SolidTexture;
    readonly bool StartedSolid;

    List<ActionCassetteBlock>? BlockGroup = null;
    bool WantsCollide;
    bool IsLeader;
    bool Silent;
    bool? IsGroupLeader;
    Vector2 GroupOrigin;
    BoxSide? side;
    Wiggler? Wiggler;
    Vector2 WigglerScaler;
    int BlockHeight = 0;

    public ActionCassetteBlock(EntityData data, Vector2 offset)
        : base(data.Position + offset, data.Width, data.Height, safe: false)
    {
        SurfaceSoundIndex = data.Int("SurfaceSoundIndex", 35);
        PressedTexture = data.String("PressedTexture", "objects/cassetteblock/pressed00");
        SolidTexture = data.String("SolidTexture", "objects/cassetteblock/solid");
        WantsCollide = Collidable = StartedSolid = data.Bool("StartSolid");
        if (Collidable)
        {
            BlockHeight = 2;
            Position.Y -= 2;
        }
        Groups = IAction.GetGroups(data);
        Flag = data.String("Flag");
        Silent = data.Bool("Silent");
        FlagState = data.Bool("FlagState");
        GroupHash = (data.String("Groups", ""), Flag, FlagState, StartedSolid).GetHashCode();
        Color = data.HexColor("Color", Calc.HexToColor("49aaf0"));
        Add(Occluder = new LightOcclude());
        Add(new ActionListener(Groups, ActionToggle));
    }

    static readonly Color DisableColor = Calc.HexToColor("667da5");

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (Flag is string flag && (scene as Level)!.Session.GetFlag(flag) == FlagState && !StartedSolid)
        {
            WantsCollide = Collidable = true;
            BlockHeight = 2;
            Position.Y -= 2;
        }
        Color disabledColor = new(DisableColor.R / 255f * (Color.R / 255f), DisableColor.G / 255f * (Color.G / 255f), DisableColor.B / 255f * (Color.B / 255f), 1f);
        scene.Add(side = new BoxSide(this, disabledColor));
        foreach (StaticMover staticMover in staticMovers)
        {
            if (staticMover.Entity is Spikes spikes)
            {
                spikes.EnabledColor = Color;
                spikes.DisabledColor = disabledColor;
                spikes.VisibleWhenDisabled = true;
                spikes.SetSpikeColor(Color);
            }

            if (staticMover.Entity is Spring spring)
            {
                spring.DisabledColor = disabledColor;
                spring.VisibleWhenDisabled = true;
            }
        }

        if (IsGroupLeader == null)
        {
            foreach (ActionCassetteBlock acb in Scene.Tracker.GetEntities<ActionCassetteBlock>())
            {
                if (acb != this && acb.GroupHash == GroupHash) {
                    acb.IsGroupLeader = false;
                    if (!acb.Silent)
                        Silent = false;
                }
            }
            IsGroupLeader = true;
        }

        Logger.Log(nameof(ScugHelperModule), $"Group leader: {IsGroupLeader}");

        if (BlockGroup == null)
        {
            IsLeader = true;
            BlockGroup = [this];
            FindInGroup(this);
            float totalLeft = float.MaxValue;
            float totalRight = float.MinValue;
            float totalTop = float.MaxValue;
            float totalBottom = float.MinValue;
            foreach (ActionCassetteBlock i in BlockGroup)
            {
                if (i.Left < totalLeft) totalLeft = i.Left;
                if (i.Right > totalRight) totalRight = i.Right;
                if (i.Bottom > totalBottom) totalBottom = i.Bottom;
                if (i.Top < totalTop) totalTop = i.Top;
            }

            GroupOrigin = new Vector2((int)(totalLeft + (totalRight - totalLeft) / 2f), (int)totalBottom);
            WigglerScaler = new Vector2(Calc.ClampedMap(totalRight - totalLeft, 32f, 96f, 1f, 0.2f), Calc.ClampedMap(totalBottom - totalTop, 32f, 96f, 1f, 0.2f));
            Add(Wiggler = Wiggler.Create(0.3f, 3f));
            foreach (ActionCassetteBlock j in BlockGroup)
            {
                j.Wiggler = Wiggler;
                j.WigglerScaler = WigglerScaler;
                j.GroupOrigin = GroupOrigin;
            }
        }

        foreach (StaticMover staticMover2 in staticMovers)
            if (staticMover2.Entity is Spikes spikes2)
                spikes2.SetOrigins(GroupOrigin);

        for (float x = Left; x < Right; x += 8f)
        {
            for (float y = Top; y < Bottom; y += 8f)
            {
                bool left = CheckForSame(x - 8f, y);
                bool right = CheckForSame(x + 8f, y);
                bool top = CheckForSame(x, y - 8f);
                bool bottom = CheckForSame(x, y + 8f);
                if (left && right && top && bottom)
                {
                    if (!CheckForSame(x + 8f, y - 8f)) SetImage(x, y, 3, 0);
                    else if (!CheckForSame(x - 8f, y - 8f)) SetImage(x, y, 3, 1);
                    else if (!CheckForSame(x + 8f, y + 8f)) SetImage(x, y, 3, 2);
                    else if (!CheckForSame(x - 8f, y + 8f)) SetImage(x, y, 3, 3);
                    else SetImage(x, y, 1, 1);
                }
                else if (left && right && !top && bottom) SetImage(x, y, 1, 0);
                else if (left && right && top && !bottom) SetImage(x, y, 1, 2);
                else if (left && !right && top && bottom) SetImage(x, y, 2, 1);
                else if (!left && right && top && bottom) SetImage(x, y, 0, 1);
                else if (left && !right && !top && bottom) SetImage(x, y, 2, 0);
                else if (!left && right && !top && bottom) SetImage(x, y, 0, 0);
                else if (left && !right && top && !bottom) SetImage(x, y, 2, 2);
                else if (!left && right && top && !bottom) SetImage(x, y, 0, 2);
            }
        }

        if (!Collidable) DisableStaticMovers();

        UpdateVisualState();
    }

    public void FindInGroup(ActionCassetteBlock block)
    {
        foreach (ActionCassetteBlock acb in Scene.Tracker.GetEntities<ActionCassetteBlock>())
        {
            if (acb != this && acb != block && acb.GroupHash == GroupHash && (acb.CollideRect(new Rectangle((int)block.X - 1, (int)block.Y, (int)block.Width + 2, (int)block.Height)) || acb.CollideRect(new Rectangle((int)block.X, (int)block.Y - 1, (int)block.Width, (int)block.Height + 2))) && !BlockGroup.Contains(acb))
            {
                BlockGroup.Add(acb);
                FindInGroup(acb);
                acb.BlockGroup = BlockGroup;
            }
        }
    }

    public bool CheckForSame(float x, float y)
        => Scene.Tracker.GetEntities<ActionCassetteBlock>().Select(v => (v as ActionCassetteBlock)!).Any(acb => acb.GroupHash == GroupHash && acb.Collider.Collide(new Rectangle((int)x, (int)y, 8, 8)));

    public void SetImage(float x, float y, int tx, int ty)
    {
        PressedTextures.Add(CreateImage(x, y, tx, ty, GFX.Game[PressedTexture]));
        SolidTextures.Add(CreateImage(x, y, tx, ty, GFX.Game[SolidTexture]));
    }

    public Image CreateImage(float x, float y, int tx, int ty, MTexture tex)
    {
        Vector2 relPos = new(x - X, y - Y);
        Image image = new(tex.GetSubtexture(tx * 8, ty * 8, 8, 8));
        Vector2 groupPos = GroupOrigin - Position;
        image.Origin = groupPos - relPos;
        image.Position = groupPos;
        image.Color = Color;
        Add(image);
        AllTextures.Add(image);
        return image;
    }

    public override void Update()
    {
        base.Update();
        if (Flag is string flag)
        {
            var oldWantsCollide = WantsCollide;
            WantsCollide = SceneAs<Level>().Session.GetFlag(flag) == FlagState;
            if (WantsCollide != oldWantsCollide && IsGroupLeader == true && !Silent)
                Audio.Play(WantsCollide ? "event:/game/general/cassette_block_switch_2" : "event:/game/general/cassette_block_switch_1");
        }
        if (IsLeader)
        {
            if (!Collidable && WantsCollide)
            {
                bool anyBlocked = BlockGroup!.Any(i => i.BlockedCheck());

                if (!anyBlocked)
                {
                    foreach (ActionCassetteBlock j in BlockGroup!)
                    {
                        j.Collidable = j.WantsCollide = true;
                        j.EnableStaticMovers();
                        j.ShiftSize(-2);
                    }

                    Wiggler?.Start();
                }
            }
            else if (Collidable && !WantsCollide)
            {
                foreach (ActionCassetteBlock j in BlockGroup!)
                {
                    j.Collidable = j.WantsCollide = false;
                    j.DisableStaticMovers();
                    j.ShiftSize(2);
                }

                Wiggler?.Stop();
                Wiggler?.Value = 0f;
            }
        }

        UpdateVisualState();
    }

    public bool BlockedCheck()
    {
        TheoCrystal theoCrystal = CollideFirst<TheoCrystal>();
        if (theoCrystal != null && !TryActorWiggleUp(theoCrystal))
            return true;

        Player player = CollideFirst<Player>();
        if (player != null && !TryActorWiggleUp(player))
            return true;

        return false;
    }

    public void UpdateVisualState()
    {
        if (!Collidable) Depth = 8990;
        else
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            Depth = entity != null && entity.Top >= Bottom - 1f ? 10 : -10;
        }

        foreach (StaticMover staticMover in staticMovers)
            staticMover.Entity.Depth = Depth + 1;

        side?.Depth = Depth + 5;
        side?.Visible = BlockHeight > 0;
        Occluder.Visible = Collidable;
        foreach (Image solidTex in SolidTextures)
            solidTex.Visible = Collidable;

        foreach (Image pressedTex in PressedTextures)
            pressedTex.Visible = !Collidable;

        if (!IsLeader) return;
        if (Wiggler is null) return;
        Vector2 scale = new(1f + Wiggler.Value * 0.05f * WigglerScaler.X, 1f + Wiggler.Value * 0.15f * WigglerScaler.Y);
        foreach (ActionCassetteBlock block in BlockGroup!)
        {
            foreach (Image item4 in block.AllTextures)
                item4.Scale = scale;

            foreach (StaticMover mover in block.staticMovers)
            {
                if (mover.Entity is not Spikes spikes) continue;

                foreach (Component component in spikes.Components)
                    if (component is Image image)
                        image.Scale = scale;
            }
        }
    }

    public void ShiftSize(int amount)
    {
        MoveV(amount);
        BlockHeight -= amount;
    }

    public bool TryActorWiggleUp(Entity actor)
    {
        foreach (ActionCassetteBlock item in BlockGroup!)
            if (item != this && item.CollideCheck(actor, item.Position + Vector2.UnitY * 4f))
                return false;

        bool collidable = Collidable;
        Collidable = true;
        for (int i = 1; i <= 4; i++)
        {
            if (!actor.CollideCheck<Solid>(actor.Position - Vector2.UnitY * i))
            {
                actor.Position -= Vector2.UnitY * i;
                Collidable = collidable;
                return true;
            }
        }

        Collidable = collidable;
        return false;
    }

    void ActionToggle(Level _)
    {
        if (Flag is not null) return;
        WantsCollide = !WantsCollide;
        if (IsGroupLeader == true)
            Audio.Play(WantsCollide ? "event:/game/general/cassette_block_switch_2" : "event:/game/general/cassette_block_switch_1");
    }
}
#nullable restore
