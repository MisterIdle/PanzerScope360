using Godot;
using System;

public partial class TextManager : Control
{
    public static TextManager instance { get; private set; }
    [Export] private PackedScene textScene;
    [Export] private int poolSize = 5;

    private Control topHalf;
    private Control bottomHalf;

    private ObjectPool textPool;

    public override void _Ready()
    {
        if (instance == null)
            instance = this;
        else
        {
            QueueFree();
            return;
        }

        string lSeparation = "separation";

        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;

        VBoxContainer lSplit = new VBoxContainer();
        lSplit.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        lSplit.AddThemeConstantOverride(lSeparation, 0);

        AddChild(lSplit);

        topHalf = new Control { SizeFlagsVertical = SizeFlags.ExpandFill };
        bottomHalf = new Control { SizeFlagsVertical = SizeFlags.ExpandFill };

        lSplit.AddChild(topHalf);
        lSplit.AddChild(bottomHalf);

        textPool = new ObjectPool(textScene, poolSize, this, InitializeText);
    }

    private void InitializeText(Node pNode)
    {
        if (pNode is FloatingText pText)
            pText.Visible = false;
    }

    public void TextDisplay(string pText, Color pColor, int pSize, float pEnterDur = 0.3f, float pDriftDur = 1f, float pExitDur = 0.3f)
    {
        Spawn(pText, pColor, pSize, topHalf, true, pEnterDur, pDriftDur, pExitDur);
        Spawn(pText, pColor, pSize, bottomHalf, false, pEnterDur, pDriftDur, pExitDur);
    }

    public void TextDisplaySingle(string pText, Color pColor, int pSize, bool pIsTop, float pEnterDur = 0.3f, float pDriftDur = 1f, float pExitDur = 0.3f)
    {
        Control lParent = pIsTop ? topHalf : bottomHalf;
        Spawn(pText, pColor, pSize, lParent, pIsTop, pEnterDur, pDriftDur, pExitDur);
    }

    private void Spawn(string pText, Color pColor, int pSize, Control pParent, bool pIsTop, float pEnterDur, float pDriftDur, float pExitDur)
    {
        FloatingText lFloatingText = (FloatingText)textPool.Get();

        lFloatingText.GetParent()?.RemoveChild(lFloatingText);
        pParent.AddChild(lFloatingText);

        lFloatingText.Visible = true;
        lFloatingText.Setup(pText, pColor, pSize, pIsTop, pEnterDur, pDriftDur, pExitDur);
    }

    public void ReturnToPool(FloatingText txt)
    {
        txt.Visible = false;
        textPool.Release(txt);
    }

    protected override void Dispose(bool pDisposing)
    {
        if (instance == this)
            instance = null;

        base.Dispose(pDisposing);
    }
}
