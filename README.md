# CueSwap

Sound cues in Stardew Valley are key'd by a string which can be reused in many contexts. For example "sandyStep" is used for footsteps, but also as general sfx in menus.

This mod helps mod authors change certain sound cues in the game without changing every instance of the cue, by changing the key to something else that can be individually targeted.

## Supported Cues

### Shipping Bin
- doorCreak &rArr; doorCreak.ShippingBin
- doorCreakReverse &rArr; doorCreakReverse.ShippingBin
- Ship &rArr; Ship.ShippingBin

### Mini Shipping Bin
- doorCreak &rArr; doorCreak.MiniBin
- doorCreakReverse &rArr; doorCreakReverse.MiniBin
- Ship &rArr; Ship.MiniBin

### Shipping Bin (Island Farm)
- doorCreak &rArr; doorCreak.IslandBin
- doorCreakReverse &rArr; doorCreakReverse.IslandBin
- Ship &rArr; Ship.IslandBin

### Intro
- Duck &rArr; Duck.Intro

### Wild Tree
- treethud &rArr; treethud.TreeFall (when the tree falls down after chop)
- treethud &rArr; treethud.StumpFall (when the stump is chopped)

### Fruit Tree
- treethud &rArr; treethud.TreeFall (when the tree falls down after chop, intentionally same as wild tree)

### Bush
- treethud &rArr; treethud.BushFall (when bush is chopped)

### Parrot Platform
- treethud &rArr; treethud.ParrotPlatform (this is the parrot takeoff sound)

### Sound In the Night
- thunder_small &rArr; thunder_small.Earthquake (day 31 railroad unlock)

### Dino Monster (Pepper Rex)
- croak &rArr; croak.DinoMonster
- furnace &rArr; furnace.DinoMonster

### Fireplace
- fireball &rArr; fireball.Fireplace1 (gamelocation fireplace)
- fireball &rArr; fireball.Fireplace2 (furniture fireplace)

Additional sound cue replacements must be manually added to this mod, if you have a need for any particular sound please let me know and I'll look into adding it.

## Useful Console Commands

These are both base game console commands, listed here because they are helpful.

- `debug ps <cue> <pitch>`: Plays a certain sound cue with a certain pitch.
- `debug logsounds <cue> <pitch>`: Begin logging all sound cues, you can use this to find out what cue might need replacing.

## Modder Features

The debug build provide verbose logging of when the sound replacement happens.

## How to Add Another Cue

You need to edit the file named `CueSwap/Patches.cs`. This will require you to read the game's decompile and IL through ILSpy or similar.

1. Find place to patch, e.g. for shipping bin open:
```cs
private void openShippingBinLid()
{
    if (shippingBinLid != null)
    {
        if (shippingBinLid.pingPongMotion != 1 && IsInCurrentLocation())
        {
            // this is what we want to change, using a transpiler
            Game1.currentLocation.localSound("doorCreak");
        }
        shippingBinLid.pingPongMotion = 1;
        shippingBinLid.paused = false;
    }
}
```
Then, find the corresponding bit of IL `Game1.currentLocation.localSound("doorCreak");`:
```ini
    call class StardewValley.GameLocation StardewValley.Game1::get_currentLocation()
    ldstr "doorCreak"
    # we need to add a shim here that will attempt to use a new cue if it exists
    ldloca.s 0
    initobj valuetype [System.Runtime]System.Nullable`1<valuetype [MonoGame.Framework]Microsoft.Xna.Framework.Vector2>
    ldloc.0
    ldloca.s 1
    initobj valuetype [System.Runtime]System.Nullable`1<int32>
    ldloc.1
    ldc.i4.0
    # this is the call that we will use to match, note that there are 7 IL between this and ldstr "doorCreak"
    callvirt instance void StardewValley.GameLocation::localSound(string, valuetype [System.Runtime]System.Nullable`1<valuetype [MonoGame.Framework]Microsoft.Xna.Framework.Vector2>, valuetype [System.Runtime]System.Nullable`1<int32>, valuetype StardewValley.Audio.SoundContext)
```
This is same as typical starting point of transpiler.

2. An attribute for generating the transpiler
```cs
// attribute name
[CueSwapTranspiler(
    // Str values for Opcode, Type, and Method name, needed to find callvirt instance void StardewValley.GameLocation::localSound
    nameof(OpCodes.Callvirt),
    nameof(GameLocation),
    nameof(GameLocation.localSound),
    // number of IL between ldstr "doorCreak" and callvirt instance void StardewValley.GameLocation::localSound
    7,
    // original cue name
    "doorCreak",
    // new cue name
    "doorCreak.ShippingBin"
)]
// this class will have many attributes on it
internal static partial class Patches
{
...
```

This generates the method `Patches.T_GameLocation_localSound_doorCreak_doorCreakShippingBin`, which can be viewed in `CueSwap/obj/Debug/net6.0/generated/CueSwapGenerator/CueSwapGenerator.CueSwapTranspilerGenerator` once you do a build.

If you need to match against a particular overload, do `$"{nameof(Game1.playSound)} string int"` with a space deliminated list of type names. following the method name.

3. Add call to `harmony.Patch`, just like normal transpiler
```cs
internal static void Patch(string modId)
{
    ...
    TranspileWithLog(
        harmony,
        AccessTools.DeclaredMethod(typeof(ShippingBin), "openShippingBinLid"),
        new HarmonyMethod(
            typeof(Patches),
            nameof(T_GameLocation_localSound_doorCreak_doorCreakShippingBin)
        )
    );
    ...
}
// helper method, does logging before calling harmony.Patch
internal static void TranspileWithLog(
    Harmony harmony,
    MethodBase original,
    HarmonyMethod transpiler
)
{
    ModEntry.Log(
        $"Patch '{original.DeclaringType?.Name}.{original.Name}' with '{transpiler.method.DeclaringType?.Name}.{transpiler.method.Name}'"
    );
    harmony.Patch(original: original, transpiler: transpiler);
}
```
