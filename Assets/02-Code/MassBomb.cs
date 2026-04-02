using UnityEngine;

public class MassBomb : MonoBehaviour
{
    private float timer = 0f;

    void Update()
    {
        if (timer <= 0.75f)
            transform.Translate(0, Time.deltaTime * 0.5f, 0);
        else if (timer <= 1.5f)
            transform.Translate(0, Time.deltaTime * -0.5f, 0);
        else
            timer = 0f;

        timer += Time.deltaTime;
    }
}
