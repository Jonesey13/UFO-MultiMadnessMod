# Mortol 4P Mods

This is a mod to enable 4 players for various games in the game UFO50. Currently the following games are supported:
* Mortol
* Paint Chase

This mod is provided as a combination of code diffs and additional assets and is *heavily* inspired from [p-sam's UFO50 mods repo](https://github.com/p-sam/ufo50-mods).

All original rights belong to Mossmouth (and am happy to take down this repo at their request).
* Data from the original game has been minimised by providing the smallest diff context needed (1 line)
* Some of the "new" sprites are palette swaps of existing sprites

## How to use this repo

> These are instructions for developers. Please use a delta for the `data.win` file provided on [the Releases page](https://github.com/Jonesey13/Mortol4PMod/releases) if you are not a developer!

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
3. `patch_mortol4P.csx`
    * This copies `data.win` into the `output` folder and imports the modded code and textures/sprites

After completing these steps the modded `data.win` file can be copied back into your original UFO50 game folder to apply the mod.

The `.csx` scripts can be run using the VSCode launch configuration provided (first you must select the script you want to run in the "Explorer" menu before running it from the "Run and Debug" menu).

## Credits
Thank you to the incredible [Underminers team](https://github.com/UnderminersTeam) for providing such a useful and versatile modding library for Game Maker Studio.

Credit also goes to [this UFO50 Wiki](https://ufo50.miraheze.org/) for documenting where to begin with modding UFO50.

Next up a big thanks to [p-sam](https://github.com/p-sam) for providing such a good modding framework (this is a pale imitation of what you've already done).

Lastly a massive thanks to [Mossmouth](https://www.mossmouth.com/) for making such a rich collection of games!