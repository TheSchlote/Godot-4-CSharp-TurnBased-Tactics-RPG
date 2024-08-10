using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UnitHandler : Node3D
{
    private List<Unit> units = new List<Unit>();

    public override void _Ready()
    {
        Events.Instance.Connect("UnitActionCompleted", new Callable(this, nameof(OnUnitActionCompleted)));
        PopulateUnits();
        SortUnitsBySpeed();
    }

    private void PopulateUnits()
    {
        units = GetChildren().OfType<Unit>().ToList();
    }

    private void SortUnitsBySpeed()
    {
        units = units.OrderByDescending(unit => unit.Stats.Speed).ToList();
    }

    public void ResetUnitActions()
    {
        foreach (Unit unit in units)
        {
            unit.CurrentAction = null;
            unit.UpdateAction();
            if (unit.Stats.IsPlayerControlledUnit)
            {
                unit.CurrentAction.TargetUnit = units.FirstOrDefault(x => x.Stats.IsPlayerControlledUnit == false);
            }
            else
            {
                unit.CurrentAction.TargetUnit = units.FirstOrDefault(x => x.Stats.IsPlayerControlledUnit == true);
            }
        }
    }

    public void StartTurn()
    {
        if (units.Count == 0)
        {
            return;
        }

        Unit firstUnit = units[0];
        firstUnit.DoTurn();
    }

    private void OnUnitActionCompleted(Unit unit)
    {
        int unitIndex = units.IndexOf(unit);

        if (unitIndex == units.Count - 1)
        {
            Events.Instance.EmitSignal("UnitTurnEnded");
            return;
        }

        Unit nextUnit = units[unitIndex + 1];
        nextUnit.DoTurn();
    }
}
