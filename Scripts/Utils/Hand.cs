using Godot;
using System;

public partial class Hand : Control
{
    private AnimationPlayer anim;
    [Export] private NodePath animPath;

    private string touch = "Touch";

    public override void _Ready()
    {
        anim = GetNode<AnimationPlayer>(animPath);
        anim.Play(touch);
    }
}
