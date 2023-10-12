- v1.44
  - Adds new parameter `pin` to show results on the map (requires Server Devcommands mod).
  - Changes invalid id check to be warning instead of error (doesn't block executing).
  - Fixes `locations_list` not using x,z,y format.
  - Fixes `world_clean` clearing all drawers from Item Drawers mod.

- v1.43
  - Fixed for the new patch.
  - Removes the `upgrade EVA` command as obsolete.

- v1.42
  - Fixes "item with the same key..." error that sometimes happens.

- v1.41
  - Changes `world_clean` to optimize dungeons instead of removing extra rooms.
  - Changes the location distribution to be affected by safe zones.
  - Fixes `locations_add` command placing every unplaced location, instead of just the specified locations (mostly affects some custom maps).
  - Fixes `world_clean` command not working for new chest format.
  - Fixes `locations_remove` command not being affected by safe zones.
  - Improves the performance `locations_add` command (used by most upgrade commands).

- v1.40
  - Adds a new upgrade `hildir`.
  - Fixes for the new update.
  - Fixes custom clear area not affecting location reset.
  - Optimizes ZDO removing.
  - Removes settings "Terrain compiler name" and "Zone control name" as obsolete.
  - Removes the parameter `noclearing` as obsolete.

- v1.39
  - Adds a new setting "Disable automatic genloc".
  - Changes `world_clean` to remove all spawn data from zone controls.
  - Changes `zones_reset` to also reset height coordinate of locations (stolen from Better Continents).
  - Fixes error when trying to reset world where a player has teleported extremely far away.
  - Removes the "Prevent double ZNet view" setting as obsolete.
