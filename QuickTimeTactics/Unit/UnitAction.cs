using Godot;
using System;

public partial class UnitAction : Node
{
    public enum ActionType
    {
        Conditional,
        ChanceBased
    }

    [Export]
    public ActionType Type { get; set; }

    [Export(PropertyHint.Range, "0.0,10.0")]
    public float ChanceWeight { get; set; } = 0.0f;

    public float AccumulatedWeight = 0.0f;

    public Unit CurrentUnit { get; set; }
    public Unit TargetUnit { get; set; }

    public override void _Ready()
    {
        AccumulatedWeight = 0.0f;
    }

    public virtual bool IsPerformable()
    {
        return false;
    }

    public virtual void PerformAction()
    {
    }
}
