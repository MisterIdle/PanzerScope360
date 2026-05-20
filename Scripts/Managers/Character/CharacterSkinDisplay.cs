using Godot;

public partial class CharacterSkinDisplay : Node
{
    public enum FaceState
    {
        Normal,
        Happy,
        Sad
    }

    [ExportCategory("Visual")]
    [Export] public CanvasItem bodyNode;
    [Export] public TextureRect headNode;
    [Export] public TextureRect faceNode;
    [Export] public AnimationPlayer animPlayer;

    [ExportCategory("Face")]
    [Export] public Texture2D normalFace;
    [Export] public Texture2D happyFace;
    [Export] public Texture2D sadFace;

    [ExportCategory("Juice")]
    [Export] public Vector2 reactionScale = new Vector2(1.35f, 1.35f);
    [Export] public float reactionDuration = 0.15f;
    [Export] public Tween.TransitionType reactionTrans = Tween.TransitionType.Back;
    [Export] public Tween.EaseType reactionEase = Tween.EaseType.Out;

    [ExportCategory("ID")]
    [Export] public int targetPlayerId = -1;
    public int id = -1;

    private Tween reactionTween;

    public override void _Ready()
    {
        CallDeferred(MethodName.ApplySkin);
    }

    public void ApplySkin()
    {
        if (SkinManager.instance == null)
            return;

        id = (targetPlayerId == -1) ? SkinManager.instance.GetNextAutoId() : targetPlayerId;
        CharacterSkin lSkin = SkinManager.instance.GetCharacterSkin(id);

        if (lSkin == null)
            return;

        bodyNode.Modulate = lSkin.color;
        headNode.Texture = lSkin.headTexture;

        SetFace(FaceState.Normal);
    }

    public void PlayStatusAnimation(int pWinnerId)
    {
        bool lIsWinner = id == pWinnerId;

        SetFace(lIsWinner ? FaceState.Happy : FaceState.Sad);

        if (animPlayer == null)
            return;


        string lReset = "Reset";
        string lAnimName = $"Variant{id}{(lIsWinner ? "Win" : "Lose")}";

        animPlayer.Play(lReset);

        if (animPlayer.HasAnimation(lAnimName))
        {
            if (animPlayer.CurrentAnimation == lReset)
                animPlayer.Queue(lAnimName);
        }
    }

    public void SetFace(FaceState pState)
    {
        if (faceNode == null)
            return;

        switch (pState)
        {
            case FaceState.Happy:
                faceNode.Texture = happyFace;
                break;
            case FaceState.Sad:
                faceNode.Texture = sadFace;
                break;
            case FaceState.Normal:
            default:
                faceNode.Texture = normalFace;
                break;
        }
    }

    public void PlayReaction()
    {
        if (headNode == null)
            return;

        headNode.PivotOffset = headNode.Size / 2.0f;

        reactionTween?.Kill();
        reactionTween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Idle);

        reactionTween.SetParallel(false);

        reactionTween.TweenProperty(headNode, TweenPaths.scale, reactionScale, reactionDuration);
        reactionTween.TweenProperty(headNode, TweenPaths.scale, Vector2.One, reactionDuration);

        reactionTween.SetTrans(reactionTrans).SetEase(reactionEase);
    }
}