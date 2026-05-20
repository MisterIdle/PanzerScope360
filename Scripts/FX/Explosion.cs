using Godot;

public partial class Explosion : GpuParticles2D
{
    public static Explosion CreateExplosion(PackedScene pScene, Vector2 pPos, Node pParent)
    {
        Explosion lExplosion = pScene.Instantiate<Explosion>();
        lExplosion.Position = pPos;

        pParent.AddChild(lExplosion);
        return lExplosion;
    }

    public override void _Ready()
    {
        Emitting = true;
        Finished += QueueFree;
    }
}
