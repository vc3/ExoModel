﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace ExoGraph
{
	/// <summary>
	/// Base class for graph contexts that work with strongly-typed object graphs based on compiled types
	/// using inheritence and declared properties for associations and intrinsic types.
	/// </summary>
	public abstract class StronglyTypedGraphContext : GraphContext
	{
		#region Fields

		IDictionary<Type, GraphType> graphTypes;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new <see cref="StronglyTypesGraphContext"/> based on the specified types
		/// and also including properties declared on the specified base types.
		/// </summary>
		/// <param name="types">The types to create graph types from</param>
		/// <param name="baseTypes">The base types that contain properties to include on graph types</param>
		public StronglyTypedGraphContext(IEnumerable<Type> types, IEnumerable<Type> baseTypes)
		{
			// The list of types cannot be null
			if (types == null)
				throw new ArgumentNullException("types");

			// The list of base types is not required, so convert null to empty set
			if (baseTypes == null)
				baseTypes = new Type[0];

			// Infer the graph types based on the specified types
			graphTypes = InferGraphTypes(types, baseTypes);
		}
		
		#endregion

		#region Methods

		/// <summary>
		/// Gets the <see cref="GraphType"/> for the specified graph object instance.
		/// </summary>
		/// <param name="instance">The actual graph object instance</param>
		/// <returns>The graph type of the object if it is a valid graph type, otherwise null</returns>
		protected internal override GraphType GetGraphType(object instance)
		{
			if (instance == null)
				return null;

			GraphType graphType;
			graphTypes.TryGetValue(instance.GetType(), out graphType);
			return graphType;
		}

		/// <summary>
		/// Infers the <see cref="GraphType"/> instances based on the specified types and also
		/// includes properties declared on the specified base types.
		/// </summary>
		/// <param name="types">The types to create graph types from</param>
		/// <param name="baseTypes">The base types that contain properties to include on graph types</param>
		/// <returns>A dictionary of inferred <see cref="Type"/> and <see cref="GraphType"/> pairs</returns>
		IDictionary<Type, GraphType> InferGraphTypes(IEnumerable<Type> types, IEnumerable<Type> baseTypes)
		{
			// Create instance types for each specified type
			SortedList<Type, GraphType> graphTypes = new SortedList<Type, GraphType>(new TypeComparer());
			foreach (Type type in types)
			{
				GraphType graphType = CreateGraphType(type.AssemblyQualifiedName);
				graphTypes.Add(type, graphType);
			}

			// Create a dictionary of valid declaring types to introspect
			Dictionary<Type, Type> declaringTypes = new Dictionary<Type, Type>();
			foreach (Type type in types)
			{
				if (!declaringTypes.ContainsKey(type))
					declaringTypes.Add(type, type);
			}
			foreach (Type baseType in baseTypes)
			{
				if (!declaringTypes.ContainsKey(baseType))
					declaringTypes.Add(baseType, baseType);
			}

			// Declare graph type variables to track base and sub types
			GraphType baseGraphType = null;
			GraphType subGraphType = null;

			// Initialize each instance type
			foreach (KeyValuePair<Type, GraphType> typePair in graphTypes)
			{
				// Establish parent-child relationships between instance types
				Type type = typePair.Key;
				GraphType graphType = typePair.Value;
				Type baseType = type.BaseType;
				while (baseType != null && !graphTypes.TryGetValue(baseType, out baseGraphType))
					baseType = baseType.BaseType;
				if (baseGraphType != null)
					SetBaseType(graphType, baseGraphType);

				// Process all properties on the instance type to create references
				foreach (PropertyInfo property in type.GetProperties())
				{
					// Exit immediately if the property was not in the list of valid declaring types
					if (!declaringTypes.ContainsKey(property.DeclaringType))
						continue;

					// Copy properties inherited from base graph types
					if (baseGraphType != null && baseGraphType.Properties.Contains(property.Name))
						AddProperty(graphType, baseGraphType.Properties[property.Name]);

					// Create references based on properties that relate to other instance types
					else if (graphTypes.TryGetValue(property.PropertyType, out subGraphType))
						AddProperty(graphType, property.Name, subGraphType, false);

					// Create references based on properties that are lists of other instance types
					else if (typeof(IList).IsAssignableFrom(property.PropertyType) &&
						property.PropertyType.GetProperty("Item", new Type[] { typeof(int) }) != null &&
						graphTypes.TryGetValue(property.PropertyType.GetProperty("Item", new Type[] { typeof(int) }).PropertyType, out subGraphType))
						AddProperty(graphType, property.Name, subGraphType, true);

					// Create values for all other properties
					else
						AddProperty(graphType, property.Name, property.PropertyType);
				}
			}

			// Return the inferred graph types
			return graphTypes;
		}

		protected internal override object GetProperty(object instance, string property)
		{
			return instance.GetType().GetProperty(property).GetValue(instance, null);
		}

		protected internal override void SetProperty(object instance, string property, object value)
		{
			instance.GetType().GetProperty(property).SetValue(instance, value, null);
		}

		protected internal override void AddToList(object instance, string property, object value)
		{
			((IList)GetProperty(instance, property)).Add(value);
		}

		protected internal override bool RemoveFromList(object instance, string property, object value)
		{
			IList list = (IList)GetProperty(instance, property);
			if (list.Contains(value))
			{
				list.Remove(value);
				return true;
			}
			return false;
		}

		#endregion

		#region TypeComparer

		/// <summary>
		/// Specialized <see cref="IComparer<T>"/> implementation that sorts types
		/// first in order of inheritance and second by name.
		/// </summary>
		class TypeComparer : Comparer<Type>
		{
			public override int Compare(Type x, Type y)
			{
				if (x == y)
					return 0;
				else if (x.IsSubclassOf(y))
					return -1;
				else if (y.IsSubclassOf(x))
					return 1;
				else
					return x.FullName.CompareTo(y.FullName);
			}
		}

		#endregion
	}
}
