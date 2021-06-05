using UnityEngine;

public class HandleCrosshair : MonoBehaviour
{
    [SerializeField]
    World world;

    float checkIncrement = 0.1f;
    float reach = 8f;
    [SerializeField]
    Transform highlightBlock;
    [SerializeField]
    Transform placeBlock;
    [SerializeField]
    Transform camera;
    void Update()
    {
        PlaceCursorBlock();

        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                world.GetChunkFromVector3(hit.point).EditVoxel(hit.point, 0);
            }
        }
    }

    void PlaceCursorBlock()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();

        while (step < reach)
        {
            Vector3 pos = camera.position + (camera.forward * step);

            if (world.IsVoxelInWorld(pos))
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
