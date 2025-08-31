using UnityEngine;

namespace CARVES.Utls
{
    public interface ISingletonDependency
    {

    }

    public class DependencySingleton<T> : MonoBehaviour, ISingletonDependency where T : ISingletonDependency
    {
        static T _instance;
        public static T Instance => _instance;

        void Awake()
        {
            if (_instance == null)
            {
                _instance = (T)(this as ISingletonDependency);
            }
            else
            {
                Destroy(gameObject);
            }

            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }
    }
}