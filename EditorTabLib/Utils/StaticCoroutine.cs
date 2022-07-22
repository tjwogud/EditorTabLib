using System.Collections;
using UnityEngine;

namespace EditorTabLib.Utils
{
    public class StaticCoroutine : MonoBehaviour
    {
        private static StaticCoroutine instance;

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }

        public static void Do(IEnumerator coroutine)
        {
            (instance ?? new GameObject("StaticCoroutine").AddComponent<StaticCoroutine>()).StartCoroutine(coroutine);
        }
    }
}
