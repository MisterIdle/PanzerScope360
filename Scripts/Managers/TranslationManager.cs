using Godot;
using System;

public partial class TranslationManager : Node
{
    private enum Lang
    {
        None,
        fr,
        en,
    }

    public override void _Ready()
    {
        string lOSLang = OS.GetLocaleLanguage().ToLower();
        Lang lCurrentLang = Lang.en;

        if (Enum.IsDefined(typeof(Lang), lOSLang))
            lCurrentLang = (Lang)Enum.Parse(typeof(Lang), lOSLang);

        switch (lCurrentLang)
        {
            case Lang.fr:
                TranslationServer.SetLocale(nameof(Lang.fr));
                break;

            default:
                TranslationServer.SetLocale(nameof(Lang.en));
                break;
        }
    }
}