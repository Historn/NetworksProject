using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{

    /// <summary>
    /// Function to load asyncronously a scene and wait until loaded.
    /// </summary>
    /// <param name="sceneName"></param>
    //public static void AsyncLoadScene(string sceneName)
    //{
    //    StartCoroutine(LoadYourAsyncScene(sceneName));
    //}

    public static IEnumerator LoadYourAsyncScene(string sceneName)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
