using Godot;
using System;

[GlobalClass]
public partial class UnitStats : Stats
{
    [Export]
    public PackedScene AI { get; set; }
}
