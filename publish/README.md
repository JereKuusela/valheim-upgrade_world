# Upgrade world

This single player mod allows regenerating or adding new content to already explored areas.

For dedicated servers, open the world in single player.

Always back up your world before making any changes!

# Things you can do

- Place tar pits on already explored areas.
- Reroll chest loots to have onion seeds on already explored areas.
- Completely regenerate Mistlands.
- Remove any object from the game world.
- Generate the whole world without having to physically go anywhere.
- Explore/hide parts of the map (unfortunately only for the current character).

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim).
2. Download the latest zip.
3. Extract it in the \<GameDirectory\>\BepInEx\plugins\ folder.
4. Optionally also install the [Configuration manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/tag/v16.4).

# Quick instructions:

1. Back up your world!
2. Install the mod.
3. Load the world.
4. Open console with F5. write upgrade and use tab to cycle through available options.
5. Wait.
6. Check that your bases are ok (especially if in Plains).
7. If not, restore back up. read more detailed instructions and go back to the first step.
8. Uninstall the mod.
9. Enjoy the new stuff!

This mod also adds current coordinates (and zone index) to the minimaps which should help using the commands and setting up the config.

# Parameters

Effective usage of this tool requires understanding the general parameters that are used to filter out affected zones and entities. Following parameters are available to most commands:

- Biome names to only include those biomes. If not given, all biomes are included. Available options are: "AshLands", "BlackForest", "DeepNorth", "Meadows", "Mistlands", "Mountain", "Ocean", "Plains" and "Swamp".
- Minimum and maximum distance as a range "min-max". Either of the values can be omitted for either "min" or "-max".
- Center position as two parameters "x" and "z" (y coordinate is used the altitude).
- Flag "noedges" to only include zones that have included biomes in all of its corners. Without the flag, it's enough if just one of the corners is in the included biomes.
- Flag "ignorebase" to skip automatic player base detection. Can be permanently turned on from the config.
- Flag "zones" to change distance and position parameters to be zone based. "x" and "z" are zone indices. "min" and "max" determine how many adjacent zones are included.
- Flag "force" to automatically execute the command. Can be permanently turned on from the config.

Examples:
- command mistlands: Affects zones which have Mistlands biome at any of their corners.
- command 5000 0 0: Affects zones which are 5000 meters away from the world center.
- command 3000-5000 0 0: Affects zones which are from 3000 to 5000 meters away from the world center.
- command 3 -3 zones ignorebase: Affects the zone at indices 3,-3 while ignoring any nearby player base structures.
- command zones: Affects the current zone at the player's position.
- command blackforest ocean -1000 noedges: Affects zones that only have Black Forest or Ocean at their corners and that are up to 1000 meters away from the player.

# Commands

Overview of available commands (remember that tab key can be used for autocomplete / cycle through options):

## upgrade [operation] [...args]

Performs a predefined upgrade operation. Available operations are tarpits, onions and mistlands.

Examples
- upgrade tarpits: Places tar pits to already explored areas.
- upgrade onions: Rerolls already generated and unlooted mountain chests.
- upgrade mistlands: Fully regenerates mistlands biomes.
- upgrade old_mistlands: Fully regenerates mistlands biomes with the legacy content (webs, etc.).

## destroy [..args]

Destroys zones which allows the world generator to regenerate them when visited.

Examples:
- destroy mistlands: Destryoing a biome.
- destroy 5000 0 0 : Destroying areas after 5000 meters from the world center.
- destroy 3 -3 zones ignorebase: Destroy a single zone at indices 3,-3 to fix any local issues.

## generate [..args]

Generates zones which allows pregenerating the world without having to move there physically.

Examples:
- generate: To generate the entire world (takes hours) and then use count_entities command to check how many of each entity exists.
- generate: To generate the entire world (takes hours) and then use remove_entities for modifications.

## place_locations [noclearing] [...location_ids] [...args]

Distributes unplaced locations to already explored areas and then places them to the world. With "noclearing" flag, the area under placed location is not cleared of existing entities.

Normally this command (or the similar genloc command) won't do anything because the locations will be generated the same way. However if a new update adds new locations or the world generating otherwise changes this causes changed to the world.

## reroll_chests [chest name] [...item_ids] [...args]

