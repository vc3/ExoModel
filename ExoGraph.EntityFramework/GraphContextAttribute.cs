﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Afterthought;
using System.Data.Objects;
using System.Data.Entity;

namespace ExoGraph.EntityFramework
{
	public class GraphContextAttribute : Attribute, IAmendmentAttribute
	{
		IEnumerable<ITypeAmendment> IAmendmentAttribute.GetAmendments(Type target)
		{
			// Ensure the target is a DbContext or ObjectContext subclass
			// Use late-binding approach for DbContext to ensure implementation can still support 4.0 without NuGet EF package
			if ((target.IsSubclassOf(typeof(ObjectContext)) && target != typeof(GraphObjectContext)) || (target.BaseType != null && target.BaseType.Name == "System.Data.Entity.DbContext"))
			{
				// Determine the set of entity types from the object context target type
				var entityTypes = target.GetProperties()
					.Select(p => p.PropertyType)
					.Where(t => t.IsGenericType && t.GetGenericTypeDefinition().GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryable<>)))
					.Select(t => t.GetGenericArguments()[0])
					.ToHashSet();

				// Return amendments for each entity type
				foreach (var type in entityTypes)
				{
					var constructor = typeof(EntityAmendment<>).MakeGenericType(type).GetConstructor(new Type[] { typeof(HashSet<Type>) });
					var amendment = constructor.Invoke(new object[] { entityTypes });
					yield return (ITypeAmendment)amendment;
				}
			}
		}
	}
}
