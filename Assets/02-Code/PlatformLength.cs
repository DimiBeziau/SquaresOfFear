using System.Collections;
using UnityEngine;

public class PlatformLength : MonoBehaviour
{
    public GameObject platformCube;
    [SerializeField] private float wallHeight = 3f;
    [SerializeField] private float wallThickness = 0.5f;

    private BoxCollider leftWall;
    private BoxCollider rightWall;
    private BoxCollider farWall;
    private Collider platformCollider;

    void Start()
    {
        platformCollider = GetComponent<Collider>();
        CreateInvisibleWalls();
        UpdateInvisibleWalls();
    }

    void LateUpdate()
    {
        UpdateInvisibleWalls();
    }

    public IEnumerator Enlargement()
    {
        yield return new WaitForSeconds(3);
        transform.localScale += new Vector3(0, 0, 1f);
        transform.Translate(0, 0, -0.5f);
    }

    public void Decrease()
    {
        transform.localScale -= new Vector3(0, 0, 1f);
        transform.Translate(0, 0, -0.5f);
        float z = transform.localScale.z;
        for (int i = 0; i < transform.localScale.x; i++)
            Instantiate(platformCube, new Vector3(i, 0.48f, z), Quaternion.identity);
    }

    public int PlatformWidth(int i)
    {
        int j = (int)transform.localScale.x;
        transform.position = new Vector3(((float)i - 1) / 2, transform.position.y, transform.position.z);
        transform.localScale = new Vector3(i, transform.localScale.y, transform.localScale.z);
        return j;
    }

    private void CreateInvisibleWalls()
    {
        leftWall = CreateWall("InvisibleWall_Left");
        rightWall = CreateWall("InvisibleWall_Right");
        farWall = CreateWall("InvisibleWall_Far");
    }

    private BoxCollider CreateWall(string wallName)
    {
        GameObject wall = new GameObject(wallName);
        BoxCollider collider = wall.AddComponent<BoxCollider>();
        collider.isTrigger = false;
        return collider;
    }

    private void UpdateInvisibleWalls()
    {
        Bounds bounds = GetPlatformBounds();
        float yCenter = bounds.max.y + wallHeight * 0.5f;
        float zSizeForSideWalls = bounds.size.z + wallThickness * 2f;
        float xSizeForFarWall = bounds.size.x + wallThickness * 2f;

        leftWall.transform.position = new Vector3(bounds.min.x - wallThickness * 0.5f, yCenter, bounds.center.z);
        leftWall.size = new Vector3(wallThickness, wallHeight, zSizeForSideWalls);

        rightWall.transform.position = new Vector3(bounds.max.x + wallThickness * 0.5f, yCenter, bounds.center.z);
        rightWall.size = new Vector3(wallThickness, wallHeight, zSizeForSideWalls);

        farWall.transform.position = new Vector3(bounds.center.x, yCenter, bounds.max.z + wallThickness * 0.5f);
        farWall.size = new Vector3(xSizeForFarWall, wallHeight, wallThickness);
    }

    private Bounds GetPlatformBounds()
    {
        if (platformCollider != null)
            return platformCollider.bounds;

        Renderer platformRenderer = GetComponent<Renderer>();
        if (platformRenderer != null)
            return platformRenderer.bounds;

        return new Bounds(transform.position, transform.localScale);
    }
}
