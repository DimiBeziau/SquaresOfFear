using UnityEngine;
using UnityEngine.UI;

public class MenuSetup : MonoBehaviour
{
    void Start()
    {
        Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtinFont == null)
            return;

        foreach (Text t in GetComponentsInChildren<Text>(true))
            t.font = builtinFont;
    }
}
