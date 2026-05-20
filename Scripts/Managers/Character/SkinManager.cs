using Godot;
using Godot.Collections;

public partial class SkinManager : Node
{
    public static SkinManager instance { get; private set; }

    [Export] public Array<CharacterSkin> characterSkins = new Array<CharacterSkin>();
    [Export] public Array<TankSkin> tankSkins = new Array<TankSkin>();

    private int lastAutoId = -1;

    public override void _Ready()
    {
        if (instance == null)
            instance = this;
        else
        {
            QueueFree();
            return;
        }
    }

    public int GetNextAutoId()
    {
        lastAutoId++;
        return lastAutoId;
    }

    public void ResetAutoId() => lastAutoId = -1;

    public CharacterSkin GetCharacterSkin(int pId)
    {
        if (characterSkins.Count == 0 || pId < 0)
            return null;

        return characterSkins[pId % characterSkins.Count];
    }

    public TankSkin GetTankSkin(int pId)
    {
        if (tankSkins.Count == 0 || pId < 0)
            return null;

        return tankSkins[pId % tankSkins.Count];
    }

    protected override void Dispose(bool pDisposing)
    {
        if (instance == this)
            instance = null;

        base.Dispose(pDisposing);
    }
}