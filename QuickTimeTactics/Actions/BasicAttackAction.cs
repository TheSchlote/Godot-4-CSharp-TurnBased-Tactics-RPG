using Godot;
using System;

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
        DamageEffect damageEffect = new DamageEffect
        {
            Amount = Damage
        };
        Godot.Collections.Array<Node> targetArray = new Godot.Collections.Array<Node> { TargetUnit };

        tween.TweenProperty(CurrentUnit, "global_position", end, MoveDuration);
        tween.TweenCallback(Callable.From(() => damageEffect.Execute(targetArray)));
        CurrentUnit.PlayAnimation("Run");
        tween.TweenInterval(AttackDelay);
        CurrentUnit.PlayAnimation("Attack1");
        tween.TweenCallback(Callable.From(() => damageEffect.Execute(targetArray)));
        tween.TweenInterval(AttackDelay);
        tween.TweenProperty(CurrentUnit, "global_position", start, MoveDuration);
        tween.Finished += () =>
        {
            Events.Instance.EmitSignal(nameof(Events.UnitActionCompleted), CurrentUnit);
        };
        tween.TweenCallback(Callable.From(() => {
            CurrentUnit.PlayAnimation("Idle");
        }));
    }
}
