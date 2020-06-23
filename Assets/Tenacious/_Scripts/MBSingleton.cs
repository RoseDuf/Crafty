using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tenacious
{
    /// <summary>
    /// Inherit from this base class to create a monobehaviour singleton. 
    /// e.g. public class MyClassName : MBSingleton<MyClassName> {...} 
    /// This script will not prevent non singleton constructors from being called in your derived classes. 
    /// To prevent this, add a protected constructor to each derived class.
    /// e.g. protected MyClassName() {}
    /// </summary>
    public abstract class MBSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static bool reinitializationPermitted = false;
        private static bool destroyed = false;
        private static object mutex = new object();
        private static T instance;

        [Tooltip("Allow this object to be initialized after being destroyed.")]
        [SerializeField] private bool allowReinitialization = false;
        [Tooltip("Destroy this object when a new scene is loaded.")]
        [SerializeField] private bool destroyOnLoad = false;

        /// <summary>
        /// Access singleton instance through this propriety.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (destroyed)
                {
                    string log_message = "[MBSingleton] Instance '" + typeof(T) + "' has already been destroyed";
                    if (reinitializationPermitted)
                    {
                        Debug.LogWarning(log_message + " - Creating new instance.");
                        destroyed = false;
                    }
                    else
                    {
                        Debug.LogWarning(log_message + " - Returning null.");
                        return null;
                    }
                }

                return CreateInstance(null);
            }
        }

        private static T CreateInstance(GameObject game_object)
        {
            lock (mutex)
            {
                // Create new instance if one doesn't already exist
                if (instance == null)
                {
                    if (game_object == null)
                    {
                        // create the GameObject that will house the singleton instance
                        game_object = new GameObject(typeof(T).Name + " (" + nameof(MBSingleton<T>) + ")");
                        game_object.AddComponent<T>();
                    }

                    instance = game_object.GetComponent<T>();
                }

                return instance;
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                if (destroyed && !allowReinitialization)
                {
                    Destroy(this.gameObject);
                    return;
                }
                else
                    CreateInstance(this.gameObject);
            }
            else
            {
                // an instance already exists and this is a duplicate
                Destroy(this.gameObject);
                destroyed = false; // the "true" instance has not yet been destroyed
                return;
            }

            reinitializationPermitted = allowReinitialization;

            if (!DestroyOnLoad)
                DontDestroyOnLoad(this.gameObject);
        }

        protected virtual void OnApplicationQuit()
        {
            // When Unity quits it destroys objects in a random order and this can create issues for singletons. 
            // So we prevent reinitialization and access to the instance when the application quits to prevent problems.
            reinitializationPermitted = false;
            destroyed = true; // pretend its already destroyed
        }

        protected virtual void OnDestroy()
        {
            instance = null;
            destroyed = true;
        }

        public bool DestroyOnLoad
        {
            get { return destroyOnLoad; }
            set
            {
                destroyOnLoad = value;
                if (destroyOnLoad)
                    SceneManager.MoveGameObjectToScene(this.gameObject, SceneManager.GetActiveScene());
                else
                    DontDestroyOnLoad(this.gameObject);
            }
        }
    }
}