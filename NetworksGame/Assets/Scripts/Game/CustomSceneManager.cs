using System;
using System.Collections;
using System.Threading.Tasks; // For Task
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomSceneManager : MonoBehaviour
{

    /// <summary>
    /// Function to load asyncronously a scene and execute an action when setting it active.
    /// This function doesn't wait for the action to be finished the execution.
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="action"></param>
    /// <param name="args"></param>
    public static IEnumerator LoadAsyncSceneWithAction(string sceneName, Action<object[]> action, params object[] args)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // We not allow the scene to activate by itself
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads but not set active the scene
        while (!asyncLoad.isDone && asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // We execute whatever we want before activating the scene
        action?.Invoke(args);

        asyncLoad.allowSceneActivation = false;
    }


    /// <summary>
    /// Loads a scene asyncronously, before setting it active executes a method also asyncronously. 
    /// This function waits until scene is loaded and the whole method executed.
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IEnumerator LoadSceneWithMethodAsync(string scene, Func<object[], Task> method, params object[] args)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(scene);
        op.allowSceneActivation = false;

        while (!op.isDone && op.progress < 0.9f)
        {
            yield return null;
        }

        if (method != null)
        {
            Task task = method(args);
            while (!task.IsCompleted)
            {
                yield return null; // Wait for async method to complete
            }
        }

        op.allowSceneActivation = true;
    }
}
