- v1.85
  - Adds new command `clean_ungenerated` to remove objects from non-generated zones (also included in `world_clean`).
  - Fixes the save system not clearing up fully emptied areas. Remember to create back ups before using this mod!

- v1.84
  - Fixes vegetation commands running with invalid vegetation ids.
  - Fixes invalid locations causing errors during location id check.

- v1.83
  - Adds more locations to the `bearcave` upgrade operation.
  - Adds new filter `altbiomes` to filter zones based on alternate biomes.
  - Fixes `locations_add` command always pinning added locations.

- v1.82
  - Adds new upgrade operation `bearcave` for some latest locations.
  - Adds support for new inventory format (including old format for compatibility). Thanks JPValheim!
  - Changes `clean_*` commands to be queued operations (requires using `start` to execute).
  - Fixes possible error when objects have been removed before processing. Thanks JPValheim!
  - Fixes filtering zones by location not working. Thanks JPValheim!
  - Fixes `clean_duplicates` command not using zone based filters.
  - Improves error handling when multiple operations are queued. Thanks JPValheim!
  - Improves object modification to not take ownership of the object.
  - Improves support for `ignore` parameter in entity operations. Thanks JPValheim!

- v1.81
  - Fixes for the new game update. Thanks andrewstevenson91!
  - More upgrade operations will probably come a bit later.
