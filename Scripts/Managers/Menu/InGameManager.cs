using Godot;
using Godot.Collections;

public partial class InGameManager : BaseMenu
{
    public static InGameManager instance { get; private set; }

    [Export] public PackedScene tankScene;
    [Export] public VBoxContainer container;

    [ExportGroup("Health Bars")]
    [Export] public Array<ProgressBar> healthBars;
    [Export] public Color highHealthColor = new Color(0.2f, 0.8f, 0.2f);
    [Export] public Color mediumHealthColor = new Color(0.9f, 0.8f, 0.1f);
    [Export] public Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f);
    [Export] public float healthMediumThreshold = 0.5f;

    [ExportGroup("Timer")]
    [Export] public Label timerLabel;
    [Export] public TextureRect timerBackground;
    [Export] public float gameDuration = 45f;
    [Export] public float timerWarningTime = 5f;
    [Export] public Vector2 timerBounceScale = new Vector2(1.3f, 1.3f);
    [Export] public float timerBounceDuration = 0.15f;

    [ExportGroup("Spawn")]
    [Export] public float spawnMargin = 50f;
    [Export] public float introDuration = 1.5f;
    [Export] public float topRotation = 90f;
    [Export] public float bottomRotation = 270f;

    [ExportGroup("Ready")]
    [Export] public string readyText = "ID_READY";
    [Export] public Color readyColor = new Color(0.33f, 0.82f, 0.93f);
    [Export] public float readyEnterDur = 0.3f;
    [Export] public float readyDriftDur = 1.0f;
    [Export] public float readyExitDur = 0.3f;
    [Export] public int readySize = 180;

    [ExportGroup("Go")]
    [Export] public string goText = "ID_GO";
    [Export] public Color goColor = new Color(0.99f, 0.92f, 0.29f);
    [Export] public float goEnterDur = 0.2f;
    [Export] public float goDriftDur = 0.3f;
    [Export] public float goExitDur = 0.1f;
    [Export] public int goSize = 230;

    [ExportGroup("End Game")]
    [Export] public string winText = "ID_WIN";
    [Export] public string loseText = "ID_LOSE";
    [Export] public string drawText = "ID_DRAW";
    [Export] public Color winColor = new Color(0.627f, 0.871f, 0.294f);
    [Export] public Color loseColor = new Color(0.996f, 0.396f, 0.0f);
    [Export] public Color drawColor = new Color(0.96f, 0.55f, 0.74f);
    [Export] public float delayBeforeWinState = 3.0f;
    [Export] public float endGameEnterDur = 0.3f;
    [Export] public float endGameExitDur = 0.3f;

    [ExportGroup("End Game Particles")]
    [Export] public GpuParticles2D winParticles;
    [Export] public Array<AnimatedTextureRect> winMiniSpin;

    [Export] public string timerBase = "0:00";

    private readonly Dictionary<int, Tank> spawnedTanks = new();
    private bool isGameOver = false;
    private Tween introTween;

    private Timer gameTimer;
    private Tween timerBounceTween;
    private bool isTimerWarning = false;
    private int lastPlayedSecond = -1;

    public override void _Ready()
    {
        base._Ready();

        if (instance == null)
            instance = this;
        else
        {
            QueueFree();
            return;
        }

        SkinManager.instance.ResetAutoId();

        gameTimer = new Timer();
        gameTimer.OneShot = true;
        gameTimer.Timeout += OnTimeUp;

        AddChild(gameTimer);

        for (int i = 0; i < GameManager.instance.playerNum; i++)
        {
            if (tankScene == null) 
                continue;

            Tank lTank = tankScene.Instantiate<Tank>();
            lTank.playerId = i;

            AddChild(lTank);
            spawnedTanks[i] = lTank;

            if (i < healthBars.Count)
                UpdateHealthBar(i, lTank.maxHealth, lTank.maxHealth);
        }

        if (GameManager.instance.currentState == GameStats.Waiting)
            CallDeferred(MethodName.SetupWaitingPositions);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);

        if (!gameTimer.IsStopped() && !isGameOver)
            UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        float lTimeLeft = (float)gameTimer.TimeLeft;

        int lTotalSeconds = Mathf.CeilToInt(lTimeLeft);

        int lMin = lTotalSeconds / 60;
        int lSec = lTotalSeconds % 60;

        timerLabel.Text = $"{lMin:0}:{lSec:00}";

        if (lTotalSeconds != lastPlayedSecond)
        {
            lastPlayedSecond = lTotalSeconds;

            if (lTotalSeconds <= timerWarningTime && lTotalSeconds > 0)
                AudioManager.instance.PlaySound(AudioManager.SoundType.Time, false);
        }

        if (lTimeLeft <= timerWarningTime && lTimeLeft > 0 && !isTimerWarning)
        {
            isTimerWarning = true;

            if (timerBackground != null)
                timerBackground.PivotOffset = timerBackground.Size / 2f;

            timerBounceTween?.Kill();
            timerBounceTween = CreateTween().SetProcessMode(Tween.TweenProcessMode.Idle).SetLoops();

            timerBounceTween.TweenProperty(timerBackground, TweenPaths.scale, timerBounceScale, timerBounceDuration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);

            timerBounceTween.TweenProperty(timerBackground, TweenPaths.scale, Vector2.One, timerBounceDuration)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.In);
        }
    }

    public override void OnStateChanged(GameStats pState)
    {
        base.OnStateChanged(pState);

        if (pState == GameStats.Waiting)
        {
            CallDeferred(MethodName.SetupWaitingPositions);
            AudioManager.instance.PlaySound(AudioManager.SoundType.Attention);
        }
        else if (pState == GameStats.InGame)
        {
            isGameOver = false;
            StartIntroAnimation();
        }
        else introTween?.Kill();
    }

    private void SetupWaitingPositions()
    {
        Rect2 lBounds = container.GetGlobalRect();
        Vector2 lCenter = lBounds.GetCenter();

        foreach (var (lPlayerId, lTank) in spawnedTanks)
        {
            bool lIsTop = lPlayerId % 2 == 0;
            float lY = lIsTop ? lBounds.Position.Y + spawnMargin : lBounds.End.Y - spawnMargin;

            lTank.GlobalPosition = new Vector2(lCenter.X, lY);
            lTank.RotationDegrees = lIsTop ? topRotation : bottomRotation;

            lTank.InitStopped();
        }
    }

    private void StartIntroAnimation()
    {
        introTween?.Kill();

        foreach (Tank lTank in spawnedTanks.Values)
            lTank.InitIntro();

        TextManager.instance.TextDisplay(readyText, readyColor, readySize, readyEnterDur, readyDriftDur, readyExitDur);

        introTween = CreateTween();
        introTween.TweenInterval(introDuration);
        introTween.TweenCallback(Callable.From(EndIntroAndStartPlay));
    }

    private void EndIntroAndStartPlay()
    {
        foreach (Tank lTank in spawnedTanks.Values)
            lTank.InitNormal();

        TextManager.instance.TextDisplay(goText, goColor, goSize, goEnterDur, goDriftDur, goExitDur);

        isTimerWarning = false;
        lastPlayedSecond = -1;

        if (timerBackground != null)
            timerBackground.Scale = Vector2.One;

        gameTimer.WaitTime = gameDuration;
        gameTimer.Start();

        AudioManager.instance.PlayMusic(AudioManager.MusicType.InGame);

        UpdateTimerUI();
    }

    public override void OnPlayerTouched(int pPlayerId, bool isPressed)
    {
        if (GameManager.instance.currentState != GameStats.InGame || isGameOver)
            return;

        if (spawnedTanks.ContainsKey(pPlayerId))
        {
            Tank lTank = spawnedTanks[pPlayerId];
            lTank.SetRotating(isPressed);
        }
    }

    public void UpdateHealthBar(int pPlayerId, int pCurrentHealth, int pMaxHealth)
    {
        ProgressBar lHealthBar = healthBars[pPlayerId];
        lHealthBar.MaxValue = pMaxHealth;
        lHealthBar.Value = pCurrentHealth;

        Color lColor;
        string lFill = "fill";

        switch (pCurrentHealth)
        {
            case 5:
            case 4:
                lColor = highHealthColor;
                break;
            case 3:
            case 2:
                lColor = mediumHealthColor;
                break;
            default:
                lColor = lowHealthColor;
                break;
        }

        StyleBoxFlat lStyle = new StyleBoxFlat();
        lStyle.BgColor = lColor;
        lHealthBar.AddThemeStyleboxOverride(lFill, lStyle);
    }

    public void OnTankDied(int pDeadPlayerId)
    {
        if (isGameOver)
            return;

        int lWinnerId = pDeadPlayerId == 0 ? 1 : 0;

        TriggerEndGame(lWinnerId);
    }

    private void OnTimeUp()
    {
        timerLabel.Text = timerBase;
        AudioManager.instance.PlaySound(AudioManager.SoundType.TimeUp, false);

        int lMaxHealth = -1;
        int lWinnerId = -1;
        bool lIsDraw = false;

        foreach (var (lPlayerId, lTank) in spawnedTanks)
        {
            int lCurrentHealth = lTank?.currentHealth ?? 0;

            if (lCurrentHealth > lMaxHealth)
            {
                lMaxHealth = lCurrentHealth;
                lWinnerId = lPlayerId;
                lIsDraw = false;
            }
            else if (lCurrentHealth == lMaxHealth)
                lIsDraw = true;
        }

        if (lIsDraw || lWinnerId == -1)
            TriggerDraw();
        else
            TriggerEndGame(lWinnerId);
    }

    private void TriggerEndGame(int pWinnerId)
    {
        PrepareGameOver();

        TitlecardManager.lastWinnerId = pWinnerId;

        StopAllTanks(CharacterSkinDisplay.FaceState.Normal);

        UpdateWinUI(pWinnerId);
        ShowWinLoseText(pWinnerId);
        SpawnWinParticles(pWinnerId);

        ReturnToMenu();
    }

    private void TriggerDraw()
    {
        PrepareGameOver();

        TitlecardManager.lastWinnerId = -2;

        StopAllTanks(CharacterSkinDisplay.FaceState.Sad);

        TextManager.instance.TextDisplaySingle(drawText, drawColor, readySize, true, endGameEnterDur, delayBeforeWinState, endGameExitDur);
        TextManager.instance.TextDisplaySingle(drawText, drawColor, readySize, false, endGameEnterDur, delayBeforeWinState, endGameExitDur);

        ReturnToMenu();
    }

    private void PrepareGameOver()
    {
        isGameOver = true;
        gameTimer?.Stop();
        timerBounceTween?.Kill();

        timerBackground.Scale = Vector2.One;

        AudioManager.instance.PlayMusic(AudioManager.MusicType.Victory);

        foreach (ProgressBar lHealthBar in healthBars)
            lHealthBar.Visible = false;
    }

    private void StopAllTanks(CharacterSkinDisplay.FaceState pFace)
    {
        foreach (Tank lTank in spawnedTanks.Values)
        {
            lTank.InitStopped();
            lTank.ChangeEmotion(pFace);
        }
    }

    private void UpdateWinUI(int pWinnerId)
    {
        for (int i = 0; i < winMiniSpin.Count; i++)
        {
            if (winMiniSpin[i] != null)
                winMiniSpin[i].Visible = (i == pWinnerId);
        }
    }

    private void SpawnWinParticles(int pWinnerId)
    {
        Rect2 lBounds = container.GetGlobalRect();
        Vector2 lCenter = lBounds.GetCenter();

        bool lIsTop = pWinnerId % 2 == 0;
        float lTargetY = lIsTop ? lBounds.Position.Y + spawnMargin : lBounds.End.Y - spawnMargin;

        winParticles.GlobalPosition = new Vector2(lCenter.X, lTargetY);
        winParticles.Restart();
        winParticles.Emitting = true;
    }

    private void ShowWinLoseText(int pWinnerId)
    {
        int lId = (pWinnerId == 0) ? 1 : 0;

        TextManager.instance.TextDisplaySingle(loseText, loseColor, readySize, lId % 2 == 0, endGameEnterDur, delayBeforeWinState, endGameExitDur);
        TextManager.instance.TextDisplaySingle(winText, winColor, readySize, pWinnerId % 2 == 0, endGameEnterDur, delayBeforeWinState, endGameExitDur);
    }

    private void ReturnToMenu()
    {
        Tween lTimer = CreateTween();
        lTimer.TweenInterval(endGameEnterDur + delayBeforeWinState + endGameExitDur);
        lTimer.TweenCallback(Callable.From(() => MenuManager.instance.OpenMenu(GameStats.TitleScreen)));
    }

    public void ResetBoard()
    {
        isGameOver = false;
        gameTimer.Stop();

        int lMin = Mathf.FloorToInt(gameDuration / 60);
        int lSec = Mathf.FloorToInt(gameDuration % 60);
        timerLabel.Text = $"{lMin:0}:{lSec:00}";

        timerBackground.Scale = Vector2.One;

        timerBounceTween?.Kill();

        foreach (Node lChild in GetChildren())
        {
            if (lChild is Obus lObus)
                lObus.ForceDeactivate();

            else if (lChild is GpuParticles2D lParticule && lParticule != winParticles)
                lParticule.QueueFree();
        }

        winParticles.Emitting = false;

        foreach (AnimatedTextureRect lMini in winMiniSpin)
            lMini.Visible = false;

        foreach (ProgressBar lHealthBar in healthBars)
            lHealthBar.Visible = true;

        foreach (var (lId, lTank) in spawnedTanks)
        {
            lTank.FullReset();
            UpdateHealthBar(lId, lTank.maxHealth, lTank.maxHealth);
        }
    }

    protected override void Dispose(bool pDisposing)
    {
        if (instance == this)
            instance = null;

        base.Dispose(pDisposing);
    }
}