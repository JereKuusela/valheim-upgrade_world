- v1.38
  - Fixes the `object_refresh` command not always working for spawn points.
  - Removes the upgrade `hildir` because this mod doesn't work on PTB.

- v1.37
  - Adds a new upgrade `hildir`.
  - Changes the `objects_list` command to support wildcards for the object ids.
  - Updates the `world_clean` command to remove excess rooms from dungeons.

- v1.36
  - Updates the `world_clean` command to fix corrupted zone controls.

- v1.35
  - Updated for the new game version.

- v1.34
  - Adds more error checks to the `locations_reset` command.
  - Improves the `locations_swap` command to work better with missing locations.
  - Fixes error with the new update.

- v1.33
  - Adds a new setting "Safe zone objects" to allow excluding areas by non-player built objects.
  - Adds a new command `locations_swap` to swap locations.
  - Changes the `world_clean` command to also remove missing locations.
  - Fixes the `zones_reset` command not always visually updating the nearby terrain.

- v1.32
  - Adds a new command `chests_search` to search chests and stands.
  - Adds underscore support for data setting and filtering (for space bars).

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
