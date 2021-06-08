using UnityEngine;

public class HandleCrosshair : MonoBehaviour
{
    [SerializeField]
    World world;
    [SerializeField]
    Transform highlightBlock;
    [SerializeField]
    Transform placeBlock;
    readonly float checkIncrement = 0.1f;
    readonly float reach = 8f;
    AudioSource audioData;
    Vector3 diggedVoxelPosition;
    bool diggingStarted;
    Voxel diggedVoxel;
    float timeToDestroyVoxel;

    void Start()
    {
        audioData = GetComponent<AudioSource>();
    }

    void Update()
    {
        PlaceCursorBlock();

        if (diggedVoxelPosition != null && diggedVoxelPosition != highlightBlock.position)
            diggingStarted = false;

        if (highlightBlock.gameObject.activeSelf)
        {
            if (Input.GetMouseButton(0))
            {
                if (!diggingStarted)
                {
                    diggedVoxelPosition = highlightBlock.position;
                    diggedVoxel = world.GetChunkFromVector3(highlightBlock.position).GetVoxelFromGlobalVector3(highlightBlock.position);
                    timeToDestroyVoxel = diggedVoxel.TimeToDestroy;
                    diggingStarted = true;
                }
                else
                {
                    if (timeToDestroyVoxel > 0)
                    {
                        timeToDestroyVoxel -= Time.deltaTime;
                        Debug.Log(timeToDestroyVoxel);
                    }
                    else
                    {
                        world.GetChunkFromVector3(highlightBlock.position).ModifyVoxel(highlightBlock.position, new Voxel(BlockType.AirBlock));
                        diggingStarted = false;
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                world.GetChunkFromVector3(placeBlock.position).ModifyVoxel(placeBlock.position, new Voxel(BlockType.DirtBlock));
                audioData.Play();
            }
        }
    }

    void PlaceCursorBlock()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = Camera.main.transform.position + (Camera.main.transform.forward * step);

            if (world.VoxelExistsAndIsSolid(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;

                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);

                return;
            }

            lastPos = new Vector3(Mathf.FloorToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));

            step += checkIncrement;
        }

        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);

    }
}
