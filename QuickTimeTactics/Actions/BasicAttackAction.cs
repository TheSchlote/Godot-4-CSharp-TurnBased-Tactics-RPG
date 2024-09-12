using Godot;

public partial class BasicAttackAction : UnitAction
{
    [Export]
    public int Damage { get; set; } = 4;
    public float AttackDelay { get; set; } = 0.5f;
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

        // Check if the target is within range
        float distance = CurrentUnit.GlobalPosition.DistanceTo(TargetUnit.GlobalPosition);
        bool inRange = distance <= ActionRange;

        GD.Print($"{CurrentUnit.Name} BasicAttackAction: In range: {inRange}, Distance: {distance}, Action Range: {ActionRange}");

        return inRange;
    }
    public override void PerformAction()
    {
        if (CurrentUnit == null || TargetUnit == null)
        {
            return;
        }

        GD.Print($"{CurrentUnit.Name} is performing BasicAttackAction");

        Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quint);

        tween.TweenCallback(Callable.From(() => CurrentUnit.PlayAnimation("Attack1")));
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
        }));

        tween.Finished += () =>
        {
            Events.Instance.EmitSignal(nameof(Events.UnitActionCompleted), CurrentUnit);
        };
    }
}
