using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Battle : Node3D
{
    private UnitHandler _unitHandler;
    [Export]
    public PackedScene PlayerUnitScene;
    [Export]
    public PackedScene EnemyUnitScene;
    [Export]
    public UnitStats PlayerUnitStats;
    [Export]
    public UnitStats EnemyUnitStats;

    public override void _Ready()
    {
        _unitHandler = GetNode<UnitHandler>("UnitHandler");

        // Defer the battle start until after everything is loaded
        CallDeferred(nameof(SetupUnitsAndStartBattle));
    }

    // Defered setup function
    private async void SetupUnitsAndStartBattle()
    {
        // Example of spawning units with stats
        List<Task> unitTasks = new List<Task>
        {
            _unitHandler.SpawnUnitAsync(PlayerUnitScene, new Vector3I(0, 0, 2), true, PlayerUnitStats),
            _unitHandler.SpawnUnitAsync(EnemyUnitScene, new Vector3I(4, 0, 2), false, EnemyUnitStats)
        };

        // Wait for all units to signal they are ready
        await Task.WhenAll(unitTasks);

        // Start the battle after all units are ready
        StartBattle();
    }

    private void StartBattle()
    {
        GetTree().Paused = false;
        _unitHandler.ResetUnitActions();
        _unitHandler.StartBattle();
    }
}
