using Godot;
using System;

public partial class Events : Node
{
    public static Events Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

    [Signal]
    public delegate void UnitActionCompletedEventHandler();
    [Signal]
    public delegate void UnitTurnEndedEventHandler();
    [Signal]
    public delegate void AllUnitsTurnCompletedEventHandler();
    [Signal]
    public delegate void UnitDefeatedEventHandler(Stats unitStats);
}
