using UnityEngine;

/// <summary>
/// Generic Singleton pattern implementation for MonoBehaviour classes.
/// Ensures only one instance of type <typeparamref name="T"/> exists in the scene.
/// If multiple instances are found, only the first one remains active.
/// </summary>
/// <typeparam name="T">Type of the singleton class that inherits from this base.</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = (T)this;
    }
}