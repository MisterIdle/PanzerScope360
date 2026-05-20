using Godot;

[GlobalClass]
public partial class CharacterSkin : Resource
{
    [Export] public Texture2D headTexture;
    [Export] public Color color = Colors.White;
}