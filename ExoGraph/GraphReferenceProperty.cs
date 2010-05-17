﻿using System;
namespace ExoGraph
{
	/// <summary>
	/// Represents a property that associates two types in a graph hierarchy.
	/// </summary>
	[Serializable]
	public abstract class GraphReferenceProperty : GraphProperty
	{
		#region Fields

		GraphType propertyType;
		bool isList;
		bool isBoundary;

		#endregion

		#region Constructors

		protected internal GraphReferenceProperty(GraphType declaringType, string name, bool isStatic, bool isBoundary, GraphType propertyType, bool isList, Attribute[] attributes)
			: base(declaringType, name, isStatic, isList, attributes)
		{
			this.propertyType = propertyType;
			this.isBoundary = isBoundary;
		}

		#endregion

		#region Properties

		public GraphType PropertyType
		{
			get
			{
				return propertyType;
			}
		}

		public bool IsBoundary
		{
			get
			{
				return isBoundary;
			}
		}

		#endregion
	}
}
