# Upgrade world

This single player mod allows regenerating or adding new content to already explored areas.

For dedicated servers, open the world in single player.

Always back up your world before making any changes!

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim).
2. Download the latest zip.
3. Extract it in the \<GameDirectory\>\BepInEx\plugins\ folder.
4. Optionally also install the [Configuration manager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/tag/v16.4).

# Quick instructions:

1. Back up your world!
2. Install the mod.
3. Load the world.
4. Open console with F5. write upgrade and press enter.
5. Wait.
6. Check that your bases are ok (especially if in Plains).
7. If not, restore back up. read more detailed instructions and go back to the first step.
8. Uninstall the mod.
9. Enjoy tar pits!

# Features

Overview of available commands:

- query: Prints how many zones would get operated with the current config.
- upgrade: Upgrades areas defined by the config. This gracefully adds new content, only destroying what's necessary (currently only tar pits).
- regenerate_all: Destroys areas defined by the config. This allows the world generator to regenerate them with new content.
- regenerate_position: Destroys zones at a given coordinates and radius.
- regenerate_zones: Destroys zones at a given zone index.
- redistribute: Runs the "genloc" command for locations defined by the config. This is needed to redistribute locations after regenerating zones.
- stop: Stops execution of the current operation.  Can be useful if it takes too long some reason.

This mod also adds current coordinates (and zone index) to the minimaps which should help using the commands and setting up the config.

# Configuration

Zones affected by "upgrade" and "regenerate_all" commands can be filtered to exclude some parts of the world (for example your base).

The config can be found in the \<GameDirectory\>\BepInEx\config\ folder after the first start up.

Filtering options in the config:
- Included biomes: Only zones that contain any of these biomes get operated. This includes edge zones with multiple biomes.
- Excluded biomes: Only zones that don't contain any of these biomes get operated. This can be used to exclude edge zones.
- Minimum distance from the center: Only zones that are fully outside the given distance.
- Maximum distance from the center: Only zones that are fully inside the given distance.
- Minimum distance from the player: Only zones that are fully outside the given distance.
- Maximum distance from the player: Only zones that are fully inside the given distance.
- Custom points: Coordinates and distances to create excluded areas. Format: x1,z1,min1,max1|x2,z2,min2,max2|...
- Locations: Locations to place with the upgrade command. By default places only tar pits.

# Examples

- Setting "plains" to included biomes would only operate on plains and their edge zones.
- Setting "p" to included biomes and "bl,mo,me,o,s" to excluded biomes would only operate inner parts of plains biomes.
- Setting "300,500,500,0|1000,2000,500,0" to custom points would protect areas at coordinates 300,500 and 1000,2000 up to 500 radius.
- Setting "300,500,0,100" to custom points would only operate near coordinates 300,500 witin 100 meters.

# Upgrade

1. Runs a modified genloc command which allows redistributing unplaced locations to already generated areas.
2. Skip zones that didn't get a new loctaion.
3. For each redistributed location, destroy everything within the location exterior radius and place the location to the world.

# Regenerate

For technical reasons, currently loaded areas are not included (about 200 meters around the player). This can be overridden by config but contains more risks.

1. Removes all objects from a zone (including player placed structures).
2. If the zone has a location, marks the location as unplaced (to allow redistributing it).
3. Marks the zone as ungenerated.  Visiting the zone will regenerate like it were at start of the game.
4. Locations are not automatically redistributed. Use "redistribute" command (otherwise you get the same locations as before).
5. Visiting zones will regenerate them.

Portals in the loaded area won't be autoamtically disconnected but relogging fixes that. 

# Glossary

- Center of the world: The center point of the map, not the spawn point (but the spawn point is usually close to the center).
- Generated area: The world generator generates up to 500 meters from places any player has visited. This is much bigger area than what gets revelead on the minimap.
- Location: Special places like rune stones, dungeon entrances or abandoned houses that are placed to the world by the world generator.
- Zone: The world is split to tiles of 64 m x 64 m size. This is the granularity of the world generation. See https://valheim.fandom.com/wiki/Zones for more info.

# Changelog

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