using UnityEngine;

public class CubeMove : MonoBehaviour
{
    public static int destroyedCubes = 0;
    public static int destroyedMistake = 0;
    public static int blackFallen = 0;
    public static int countAudio = 0;

    public enum CubeKind
    {
        Basic    = 1,
        Green    = 2,
        Black    = 3,
        Platform = 4
    }

    public CubeKind kind;
    public float gravity = 0f;

    private bool destroyed = false;

    void Update()
    {
        if (gravity != 0f)
            transform.Translate(0, gravity * Time.deltaTime, 0, Space.World);

        if (transform.position.y < -20f)
        {
            if ((int)kind != 4)
                destroyedMistake++;
            Destroy(gameObject);
        }
    }

    public void ReactToHit(bool success)
    {
        if (destroyed) return;
        destroyed = true;

        if (success) destroyedCubes++;
        else         destroyedMistake++;

        countAudio++;
        Destroy(gameObject);
    }

    public void cubeFall()
    {
        gravity = -9.8f;
    }
}
