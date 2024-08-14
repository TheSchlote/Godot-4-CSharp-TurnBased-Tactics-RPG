using Godot;
using System;

[GlobalClass]
public partial class Stats : Resource
{
    [Signal]
    public delegate void StatsChangedEventHandler();
    [Signal]
    public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
    [Signal]
    public delegate void UnitDefeatedEventHandler();
    [Export]
    public int MaxHealth { get; set; }

    private int currentHealth;
    [Export]
    public int CurrentHealth
    {
        get => currentHealth;
        set
        {
            currentHealth = Math.Clamp(value, 0, MaxHealth);
            EmitSignal(nameof(StatsChanged));
            EmitSignal(nameof(HealthChanged), currentHealth, MaxHealth);
        }
    }

    [Export]
    public int Attack { get; set; }

    [Export]
    public int Movement { get; set; }

    [Export]
    public bool IsPlayerControlledUnit { get; set; }

    [Export]
    public string UnitName { get; set; }

    private int speed;
    [Export]
    public int Speed
    {
        get => speed;
        set
        {
            speed = value;
            RecalculateActionPoints();
        }
    }

    [Export]
    public float MoveSpeed { get; set; } = 1f;

    public int ActionPoints { get; set; } = 0;

    public void RecalculateActionPoints()
    {
        // Adjust Action Points according to new speed
    }

    public void AccumulateActionPoints()
    {
        ActionPoints += Speed;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            GD.Print($"{UnitName} has been defeated.");
            Events.Instance.EmitSignal("UnitDefeated", this);
        }
    }

    public Stats CreateInstance()
    {
        Stats instance = (Stats)Duplicate();
        instance.CurrentHealth = MaxHealth;
        instance.ActionPoints = 0;
        return instance;
    }
}
