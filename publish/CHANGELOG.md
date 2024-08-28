- v1.58
  - Fixed for the new update.

- v1.57
  - Fixes chance not working with the command `locations_reset` (this might break chance with other commands).

- v1.56
  - Adds new `clean_*` commands as individual clean operations.
  - Adds a new command `clean_health` to remove excess health data from creatures and structures.
  - Fixes the command `world_clean` removing Expand World Data blueprint locations.
  - Fixes autocomplete showing duplicate values if the same location has multiple entries.
  - Fixes the command `locations_add` not adding all locations if the location has multiple entries.
  - Fixes pins not working with the command `world_clean`.

- v1.55
  - Fixes the command `chests_search` not working.

- v1.54
  - Adds a new upgrade operation "deepnorth" to reset the Deep North and the ocean gap.
  - Fixes the operation `zones_restore` not always working.
  - Fixes some individual locations not always spawning.
  - Fixes pins not working.
  - Removes the settings "Show map coordinates" and "Show minimap coordinates" as obsolete (use Server Devcommands if needed).
  - Some minor optimizations.

- v1.53
  - Fixes the upgrade operation "ashlands" not resetting the ocean gap.

- v1.52.1
  - Fixes wrong version number.

- v1.52
  - Adds a new command `temple_gen` to update the Start Temple boss stone positions.
  - Adds Start Temple boss stone position fix to the "ashlands" upgrade operation.

- v1.51
  - Adds a new upgrade operation "ashlands" to reset existing ashlands.

- v1.50
  - Fixes some resets not always working.
