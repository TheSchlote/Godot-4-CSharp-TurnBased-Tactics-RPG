using Godot;
using System;

public partial class Battle : Node3D
{

	private UnitHandler _unitHandler;

	public override void _Ready()
	{
        _unitHandler = GetNode<UnitHandler>("UnitHandler");

		StartBattle();
    }

    private void StartBattle()
    {
        GetTree().Paused = false;
        _unitHandler.ResetUnitActions();
        _unitHandler.StartTurn();
    }
}
