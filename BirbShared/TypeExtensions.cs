using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using HarmonyLib;

namespace MoonShared
{
    public static class TypeExtensions
    {
        /// <summary>Gets a method and asserts that it was found.</summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <param name="name">The method name.</param>
        /// <param name="parameters">The method parameter types, or <c>null</c> if it's not overloaded.</param>
        /// <returns>The corresponding <see cref="MethodInfo"/>, if found.</returns>
        /// <exception cref="MissingMethodException">If a matching method is not found.</exception>
        [DebuggerStepThrough]
        public static MethodInfo RequireMethod(this Type type, string name, Type[]? parameters)
        {
            return AccessTools.Method(type, name, parameters);
        }

        /// <summary>Get a value from an array if it's in range.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="array">The array to search.</param>
        /// <param name="index">The index of the value within the array to find.</param>
        /// <param name="value">The value at the given index, if found.</param>
        /// <returns>Returns whether the index was within the array bounds.</returns>
        public static bool TryGetIndex<T>(this T[] array, int index, out T value)
        {
            if (array == null || index < 0 || index >= array.Length)
            {
                value = default;
                return false;
            }

            value = array[index];
            return true;
        }

        /// <summary>Get a value from an array if it's in range, else get the default value.</summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="array">The array to search.</param>
        /// <param name="index">The index of the value within the array to find.</param>
        /// <param name="defaultValue">The default value if the value isn't in range.</param>
        public static T GetOrDefault<T>(this T[] array, int index, T defaultValue = default)
        {
            return array.TryGetIndex(index, out T value)
                ? value
                : defaultValue;
        }
    }
}
