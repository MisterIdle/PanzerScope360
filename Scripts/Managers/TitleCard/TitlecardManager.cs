using Godot;
using System.Collections.Generic;

public partial class TitlecardManager : BaseMenu
{
    public static int lastWinnerId = -1;

    [Export] private PackedScene playerSubMenuScene;
    [Export] private float reloadDelay = 0.5f;

    private GridContainer gridContainer;
    private readonly List<TitlecardSub> playerMenus = new();
    private bool isReloading = false;

    public override void _Ready()
    {
        gridContainer = new GridContainer
        {
            Columns = GameManager.instance.playerNum > 2 ? 2 : 1,
            MouseFilter = MouseFilterEnum.Ignore
        };

        gridContainer.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(gridContainer);

        for (int i = 0; i < GameManager.instance.playerNum; i++)
            CreatePlayerMenu(i);

        base._Ready();
    }

    private void CreatePlayerMenu(int pPlayerId)
    {
        Control lWrapper = new Control
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore
        };
        gridContainer.AddChild(lWrapper);

        TitlecardSub lMenu = playerSubMenuScene.Instantiate<TitlecardSub>();
        lMenu.playerId = pPlayerId;

        lWrapper.AddChild(lMenu);
        lMenu.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        playerMenus.Add(lMenu);

        lWrapper.Resized += () => ApplyRotation(lMenu, lWrapper, pPlayerId < GameManager.instance.playerNum / 2.0f);
    }

    private void ApplyRotation(Control pMenu, Control pWrapper, bool pRotate)
    {
        pMenu.PivotOffset = pWrapper.Size / 2.0f;

        if (pRotate) 
            pMenu.Rotation = Mathf.Pi;
    }

    public override void OnStateChanged(GameStats pState)
    {
        base.OnStateChanged(pState);

        if (pState == GameStats.TitleScreen)
        {
            bool lIsGameOver = lastWinnerId != -1;

            if (lIsGameOver)
                AudioManager.instance.PlayMusic(AudioManager.MusicType.End);
            else
                AudioManager.instance.PlayMusic(AudioManager.MusicType.Intro);
        }

        foreach (TitlecardSub lMenu in playerMenus)
            lMenu.UpdateState(pState);
    }

    public override void OnPlayerTouched(int pPlayerId, bool pIsPressed)
    {
        foreach (TitlecardSub lMenu in playerMenus)
        {
            if (lMenu.IsAnimating())
                return;
        }

        if (pIsPressed && GameManager.instance.currentState == GameStats.TitleScreen && lastWinnerId != -1)
        {
            if (isReloading)
                return;

            isReloading = true;

            InGameManager.instance.ResetBoard();

            foreach (TitlecardSub lMenu in playerMenus)
                lMenu.PlayExitAnimation();

            Tween lTween = CreateTween();

            lTween.TweenInterval(reloadDelay);
            lTween.TweenCallback(Callable.From(() => {
                lastWinnerId = -1;
                isReloading = false;
                OnStateChanged(GameStats.TitleScreen);
            }));

            return;
        }

        foreach (TitlecardSub lMenu in playerMenus)
            lMenu.HandleInput(pPlayerId, pIsPressed);
    }
}
