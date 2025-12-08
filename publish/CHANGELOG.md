- v1.75
  - Add support for wildcard `*` in locations ids.
  - Adds a new parameter `locations` to include only objects or zones that have any of the given locations.
  - Changes commands not to run if any invalid ids are provided.
  - Changes location commands to not run if no ids are provided.
  - Reworked the execution flow to work more smoothly and efficiently.

- v1.74
  - Adds a new upgrade `combatruins` to add the new location from combat update to already explored areas.

- v1.73
  - Adds a new parameter `amount` to multiply affected vegetation objects.
  - Fixes heightmap error when resetting vegetation near the player on single player.
  - Improves performance of commands that require loading the area (for example vegetation commands).

- v1.72
  - Fixes to some location related commands.

- v1.71
  - Fixes the command `world_reset` not working correctly.
  - Removes conversion of underscores to spacebars in some commands (use Server Devcommands if needed).
