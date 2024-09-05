using Godot;

public partial class BasicAttackAction : UnitAction
{
    [Export]
    public int Damage { get; set; } = 4;

    public float MoveDuration { get; set; } = 1.0f;
    public float AttackDelay { get; set; } = 0.5f;

    public override void PerformAction()
    {
        if (CurrentUnit == null || TargetUnit == null)
        {
            return;
        }

        Tween tween = CreateTween().SetTrans(Tween.TransitionType.Quint);

        Vector3 start = CurrentUnit.GlobalPosition;
        Vector3 end = TargetUnit.GlobalPosition - (TargetUnit.GlobalPosition - start).Normalized();

        tween.TweenCallback(Callable.From(() => {
            CurrentUnit.PlayAnimation("Run");
        }));
        tween.TweenProperty(CurrentUnit, "global_position", end, MoveDuration);

        tween.TweenCallback(Callable.From(() => CurrentUnit.PlayAnimation("Attack1")));
        tween.TweenInterval((float)CurrentUnit.RobotAnimationPlayer.GetAnimation("Attack1").Length);
        tween.TweenCallback(Callable.From(() =>
        {
            DamageEffect damageEffect = new DamageEffect { Amount = Damage };
            Godot.Collections.Array<Node> targetArray = new Godot.Collections.Array<Node> { TargetUnit };
            damageEffect.Execute(targetArray);
        }));

        tween.TweenCallback(Callable.From(() => {
            CurrentUnit.PlayAnimation("Run");
        }));
        tween.TweenProperty(CurrentUnit, "global_position", start, MoveDuration);

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
