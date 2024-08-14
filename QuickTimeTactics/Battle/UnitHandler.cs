using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class UnitHandler : Node3D
{
    private List<Unit> _units = new List<Unit>();
    private int _currentUnitIndex = 0;

    public override void _Ready()
    {
        Events.Instance.Connect("UnitActionCompleted", new Callable(this, nameof(OnUnitActionCompleted)));
        Events.Instance.Connect("UnitDefeated", new Callable(this, nameof(OnUnitDefeated))); // Connect to global UnitDefeated signal
        PopulateUnits();
        SortUnitsBySpeed();
    }

    private void PopulateUnits()
    {
        _units = GetChildren().OfType<Unit>().ToList();
    }

    private void SortUnitsBySpeed()
    {
        _units = _units.OrderByDescending(unit => unit.Stats.Speed).ToList();
    }

    public void ResetUnitActions()
    {
        foreach (Unit unit in _units)
        {
            unit.CurrentAction = null;
            unit.UpdateAction();
            if (unit.Stats.IsPlayerControlledUnit)
            {
                unit.CurrentAction.TargetUnit = _units.FirstOrDefault(x => x.Stats.IsPlayerControlledUnit == false);
            }
            else
            {
                unit.CurrentAction.TargetUnit = _units.FirstOrDefault(x => x.Stats.IsPlayerControlledUnit == true);
            }
        }
    }

    public void StartBattle()
    {
        if (_units.Count == 0)
        {
            return;
        }

        _currentUnitIndex = 0;
        StartNextUnitTurn();
    }

    private void StartNextUnitTurn()
    {
        if (_currentUnitIndex >= _units.Count)
        {
            _currentUnitIndex = 0; // Reset to the first unit after all units have had their turn.
        }
        if (_units[_currentUnitIndex].Stats.IsPlayerControlledUnit)
        {
            _units[_currentUnitIndex].CurrentAction.TargetUnit = _units.FirstOrDefault(x => x.Stats.IsPlayerControlledUnit == false);
        }
        else
        {
            _units[_currentUnitIndex].CurrentAction.TargetUnit = _units.FirstOrDefault(x => x.Stats.IsPlayerControlledUnit == true);
        }
        if(_units[_currentUnitIndex].CurrentAction.TargetUnit == null)
        {
            //no more targets, battle over
            return;
        }
        _units[_currentUnitIndex].DoTurn();
    }

    private void OnUnitActionCompleted(Unit unit)
    {
        _currentUnitIndex++;

        if (_currentUnitIndex < _units.Count)
        {
            StartNextUnitTurn();
        }
        else
        {
            Events.Instance.EmitSignal("AllUnitsTurnCompleted");
            StartBattle(); // Restart the turn sequence after all units have taken their turn.
        }
    }
    private void OnUnitDefeated(Stats unitStats)
    {
        Unit defeatedUnit = _units.FirstOrDefault(unit => unit.Stats == unitStats);
        if (defeatedUnit != null)
        {
            _units.Remove(defeatedUnit); // Remove the unit from the list
            defeatedUnit.QueueFree();
            GD.Print($"{defeatedUnit.Stats.UnitName} has been removed from the UnitHandler.");
        }
    }
}
