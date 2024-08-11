using Godot;

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
        GD.Print($"Enemy stats set: {stats}");

        if (!stats.IsConnected(nameof(Stats.StatsChanged), new Callable(this, nameof(UpdateStats))))
        {
            stats.Connect(nameof(Stats.StatsChanged), new Callable(this, nameof(UpdateStats)));
            stats.Connect(nameof(Stats.StatsChanged), new Callable(this, nameof(UpdateAction)));
        }

        UpdateEnemy();
    }
    private async void UpdateEnemy()
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
    private void SetupAI()
    {
        if (unitActionPicker != null)
        {
            unitActionPicker.QueueFree();
        }
        UnitActionPicker newActionPicker = (UnitActionPicker)stats.AI.Instantiate();
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
                QueueFree();
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
}
