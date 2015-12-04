using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace June {

	/// <summary>
	/// Utility methods.
	/// </summary>
	public class Util {

		/// <summary>
		/// Reads the text from resource.
		/// </summary>
		/// <returns>The text from resource.</returns>
		/// <param name="resourceName">Resource name.</param>
		public static string ReadTextFromResource (string resourceName) {
			TextAsset data = (TextAsset)Resources.Load (resourceName, typeof(TextAsset));
			return null != data ? data.text : null;
		}

		/// <summary>
		/// Gets the first element in the collection.
		/// </summary>
		/// <typeparam name="T">Type</typeparam>
		/// <param name="list">The list.</param>
		/// <param name="comparator">The comparator.</param>
		/// <returns></returns>
		public static T FirstOrDefault<T> (IEnumerable<T> list, Predicate<T> predicate) {
			foreach (T item in list) {
				if (predicate (item)) {
					return item;
				}
			}
			return default(T);
		}
	}
}