# Upgrade World

This tool includes console commands to add new content to already explored areas (and more).

Always back up your world before making any changes!

# Things you can do

- Place tar pits and mountain caves on already explored areas.
- Reroll chest loots to have onion seeds on already explored areas.
- Completely regenerate Mistlands (with new or legacy content).
- Remove any object from the game world.
- Generate the whole world without having to physically go anywhere.
- Manually regenerate areas or dungeons.

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim).
2. Download the zip and extract the DLL file to the \<GameDirectory\>\BepInEx\plugins\ folder.
3. For dedicated servers, install this mod also on the server.
4. Optionally install the [Configuration manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/tag/v16.4).
5. Optionally install the [Server Devcommands](https://valheim.thunderstore.io/package/JereKuusela/Server_devcommands/) for better autocomplete.

# Quick instructions:

1. Back up your world!
2. Install the mod and load the world.
3. Open console with F5, write `upgrade` and use tab to cycle through available options.
4. Write `start` to execute and wait until tue operation completes.
5. Verify that your bases are ok (should be because of the automatic base detection).
6. If not, restore back up. read more detailed instructions and go back to the first step.
7. Uninstall the mod and enjoy the new stuff!

This mod also adds current coordinates (and zone index) to the minimaps which should help using the commands and setting up the config.

Note: The default base detection is very conservative. Single workbenches, campfires, etc. will exclude a significant area around them, unless configured otherwise.

# Dedicated servers

By default, all admins can execute commands of this mod.

If you wish to restrict this, edit the config file and add Steam IDs to the root users setting (separated by ,). After that only those users can execute commands.

# Parameters

Most commands allow fine-tuning the affected area. Following parameters are available:

- `biomes=biome1,biome2,...`: Only includes given biomes. If not given, all biomes are included. Available options are: "AshLands", "BlackForest", "DeepNorth", "Meadows", "Mistlands", "Mountain", "Ocean", "Plains" and "Swamp".
- `chance=percentage`: Makes a single operation to be applied randomly. Only works for some commands.
- `distance=min,max`: Short-hand for setting both distances.
- `force`: Disables player base detection (same as `safeZones=0`).
- `pos=x,z`: Position of the center. If not given, player's position is used. Default distance is all of the map.
- `max=distance` or `maxDistance=distance`: Maximum distance from the center. Meters for `pos` and adjacent zones for `zone`.
- `min=distance` or `minDistance=distance`: Minimum distance from the center. Meters for `pos` and adjacent zones for `zone`.
- `noEdges`: Only include zones that have included biomes in all of its corners. Without the flag, it's enough if just one of the corners is in the included biomes.
- `safeZones=distance`: Set safe zone size of major structures (0 to disable). Default value 2 is defined in the config. List of major structures is also defined in the config.
- `start`: Automatically executes the command without having to use `start`. Can be permanently turned on from the config.
- `zone=x,z`: Position of the center zone. Can't be used with `pos`. Default distance is the single zone.

# Commands

Overview of available commands (remember that tab key can be used for autocomplete / cycle through options):

- `backup`: Saves the game with a timestamped file name.
- `biomes_count [precision] [...args]`: Counts biomes by sampling points with a given precision (meters). Result is also printed to the player.log file.
- `chests_reset [chest name] [looted] [...item_ids] [...args]`: Rerolls contents of a given treasure chest (use tab key to cycle through available treasure chests). Chest name * rerolls all treasure chests. Empty (looted) chests are only rerolled with `looted` flag. Item ids can be used to detect and prevent rerolling chests which players are using to store items.
- `locations_add [id1,id2,...] [noclearing] [...args]`: Adds locations to already explored areas. With `noclearing`, the location area is not cleared of existing objects.
- `locations_remove [id1,id2,...] [...args]`: Removes locations and prevents new ones from appearing (until a command like `genloc` or `locations_add` is used).
- `locations_reset [id1,id2,...] [...args]`: Resets locations by removing them and then placing them at the same position. Dungeons which have a random rotation will also get a new layout.
- `objects_count [all] [id1,id2,...] [...args]`: Counts objects. If no ids given then counts all objects. All objects are listed with the flag `all`. Result is also printed to the player.log file. Wildcards are also supported.
- `objects_list [id1,id2,...] [...args]`: Lists objects showing their position and biome. Result is also printed to the player.log file.
- `objects_remove [id1,id2,...] [...args]`: Removes objects. Recommended to use `count_objects` to check that you don't remove too much.
- `save_disable`: Disables world saving. But still a good idea to make backups.
- `save_enable`: Enables world saving.
- `start`: Most commands don't execute instantly but instead print the zones being affected. This command can be then used to start executing.
- `stop`: Stops the current execution and clears any pending command.
- `time_change [seconds]`: Changes the world time and updates object timestamps.
- `time_change_day [days]`: Changes the world time and updates object timestamps.
- `time_set [seconds]`: Sets the world time and updates object timestamps.
- `time_set_day [days]`: Sets the world time and updates object timestamps.
- `upgrade [operation] [...args]`: Short-hand for using common operations (mainly to add new content).
- `vegetation_default`: Restored the default vegetation for the world generator.
- `vegetation_add [id1,id2,...] [...args]`: Adds vegetation to generated areas. If ids are not given, adds every vegetation.
- `vegetation_disable [id1,id2,...]`: Disables vegetation for the world generator, affecting any forced (with `generate` command) or natural generation.
- `vegetation_enable [id1,id2,...]`: Enables vegetation for the world generator, affecting any forced (with `generate` command) or natural generation.
- `vegetation_reset [id1,id2,...] [...args]`: Removes and adds vegetation to generated areas. If ids are not given, resets every vegetation.
- `verbose`: Toggles the verbose mode which prints more information when enabled. Can also be toggled from the config.
- `zones_generate [...args]`: Pre-generates areas without having to visit them.
- `zones_reset [...args]`: Destroys areas making them ungenerated. These areas will be generated when visited. Can be also used to reduce save file size.

Examples:
- `biomes_count 100 min=5000 pos=0,0`: Counts only biomes after 5000 meters from the world center by checking the biom every 100 meters.
- `chests_reset TreasureChest_mountains Amber Coins AmberPearl Ruby Obsidian ArrowFrost OnionSeeds`: Rerolls mountain treasure chests which only have naturally occurring items.
- `chests_reset * looted min=1500 pos=0,0`: Resets all chests which are 1500 meters away from the world center.
- `locations_remove Meteorite`: Removes all flametal ores.
- `locations_reset SunkenCrypt4,Crypt2,Crypt3,Crypt4,MountainCave02,TrollCave02`: To regenerate dungeons. Some entraces will randomly rotate which will also randomize the dungeon layout.
- `objects_count Spawner_\*`: Counts all creature spawnpoints.
- `objects_count \*\_wall\*\_`: Counts all walls structures.
- `objects_list VikingShip,Karve,Raft`: Lists coordinates of all ships.
- `objects_remove FirTree chance=33`: Removes 33% of Fir Trees.
- `objects_remove StatueCorgi`: Removes all corgi statues.
- `objects_remove Spawner_\* max=200`: Removes all creature spawnpoints within 200 meters.
- `upgrade mountain_caves`: Places mountain caves to already explored areas.
- `upgrade tarpits`: Places tar pits to already explored areas.
- `upgrade onions`: Rerolls already generated and unlooted mountain chests.
- `upgrade new_mistlands`: Fully regenerates mistlands biomes.
- `upgrade old_mistlands`: Fully regenerates mistlands biomes with the legacy content (webs, etc.).
- `vegetation_disable BlueberryBush`: Disables generation of blueberry bushes.
- `vegetation_enable vertical_web horizontal_web tunnel_web`: Enables webs to generate in Mistlands.
- `vegetation_reset biome=Meadows force`: Resets all vegetation in Meadows, including areas with player bases.
- `zones_generate`: To generate the entire world (takes hours) and then use `objects_count` command to check how many of each object exists.
- `zones_generate`: To generate the entire world (takes hours) and then use `objects_remove` for modifications.
- `zones_reset biomes=mistlands`: Destroying a biome.
- `zones_reset min=5000 pos=0,0`: Destroying areas after 5000 meters from the world center.
- `zones_reset zone=3,-3 safeZones=0`: Destroy a single zone at indices 3,-3 to fix any local issues.

# Configuration

The config can be found in the \<GameDirectory\>\BepInEx\config\ folder after the first start up.

Filtering options in the config:

- Safe zone items: List of items used for the automatic player base detection. By default includes structures that have the player base effect.
- How many adjacent zones are included in the safe zone: Size of the player base protection. Default value 1 means 3x3 zones per player base structure to ensure proper coverage. Value -1 can be used to disable the player base detection.
- Safe distance around the player: Excludes zones that are too close to the players (in meters). All commands allow specifying the minimum distance so using this setting is not really needed.
- Custom points: Coordinates and distances to exclude zones. Format: x1,z1,min1,max1|x2,z2,min2,max2|...

Other settings are:

- Verbose output: Prints more output which gives a better understanding how the mod operators. However this can spoil things of your world.
- Prevent double ZNet view: Some bugged objects keep duplicating and corrupting the save. This prevents that from happening which allows removing these objects.
- Automatic start: Starts commands automatically without having to use the start command. This allows using the commands more easily but can lead to more mistakes.
- Operations per update: How many zones are destroyed per Unity update. Can be useful if destroying large parts of a world, but recommended to keep it as it is.
- Show map coordinates: If true, shows coordinates and distance on the big map.
- Show minimap coordinates: If true, shows coordinates on the minimap.
- Root users: SteamIds that are allowed to execute commands. If not set, all admins can use the commands.

Examples:
- Setting "300,500,500,0|1000,2000,500,0" to custom points would protect areas at coordinates 300,500 and 1000,2000 within 500 meters.
- Setting "300,500,0,100" to custom points would only operate near coordinates 300,500 witin 100 meters.

# How it works

## Glossary

- Generated area: The world generator generates up to 500 meters from places any player has visited (usually abour 350 meters). This is much bigger area than what gets revelead on the minimap.
- Location: Special places like rune stones, dungeon entrances or abandoned houses that are placed to the world by the world generator.
- Zone: The world is split to tiles of 64 m x 64 m size. This is the granularity of the world generation. See https://valheim.fandom.com/wiki/Zones for more info.

## Resetting zones

1. Removes all objects from a zone (including player placed structures).
2. If the zone has a location, marks the location as unplaced (to allow redistributing it).
3. Marks the zone as ungenerated. Visiting the zone will regenerate like it were at start of the game.
4. Locations are not automatically redistributed. Use "redistribute" command (otherwise you get the same locations as before).
5. Visiting zones will regenerate them.

Portals in the loaded area won't be automatically disconnected but relogging fixes that.

## Generating zones

1. Calls the generating function for each zone.

## Adding locations

1. Runs a modified genloc command which allows redistributing unplaced locations to already generated areas.
2. Skip zones that didn't get a new location.
3. For each redistributed location, destroy everything within the location exterior radius and place the location to the world.

## Resetting chests

1. Gets all chests from the save file. Filters chests that are empty (looted or loot not rolled yet) or include a wrong item (to not replace manually put items).
2. If the chest is in a loaded area, remove all items and roll loot.
3. Otherwise remove all items and set the chest as "not rolled yet" so that the loot is rolled when the chest is loaded. This is done directly by modifying the save file without actually loading the chest.

## Counting biomes

1. Checks from the map which biome is at each position.

The map returns the exact biome information. This is slightly different than what the minimap shows as it will show the average biome.

## Counting/listing/removing objects

1. Uses the available id list (same what spawn command uses) to resolve wildcards.
2. Directly counts objects from the save file data without loading them to the world.
3. Biome is the exact biome like with count biomes.

This can result in objects near biome edges showing "wrong" biome because the generation code uses the average biome.

## Changing time/day

1. Changes the world time like the skiptime command.
2. Changes the time data values of objects to match the new time.
3. When moving the time forward, this prevents the time skip (plants don't grow instantly, smelters don't work faster, etc.).
4. When moving the time backward, this prevents things getting stuck (plants, smelters, enemy spawning, etc. won't progress until back to the original time).

Following data values are updated if their value is not 0:

- For general enemy spawning, all data values of the zone control objects are updated. Each of these data values affect spawning of a one enemy type.
- "spawntime" for offspring growth timer and also for ship build timer (but unlikely to use the command when constructing a ship).
- "SpawnTime" for item drop despawn timer.
- "lastTime" for beehive and fireplace progressing.
- "StartTime" for cooking, smelting and fermenting.
- "timeOfDeath" is not included as this is for coropses and gravestones. The value doesn't seem to be currently used at all.
- "alive_time" for spawn point timers.
- "spawn_time" for loot spawners (not used in the game at the moment).
- "picked_time" for pickable respawn timers.
- "plantTime" for growth timers.
- "pregrant" for breeding timers.
- "TameLastFeeding" for animal hunger timer.

Affected data values can be configured but recommended to keep them as it is.

# Changelog

- v1.14
	- Changes the `vegetation_add` and `vegetation_reset` to work with all vegetation when ids are not given.

- v1.13
	- Improves command output.
	- Merges EVA upgrade commands to a single command.
	- Fixes EVA upgrade command failintg to start.

- v1.12
	- Adds a new command `backup` to backup the world.
	- Adds a new command `save_disable` and `save_enable` to disable or enable world saving.
	- Adds support for automatic player position when executing commands on dedicated servers.
	- Improves output when executing commands on dedicated servers.
	- Fixes `start` and `stop` commands not being sent to the server.

- v1.11
	- Adds support for executing commands on dedicated servers.
	- Adds a new setting to authorize certain users to execute commands (by default all admins can execute).
	- Adds a new command `locations_reset` to destroy and place locations at the same place.
	- Adds a new command `locations_remove` to destroy placed and unplaced locations.
	- Adds new upgrade types for Epic Valheim Additions (`EVA_1.3+1.4`, `EVA_1.3+1.4_locations_only` and `EVA_1.4`).
	- Adds a better player detection to avoid destroying players on multiplayer.
	- Adds a new setting for preventing double ZNetViews.
	- Adds distance to the map coordinates.
	- Adds new settings for map and minimap coordinates.
	- Adds better autocomplete when using Server Devcommands mod.
	- Renames most commands to be more intuitive to use.
	- Renames the parameter `force` to `start`.
	- Adds a new parameter `force` to disable the player base detection.
	- Removes commands `reveal_map`, `hide_map` and `remove_pins` as obsolete (moved to Server Devcommands mod).
	- Fixes map and minimap coordinates conflicting with other mods.

- v1.10
	- Adds a new upgrade command for mountain caves.

- v1.9
	- Fixed change_time and change_day being able to put negative timestamp to objects.
	- Fixed change_time, change_day, set_time and set_day being able to set the time negative.
	- Fixed set_time and set_day setting object timestamps to the given value instead of using the time difference like change_time and change_day to.
	- Added better icon. Thanks Azumatt!

- v1.8
	- Fixed reroll_chests not working without giving any valid items.
	- Added reroll_chests to support all chests.
	- Added "looted" flag to reroll_chests to also reroll empty chests.

- v1.7
	- Fixed graceful object removing being bit too graceful (not working in all situations).

- v1.6
	- Fixed upgrade tarpits command needing to be used twice.

- v1.5
	- Fixed non-player placed items also counting as player bases (for example in Fuling villages).
	- Added autocomplete to place_locations command.
	- Added new commands to set specific world time (instead of a relative value).
	- More graceful object removing.
	- Changed parameter "includebases" to "safezones" which allows setting the player base safe zone size (instead of only enable/disable).
	- Chests added to the default list of major structures (won't affect existing configs).
	- Changed player base safe zone value handling (0 is disable, 1 is current zone, etc.)
	- Renamed player base safe zone size config setting to ensure the new default value is used.
	- Fixed generate command targeting only generated zones instead of ungenerated zones.
	- Fixed generate command forcing player base detection (now disables it).
	- Fixed start command causing the next command to execute instantly if used without any pending operations.

- v1.4
	- Operation split to instant and delayed operations. Delayed operations print some initial output but require start command to execute.
	- Added setting to force start operations without having to use the start command. Added parameter to force start commands.
	- Removed query command as obsolete.
	- Removed setting to prevent loaded areas being destroyed (after all testing seems to work fine).
	- Removed setting about location placement clearing the area (can be given as a flag parameter).
	- Added general argument system (same set of basic arguments for most commands).
	- Merged all destroy commands to a single command.
	- Merged all generate commands to a single command.
	- Merged object counting commands to a single command.
	- Added a new command to count biomes.
	- Added new command to change world time (while also updating objects).
	- Added new command to list each object and their position.
	- Added automatic player base detection.
	- Count, remove and list objects support wildcards.
	- Added setting to enable/disable verbose mode.
	- Added command to toggle the verbose mode.
	- Improved output.

- v1.3
	- Regeneration commands renamed to destroy to make the effect more clear.
	- Destroy all command now requires biomes as a parameter (replacing the config setting and making harder to accidentally nuke the whole world).
	- New commands for generating areas without having to physically go there.
	- New command for counting amount of objects.
	- New command for counting all objects.
	- New command to remove objects.
	- New command to place any unplaced locations (replacing one of the config settings).
	- New command to reroll any chest.
	- New command to explore a part of the map.
	- New command to hide a part of the map.
	- New command to remove pins from a part of the map.
	- Upgrade command now has a second parameter to determine the operation.
	- New upgrade command for generating onion seeds.
	- New upgrade command for regenerating Mistlands.
	- New setting to disable area clearing when placing locations.
	- Improved area clearing when placing locations.
	- Added setting for verbose mode, default output is now very minimal to not include any spoilers.
	- Improved output for all commands.
	- New setting to filter zones too close to the player (effectively creating a safe zone around the player).
	- Many settings removed as obsolete.

- v1.2
	- Added coordinates to minimaps.
	- Added option to also regenerate loaded zones,
	- Renamed nuke command.
	- Added new commands for a targeted regeneration.

- v1.1
	- Commands now work from the chat.
	- Console is now enabled by default.

- v1.0
	- Initial release
