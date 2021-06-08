# MinecraftVlcek

A Unity project that is trying to replicate Minecraft's basics to learn optimization.

# Optimization improvements done:
- Instead of using gameobjects for every "cube" I decided to go the "minecraft route" as well and implement custom voxels with chunks.
That way, only one gameobject is created for a lot of voxels. 
- Disabling sides of voxels that don't have to be visible (i.e. they are below other visible voxels).
- Disabling and enabling chunks as the player moves through the game world.

# Optimization improvements that could be implemented:
- Threading, make it so that the loading chunks and generating world is pararelized. That way the game would not "lag" whenever there is a new chunk loading.
- Moving away from mesh collider and instead use custom player movement, where we check each side of player movement whether there is a voxel existing or not.
- Using a byte array for the voxel map instead of array of custom class type.