Rerolls contents of a given treasure chest (use tab key to cycle through available treasure chests). Empty (looted) chests won't be rerolled.

Item ids can be used to detect and prevent rerolling chests which players are using to store items.

Example:
- reroll_chests TreasureChest_mountains Amber Coins AmberPearl Ruby Obsidian ArrowFrost OnionSeeds: Rerolls mountain treasure chests which only have naturally occurring items.

## count_biomes [frequency=100] [...args]

Counts amounts of biomes. Frequency determines in meters how often the biome is checked. Result it also printed to the player.log file.

Example:
- count_biomes 5000 0 0: Counts only biomes after 5000 meters from the world center.

## count_entities [showzero] [...ids] [...args]

Counts amounts of given entities. If no ids given then counts all entities. All entities are listed with the flag "showzero". Result is also printed to the player.log file.

Wildcards are also supported.

Examples:
- count_entities Spawner_\*: Counts all creature spawnpoints.
- count_entities \*\_wall\*\_: Counts all walls structures.


## list_entities [...ids] [...args]

Lists given entities showing their position and biome. Result is also printed to the player.log file.

## remove_entities [...ids] [...args]

Removes entities. Recommended to use count_entities to verify the parameters.

## reveal_position [x] [y] [distance=0]

Explores the map at a given position to a given distance.

## hide_position [x] [y] [distance=0]

Hides the map at a given position to a given distance.

## remove_pins [x] [y] [distance=0]

Removes map from the map at a given position to a given distance.

## change_time [seconds]

Experimental. Changes the world time by real life seconds while also updating entities so that they keep their progression. This ensures that enemy spawning and structures won't break when going back in the time.

Verbose mode prints how many data entries were affected.

## change_day [day]

Experimental. Changes the world time by in-game days while also updating entities so that they keep their progression. This ensures that enemy spawning and structures won't break when going back in the time.

Verbose mode prints how many data entries were affected.

## remove_pins [x] [y] [distance=0]

Removes map from the map at a given position to a given distance.

## redistribute

Runs the "genloc" command without needing devcommands enabled. This can be used to redistribute locations after destroying zones.

## set_vegetation [disable] [...ids]

Enables vegetation for the world generator, affecting any forced (with "generate" command) or natural generation. Disables vegetation with the "disable" flag.

Tab key can be used to autocomplete available prefab ids.

Examples:
- set_vegetation disable BlueberryBush: Disables generation of blueberry bushes.
- set_vegetation vertical_web horizontal_web tunnel_web: Enables webs to generate in Mistlands.

## reset_vegetation

Revert vegetation generation back to the original.


## start

Zone based commands don't exeute instantly but instead print the amount of zones being affected. This command can be then used to start executing.

## stop

Stops execution of commands and removes any pending commands.

## verbose [off]

Enables the verbose mode (or disabled with "off" flag).

# Configuration

The config can be found in the \<GameDirectory\>\BepInEx\config\ folder after the first start up.

Filtering options in the config:
- Safe zone items: List of items used for the automatic player base detection. By default includes structures that have the player base effect.
- How many adjacent zones are included in the safe zone: Size of the player base protection. Default value 1 means 3x3 zones per player base structure to ensure proper coverage. Value -1 can be used to disable the player base detection.
- Safe distance around the player: Excludes zones that are too close to the players (in meters). All commands allow specifying the minimum distance so using this setting is not really needed.
- Custom points: Coordinates and distances to exclude zones. Format: x1,z1,min1,max1|x2,z2,min2,max2|...

Other settings are:

- Verbose output: Prints more output which gives a better understanding how the mod operators. However this can spoil things of your world.
- Automatic start: Starts commands automatically without having to use the start command. This allows using the commands more easily but can lead to more mistakes.
- Operations per update: How many zones are destroyed per Unity update. Can be useful if destroying large parts of a world, but recommended to keep it as it is.

Examples:

- Setting "300,500,500,0|1000,2000,500,0" to custom points would protect areas at coordinates 300,500 and 1000,2000 within 500 meters.
- Setting "300,500,0,100" to custom points would only operate near coordinates 300,500 witin 100 meters.

# Removing entities

