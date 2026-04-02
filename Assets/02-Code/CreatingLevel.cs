using UnityEngine;
using System.Collections.Generic;

public class CreatingLevel : MonoBehaviour
{
    public GameObject basicCube;
    public GameObject goldenCube;

    public static float timer = 0f;
    private float advanceInterval = 3f;
    private float cubeSpeed = 1f;

    private int currentLevel = 1;
    private List<CubeMove> activeCubes = new List<CubeMove>();

    void Start()
    {
        SpawnWave();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= advanceInterval)
        {
            timer = 0f;
            AdvanceCubes();
        }
    }

    void AdvanceCubes()
    {
        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            cube.cubeAdvance(cubeSpeed);
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
            float xOffset = Mathf.Round(floorCenterX - (cols.Length - 1) / 2f);

            for (int x = 0; x < cols.Length; x++)
            {
                int type = int.Parse(cols[x]);
                Vector3 pos = new Vector3(xOffset + x, floorSurfaceY, zStart - z);

                if (type == 1)
                {
                    GameObject cube = Instantiate(basicCube, pos, Quaternion.identity);
                    CubeMove cm = cube.GetComponent<CubeMove>();
                    cm.kind = CubeMove.CubeKind.Basic;
                    activeCubes.Add(cm);
                }
                else if (type == 2)
                {
                    GameObject cube = Instantiate(goldenCube, pos, Quaternion.identity);
                    CubeMove cm = cube.GetComponent<CubeMove>();
                    cm.kind = CubeMove.CubeKind.Golden;
                    activeCubes.Add(cm);
                }
            }
        }
    }
}
