using Godot;

namespace FirstPerson.CustomTypes.StateMachine;

//runs processing for itself, has no children
[GlobalClass]
public partial class AtomicState : State
{
    public override string GetFullStateString()
    {
        return Name;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Enabled)
        {
            StateProcessing(delta);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (Enabled)
        {
            StatePhysicsProcessing(delta);
        }
    }
}