using Godot;
using System.Collections.Generic;

public partial class MoveAndAttackAction : UnitAction
{
    [Export]
    public int Damage { get; set; } = 4;

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

        // Performable if out of range but path exists
        float distance = CurrentUnit.GlobalPosition.DistanceTo(TargetUnit.GlobalPosition);
        bool inRange = distance <= ActionRange;

        GD.Print($"MoveAndAttackAction: In range: {inRange}, Distance: {distance}, Action Range: {ActionRange}");

        return !inRange && Path.Count > 0;
    }

    public override void PerformAction()
    {
        if (Path != null && Path.Count > 0)
        {
            GD.Print($"{CurrentUnit.Name} is performing MoveAndAttackAction");
            CurrentUnit.FollowPath(Path);
            CurrentUnit.PlayAnimation("Run");
            CurrentUnit.Connect("MoveCompleted", new Callable(this, nameof(OnMoveCompleted)));
        }
        else
        {
            AttackTarget();
        }
    }

    private void OnMoveCompleted()
    {
        CurrentUnit.PlayAnimation("Idle");
        CurrentUnit.Disconnect("MoveCompleted", new Callable(this, nameof(OnMoveCompleted)));
        AttackTarget();
    }

    private void AttackTarget()
    {
        GD.Print($"{CurrentUnit.Name} is attacking {TargetUnit.Name} after moving");

        Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quint);

        tween.TweenCallback(Callable.From(() =>
        {
            CurrentUnit.PlayAnimation("Attack1");
        }));
        tween.TweenInterval((float)CurrentUnit.RobotAnimationPlayer.GetAnimation("Attack1").Length);
        tween.TweenCallback(Callable.From(() =>
        {
            DamageEffect damageEffect = new DamageEffect { Amount = Damage };
            Godot.Collections.Array<Node> targetArray = new Godot.Collections.Array<Node> { TargetUnit };
            damageEffect.Execute(targetArray);
        }));

        tween.TweenCallback(Callable.From(() =>
        {
            CurrentUnit.PlayAnimation("Idle");
            Events.Instance.EmitSignal(nameof(Events.UnitActionCompleted), CurrentUnit);
        }));
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
