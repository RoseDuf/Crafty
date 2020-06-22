using UnityEngine;

using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class AutoPreloadScene
{
    static AutoPreloadScene()
    {
        // bind to OnPlayModeChanged callback
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    [MenuItem("File/AutoPreloadScene/Load Preload Scene On Play")]
    private static void EnableLoadPreloadSceneOnPlay()
    {
        string preloadScene = EditorUtility.OpenFilePanel("Select Preload Scene", Application.dataPath, "unity");
        preloadScene = preloadScene.Replace(Application.dataPath, "Assets");

        if (!string.IsNullOrEmpty(preloadScene))
        {
            PreloadScene = preloadScene;
            LoadPreloadSceneOnPlay = true;
        }
    }

    [MenuItem("File/AutoPreloadScene/Don't Load Preload Scene On Play", true)]
    private static bool ShowDontLoadPreloadSceneOnPlay()
    {
        return LoadPreloadSceneOnPlay;
    }
    [MenuItem("File/AutoPreloadScene/Don't Load Preload Scene On Play")]
    private static void DisableLoadPreloadSceneOnPlay()
    {
        LoadPreloadSceneOnPlay = false;
    }

    // PlayModeChanged callback
    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!LoadPreloadSceneOnPlay) return;

        if (!EditorApplication.isPlaying)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // User pressed play, autoload preload scene but make sure user saves changes
                PreviousScene = EditorSceneManager.GetActiveScene().path;
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    try
                    {
                        EditorSceneManager.OpenScene(PreloadScene);
                    }
                    catch
                    {
                        Debug.LogError("Preload Scene not found: " + PreloadScene);
                        EditorApplication.isPlaying = false;
                    }
                }
                else
                {
                    // User cancelled the save operation, cancel play as well
                    EditorApplication.isPlaying = false;
                }
            }

            // we were about to play, but user pressed stop, reload previous scene
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                try
                {
                    EditorSceneManager.OpenScene(PreviousScene);
                }
                catch
                {
                    Debug.LogError("Previous Scene not found: " + PreviousScene);
                }
            }
        }
    }

    // These properties will be set as editor preferences
    public const string EDITOR_PREFERENCE_LOAD_PRELOAD_SCENE_ON_PLAY = "AutoPreloadScene.LoadPreloadSceneOnPlay";
    public const string EDITOR_PREFERENCE_PRELOAD_SCENE = "AutoPreloadScene.PreloadScene";
    public const string EDITOR_PREFERENCE_PREVIOUS_SCENE = "AutoPreloadScene.PreviousScene";

    private static bool LoadPreloadSceneOnPlay
    {
        get { return EditorPrefs.GetBool(EDITOR_PREFERENCE_LOAD_PRELOAD_SCENE_ON_PLAY, false); }
        set { EditorPrefs.SetBool(EDITOR_PREFERENCE_LOAD_PRELOAD_SCENE_ON_PLAY, value); }
    }

    private static string PreloadScene
    {
        get { return EditorPrefs.GetString(EDITOR_PREFERENCE_PRELOAD_SCENE, "Main.unity"); }
        set { EditorPrefs.SetString(EDITOR_PREFERENCE_PRELOAD_SCENE, value); }
    }

    private static string PreviousScene
    {
        get { return EditorPrefs.GetString(EDITOR_PREFERENCE_PREVIOUS_SCENE, EditorSceneManager.GetActiveScene().path); }
        set { EditorPrefs.SetString(EDITOR_PREFERENCE_PREVIOUS_SCENE, value); }
    }
}
