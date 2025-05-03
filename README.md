# Mortol 4 Player Mod

This is a mod to enable 4 players for the game UFO50.
* This **might** evolve in more 4 player modes for other games in the collection

This mod is provided as a combination of code diffs and additional assets and is heavily inspired from [p-sam's UFO50 mods repo](https://github.com/p-sam/ufo50-mods)

I have tried to minimise data from the original game by provided the minimal code context needed (1 line) and only new textures to be added to the `data.win` file.

All rights belong to Mossmouth (am happy to remove this repo at their request)


## How to use this repo

> These are instructions for developers. Please use the delta files provided on the Releases page if you are not a developer!

This repo is designed to run standalone as a Dev Container in VSCode.

The dev container will setup dotnet & dotnet script.

Once the dev container is loaded you must place your copy of UFO50's data.win file in the `input` folder.

Next you need to run the following command to download [the UndertaleModTool CLI](https://github.com/UnderminersTeam/UndertaleModTool) and set it up for use:
```
bash import/setup_lib.sh
```

From here you want to run the following scripts in order:
1. `extract_scripts_from_original.csx`
    * This extracts the relevant original source `*.gml` files from `data.win`
2. `diff_applyer.csx`
    * This generates the modded versions of the source files
3. `patch_mortol4P.csx`
    * This copies `data.win` into the `output` folder and imports the modded code and textures/sprites

After completing these steps the modded `data.win` file can be copied back into your original UFO50 game folder to apply the mod.

The `.csx` scripts can be run using the VSCode launch configuration provided.

