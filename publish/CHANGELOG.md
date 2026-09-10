# Private Deep North iteration 3 (1.80.3)

- Update reset/search/cleanup for native byte inventories and packed items; skip unreadable inventories without destructive fallback.
- Recreate refreshed treasure chests through native load-time loot; retain current stand-hash compatibility and maintenance queue/filter fixes.
- Allow `chests_reset *` with item IDs and expose one persistent `upgradeWorld•chestReset` integer receipt for optional loot integrations.
- Remove iteration 2's optional numeric selector expansion. Keep the one-line onion chest queue repair; the Deep North recipe is unchanged.
- Compiled/headless-tested against supplied assemblies; live playtest required.

- v1.80
  - Adds compatibility with Location Placement Accelerator mod. Thanks Kurios.ZeuS!
  - Improves parameter parsing to not omit empty values.

- v1.79
  - Adds wildcard `*` support for data based filtering to check any data key.
  - Fixes vegetation reset sometimes not cleaning up the spawned terrain object. Thanks warp!

- v1.78
  - Adds output to commands `location_list` and `object_list` when no objects or locations are found.
  - Changes the command `object_edit` to allow clearing data by providing only the key as a parameter.

- v1.77
  - Fixes the command `location_add` not always spawning all locations properly (related to location groups).

- v1.76
  - Adds a new command `uw_check` which prints currently queued operations.
  - Adds info to the console when a command requires `start` to begin.
  - Adds a new setting `Verbose locations` to print more detailed information about location generation.
  - Fixes error when trying to remove already removed objects.
  - Fixes the command `location_add` causing vegetation to generate on nearby areas.
  - Fixes the server not having devcommands automatically enabled.
  - Possibly fixes some commands getting stuck (unverified).
  - Removes the command `backup` as obsolete.
