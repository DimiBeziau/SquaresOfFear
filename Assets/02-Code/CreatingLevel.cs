using UnityEngine;

public class CreatingLevel : MonoBehaviour
{
    public GameObject basicCube;

    private int currentLevel = 1;

    void Start()
    {
        SpawnWave();
    }

    void SpawnWave()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("level" + currentLevel);
        if (jsonFile == null)
        {
            Debug.LogWarning("Niveau introuvable : level" + currentLevel);
            return;
        }

        Level level = JsonUtility.FromJson<Level>(jsonFile.text);

        string[] rows = level.wave.Split('/');

        float floorCenterX = 1.5f;
        float floorSurfaceY = 1.5f;
        float zStart = 11f;

        for (int z = 0; z < rows.Length; z++)
        {
            string[] cols = rows[z].Split(' ');
            float xOffset = floorCenterX - (cols.Length / 2f) + 0.5f;

            for (int x = 0; x < cols.Length; x++)
            {
                int type = int.Parse(cols[x]);
                Vector3 pos = new Vector3(xOffset + x, floorSurfaceY, zStart - z);

                if (type == 1)
                {
                    GameObject cube = Instantiate(basicCube, pos, Quaternion.identity);
                    cube.GetComponent<CubeMove>().kind = CubeMove.CubeKind.Basic;
                }
            }
        }
    }
}
