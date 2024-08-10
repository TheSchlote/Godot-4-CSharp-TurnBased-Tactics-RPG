using Godot;
using System;
using System.Collections.Generic;

public partial class UnitActionPicker : Node
{
    [Export]
    public Unit CurrentUnit
    {
        get => currentUnit;
        set => SetCurrentUnit(value);
    }
    private Unit currentUnit;

    [Export]
    public Unit TargetUnit
    {
        get => targetUnit;
        set => SetTargetUnit(value);
    }
    private Unit targetUnit;

    private float totalWeight = 0.0f;
    public override void _Ready()
    {
        //Target = GetTree().GetFirstNodeInGroup("Player") as Node2D;
        SetupChances();
    }
    public Unit AssignTargetUnit(List<Unit> potentialTargets)
    {
        Unit firstTarget = null;

        foreach (Unit target in potentialTargets)
        {
            firstTarget = target;
            return firstTarget;
        }

        return firstTarget;
    }

    public UnitAction GetAction()
    {
        UnitAction action = GetFirstConditionalAction();
        if (action != null)
        {
            return action;
        }

        return GetChanceBasedAction();
    }
    public UnitAction GetFirstConditionalAction()
    {
        foreach (Node node in GetChildren())
        {
            if (node is UnitAction action && action.Type == UnitAction.ActionType.Conditional)
            {
                if (action.IsPerformable())
                {
                    return action;
                }
            }
        }

        return null;
    }
    private UnitAction GetChanceBasedAction()
    {
        float roll = (float)GD.RandRange(0.0, totalWeight);

        foreach (Node node in GetChildren())
        {
            if (node is UnitAction action && action.Type == UnitAction.ActionType.ChanceBased)
            {
                if (action.AccumulatedWeight > roll)
                {
                    return action;
                }
            }
        }

        return null;
    }
    private void SetupChances()
    {
        totalWeight = 0.0f; // Reset total weight before recalculating

        foreach (Node node in GetChildren())
        {
            if (node is UnitAction action && action.Type == UnitAction.ActionType.ChanceBased)
            {
                totalWeight += action.ChanceWeight;
                action.AccumulatedWeight = totalWeight;
            }
        }
    }
    private void SetCurrentUnit(Unit value)
    {
        currentUnit = value;

        foreach (Node node in GetChildren())
        {
            if (node is UnitAction action)
            {
                action.CurrentUnit = currentUnit;
            }
        }
    }
    private void SetTargetUnit(Unit value)
    {
        targetUnit = value;

        foreach (Node node in GetChildren())
        {
            if (node is UnitAction action)
            {
                action.TargetUnit = targetUnit;
            }
        }
    }
}
