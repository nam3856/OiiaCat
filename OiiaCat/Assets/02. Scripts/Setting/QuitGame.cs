using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void QuitApplication()
    {
        Debug.Log("QuitApplication called. Exiting the game.");
        Application.Quit();
    }
}