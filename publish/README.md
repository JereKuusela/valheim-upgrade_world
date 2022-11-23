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

- v1.21
	- Adds Player_tombstone to the default player base item list.
	- Adds a new upgrade operation `jewelcrafting`.
	- Adds new upgrade operations `hh_worldgen` and `mistlands_worldgen`.
	- Removes the "Dedicated server execution" the setting as confusing.
	- Improves location commands to work better with server side locations.

- v1.20
	- Adds a new parameter `log` to the  `objects_count`, `objects_list` and `objects_remove` commands to print to the log file.
	- Changes `objects_count`, `objects_list` and `objects_remove` to print to the console.
	- Changes ids of the `objects_count`, `objects_list` and `objects_remove` commands to be case insensitive.

- v1.19
	- Adds support for custom biomes from Expand World mod.
	- Adds a new setting `world_radius` to support bigger worlds from Expand World mod.
	- Adds support for all locations to `locations_*` commands (when no ids are given).
	- Adds a new command `world_reset` to reset both locations and zones.
	- Improves the `chests_reset` command to work with modded chests.
	- Improves the performance of `locations_remove`.
	- Improves the Mistlands upgrade command to also regenerate locations.
	- Removes vegetation altering commands as obsolete (use Expand World mod).

- v1.18
	- Fixes `locations_*` commands not working.

- v1.17
	- Fixes `upgrade` command not working.
