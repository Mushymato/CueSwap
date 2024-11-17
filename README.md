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

Additional sound cue replacements must be manually added to this mod, if you have a need for any particular sound please let me know and I'll look into adding it.

## Modder Features

The debug build provide verbose logging of when the sound replacement happens, and provides the console command play_sound \<cue\> for playing cues by name.

Transpiler methods in this mod are written with a source generator. The generated source can be viewed in `CueSwap/obj/Debug/net6.0/generated/CueSwapGenerator/CueSwapGenerator.CueSwapTranspilerGenerator` once you do a build.