If you already know the entity id, use "count id" to ensure you are only removing what needed. If too many entities are returned, use a shorter distance until you get the right amount. Then use the remove command to remove them.

If you don't know the id, use "count_all" with a right distance to find the entity. Then use remove command to remove it.

# How it works

## Place locations

1. Runs a modified genloc command which allows redistributing unplaced locations to already generated areas.
2. Skip zones that didn't get a new location.
3. For each redistributed location, destroy everything within the location exterior radius and place the location to the world.

## Reroll chests

1. Gets all chests from the save file. Filters chests that are empty (looted or loot not rolled yet) or include a wrong item (to not replace manually put items).
2. If the chest is in a loaded area, remove all items and roll loot.
3. Otherwise remove all items and set the chest as "not rolled yet" so that the loot is rolled when the chest is loaded. This is done directly by modifying the save file without actually loading the chest.

## Destroy

1. Removes all objects from a zone (including player placed structures).
2. If the zone has a location, marks the location as unplaced (to allow redistributing it).
3. Marks the zone as ungenerated.  Visiting the zone will regenerate like it were at start of the game.
4. Locations are not automatically redistributed. Use "redistribute" command (otherwise you get the same locations as before).
5. Visiting zones will regenerate them.

Portals in the loaded area won't be automatically disconnected but relogging fixes that.

## Generate:

Calls the generating function for each zone.

## Change time/day

The main issue with changing the time is that many entities store the current time to their data. This causes issue when going back in the time as many things freeze until the time has passed back to the original. For example preventing enemies from spawning.

So after changing the time, the command also updates the following data values (if their value is not 0):
- For general enemy spawning, all data values of the zone control entity are updated. Each of these data values affect spawning of a one enemy type.
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


# Glossary

- Generated area: The world generator generates up to 500 meters from places any player has visited. This is much bigger area than what gets revelead on the minimap.
- Location: Special places like rune stones, dungeon entrances or abandoned houses that are placed to the world by the world generator.
- Zone: The world is split to tiles of 64 m x 64 m size. This is the granularity of the world generation. See https://valheim.fandom.com/wiki/Zones for more info.

# Changelog

- v1.4.0:
	- Operation split to instant and delayed operations. Delayed operations print some initial output but require start command to execute.
	- Added setting to force start operations without having to use the start command. Added parameter to force start commands.
	- Removed query command as obsolete.
	- Removed setting to prevent loaded areas being destroyed (after all testing seems to work fine).
	- Removed setting about location placement clearing the area (can be given as a flag parameter).
	- Added general argument system (same set of basic arguments for most commands).
	- Merged all destroy commands to a single command.
	- Merged all generate commands to a single command.
	- Merged entity counting commands to a single command.
	- Added a new command to count biomes.
	- Added new command to change world time (while also updating entities).
	- Added new command to list each entity and their position.
	- Added automatic player base detection.
	- Count, remove and list entities support wildcards.
	- Added setting to enable/disable verbose mode.

- v1.3.0:
	- Regeneration commands renamed to destroy to make the effect more clear.
	- Destroy all command now requires biomes as a parameter (replacing the config setting and making harder to accidentally nuke the whole world).
	- New commands for generating areas without having to physically go there.
	- New command for counting amount of entities.
	- New command for counting all entities.
	- New command to remove entities.
	- New command to place any unplaced locations (replacing one of the config settings).
	- New command to reroll any chest.
	- New command to explore a part of the map.
	- New command to hide a part of the map.
	- New command to remove pins from a part of the map.
	- Upgrade command now has a second parameter to determine the operation.
	- New upgrade command for generating onion seeds.
	- New upgrade command for regenerating Mistlands.
	- New setting to disable area clearing when placing locations..
	- Improved area clearing when placing locations.
	- Added setting for verbose mode, default output is now very minimal to not include any spoilers.
	- Improved output for all commands.
	- New setting to filter zones too close to the player (effectively creating a safe zone around the player).
	- Many settings removed as obsolete.

- v1.2.0:
	- Added coordinates to minimaps.
	- Added option to also regenerate loaded zones,
	- Renamed nuke command.
	- Added new commands for a targeted regeneration.

- v1.1.0:
	- Commands now work from the chat.
	- Console is now enabled by default.

- v1.0.0:
	- Initial release