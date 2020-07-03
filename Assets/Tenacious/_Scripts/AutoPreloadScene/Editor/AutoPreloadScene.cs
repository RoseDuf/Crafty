using UnityEngine;
using UnityEngine.SceneManagement;

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
        string preloadSceneName = EditorUtility.OpenFilePanel("Select Preload Scene", Application.dataPath, "unity");
        preloadSceneName = preloadSceneName.Replace(Application.dataPath, "Assets");

        if (!string.IsNullOrEmpty(preloadSceneName))
        {
            PreloadScenePath = preloadSceneName;
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
                // User pressed play
                PreviousActiveScenePath = SceneManager.GetActiveScene().path;

                // autoload preload scene but make sure user saves changes
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    try
                    {
                        Scene preloadScene = EditorSceneManager.OpenScene(PreloadScenePath, OpenSceneMode.Additive);
                        SceneManager.SetActiveScene(preloadScene);
                    }
                    catch
                    {
                        Debug.LogError("Preload Scene not found: " + PreloadScenePath);
                        EditorApplication.isPlaying = false;
                    }
                }
                else
                {
                    // User cancelled the save operation, cancel play as well
                    EditorApplication.isPlaying = false;
                }
            }

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // we were about to play, but user pressed stop or canceled play
                SceneManager.SetActiveScene(SceneManager.GetSceneByPath(PreviousActiveScenePath));
            }
        }

        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(PreviousActiveScenePath));
        }

        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(PreviousActiveScenePath));
        }
    }

    // These properties will be set as editor preferences
    public const string EDITOR_PREFERENCE_LOAD_PRELOAD_SCENE_ON_PLAY = "AutoPreloadScene.LoadPreloadSceneOnPlay";
    public const string EDITOR_PREFERENCE_PRELOAD_SCENE = "AutoPreloadScene.PreloadScene";
    public const string EDITOR_PREFERENCE_PREVIOUS_ACTIVE_SCENE = "AutoPreloadScene.PreviousActiveScene";

    private static bool LoadPreloadSceneOnPlay
    {
        get { return EditorPrefs.GetBool(EDITOR_PREFERENCE_LOAD_PRELOAD_SCENE_ON_PLAY, false); }
        set { EditorPrefs.SetBool(EDITOR_PREFERENCE_LOAD_PRELOAD_SCENE_ON_PLAY, value); }
    }

    private static string PreloadScenePath
    {
        get { return EditorPrefs.GetString(EDITOR_PREFERENCE_PRELOAD_SCENE, "Preload.unity"); }
        set { EditorPrefs.SetString(EDITOR_PREFERENCE_PRELOAD_SCENE, value); }
    }

    private static string PreviousActiveScenePath
    {
        get { return EditorPrefs.GetString(EDITOR_PREFERENCE_PREVIOUS_ACTIVE_SCENE, "Preload.unity"); }
        set { EditorPrefs.SetString(EDITOR_PREFERENCE_PREVIOUS_ACTIVE_SCENE, value); }
    }
}
