# Giant Extension Ladders 
_Compatible with update v56 of the game!_ 

This mod adds a few new giant Extension Ladders to the store!\
Ever got stuck on your way to the fire exit because your ladder was just not high enough? Well, with up to **SEVEN times the normal ladder size** this won't happen again!

**Don't like climbing? But I bet you like murder!** \
No one expects a deadly ladder from above that was placed far away!

## Content
> You can use an additional mod for faster climbing (I made this one "[Fast Climbing](https://thunderstore.io/c/lethal-company/p/Inoyu/Fast_Climbing/)", but there are a lot of other mods that'll work too).
- **Three giant ladders:**
  - Big ladder for climbing higher than usual
  - Huge ladder for climbing even higher
  - Ultimate ladder for climbing EVEN higher
- **A tiny ladder** to play with (still bonks players if placed correctly)\
_This ladder is climbable if the player is tiny! You'll need mods that shrink players to 0.25 and lower of their og size (tested with [LittleCompany](https://thunderstore.io/c/lethal-company/p/Toybox/LittleCompany/))._
- **Ladder Physics:**
  - **ladder stacking:** ladders can be placed on other ladders, allowing for [huge ladder-constructs](https://i.imgur.com/ueWQFiY.png)
  - ladder leaning: ladders will lean against each other and tip over if the one undeneath them is removed
  - ladder falling: when a ladder is on another one and the one underneath it is removed, it will collapse and fall down

- **Ladder Collector:** Is a ladder stuck or lost because it fell down somewhere? Then this device is your friend! A strange magnet collects all ladders on the map (that are not extended or in the facility/shiproom) and teleports them next to itself.\
_Note: If you just bought a new ladder, you need to pick it up at least once before it can be collected by the ladder collector!_

- **Configs:** This mod has a lot of settings for customizing your experience. In the mod's configs you can adjust things like item prices and ladder extension-times or enable an automatic ladder collector when the ship leaves.

## Whats new?
#### v3.2.0
- fixed some bugs (mod is now compatible with v56)

## Pictures
You can look at the pictures on the [thunderstore page](https://thunderstore.io/c/lethal-company/p/Inoyu/Giant_Extension_Ladders/) if you're interested. c:

## Configs (these are synced with lobby-host's configs!)
- set ladder prices
- enable/disable specific ladders
- set ladders to "always active", so they'll stay extended indefinitely
- set the time the ladder remains extended
- enable "auto collect" (collects all ladders on the map when the ship starts leaving)
  - _this one is a bit experimental, I can't guarantee that it is free of bugs/glitches_
- set the method used for a bugfix regarding a bug with sales (only occurs if you disable ladders in configs)
     - safe fix: (enable this to avoid most issues)
     - experimental fix (enable this if you're not afraid of any potential bugs/issues)
     - don't fix (enable this if you don't care about some sales sometimes being falsely displayed)\
  _This config might not work correctly if you are using other terminal related mods. If so, don't disable ladders, instead set their price to max via configs._

## Future Updates?
At the moment there are no updates planned. Although, if you have any cool ideas/suggestions feel free to write me on Discord via dm (inoyuuuuu), in the [LethalCompany Modding server](https://discord.gg/XeyYqRdRGC) in this mod's thread or add an issue to [GitHub](https://github.com/Inoyuuuuu/GiantExtensionLaddersV2/issues)!

## Known Issues
_If you have questions or encounter any bugs, feel free to report them on the [GitHub issue-page](https://github.com/Inoyuuuuu/GiantExtensionLaddersV2/issues) or write me directly via Discord (inoyuuuuu)!_
 - Ladders are sometimes not in complete sync, meaning they can be extended for one player but not for others
 - While the ladder stops at the roof, the ladder's collision continues above it
 - Climbing on the tiny ladder exits early when climbing a more horizontal ladder
 - If you have some ladders disabled through the configs, some other mods that make changes to the terminal might interfere with my "sales bug fix". To avoid any kind of falsely displayed sales, don't disable ladders, just set their price to max (sou you realistically can't buy them).

## Installation
 > **I highly recommend using a modmanager like [r2modman](https://thunderstore.io/package/ebkr/r2modman/) for easy mod installation and ensuring that everyone has the same mods and configs!**

This mod requires three other mods! \
If you still want to do it manually:

1. Download this mod, [BepInEx](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/), [Evaisa's "LethalLib"](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/)
 and [Evasia's "HookGenPatcher"](https://thunderstore.io/c/lethal-company/p/Evaisa/HookGenPatcher/)
2. Install BepInEx and follow the special instructions on how to install HookGenPatcher
3. Download and install [CSync](https://thunderstore.io/c/lethal-company/p/Owen3H/CSync/)
4. Put the content of LethalLib.zip and BigExtensionLadders.zip in the BepInEx\plugins folder

### Credits
Thanks to [Owen3H](https://github.com/Owen3H) the Creator of CSync for his plugin and helping me with syncing the configs! \
Thanks to [Evasia](https://github.com/EvaisaDev) for the LethalLib plugin and the awesome Unity Project. \
Thanks to [Xilophor](https://github.com/Xilophor) for the clean VisualStudio Mod Template.

\
\
\
\
_If you want to check out other mods I've made, you can click [here](https://thunderstore.io/c/lethal-company/p/Inoyu/)! :3_
#### 🌸Have fun!🌸 _~[Inoyu](https://github.com/Inoyuuuuu)_
