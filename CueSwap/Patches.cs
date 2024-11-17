using System.Reflection;
using System.Reflection.Emit;
using CueSwapGenerator;
using HarmonyLib;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using static StardewValley.Menus.ItemGrabMenu;

namespace CueSwap;

/// <summary>
/// [CueSwapTranspiler(
//     nameof(OpCodes.Callvirt), // opcode of a sound call or assignment
//     nameof(GameLocation), // type
//     nameof(GameLocation.localSound), // method
//     7, // distance between ldstr and sound call, exclusive on both ends
//     "doorCreak", // sound cue str
//     "doorCreak.ShippingBin" // new cue str
// )]
/// </summary>

// shipping bin building
[CueSwapTranspiler(
    nameof(OpCodes.Callvirt),
    nameof(GameLocation),
    nameof(GameLocation.localSound),
    7,
    "doorCreak",
    "doorCreak.ShippingBin"
)]
[CueSwapTranspiler(
    nameof(OpCodes.Callvirt),
    nameof(GameLocation),
    nameof(GameLocation.localSound),
    7,
    "doorCreakReverse",
    "doorCreakReverse.ShippingBin"
)]
[CueSwapTranspiler(
    nameof(OpCodes.Call),
    nameof(DelayedAction),
    nameof(DelayedAction.playSoundAfterDelay),
    11,
    "Ship",
    "Ship.ShippingBin"
)]
// shipping bin islandwest
[CueSwapTranspiler(
    nameof(OpCodes.Call),
    nameof(GameLocation),
    nameof(GameLocation.localSound),
    7,
    "doorCreak",
    "doorCreak.IslandBin"
)]
[CueSwapTranspiler(
    nameof(OpCodes.Call),
    nameof(GameLocation),
    nameof(GameLocation.localSound),
    7,
    "doorCreakReverse",
    "doorCreakReverse.IslandBin"
)]
[CueSwapTranspiler(
    nameof(OpCodes.Call),
    nameof(DelayedAction),
    nameof(DelayedAction.playSoundAfterDelay),
    11,
    "Ship",
    "Ship.IslandBin"
)]
// mini shipping bin chest
[CueSwapTranspiler(
    nameof(OpCodes.Callvirt),
    nameof(GameLocation),
    nameof(GameLocation.localSound),
    7,
    "doorCreak",
    "doorCreak.MiniBin"
)]
[CueSwapTranspiler(
    nameof(OpCodes.Callvirt),
    nameof(GameLocation),
    nameof(GameLocation.localSound),
    7,
    "doorCreakReverse",
    "doorCreakReverse.MiniBin"
)]
[CueSwapTranspiler(
    nameof(OpCodes.Stfld),
    nameof(InventoryMenu),
    nameof(InventoryMenu.moveItemSound),
    0,
    "Ship",
    "Ship.MiniBin"
)]
// intro menu click on CA sound
[CueSwapTranspiler(
    nameof(OpCodes.Call),
    nameof(Game1),
    nameof(Game1.playSound),
    30,
    "Duck",
    "Duck.Intro"
)]
internal static partial class Patches
{
    internal static void Patch(string modId)
    {
        Harmony harmony = new(modId);
#if SHIPPING_BIN
        // shipping bin building
        TranspileWithLog(
            harmony,
            AccessTools.DeclaredMethod(typeof(ShippingBin), "openShippingBinLid"),
            new HarmonyMethod(
                typeof(Patches),
                nameof(T_GameLocation_localSound_doorCreak_doorCreakShippingBin)
            )
        );
        TranspileWithLog(
            harmony,
            AccessTools.DeclaredMethod(typeof(ShippingBin), "closeShippingBinLid"),
            new HarmonyMethod(
                typeof(Patches),
                nameof(T_GameLocation_localSound_doorCreakReverse_doorCreakReverseShippingBin)
            )
        );
        TranspileWithLog(
            harmony,
            AccessTools.DeclaredMethod(typeof(ShippingBin), nameof(ShippingBin.showShipment)),
            new HarmonyMethod(
                typeof(Patches),
                nameof(T_DelayedAction_playSoundAfterDelay_Ship_ShipShippingBin)
            )
        );
#endif

#if ISLAND_SHIPPING_BIN
        // shipping bin for ginger island
        TranspileWithLog(
            harmony,
            AccessTools.DeclaredMethod(typeof(IslandWest), "openShippingBinLid"),
            new HarmonyMethod(
                typeof(Patches),
                nameof(T_GameLocation_localSound_doorCreak_doorCreakIslandBin)
            )
        );
        TranspileWithLog(
            harmony,
            AccessTools.DeclaredMethod(typeof(IslandWest), "closeShippingBinLid"),
            new HarmonyMethod(
                typeof(Patches),
                nameof(T_GameLocation_localSound_doorCreakReverse_doorCreakReverseIslandBin)
            )
        );
        TranspileWithLog(
            harmony,
            AccessTools.DeclaredMethod(typeof(IslandWest), nameof(IslandWest.showShipment)),
            new HarmonyMethod(
                typeof(Patches),
                nameof(T_DelayedAction_playSoundAfterDelay_Ship_ShipIslandBin)
            )
        );
        // this seems to be old shipping bin, unsure why islandwest still use it
        TranspileWithLog(
            harmony,
            AccessTools.DeclaredMethod(typeof(Farm), nameof(Farm.showShipment)),
            new HarmonyMethod(
                typeof(Patches),
                nameof(T_DelayedAction_playSoundAfterDelay_Ship_ShipIslandBin)
            )
        );
#endif

#if MINI_SHIPPING_BIN
        // mini shipping bin chest
        TranspileWithLog(
            harmony,
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.UpdateFarmerNearby)),
            new HarmonyMethod(
                typeof(Patches),
                nameof(T_GameLocation_localSound_doorCreak_doorCreakMiniBin)
            )
        );
        TranspileWithLog(
            harmony,
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.UpdateFarmerNearby)),
            new HarmonyMethod(
                typeof(Patches),
                nameof(T_GameLocation_localSound_doorCreakReverse_doorCreakReverseMiniBin)
            )
        );
        TranspileWithLog(
            harmony,
            AccessTools.DeclaredConstructor(
                typeof(ItemGrabMenu),
                [
                    typeof(IList<Item>),
                    typeof(bool),
                    typeof(bool),
                    typeof(InventoryMenu.highlightThisItem),
                    typeof(behaviorOnItemSelect),
                    typeof(string),
                    typeof(behaviorOnItemSelect),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(int),
                    typeof(Item),
                    typeof(int),
                    typeof(object),
                    typeof(ItemExitBehavior),
                    typeof(bool),
                ]
            ),
            new HarmonyMethod(
                typeof(Patches),
                nameof(T_InventoryMenu_moveItemSound_Ship_ShipMiniBin)
            )
        );
#endif

        TranspileWithLog(harmony, AccessTools.DeclaredMethod(
            typeof(TitleMenu),
            nameof(TitleMenu.receiveLeftClick)),
            new HarmonyMethod(
                typeof(Patches),
                nameof(T_Game1_playSound_Duck_DuckIntro)
            )
        );
    }

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
}
