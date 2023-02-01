# Upgrade World

This tool includes console commands to add new content to already explored areas (and more).

Always back up your world before making any changes!

Install on the admin client and on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

# Usage

See [documentation](https://github.com/JereKuusela/valheim-upgrade_world/blob/main/README.md).

# Credits

Thanks for Azumatt for creating the mod icon!

Sources: [GitHub](https://github.com/JereKuusela/valheim-upgrade_world)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)

# Changelog

- v1.31
	- Adds a new command `objects_respawn` to refresh objects.
	- Adds a new command `objects_edit` to set data values.
	- Adds a new parameter `print` to the `objects_list` to display any data value.
	- Adds a new parameter `filter` to the object commands to filter by data value.
	- Changes the command `location_register` to target only a single zone.
	- Fixes a possible crash when removing objects.
	- Optimizes `objects_*` commands.

- v1.30
	- Adds automatic terrain reset to location removing.
	- Adds a new command `objects_swap` to swap objects.
	- Adds a new command `locations_register` to register locations to the location database.
	- Adds a new command `locations_list` to print location positions.
	- Adds a new paramater `clear` to the `locations_remove` command to override the radius of cleared objects.
	- Adds a new parameter `location` to the `objects_remove` command to filter by location.
	- Fixes terrain reset not affecting paint.
	- Fixes spawn point removing not removing the spawned creature.
	- Fixes wrong name on the "Terrain compiler name" setting.

- v1.29
	- Fixes player base detection not always working.

- v1.28
	- Adds a new command `world_clean` to remove missing objects.
	- Adds caching to the player base detection (10 seconds).
	- Adds caching to the terrain reset (10 seconds).
	- Adds a new setting "Operation delay" to prevent dedicated servers from getting overloaded.
	- Removes settings "Safe distance around the player" and "Custom points" as obsolete (+ improves performance).
	- Improves performance of single zone operations.

- v1.27
	- Adds a new parameteter `terrain` to reset nearby terrain when resetting vegetation.
	- Changes most output to be printed even without verbose mode.
	- Changes the default value of verbose mode to false.
	- Fixes `zones_reset` command causing holes in the zone borders.
	- Fixes networking logging not being throttled.

- v1.26
	- Fixes location priotization not working correctly (caused boss locations being spawned last).
