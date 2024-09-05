using Godot;

public partial class BattleUI : Sprite3D
{
    [Signal]
    public delegate void NoHpLeftEventHandler();

    private ProgressBar progressBar;

    public override void _Ready()
    {
        progressBar = GetNode<ProgressBar>("%HealthBar");
    }

    public void UpdateStats(Stats stats)
    {
        progressBar.MaxValue = stats.MaxHealth;
        progressBar.Value = stats.MaxHealth;
        progressBar.Value = stats.CurrentHealth;
        if (stats.CurrentHealth <= 0)
        {
            EmitSignal(nameof(NoHpLeft));
        }
    }
}
