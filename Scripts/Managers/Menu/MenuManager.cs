using Godot;
using Godot.Collections;

public partial class MenuManager : Node
{
    public static MenuManager instance { get; private set; }

    [Export] public Array<BaseMenu> menuList = new();

    private readonly Dictionary<GameStats, BaseMenu> menus = new();
    private BaseMenu currentMenu;

    public override void _Ready()
    {
        if (instance == null)
            instance = this;
        else
        {
            QueueFree();
            return;
        }

        foreach (BaseMenu lMenu in menuList)
        {
            if (lMenu == null)
                continue;

            menus[lMenu.menuState] = lMenu;
        }
    }

    public void OpenMenu(GameStats pState)
    {
        if (currentMenu != null && GameManager.instance.currentState == pState)
            return;

        if (menus.ContainsKey(pState))
        {
            currentMenu = menus[pState];
            GameManager.instance.ChangeState(pState);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (instance == this)
            instance = null;

        base.Dispose(disposing);
    }
}
