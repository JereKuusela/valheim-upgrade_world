- v1.40
  - Optimizes ZDO removing.
  - Removes settings "Terrain compiler name" and "Zone control name" as obsolete.
  - Removes the parameter `noclearing` as obsolete.

- v1.39
  - Adds a new setting "Disable automatic genloc".
  - Changes `world_clean` to remove all spawn data from zone controls.
  - Changes `zones_reset` to also reset height coordinate of locations (stolen from Better Continents).
  - Fixes error when trying to reset world where a player has teleported extremely far away.
  - Removes the "Prevent double ZNet view" setting as obsolete.

- v1.38
  - Fixes the `object_refresh` command not always working for spawn points.
  - Removes the upgrade `hildir` because this mod doesn't work on PTB.

- v1.37
  - Adds a new upgrade `hildir`.
  - Changes the `objects_list` command to support wildcards for the object ids.
  - Updates the `world_clean` command to remove excess rooms from dungeons.

- v1.36
  - Updates the `world_clean` command to fix corrupted zone controls.
