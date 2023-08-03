using UnityEngine;

namespace MuckInternal
{
    public class Loader
    {
        private static readonly GameObject MGameObject = new GameObject();

        // The function that our injector calls inside our target process
        public static void Load()
        { 
            MGameObject.AddComponent<Main>();
            UnityEngine.Object.DontDestroyOnLoad(MGameObject);
        }
        public static void Unload()
        {
            Utils.FreeConsole();
            UnityEngine.Object.Destroy(MGameObject); 
        }
    }
}