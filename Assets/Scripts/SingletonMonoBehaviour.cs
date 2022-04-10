using UnityEngine;

#pragma warning disable 0649

namespace FlockOfBirds
{
    /// <summary>
    /// A basic singleton class for all MonoBehaviour singletons.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T>
    {
        #region Properties
        
        /// <summary>
        /// If TRUE the <see cref="Application.Quit"/> has been called, and you shouldn't use this, or any other <see cref="SingletonMonoBehaviour{T}"/>
        /// </summary>
        private static bool isApplicationQuitting = false;

        /// <summary>
        /// <inheritdoc cref="isApplicationQuitting"/>
        /// </summary>
        public static bool IsApplicationQuitting
        {
            get
            {
                return isApplicationQuitting;
            }
        }

        /// <summary>
        /// Instance of this singleton. Will be set automatically if <see cref="automaticallyAssignAsInstance"/> is set to TRUE.
        /// </summary>
        private static T instance;

        public static T Instance
        {
            get
            {
                if (isApplicationQuitting)
                {
                    Debug.LogError( "Tried to get an instance when the application is quitting!");
                }

                if (instance == null)
                {
                    Debug.LogError( "Instance doesn't exist!");
                }

                return instance;
            }
        }

        #endregion Properties

        #region Unity methods

        protected virtual void Awake()
        {
            isApplicationQuitting = false;

            instance = (T)this;

            OnAwake();
        }

        private void OnApplicationQuit()
        {
            isApplicationQuitting = true;
        }

        #endregion Unity methods

        #region Protected methods

        protected virtual void OnAwake() { }

        #endregion Protected methods
    }
}