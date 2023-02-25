# Description

# PerformanceTracker


## FPS counter and performance statistics for BepInEx

A mod that measures many performance statistics. It can be used to help determine causes of performance drops and other issues. Here are some of the features:
- Accurately measures true ms spent per frame (not calculated from FPS)
- Measures time spent in each of the steps Unity takes in order to render a frame (e.g. how long all Update methods took to run collectively)
- Measures time spent in each of the installed BepInEx plugins (easy to see performance hogs, `only counts code running in Update methods`)
- Measures memory stats, including amount of heap memory used by the GC and GC collection counts (if supported)

![preview](https://i.imgur.com/huFsbHy.png)

## How to use
1. Install [BepInExPack_Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) per the instructions. If you are using one of Thunderstore's mod managers (r2modman or Thunderstore Mod Manager), you can install it from there in the Online tab.
2. `IF YOU ARE NOT USING A MOD MANAGER` Extract the release into your game root, the .dll should end up in BepInEx\plugins directory.
3. `IF YOU ARE USING A MOD MANAGER` Download from Thunderstore using the mod manager of choice (r2modman or Thunderstore Mod Manager), the .dll should end up in the mod manager's plugin directory, allowing the mod to load just fine.
4. Start the game and press U + LeftShift.

The on/off hotkey and looks can be configured in the config file in Bepinex\config (have to run the game at least once to generate it), or by using [BepInEx.ConfigurationManager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/).


`This mod uses a file watcher. If the configuration file is not changed with BepInEx Configuration manager, but changed in the file directly on the server, upon file save, it will sync the changes to all clients.`

> This mod is not needed on a server. It's client only. Install on each client that you wish to have the mod load.



`Feel free to reach out to me on discord if you need manual download assistance.`


# Author Information

### Azumatt

`DISCORD:` Azumatt#2625

`STEAM:` https://steamcommunity.com/id/azumatt/

<details><summary><b>Credits</b></summary>

Thank you to the following people for their contributions to this mod. Their code is included in this mod and is primarily what this mod is based off of:

[ManlyMarco](https://github.com/ManlyMarco)

[Kein](https://github.com/Kein)

[VictorienXP](https://github.com/VictorienXP)




</details>

For Questions or Comments, find me in the Odin Plus Team Discord or in mine:

[![https://i.imgur.com/XXP6HCU.png](https://i.imgur.com/XXP6HCU.png)](https://discord.gg/Pb6bVMnFb2)
<a href="https://discord.gg/pdHgy6Bsng"><img src="https://i.imgur.com/Xlcbmm9.png" href="https://discord.gg/pdHgy6Bsng" width="175" height="175"></a>
***

> # Update Information (Latest listed first)

| `Version`   | `Update Notes`                                                       |
|-------------|----------------------------------------------------------------------|
| 1.0.1/1.0.2 | - README updates. Update manifest.json to include link to the GitHub |
| 1.0.0       | - Initial Release                                                    |
