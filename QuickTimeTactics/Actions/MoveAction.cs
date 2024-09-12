using Godot;
using System.Collections.Generic;

public partial class MoveAction : UnitAction
{
    public List<Vector3> Path { get; private set; }

    public override bool IsPerformable()
    {
        if (CurrentUnit == null || TargetUnit == null)
        {
            TargetUnit = CurrentUnit.FindTarget();
            if (TargetUnit == null)
            {
                return false;
            }
        }

        Pathfinding pathfinding = GetPathfinding();
        if (pathfinding == null)
        {
            return false;
        }

        Path = pathfinding.CalculatePath(CurrentUnit.GridPosition, TargetUnit.GridPosition);

        // We will perform this action if we are not already in range
        float distance = CurrentUnit.GridPosition.DistanceTo(TargetUnit.GridPosition);
        bool inRange = distance <= ActionRange;

        GD.Print($"MoveAction: In range: {inRange}, Distance: {distance}, Action Range: {ActionRange}");

        return Path.Count > 0 && !inRange;
    }

    public override void PerformAction()
    {
        if (Path != null && Path.Count > 0)
        {
            GD.Print($"{CurrentUnit.Name} is performing MoveAction");
            CurrentUnit.FollowPath(Path);
            CurrentUnit.PlayAnimation("Run");
            CurrentUnit.Connect("MoveCompleted", new Callable(this, nameof(OnMoveCompleted)));
        }
    }

    public void OnMoveCompleted()
    {
        CurrentUnit.PlayAnimation("Idle");
        CurrentUnit.Disconnect("MoveCompleted", new Callable(this, nameof(OnMoveCompleted)));
        Events.Instance.EmitSignal(nameof(Events.UnitActionCompleted), CurrentUnit);
    }

    private Pathfinding GetPathfinding()
    {
        var nodes = GetTree().GetNodesInGroup("Map");
        foreach (Node node in nodes)
        {
            if (node is Pathfinding pathfinding)
            {
                return pathfinding;
            }
        }
        GD.PrintErr("No Pathfinding node found in the 'Map' group.");
        return null;
    }
}
