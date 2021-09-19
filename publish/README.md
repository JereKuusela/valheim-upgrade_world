# Upgrade world

This single player mod allows regenerating or adding new content to parts of the world.

Always back up your world before making any changes!

# Manual Installation:

1. Install the [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/)
2. Download the latest zip
3. Extract it in the \<GameDirectory\>\BepInEx\plugins\ folder.

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

# Query

"query" command prints out how many zones would get upgraded or removed when running other commands. Useful to check if zones are filtered properly.

Filtering options in the config:
- Included biomes: Only zones that contain any of these biomes get operated. This includes edge zones with multiple biomes.
- Excluded biomes: Only zones that don't contain any of these biomes get operated. This can be used to exclude edge zones.
- Minimum distance from the center: Only zones that are fully outside the given distance.
- Maximum distance from the center: Only zones that are fully inside the given distance.
- Minimum distance from the player: Only zones that are fully outside the given distance.
- Maximum distance from the player: Only zones that are fully inside the given distance.
- Custom points:  Coordinates and distances to create excluded areas. Format: x1,z1,min1,max1|x2,z2,min2,max2|...

For example setting "plains" to included biomes would only operate on plains and their edge zones.

For example setting "p" to included biomes and "bl,mo,me,o,s" to excluded biomes would only operate inner parts of plains biomes.

For example setting "300,500,500,0|1000,2000,500,0" to custom points would protect areas at coordinates 300,500 and 1000,2000 up to 500 radius.

For example setting "300,500,0,100" to custom points would only operate near coordinates 300,500 witin 100 meters.


# Upgrade

"upgrade" command redistributes locations to already explored areas (takes a while) and then spawns them. This will destroy anything in their place so use with a caution.

Affected locations can be changed in the config and by default only includes tar pits.

# Nuke

"nuke" command fully regenerates explored areas. This will destroy everything there so use with a caution. This will also reduce the save file size until those areas are visited again.

For technical reasons, currently loaded areas are not nuked (about 200 meters around the player).

After running, use "genloc" command to redistribute locations to destroyed areas (otherwise you get the same locations as before).

Portals in the loaded area won't be autoamtically disconnected but relogging fixes that. 

# Stop

"stop" command stops the execution of current operation. Can be useful if it takes too long some reason.

# Changelog

- v1.0.0: 
	- Initial release