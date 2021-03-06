﻿using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;

namespace ExoModel
{
	/// <summary>
	/// Base class for read only lists of items keyed by name.
	/// </summary>
	public abstract class ReadOnlyList<TItem> : IEnumerable<TItem>
	{
		Dictionary<string, TItem> list = new Dictionary<string, TItem>();
		SortedList sorted;
		Converter<TItem, object> sortKey;

		protected ReadOnlyList()
		{
		}

		protected ReadOnlyList(IEnumerable<TItem> items)
			: this()
		{
			foreach (TItem item in items)
				Add(item);
		}

		protected ReadOnlyList(Converter<TItem, object> sortKey)
		{
			this.sorted = new SortedList();
			this.sortKey = sortKey;
		}

		/// <summary>
		/// Gets the number of items in the list.
		/// </summary>
		public int Count
		{
			get
			{
				return list.Count;
			}
		}

		/// <summary>
		/// Gets the item in the list with the specified name or
		/// returns null if an item does not exist with the given name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public TItem this[string name]
		{
			get
			{
				TItem type;
				list.TryGetValue(name, out type);
				return type;
			}
		}

		/// <summary>
		/// Determines whether an item in the list exists with the specified name.
		/// </summary>
		/// <param name="name">The name of the item to find</param>
		/// <returns>True if the item exists, otherwise false</returns>
		public bool Contains(string name)
		{
			return list.ContainsKey(name);
		}

		/// <summary>
		/// Determines whether an item is in the list.
		/// </summary>
		/// <param name="item">The item to find</param>
		/// <returns>True if the item exists, otherwise false</returns>
		public bool Contains(TItem item)
		{
			return list.ContainsValue(item);
		}

		/// <summary>
		/// Enumerates over the items in the list.
		/// </summary>
		/// <returns></returns>
		IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
		{
			if (sorted != null)
				return sorted.Values.Cast<TItem>().GetEnumerator();

			return list.Values.GetEnumerator();
		}

		/// <summary>
		/// Enumerates over the items in the list.
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			if (sorted != null)
				return sorted.Values.GetEnumerator();

			return list.Values.GetEnumerator();
		}

		/// <summary>
		/// Returns the name of the item.
		/// </summary>
		/// <param name="item"></param>
		protected abstract string GetName(TItem item);

		/// <summary>
		/// Allows subclasses to add items to the internal list.
		/// </summary>
		/// <param name="item">The item to add</param>
		internal void Add(TItem item)
		{
			list.Add(GetName(item), item);

			if(sorted != null)
				sorted.Add(sortKey(item), item);
		}

		/// <summary>
		/// Allows subclasses to remove items from the internal list.
		/// </summary>
		/// <param name="item">The item to remove</param>
		internal void Remove(TItem item)
		{
			list.Remove(GetName(item));

			if (sorted != null)
				sorted.Remove(sortKey(item));
		}
	}
}
