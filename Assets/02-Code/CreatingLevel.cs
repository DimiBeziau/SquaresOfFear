using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreatingLevel : MonoBehaviour
{
    public GameObject basicCube;
    public GameObject goldenCube;
    public GameObject blackCube;

    public PlatformLength platform;

    public static float timer = 0f;
    private float advanceInterval = 3f;
    private float cubeSpeed = 1f;
    private float penaltyCubeSpeed = 3f;
    private int penaltyAdvanceSteps = 3;

    private int currentLevel = 1;
    private int countToDestroy = 0; // basic + golden seulement
    private bool allDestroyed = false;
    private bool waveEnded = false;
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

        // Le joueur a détruit tous les blocs comptables (basic + golden)
        if (!allDestroyed && countToDestroy > 0 && CubeMove.destroyedCubes >= countToDestroy)
            allDestroyed = true;

        // Vague terminée quand tous les basic+golden sont partis
        // (détruits par le joueur OU tombés), sans compter les cubes noirs
        int nonBlackMistakes = CubeMove.destroyedMistake - CubeMove.blackFallen;
        int accounted = CubeMove.destroyedCubes + nonBlackMistakes;
        if (!waveEnded && countToDestroy > 0 && accounted >= countToDestroy)
        {
            waveEnded = true;
            EndWave();
        }
    }

    void AdvanceCubes()
    {
        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            cube.cubeAdvance(cubeSpeed);
    }

    public void PenaltyAdvance()
    {
        timer = 0f;
        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            cube.cubeAdvance(penaltyCubeSpeed, true, penaltyAdvanceSteps);
    }

    void EndWave()
    {
        // Détruire les cubes encore en scène (cubes noirs restants)
        activeCubes.RemoveAll(c => c == null);
        foreach (CubeMove cube in activeCubes)
            Destroy(cube.gameObject);
        activeCubes.Clear();

        if (platform != null)
        {
            if (allDestroyed)
                StartCoroutine(platform.Enlargement());
            else
                platform.Decrease();
        }

        CubeMove.destroyedCubes = 0;
        CubeMove.destroyedMistake = 0;
        CubeMove.blackFallen = 0;
        CubeMove.countAudio = 0;

        currentLevel++;
        allDestroyed = false;
        waveEnded = false;
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

        int cols = rows[0].Split(' ').Length;

        if (platform != null)
        {
            int prevWidth = platform.PlatformWidth(cols);
            if (prevWidth != cols)
            {
                CameraScript cam = Camera.main.GetComponent<CameraScript>();
                if (cam != null) cam.Posit(cols);
            }
        }

        countToDestroy = 0;
        float floorCenterX = 1.5f;
        float floorSurfaceY = 1.5f;
        float zStart = 11f;

        for (int z = 0; z < rows.Length; z++)
        {
            string[] colValues = rows[z].Split(' ');
            float xOffset = Mathf.Round(floorCenterX - (colValues.Length - 1) / 2f);

            for (int x = 0; x < colValues.Length; x++)
            {
                int type = int.Parse(colValues[x]);
                Vector3 pos = new Vector3(xOffset + x, floorSurfaceY, zStart - z);

                if (type == 1)
                {
                    GameObject cube = Instantiate(basicCube, pos, Quaternion.identity);
                    activeCubes.Add(cube.GetComponent<CubeMove>());
                    countToDestroy++;
                }
                else if (type == 2)
                {
                    GameObject cube = Instantiate(goldenCube, pos, Quaternion.identity);
                    activeCubes.Add(cube.GetComponent<CubeMove>());
                    countToDestroy++;
                }
                else if (type == 3 && blackCube != null)
                {
                    GameObject cube = Instantiate(blackCube, pos, Quaternion.identity);
                    activeCubes.Add(cube.GetComponent<CubeMove>());
                }
            }
        }
    }
}
