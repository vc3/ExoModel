﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace ExoModel
{
	public static class Extensions
	{
		public static IEnumerable<ModelType> GetModelTypes(this Assembly assembly)
		{
			ModelContext context = ModelContext.Current;
			return assembly.GetTypes().Select(t => context.GetModelType(t)).Where(t => t != null);
		}

		/// <summary>
		/// Creates a hashset from a linq expression.
		/// See http://blogs.windowsclient.net/damonwildercarr/archive/2008/09/10/expose-new-linq-operations-from-the-screaming-hashset-lt-t-gt-collection.aspx
		/// for more information
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="fromEnumerable"></param>
		/// <param name="comparer"></param>
		/// <returns></returns>
		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> fromEnumerable, IEqualityComparer<T> comparer)
		{
			if (fromEnumerable == null)
				throw new ArgumentException("fromEnumerable");

			if (comparer == null)
				comparer = EqualityComparer<T>.Default;

			return !typeof(HashSet<T>).IsAssignableFrom(fromEnumerable.GetType()) ? new HashSet<T>(fromEnumerable, comparer) : (HashSet<T>) fromEnumerable;
		}

		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> fromEnumerable)
		{
			return ToHashSet(fromEnumerable, EqualityComparer<T>.Default);
		}

		/// <summary>
		/// Utility method that converts
		/// </summary>
		/// <param name="type"></param>
		/// <param name="listType"></param>
		/// <param name="instances"></param>
		/// <returns></returns>
		public static object ToList(this IEnumerable<ModelInstance> instances, ModelType type, Type listType)
		{
			var result = listType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null).Invoke(null);
			var list = result as IList ?? type.ConvertToList(null, result);
			foreach (var instance in instances)
				list.Add(instance.Instance);
			return result;
		}
	}
}
