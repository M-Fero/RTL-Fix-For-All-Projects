using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartTheGame : MonoBehaviour
{
    [SerializeField] float delay = 10;
    public void RestartDelay()
    {
        StartCoroutine(RestartWithDelay(delay));
    }
    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    public IEnumerator RestartWithDelay(float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        RestartGame();
    }
}
