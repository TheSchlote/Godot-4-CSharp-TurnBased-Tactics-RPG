using Godot;

public partial class UnitAction : Node
{
    public enum ActionType
    {
        Conditional,
        ChanceBased
    }
    public enum ElementType
    {
        Nature,
        Water,
        Fire,
        Earth,
        Energy,
        Dark,
        Light
    }

    [Export]
    public ActionType Type { get; set; }

    [Export(PropertyHint.Range, "0.0,10.0")]
    public float ChanceWeight { get; set; } = 0.0f;

    [Export]
    public int ActionRange { get; set; } = 1; // Default to 1 for melee attacks


    public float AccumulatedWeight = 0.0f;

    public Unit CurrentUnit { get; set; }
    public Unit TargetUnit { get; set; }

    public override void _Ready()
    {
        AccumulatedWeight = 0.0f;
    }

    public virtual bool IsPerformable()
    {
        // This will be overridden in specific action classes
        return false;
    }

    public virtual void PerformAction()
    {
        // This will be overridden in specific action classes
    }
}
