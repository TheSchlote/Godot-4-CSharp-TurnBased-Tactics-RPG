using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Unit : Area3D
{
    private UnitStats stats;
    [Export]
    public UnitStats Stats
    {
        get { return stats; }
        set { SetUnitStats(value); }
    }

    private BattleUI battleUI;
    private UnitActionPicker unitActionPicker;
    private UnitAction currentAction;
    public UnitAction CurrentAction
    {
        get { return currentAction; }
        set { SetCurrentAction(value); }
    }
    public AnimationPlayer RobotAnimationPlayer;

    [Export]
    public Vector3I GridPosition { get; set; }
    [Export]
    public float MoveSpeed = 1f;

    private Queue<Vector3> pathQueue = new Queue<Vector3>();
    public override void _Ready()
    {
        RobotAnimationPlayer = GetNode<Node3D>("3DGodotRobot").GetNode<AnimationPlayer>("AnimationPlayer");
        battleUI = GetNode<BattleUI>("BattleUI");
        PlayAnimation("Idle");
    }
    private void SetCurrentAction(UnitAction value)
    {
        currentAction = value;
    }
    private void SetUnitStats(UnitStats value)
    {
        stats = (UnitStats)value.CreateInstance();

        if (!stats.IsConnected(nameof(Stats.StatsChanged), new Callable(this, nameof(UpdateStats))))
        {
            stats.Connect(nameof(Stats.StatsChanged), new Callable(this, nameof(UpdateStats)));
            stats.Connect(nameof(Stats.StatsChanged), new Callable(this, nameof(UpdateAction)));
        }

        UpdateUnit();
    }
    public async void UpdateUnit()
    {
        if (!(stats is Stats))
        {
            return;
        }

        if (!IsInsideTree())
        {
            await ToSignal(this, "ready");
        }

        SetupAI();
        UpdateStats();
    }
    private void UpdateStats()
    {
        battleUI.UpdateStats(stats);
    }
    public void UpdateAction()
    {
        if (unitActionPicker == null)
            return;

        if (CurrentAction == null)
        {
            CurrentAction = unitActionPicker.GetAction();
            return;
        }

        UnitAction newConditionalAction = unitActionPicker.GetFirstConditionalAction();
        if (newConditionalAction != null && CurrentAction != newConditionalAction)
        {
            CurrentAction = newConditionalAction;
        }
    }
    public void SetupAI()
    {
        unitActionPicker?.QueueFree();
        UnitActionPicker newActionPicker = (UnitActionPicker)stats.AI.Instantiate();
        GD.Print($"{stats.UnitName} has been assigned AI");
        AddChild(newActionPicker);
        unitActionPicker = newActionPicker;
        unitActionPicker.CurrentUnit = this;
    }

    public void DoTurn()
    {
        if (CurrentAction == null)
        {
            return;
        }

        CurrentAction.PerformAction();
    }

    private bool ShouldMoveAndAttack()
    {
        // Logic to determine if the unit should move and attack
        return true; // Placeholder, replace with your condition
    }

    private bool ShouldJustMove()
    {
        // Logic to determine if the unit should just move
        return false; // Placeholder, replace with your condition
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

    private List<Vector3> CalculatePathToTarget()
    {
        var pathfinding = GetPathfinding();
        if (pathfinding == null)
        {
            return new List<Vector3>(); // Return an empty path if no pathfinding node is found
        }

        return pathfinding.CalculatePath(this.GridPosition, FindTarget().GridPosition);
    }

    private Unit FindTarget()
    {
        // Get all units that are not on the same team
        List<Unit> potentialTargets = GetTree()
            .GetNodesInGroup("Units")
            .OfType<Unit>()
            .Where(u => u.Stats.IsPlayerControlledUnit != this.Stats.IsPlayerControlledUnit)
            .ToList();

        Unit closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Unit target in potentialTargets)
        {
            if (target == null || target == this)
            {
                continue; // Skip if the target is null or this unit itself
            }

            float distance = this.GlobalPosition.DistanceTo(target.GlobalPosition);

            if (distance < closestDistance)
            {
                closestTarget = target;
                closestDistance = distance;
            }
        }

        return closestTarget;
    }


    public void TakeDamage(int damage)
    {
        if (stats.CurrentHealth <= 0)
        {
            return;
        }

        var tween = CreateTween();
        tween.TweenCallback(Callable.From(() =>
        {
            stats.TakeDamage(damage);
            PlayAnimation("Hurt");
        }));
        tween.TweenInterval((float)RobotAnimationPlayer.GetAnimation("Hurt").Length);
        tween.TweenCallback(Callable.From(() =>
        {
            PlayAnimation("Idle");

            if (stats.CurrentHealth <= 0)
            {
                Events.Instance.EmitSignal("UnitDefeated", stats);
            }
        }));
    }

    public void PlayAnimation(string animationName)
    {
        if (RobotAnimationPlayer.HasAnimation(animationName))
        {
            RobotAnimationPlayer.CurrentAnimation = animationName;
        }
    }
    public void FollowPath(List<Vector3> path)
    {
        pathQueue.Clear();

        for (int i = 0; i <= Mathf.Min(path.Count, Stats.Movement); i++)
        {
            pathQueue.Enqueue(path[i]);
        }
        if (pathQueue.Count > 0)
        {
            SetProcess(true);
        }
    }
    public override void _Process(double delta)
    {
        if (pathQueue.Count > 0)
        {
            Vector3 nextPos = pathQueue.Peek();
            Vector3 direction = (nextPos - Position).Normalized();
            float step = (float)(MoveSpeed * delta);

            if ((Position - nextPos).Length() <= step)
            {
                nextPos = Position;
                Position = nextPos;
                pathQueue.Dequeue();

                if (pathQueue.Count == 0)
                {
                    SetProcess(false);  // Stop processing when the path is complete
                    Events.Instance.EmitSignal("MoveCompleted", this); // Emit global MoveCompleted signal with this unit

                }
            }
            else
            {
                Position += direction * step;
            }
        }
        GridPosition = (Vector3I)Position;
    }
}
