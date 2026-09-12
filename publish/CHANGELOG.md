- v1.82
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

- v1.80
  - Adds compatibility with Location Placement Accelerator mod. Thanks Kurios.ZeuS!
  - Improves parameter parsing to not omit empty values.

- v1.79
  - Adds wildcard `*` support for data based filtering to check any data key.
  - Fixes vegetation reset sometimes not cleaning up the spawned terrain object. Thanks warp!

- v1.78
  - Adds output to commands `location_list` and `object_list` when no objects or locations are found.
  - Changes the command `object_edit` to allow clearing data by providing only the key as a parameter.
