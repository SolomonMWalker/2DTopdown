using Godot;

namespace DTopdown.Assets.Components;

[GlobalClass]
public partial class LockOnComponent : Node2D
{
    [Export] public Sprite2D LockOnReticleSprite { get; set; }

    public void LockOn() => LockOnReticleSprite.Visible = true;
    public void DisableLockOn() => LockOnReticleSprite.Visible = false;
}