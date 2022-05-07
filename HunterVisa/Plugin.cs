using BepInEx;
using MonoMod.Cil;
using Menu;
using System.Reflection;

[assembly:AssemblyTrademark("Beau")]
[assembly:AssemblyDescription("Hunter finally has his visa.")]

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public void OnEnable()
    {
        On.RainWorld.Start += RainWorld_Start;
    }

    private void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
    {
        IL.Menu.SleepAndDeathScreen.AddPassageButton += SleepAndDeathScreen_AddPassageButton;
        IL.Menu.SleepAndDeathScreen.GetDataFromGame += SleepAndDeathScreen_GetDataFromGame;
        orig(self);
    }

    private void SleepAndDeathScreen_GetDataFromGame(ILContext il)
    {
        ILCursor c = new ILCursor(il);

        if (c.TryGotoNext(MoveType.After,
            i => i.MatchLdarg(1),
            i => i.MatchLdfld<KarmaLadderScreen.SleepDeathScreenDataPackage>("characterStats"),
            i => i.MatchLdfld<SlugcatStats>("name"),
            i => i.MatchLdcI4(out _),
            i => i.MatchBeq(out _)
            ))
        {
            var beq = c.Prev;
            var jumpTo = c.DefineLabel();
            c.MarkLabel(jumpTo);
            beq.Operand = jumpTo;
        }
        else UnityEngine.Debug.LogError(PluginInfo.PLUGIN_NAME + ": Couldn't find landmark for SleepAndDeathScreen.GetDataFromGame patch");
    }

    private void SleepAndDeathScreen_AddPassageButton(ILContext il)
    {
        ILCursor c = new ILCursor(il);
        ILLabel continueDest = null;

        if(c.TryGotoNext(MoveType.After,
            i => i.MatchLdarg(0),
            i => i.MatchLdfld<KarmaLadderScreen>("saveState"),
            i => i.MatchLdfld<SaveState>("saveStateNumber"),
            i => i.MatchLdcI4(2),
            i => i.MatchBneUn(out continueDest)
            ))
        {
            c.Emit(Mono.Cecil.Cil.OpCodes.Br, continueDest);
        }
        else UnityEngine.Debug.LogError(PluginInfo.PLUGIN_NAME + ": Couldn't find landmark for SleepAndDeathScreen.AddPassageButton patch");
    }
}
