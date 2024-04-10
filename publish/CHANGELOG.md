- v1.48
  - Fixes location adding affecting the entire location database instead of only selected areas.
  - Fixed for the new game version.

- v1.47
  - Adds a new parameter `type` to limit objects by their components.
  - Changes the default minimum count of `count_objects` from 0 to 1.
  - Fixes terrain reset sometimes not working properly.
  - Fixes Root user check not working properly with crossplay.

- v1.46
  - Adds a new parameter `limit` to limit the amount of affected objects.
  - Fixes `chests_reset` not working properly on servers if clients were on the area.
  - Fixes `objects_swap` not working properly on servers if clients were on the area.
  - Improves performance of object related commands.

- v1.45
  - Adds automatic clean up of duplicate terrain compilers when doing terrain resets.
  - Adds support for reseting terrain across zone boundaries.

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
