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
    void Start()
    {
        audioData = GetComponent<AudioSource>();
    }

    void Update()
    {
        PlaceCursorBlock();

        if (highlightBlock.gameObject.activeSelf)
        {
            if (Input.GetMouseButtonDown(0))
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);

            if (Input.GetMouseButtonDown(1))
            {
                world.GetChunkFromVector3(placeBlock.position).EditVoxel(placeBlock.position, 4);
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

            if (world.CheckForVoxel(pos))
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
