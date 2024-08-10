using Godot;
using System;

public partial class DamageEffect : Effect
{
    public int Amount { get; set; } = 0;
    public override void Execute(Godot.Collections.Array<Node> targets)
    {

        foreach (var target in targets)
        {
            if (target == null)
            {
                continue;
            }

            if (target is Unit unit)
            {
                unit.TakeDamage(Amount);
            }
        }
    }
}
