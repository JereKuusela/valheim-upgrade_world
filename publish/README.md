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

- v1.28
	- Adds a new command `world_clean` to remove missing objects.
	- Improves performance of single zone operations.

- v1.27
	- Adds a new parameteter `terrain` to reset nearby terrain when resetting vegetation.
	- Changes most output to be printed even without verbose mode.
	- Changes the default value of verbose mode to false.
	- Fixes `zones_reset` command causing holes in the zone borders.
	- Fixes networking logging not being throttled.

- v1.26
	- Fixes location priotization not working correctly (caused boss locations being placed last).

- v1.25
	- Improves performance of `vegetation_add` and `zones_generate` commands.
	- Fixes `vegetation_add` and `vegetation_reset` showing wrong amount of added objects.
	- Fixes location distribution not prioritizing priority locations.
	- Fixes `upgrade EVA` to work with EVA version 1.8.0
	
- v1.24
	- Fixes chest reset not working properly.
	
- v1.23
	- Fixes operation center not being at the world center by default.
	
- v1.22
	- Adds a new command `world_gen` to set the world gen.
	- Adds automatic reset for outer areas to the `mistlands_worldgen` operation.
