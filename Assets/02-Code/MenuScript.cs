using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("SquaresOfFear_scene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
