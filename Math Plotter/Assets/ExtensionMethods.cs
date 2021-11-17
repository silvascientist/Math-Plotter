using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ExtensionMethods
{
    public static class Extensions
    {
        public static bool TryPeek<T>(this Queue<T> q, out T val)
        {
            if (q.Count == 0)
            {
                val = default;
                return false;
            }
            else
            {
                val = q.Peek();
                return true;
            }
        }
    } 
}
