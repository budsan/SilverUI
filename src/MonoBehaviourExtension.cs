using System;
using UnityEngine;
using System.Collections.Generic;

namespace Silver
{
    /// <summary>
    /// A static class that extends the functionalities of Unity's GameObject class.
    /// </summary>
    public static class MonoBehaviourExtension
    {

        /// <summary>
        /// Gets the component T of the GameObject. If such component does not exists, it creates a new 
        /// component of type T and returns it. 
        /// </summary>
        /// <returns>
        /// The component T of the GameObject. 
        /// </returns>
        /// </param>
        /// <typeparam name='T'>
        /// The type of component to return. 
        /// </typeparam>
        public static T AddOrGetComponent<T>(this GameObject gameObject) where T : Component
        {
            T comp = gameObject.GetComponent<T>();
            if (comp == null)
                comp = gameObject.AddComponent<T>();

            return comp;
        }

        /// <summary>
        /// Finds the requested manager in the scene. If the manager doesn't exist it creates the manager. 
        /// Example call: gameObject.FindManagerOrCreateIt<ClientManager>();
        /// </summary>
        /// <returns>
        /// The manager.
        /// </returns>
        public static T FindManagerOrCreateIt<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.FindObjectOrCreateIt<T>(typeof(T).FullName);
        }

        /// <summary>
        /// Finds the requested object in the scene. If the object doesn't exist it creates the manager.
        /// </summary>
        /// <returns>
        /// The object
        /// </returns>
        public static T FindObjectOrCreateIt<T>(this GameObject gameObject, string nameIfNotFound) where T : Component
        {
            T res = GameObject.FindObjectOfType<T>();
            if (res == null)
            {
                GameObject go = new GameObject(nameIfNotFound);
                res = go.AddComponent<T>();
            }
            return res;
        }

        /// <summary>
        /// Returns a list of all active loaded objects of Type type.
        /// </summary>
        /// <returns>
        /// Returns a list of all active loaded objects of Type type.
        /// </returns>
        public static T[] FindObjectsOfType<T>(this GameObject gameObject)
        {
            T[] res = GameObject.FindObjectsOfType(typeof(T)) as T[];
            return res;
        }
    }

}