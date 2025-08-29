# UFO-MultiMadnessMod

This is a mod to enable 4 players for various games in the game UFO50. 

![velgress4P](./images/Velgress4P.png)

Currently the following games are supported:
* Velgress (2/3/4P Co-op & VS)
* Bushido Ball (4P VS)
* Hot Foot (4P VS)
* Paint Chase (4P Team VS)
* Attactics (4P Team VS)
* Waldorf's Journey (2/3/4P Free For All)
* The Big Bell Race (up to 6 Players)
* Party House (3/4P VS)
* Ninpek (3/4P Co-op)
* Mortol (2/3/4P Co-op)

![waldorf4P](./images/Waldorf4P.png)

This also incorporates p-sam's rich presence for Discord mod.

This mod is provided as a combination of code diffs and additional assets and is *heavily* inspired from [p-sam's UFO50 mods repo](https://github.com/p-sam/ufo50-mods).

All original rights belong to Mossmouth (and am happy to take down this repo at their request).
* Data from the original game has been minimised by providing the smallest diff context needed (1 line)
* Some of the "new" sprites are palette swaps of existing sprites

## How to use this repo

### Instructions for Non-Developers
UFO 50 mods commonly use an `xdelta` file to patch the main gamefile (`data.win`) directly. Please use a delta [from the Releases page](https://github.com/Jonesey13/Mortol4PMod/releases). 

A quick and easy way to apply a delta file is to use a site like [https://kotcrab.github.io/xdelta-wasm/](https://kotcrab.github.io/xdelta-wasm/) or download a standalone tool such as [https://github.com/marco-calautti/DeltaPatcher](https://github.com/marco-calautti/DeltaPatcher).
* You can find more details at [https://ufo50.miraheze.org/wiki/Guide_to_Modding_UFO_50](https://ufo50.miraheze.org/wiki/Guide_to_Modding_UFO_50)

### Instructions for Developers


This repo is designed to run standalone as a Dev Container in VSCode.

The dev container will setup dotnet & dotnet script.

Once the dev container is loaded you must place your copy of UFO50's `data.win` file in the `input` folder.

Next you need to run the following command to download [the UndertaleModTool CLI](https://github.com/UnderminersTeam/UndertaleModTool) and set it up for use:
```
bash import/setup_lib.sh
```

From here you want to run the following scripts in order:
1. `extract_scripts_from_original.csx`
    * This extracts the relevant original source `*.gml` files from `data.win` and puts them in the `ufo50_original_scripts` folder
2. `diff_applyer.csx`
    * This generates the modded versions of the source files (you can view the modded files in the `ufo50_modded_scripts` folder)
3. `patch_for_local.csx`
    * This copies `data.win` into the `output` folder and imports the modded code and textures/sprites

After completing these steps the files in the `output` folder can be copied back into your original UFO50 game folder to apply the mod.

The `.csx` scripts can be run using the VSCode launch configuration provided (first you must select the script you want to run in the "Explorer" menu before running it from the "Run and Debug" menu).

## Credits
Thank you to the incredible [Underminers team](https://github.com/UnderminersTeam) for providing such a useful and versatile modding library for Game Maker Studio.

Credit also goes to [this UFO50 Wiki](https://ufo50.miraheze.org/) for documenting where to begin with modding UFO50.

Kudos to [Terrance](https://github.com/Terrance) for putting in the legwork for making Bushido Ball 4 player.

Next up a big thanks to [p-sam](https://github.com/p-sam) for providing such a good modding framework (this is a pale imitation of what you've already done).

Lastly a massive thanks to [Mossmouth](https://www.mossmouth.com/) for making such a rich collection of games!