using Godot;
using Godot.Collections;

public partial class TitlecardSub : Control
{
    public int playerId;

    [ExportGroup("References")]
    [Export] private Control titleContainer;
    [Export] private Control endGameContainer;
    [Export] private AnimatedTextureRect animatedBackground;
    [Export] private Array<CharacterSkinDisplay> characters = new Array<CharacterSkinDisplay>();

    [ExportGroup("Settings")]
    [Export] private string shaderToggleName = "is_loser";
    [Export] private Vector2 inPosition = new Vector2(0, 0);
    [Export] private Vector2 outPosition = new Vector2(1080, 0);
    [Export] private Vector2 winOutPosition = new Vector2(-1080, 0);
    [Export] private float animDuration = 0.5f;
    [Export] private Vector2 enterStartScale = new Vector2(0.85f, 0.85f);
    [Export] private Vector2 exitEndScale = new Vector2(0.85f, 0.85f);

    private Tween moveTween;
    private Tween bumpTween;

    public override void _Ready()
    {
        Position = outPosition;

        if (animatedBackground != null && animatedBackground.Material != null)
        {
            animatedBackground.Material = (Material)animatedBackground.Material.Duplicate();

            if (animatedBackground.Material is ShaderMaterial lMat)
            {
                lMat.SetShaderParameter(shaderToggleName, Variant.From(true));
                lMat.SetShaderParameter(shaderToggleName, Variant.From(false));
            }
        }

        Bump();
    }

    public bool IsAnimating()
    {
        return moveTween?.IsRunning() ?? false;
    }

    public void HandleInput(int pPlayerId, bool pIsPressed)
    {
        if (!pIsPressed || pPlayerId != playerId || IsAnimating())
            return;

        if (GameManager.instance.currentState == GameStats.TitleScreen)
            MenuManager.instance.OpenMenu(GameStats.Waiting);
    }

    public void UpdateState(GameStats pState)
    {
        switch (pState)
        {
            case GameStats.TitleScreen:
                bool lIsGameOver = TitlecardManager.lastWinnerId != -1;

                titleContainer.Modulate = new Color(1, 1, 1, lIsGameOver ? 0 : 1);
                endGameContainer.Modulate = new Color(1, 1, 1, lIsGameOver ? 1 : 0);

                titleContainer.MouseFilter = lIsGameOver ? MouseFilterEnum.Ignore : MouseFilterEnum.Stop;
                endGameContainer.MouseFilter = lIsGameOver ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;

                if (lIsGameOver)
                {
                    bool lIsMenuWinner = playerId == TitlecardManager.lastWinnerId;
                    UpdateBackground(!lIsMenuWinner);
                    Position = winOutPosition;

                    foreach (CharacterSkinDisplay lCharacter in characters)
                        lCharacter.PlayStatusAnimation(TitlecardManager.lastWinnerId);
                }
                else
                {
                    UpdateBackground(false);
                    Bump();
                }

                AnimateTo(inPosition, Tween.EaseType.Out);
                break;

            case GameStats.Waiting:
                AnimateTo(outPosition, Tween.EaseType.In);
                break;
        }
    }

    public void PlayExitAnimation()
    {
        moveTween?.Kill();
        moveTween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Idle);

        moveTween.SetParallel(true);

        moveTween.TweenProperty(this, TweenPaths.scale, exitEndScale, animDuration)
                 .SetEase(Tween.EaseType.In)
                 .SetTrans(Tween.TransitionType.Back);
    }

    private void UpdateBackground(bool pIsLoser)
    {
        if (animatedBackground == null)
            return;

        animatedBackground.Play();

        if (animatedBackground.Material is ShaderMaterial lMat)
            lMat.SetShaderParameter(shaderToggleName, Variant.From(pIsLoser));
    }

    private void AnimateTo(Vector2 pTarget, Tween.EaseType pEase)
    {
        moveTween?.Kill();
        moveTween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Idle);

        moveTween.SetParallel(true);

        moveTween.TweenProperty(this, TweenPaths.position, pTarget, animDuration)
                 .SetEase(pEase)
                 .SetTrans(Tween.TransitionType.Quint);
    }

    private void Bump()
    {
        bumpTween?.Kill();

        Scale = enterStartScale;

        bumpTween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Idle);
        bumpTween.TweenProperty(this, TweenPaths.scale, Vector2.One, animDuration)
                 .SetEase(Tween.EaseType.Out)
                 .SetTrans(Tween.TransitionType.Back);

        bumpTween.Play();
    }
}