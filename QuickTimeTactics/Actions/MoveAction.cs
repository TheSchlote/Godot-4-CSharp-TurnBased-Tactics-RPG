using Godot;
using System.Collections.Generic;

public partial class MoveAction : UnitAction
{
    public List<Vector3> Path { get; private set; }

    public override bool IsPerformable()
    {
        // Check if the unit can move and has a valid path
        if (CurrentUnit == null || TargetUnit == null)
        {
            return false;
        }

        Pathfinding pathfinding = GetPathfinding();
        if (pathfinding == null)
        {
            return false;
        }

        Path = pathfinding.CalculatePath(CurrentUnit.GridPosition, TargetUnit.GridPosition);
        return Path.Count > 0;
    }

    public override void PerformAction()
    {
        if (Path != null && Path.Count > 0)
        {
            CurrentUnit.FollowPath(Path);
            CurrentUnit.Connect("MoveCompleted", new Callable(this, nameof(OnMoveCompleted)));
        }
    }

    private void OnMoveCompleted()
    {
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
