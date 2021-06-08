using System;
using UnityEngine;
using UnityEngine.UI;

public class HandleCrosshair : MonoBehaviour
{
    [SerializeField]
    World world;
    [SerializeField]
    Transform highlightBlock;
    [SerializeField]
    Transform placeBlock;
    [SerializeField]
    HandMover handMover;
    [SerializeField]
    Text currentBlockText;
    [SerializeField]
    Text destroyTimeText;
    readonly float checkIncrement = 0.1f;
    readonly float reach = 8f;
    AudioSource audioData;
    Vector3 diggedVoxelPosition;
    bool diggingStarted;
    Voxel diggedVoxel;
    float timeToDestroyVoxel;
    int selectedBlockIndex = 1;

    void Awake() => audioData = GetComponent<AudioSource>();

    void Start() => currentBlockText.text = $"Current Block: {world.VoxelTypes[selectedBlockIndex].BlockType}";

    void Update()
    {
        PlaceCursorBlock();

        if (diggedVoxelPosition != null && diggedVoxelPosition != highlightBlock.position)
            diggingStarted = false;

        if (highlightBlock.gameObject.activeSelf)
        {
            if (Input.GetMouseButton(0))
                HandleDigging();

            if (Input.GetMouseButtonUp(0))
            {
                diggingStarted = false;
                handMover.IsHandMoving = false;
                destroyTimeText.text = "";
            }

            if (Input.GetMouseButtonDown(1))
            {
                world.GetChunkFromVector3(placeBlock.position).ModifyVoxel(placeBlock.position, world.VoxelTypes[selectedBlockIndex].BlockType);
                audioData.Play();
            }

            HandleScrollWheel();
        }
    }

    void HandleScrollWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0)
                selectedBlockIndex++;
            else
                selectedBlockIndex--;

            if (selectedBlockIndex > world.VoxelTypes.Length - 1)
                selectedBlockIndex = 1;

            if (selectedBlockIndex < 1)
                selectedBlockIndex = world.VoxelTypes.Length - 1;

            currentBlockText.text = $"Current Block: {world.VoxelTypes[selectedBlockIndex].BlockType}";
        }
    }

    void HandleDigging()
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
            handMover.IsHandMoving = true;
            if (timeToDestroyVoxel > 0)
            {
                timeToDestroyVoxel -= Time.deltaTime;
                destroyTimeText.text = $"{Math.Round(timeToDestroyVoxel, 1)}";
            }
            else
            {
                world.GetChunkFromVector3(highlightBlock.position).ModifyVoxel(highlightBlock.position, BlockType.AirBlock);
                destroyTimeText.text = "";
                diggingStarted = false;
                handMover.IsHandMoving = false;
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
