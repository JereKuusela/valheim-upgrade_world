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

- v1.75
  - Add support for wildcard `*` in locations ids.
  - Adds a new parameter `locations` to include only objects or zones that have any of the given locations.
  - Adds a new command `locations_count` to count locations of specified types.
  - Adds server sync for location and vegetation ids to provide better autocompletion.
  - Changes commands not to run if any invalid ids are provided.
  - Changes location commands to not run if no ids are provided.
  - Fixed for the new update.
  - Fixes `pos=x,z,y` not working for the command `location_register`.
  - Reworked the execution flow to work more smoothly and efficiently.
