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

# Features

Overview of available commands (remember that tab key can be used for autocomplete / cycle through options):

- upgrade [operation]: Performs a predefined upgrade operation. Available operations are tarpits, onions and mistlands.
- place_locations [location1, location2, ...]: Distributes locations to already explored areas (used by the tarpits operation). Also affected by the config.
- reroll_chests [chest name] [item1, item2, ...]: Rerolls contents of a given chest, if they only have given items (all chests if no items specified).
- destroy_biomes [biome1, biome2, ...] [includeEdges=true]: Destroys zones in given biomes (\* for all) which allows the world generator to regenerate them with new content. true/false can be used to determine if zones with multiple biomes are included. Also affected by the config.
- destroy_position [x] [y] [distance=0]: Destroys zones at a given coordinates and distance.
- destroy_zones [x] [y] [adjacent=0]: Destroys zones at a given zone index and adjacent zones.
- generate_biomes [biome1, biome2, ...] [includeEdges=true]: Generates zones in given biomes (\* for all) without having to physically move there. true/false can be used to determine if zones with multiple biomes are included. Also affected by the config.
- generate_position [x] [y] [distance=0]: Generates zones at a given coordinates and distance.
- generate_zones [x] [y] [adjacent=0]: Generates zones at a given zone index and adjacent zones.
- count_all [distance]: Counts all entities within a given distance (use 0 for infinite).
- count [id1, id2, id3] [distance=0]: Counts given entities within a given distance (use 0 for infinite).
- remove [id1, id2, id3] [distance=0]: Removes given entities within a given distance (use 0 for infinite).
- redistribute: Runs the "genloc" command for locations defined by the config. This is needed to redistribute locations after destroying zones.
- stop: Stops execution of the current operation.  Can be useful if it takes too long some reason.
- query: Prints how many zones would get operated with the current config.

This mod also adds current coordinates (and zone index) to the minimaps which should help using the commands and setting up the config.

# Configuration

Zones affected by "place_locations", "destroy_biomes" and "generate_biomes" commands can be filtered to exclude some parts of the world (for example your base).

The config can be found in the \<GameDirectory\>\BepInEx\config\ folder after the first start up.

Filtering options in the config:
- Safe distance around the player: Only zones that are fully outside the given distance of the player.
- Custom points: Coordinates and distances to create excluded areas. Format: x1,z1,min1,max1|x2,z2,min2,max2|...

Other settings are:

- Verbose output: Prints more output which gives a better understanding how the mod operators. However this can spoil things of your world.
- Clear location areas: Whether location placement destroyes anything under the location. Recommended to keep this true to prevent things clipping with each other.
- Destroy loaded areas: Whether destroy command affects currently loaded areas. Destroying already loaded areas has more risks which is why this is false by default. However no issues have been found by testing.
- Operations per update: How many zones are destroyed per Unity update. Can be useful if destroying large parts of a world, but recommended to keep it as it is.

Examples:

- Setting "300,500,500,0|1000,2000,500,0" to custom points would protect areas at coordinates 300,500 and 1000,2000 within 500 meters.
- Setting "300,500,0,100" to custom points would only operate near coordinates 300,500 witin 100 meters.

# Removing entities

If you already know the entity id, use "count id" to ensure you are only removing what needed. If too many entities are returned, use a shorter distance until you get the right amount. Then use the remove command to remove them.

If you don't know the id, use "count_all" with a right distance to find the entity. Then use remove command to remove it.

# How it works

Place locations:

1. Runs a modified genloc command which allows redistributing unplaced locations to already generated areas.
2. Skip zones that didn't get a new location.
3. For each redistributed location, destroy everything within the location exterior radius and place the location to the world.

Reroll chests:

1. Gets all chests from the save file. Filters chests that are empty (looted or loot not rolled yet) or include a wrong item (to not replace manually put items).
2. If the chest is in a loaded area, remove all items and roll loot.
3. Otherwise remove all items and set the chest as "not rolled yet" so that the loot is rolled when the chest is loaded. This is done directly by modifying the save file without actually loading the chest.

Destroy:

For technical reasons, currently loaded areas are not included (about 200 meters around the player). This can be overridden by config but contains more risks.

1. Removes all objects from a zone (including player placed structures).
2. If the zone has a location, marks the location as unplaced (to allow redistributing it).
3. Marks the zone as ungenerated.  Visiting the zone will regenerate like it were at start of the game.
4. Locations are not automatically redistributed. Use "redistribute" command (otherwise you get the same locations as before).
5. Visiting zones will regenerate them.

Portals in the loaded area won't be automatically disconnected but relogging fixes that.

Generate:

Calls the generating function for each zone.

# Glossary

- Generated area: The world generator generates up to 500 meters from places any player has visited. This is much bigger area than what gets revelead on the minimap.
- Location: Special places like rune stones, dungeon entrances or abandoned houses that are placed to the world by the world generator.
- Zone: The world is split to tiles of 64 m x 64 m size. This is the granularity of the world generation. See https://valheim.fandom.com/wiki/Zones for more info.

# Changelog

- v1.3.0:
	- Regeneration commands renamed to destroy to make the effect more clear.
	- Destroy all command now requires biomes as a parameter (replacing the config setting and making harder to accidentally nuke the whole world).
	- New commands for generating areas without having to physically go there.
	- New command for counting amount of entities.
	- New command for counting all entities.
	- New command to remove entities.
	- New command to place any unplaced locations (replacing one of the config settings).
	- New command to reroll any chest.
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