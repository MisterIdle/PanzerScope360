using Godot;

public partial class AnimatedTextureRect : TextureRect
{
    [Export] private SpriteFrames sprites;
    [Export] private string currentAnimation = "default";
    [Export] private float speedScale = 1f;
    [Export] private bool autoPlay = false;

    private Tween animTween;

    public override void _Ready()
    {
        if (autoPlay)
            Play(currentAnimation);
    }

    public void Play(string pAnimName = "")
    {
        if (!string.IsNullOrEmpty(pAnimName))
            currentAnimation = pAnimName;

        if (sprites == null || !sprites.HasAnimation(currentAnimation))
            return;

        animTween?.Kill();

        int lFrameCount = sprites.GetFrameCount(currentAnimation);
        float lFps = (float)sprites.GetAnimationSpeed(currentAnimation) * speedScale;
        float lDuration = lFrameCount / (lFps > 0 ? lFps : 1f);
        bool lIsLooping = sprites.GetAnimationLoop(currentAnimation);

        animTween = CreateTween();

        if (lIsLooping)
            animTween.SetLoops();

        animTween.TweenMethod(Callable.From<int>(SetFrame), 0, lFrameCount - 1, lDuration)
                 .SetTrans(Tween.TransitionType.Linear);
    }

    private void SetFrame(int pFrameIndex)
    {
        if (sprites != null)
            Texture = sprites.GetFrameTexture(currentAnimation, pFrameIndex);
    }

    public void Stop() => animTween?.Kill();
}