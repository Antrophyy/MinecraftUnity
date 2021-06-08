# MinecraftVlcek

A Unity project that is trying to replicate Minecraft's basics to learn optimization.

# Optimization improvements done:
- Instead of using gameobjects for every "cube" I decided to go the "minecraft route" as well and implement custom voxels with chunks.
That way, only one gameobject is created for a lot of voxels. 
- Disabling sides of voxels that don't have to be visible (i.e. they are below other visible voxels). This is done by having an array of vertices for the voxel and "connecting" the 3 vertices to create a triangle, if you create two triangles you get one side of the cube.
- Disabling and enabling chunks as the player moves through the game world.

# Optimization improvements that could be implemented:
- Threading, make it so that the loading chunks and generating world is pararelized. That way the game would not "lag" whenever there is a new chunk loading.
- Regarding loading a new chunk, make it so that they load over time and not in an instant, there could also be a queue container implemented for this optimization.
- Moving away from mesh collider and instead use custom player movement, where we check each side of player movement whether there is a voxel existing or not.
- Using a byte array for the voxel map instead of array of custom class type.
