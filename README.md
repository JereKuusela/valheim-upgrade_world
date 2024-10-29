# Upgrade World

This tool includes console commands to add new content to already explored areas (and more).

Always back up your world before making any changes!

Install on the admin client and on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

## Things you can do

- Spawn tar pits and mountain caves on already explored areas.
- Reroll chest loots to have onion seeds on already explored areas.
- Completely regenerate Mistlands.
- Remove any object from the game world.
- Generate the whole world without having to physically go anywhere.
- Manually regenerate areas or dungeons.

## Quick instructions

1. Back up your world!
2. Install the mod and load the world.
3. Open console with F5, write `upgrade` and use tab to cycle through available options.
4. Write `start` to execute and wait until tue operation completes.
5. Verify that your bases are ok (should be because of the automatic base detection).
6. If not, restore back up. read more detailed instructions and go back to the first step.
7. Uninstall the mod and enjoy the new stuff!

This mod also adds current coordinates (and zone index) to the minimaps which should help using the commands and setting up the config.

Note: The default base detection is very conservative. Single workbenches, campfires, etc. will exclude a significant area around them, unless configured otherwise.

[<img width="80px" style="margin-bottom: -4" src="https://cdn.prod.website-files.com/6257adef93867e50d84d30e2/636e0b5493894cf60b300587_full_logo_white_RGB.svg">](https://discord.gg/VFRJcPwUdm) for help and examples.

## Upgrade operations

For `upgrade` command:

- `ashlands`: Fully regenerates ashlands biomes and nearby water areas. Terrain is automatically updated by the base game and not affected by this operation.
- `bogwitch`: Adds possible spawn locations to already explored areas.
- `deepnorth`: Fully regenerates ashlands biomes and nearby water areas. Terrain is automatically updated by the base game and not affected by this operation.
- `hildir`: Adds new locations to already explored areas.
- `mountain_caves`: Adds mountain caves to already explored areas.
- `tarpits`: Adds tar pits to already explored areas.
- `onions`: Rerolls already generated and unlooted mountain chests.
- `mistlands`: Fully regenerates mistlands biomes. Terrain is automatically updated by the base game and not affected by this operation.
- `mistlands_worldgen`: Upgrades biome distribution to the Mistlands version. Biomes will change in the outer areas. Areas past 5900 meters will be reseted. Rivers will move everywhere in the world and can destroy bases (even with the player base protection).
- `hh_worldgen`: Downgrades biome distribution to the Heart & Home version.
- `legacy_worldgen`: Downgrades biome distribution to the Early access release version.

## Dedicated servers

By default, all admins can execute commands of this mod.

If you wish to restrict this, edit the config file and add Steam IDs to the root users setting (separated by ,). After that only those users can execute commands.

## Parameters

Most commands allow fine-tuning the affected area. Following parameters are available:

- `biomes=biome1,biome2,...`: Only includes given biomes. If not given, all biomes are included. Available options are: "AshLands", "BlackForest", "DeepNorth", "Meadows", "Mistlands", "Mountain", "Ocean", "Plains" and "Swamp".
- `chance=percentage`: Makes a single operation to be applied randomly.
- `clear=meters`: Overrides the cleared radius when using `locations_remove`.
- `count=min-max`: Filters objects by their amount. Only applies to `objects_count`.
- `data=key,value,type`: Sets object data. Type is only needed if the key doesn't already exist. Only applies to `objects_edit`. Multiple data values can be set at once. Use `_` instead of space bars.
- `distance=min-max`: Short-hand for setting both distances.
- `filter=key,value,includeMissing`: Filters object by data value. Third parameter must be truthy to include objects that don't have the data value set. Only applies to `objects_*` commands. Multiple filters can be set at once. Use `_` instead of space bars.
- `force`: Disables player base detection (same as `safeZones=0`).
- `level=min-max`: Filters objects by their creature level. Only applies to `objects_*` commands.
- `limit=amount`: Limits the amount of affected objects. Only applies to `objects_*` commands.
- `location=id`: Filters objects by their location. Only applies to `objects_*` commands.
- `log`: Object commands print to the log file instead of the console.
- `max=distance` or `maxDistance=distance`: Maximum distance from the center. Meters for `pos` and adjacent zones for `zone`.
- `min=distance` or `minDistance=distance`: Minimum distance from the center. Meters for `pos` and adjacent zones for `zone`.
- `noEdges`: Only include zones that have included biomes in all of its corners. Without the flag, it's enough if just one of the corners is in the included biomes.
- `pin`: Shows the result on the map. Requires Server Devcommands mod.
- `pos=x,z`: Position of the center. Default value is the world center. Default distance is all of the map.
- `print=key,type`: Prints object data. Type is only needed if the same key is used for multiple types. Only applies to `objects_list` command. Multiple values can be printed at once.
- `safeZones=distance`: Set safe zone size of major structures (0 to disable). Default value 2 is defined in the config. List of major structures is also defined in the config.
- `start`: Automatically executes the command without having to use `start`. Can be permanently turned on from the config.
- `terrain=meters`: Resets terrain around the object when using `vegetation_add` or `vegetation_reset`.
- `type=type1,type2,...`: Only includes objects that have all of the given types.
  - If the parameter is given multiple times, then objects that have any of the given type sets are included.
  - For example `type=Container,Ship` only includes ships with a chest.
  - For example `type=Container type=Ship` includes all ships and all chests.
- `zone=x,z`: Position of the center zone. Can't be used with `pos`. Default distance is the single zone.

## Commands

Overview of available commands (remember that tab key can be used for autocomplete / cycle through options):

- `backup`: Saves the game with a timestamped file name.
- `biomes_count [precision] [...args]`: Counts biomes by sampling points with a given precision (meters). Result is also printed to the player.log file.
- `chests_reset [chest name] [looted] [...item_ids] [...args]`: Rerolls contents of a given treasure chest (use tab key to cycle through available treasure chests). Without chest name, all treasure chests are rerolled. Empty (looted) chests are only rerolled with `looted` flag. Item ids can be used to detect and prevent rerolling chests which players are using to store items. `chance` determines how many of the chests are reseted.
- `chests_search [id1,id2,...] [...args]`: Searches chests and stands for given items.
- `clean_chests [...args]`: Removes missing objects from chests.
- `clean_dungeons [...args]`: Optimizes old dungeons.
- `clean_health [...args]`: Removes excess health data from creatures and structures.
  - This removes the current health if it equals the maximum health. When current health is missing, the game uses the maximum health as the default value.
  - This will affect creatures that have taken damage but healed back to full health.
  - This will affect structures that have taken damage but repaired back to full health.
  - Note: May not work correctly if another mod modifies the maximum health.
- `clean_locations [...args]`: Removes missing locations from the world and from the location database.
- `clean_objects [...args]`: Removes missing objects from the world.
- `clean_spawns [...args]`: Removes timestamps from the spawn system.
  - Note: This resets the spawn delays, which may cause creatures to respawn immediately.
- `clean_stands [...args]`: Removes missing objects from armor and item stands.
- `locations_add [id1,id2,...] [...args]`: Adds locations to already explored areas. `chance` determines how many of the locations are added.
- `locations_list [id1,id2,...] [...args]`: Lists locations showing their position and biome.
- `location_register [id] [x,z,y=player position]`: Registers a location to the database (without spawning it). If the location already exists, then its position is automatically used.
- `locations_remove [id1,id2,...] [...args]`: Removes locations and prevents new ones from appearing (until a command like `genloc` or `locations_add` is used). `chance` determines how many of the locations are removed.
- `locations_reset [id1,id2,...] [...args]`: Resets locations by removing them and then placing them at the same position. Dungeons which have a random rotation will also get a new layout. `chance` determines how many of the locations are reseted.
- `locations_swap [new id,id1,id2,...] [...args]`: Replaces locations with a new one.
- `objects_count [id1,id2,...] [...args]`: Counts objects. If no ids given then counts all objects. Parameter `count=1` can be used to exclude non-existing objects.
- `objects_edit [id1,id2,...] [data=key,value,type] [...args]`: Edits data of objects.
- `objects_list [id1,id2,...] [print=key,type] [...args]`: Lists objects showing their position and biome. `print` allows displaying custom data.
- `objects_refresh [id1,id2,...] [...args]`: Refresh/respawns objects.
- `objects_remove [id1,id2,...] [...args]`: Removes objects. Recommended to use `objects_count` to check that you don't remove too much.
- `objects_swap [new id,id1,id2,...] [...args]`: Replaces objects with a new one.
- `save_disable`: Disables world saving. But still a good idea to make backups.
- `save_enable`: Enables world saving.
- `start`: Most commands don't execute instantly but instead print the zones being affected. This command can be then used to start executing.
- `stop`: Stops the current execution and clears any pending command.
- `temple_gen`: Updates the Start Temple boss stone positions.
- `time_change [seconds]`: Changes the world time and updates object timestamps.
- `time_change_day [days]`: Changes the world time and updates object timestamps.
- `time_set [seconds]`: Sets the world time and updates object timestamps.
- `time_set_day [days]`: Sets the world time and updates object timestamps.
- `upgrade [operation] [...args]`: Short-hand for using common operations (mainly to add new content).
- `vegetation_add [id1,id2,...] [...args]`: Adds vegetation to generated areas. If ids are not given, adds every vegetation.
- `vegetation_remove [id1,id2,...] [...args]`: Removes vegetation from generated areas. If ids are not given, removes every vegetation.
- `vegetation_reset [id1,id2,...] [...args]`: Removes and adds vegetation to generated areas. If ids are not given, resets every vegetation.
- `verbose`: Toggles the verbose mode which prints more information when enabled. Can also be toggled from the config.
- `world_clean`: Combines all `clean_*` commands to fully clean the world.
- `world_gen [legacy/hh/mistlands] [start]`: Sets the world generation version.
  - Biomes will change in the outer areas.
  - Rivers will move everywhere in the world and can destroy bases (even with the player base protection).
- `world_reset [...args]`: Resets locations and zones.
- `zones_generate [...args]`: Pre-generates areas without having to visit them.
- `zones_reset [...args]`: Destroys areas making them ungenerated. These areas will be generated when visited. Can be also used to reduce save file size. `chance` determines how many of the zones are reseted.
- `zones_restore [...args]`: Adds missing zone control objects (responsible for random spawns).

Examples:

- `biomes_count 100 min=5000`: Counts only biomes after 5000 meters from the world center by checking the biom every 100 meters.
- `chests_reset TreasureChest_mountains Amber Coins AmberPearl Ruby Obsidian ArrowFrost OnionSeeds`: Rerolls mountain treasure chests which only have naturally occurring items.
- `chests_reset looted min=1500`: Resets all chests which are 1500 meters away from the world center.
- `locations_remove Meteorite`: Removes all flametal ores.
- `locations_reset SunkenCrypt4,Crypt2,Crypt3,Crypt4,MountainCave02,TrollCave02`: To regenerate dungeons. Some entraces will randomly rotate which will also randomize the dungeon layout.
- `objects_count Spawner_\*`: Counts all creature spawnpoints.
- `objects_count \*\_wall\*\_`: Counts all walls structures.
- `objects_list VikingShip,Karve,Raft`: Lists coordinates of all ships.
- `objects_remove FirTree chance=33`: Removes 33% of Fir Trees.
- `objects_remove StatueCorgi`: Removes all corgi statues.
- `objects_remove Spawner_\* max=200`: Removes all creature spawnpoints within 200 meters.
- `vegetation_reset biome=Meadows force`: Resets all vegetation in Meadows, including areas with player bases
- `zones_generate`: To generate the entire world (takes hours) and then use `objects_count` command to check how many of each object exists.
- `zones_generate`: To generate the entire world (takes hours) and then use `objects_remove` for modifications.
- `zones_reset biomes=mistlands`: Destroying a biome.
- `zones_reset min=5000`: Destroying areas after 5000 meters from the world center.
- `zones_reset zone=3,-3 safeZones=0`: Destroy a single zone at indices 3,-3 to fix any local issues.

## Configuration

The config can be found in the \<GameDirectory\>\BepInEx\config\upgrade_world.cfg folder after the first start up.

Filtering options in the config:

- Safe zone items: List of items placed by players that are used for the automatic player base detection. By default includes structures that have the player base effect.
- Safe zone objects: List of items that are used for the automatic player base detection. By default includes the tombstone.
- How many adjacent zones are included in the safe zone: Size of the player base protection. Default value 1 means 3x3 zones per player base structure to ensure proper coverage. Value -1 can be used to disable the player base detection.

Other settings are:

- Verbose output: Prints more output which gives a better understanding how the mod operators. However this can spoil things of your world.
- Automatic start: Starts commands automatically without having to use the start command. This allows using the commands more easily but can lead to more mistakes.
- Disable automatic genloc: After new content updates, Valheim automatically redistributes unplaced locations with genloc command. This can mess up custom worlds with manually defined locations.
- Operations per update: How many zones are destroyed per Unity update. Can be useful if destroying large parts of a world, but recommended to keep it as it is.
- Root users: SteamIds that are allowed to execute commands (-1 for the dedicated server). If not set, all admins can use the commands.
- Operation delay: Milliseconds between each command. Prevents lots of small operations overloading the dedicated server.
- World size: Max radius for operations (if using a mod to change the world size or need to affect areas outside the play area).

## How it works

### Glossary

- Generated area: The world generator generates up to 500 meters from places any player has visited (usually abour 350 meters). This is much bigger area than what gets revelead on the minimap.
- Location: Special places like rune stones, dungeon entrances or abandoned houses that are spawned to the world by the world generator.
- Zone: The world is split to tiles of 64 m x 64 m size. This is the granularity of the world generation. See <https://valheim.fandom.com/wiki/Zones> for more info.

### Resetting zones

1. Removes all objects from a zone (including player placed structures).
2. If the zone has a location, marks the location as not spawned (to allow redistributing it).
3. Marks the zone as ungenerated. Visiting the zone will regenerate like it were at start of the game.
4. Locations are not automatically redistributed. Use "genloc" command (otherwise you get the same locations as before).
5. Visiting zones will regenerate them.

Portals in the loaded area won't be automatically disconnected but relogging fixes that.

### Generating zones

1. Calls the generating function for each zone.

### Adding locations

1. Runs a modified genloc command which allows redistributing not spawned locations to already generated areas.
2. Skip zones that didn't get a new location.
3. For each redistributed location, destroy everything within the location exterior radius and spawn the location to the world.

### Resetting chests

1. Gets all chests from the save file. Filters chests that are empty (looted or loot not rolled yet) or include a wrong item (to not replace manually put items).
2. If the chest is in a loaded area, remove all items and roll loot.
3. Otherwise remove all items and set the chest as "not rolled yet" so that the loot is rolled when the chest is loaded. This is done directly by modifying the save file without actually loading the chest.

### Counting biomes

1. Checks from the map which biome is at each position.

The map returns the exact biome information. This is slightly different than what the minimap shows as it will show the average biome.

### Counting/listing/removing objects

1. Uses the available id list (same what spawn command uses) to resolve wildcards.
2. Directly counts objects from the save file data without loading them to the world.
3. Biome is the exact biome like with count biomes.

This can result in objects near biome edges showing "wrong" biome because the generation code uses the average biome.

### Changing time/day

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
