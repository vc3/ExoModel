﻿using System;
using System.Linq;
using System.Collections.Generic;

namespace ExoModel
{
	public class ModelStep
	{
		#region Constructors

		internal ModelStep(ModelPath path)
		{
			this.Path = path;
			this.NextSteps = new ModelStepList();
		}

		#endregion

		#region Properties

		public ModelPath Path { get; private set; }

		public ModelStep PreviousStep { get; internal set; }

		public ModelProperty Property { get; internal set; }

		public ModelType Filter { get; internal set; }

		public ModelStepList NextSteps { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Recursively walks up the path the current step is a member of until the
		/// root is reached and then initiates path change notification events.
		/// </summary>
		/// <param name="instance"></param>
		internal void Notify(ModelInstance instance)
		{
			// Exit immediately if the instance is not valid for the current step filter
			if (Filter != null && !Filter.IsInstanceOfType(instance))
				return;

			// Keep walking if there are more steps
			if (PreviousStep != null)
			{
				foreach (ModelReference parentReference in instance.GetInReferences((ModelReferenceProperty)PreviousStep.Property))
					PreviousStep.Notify(parentReference.In);
			}
			// Otherwise, notify the path with the first instance instance
			else
				Path.Notify(instance);
		}

		/// <summary>
		/// Enumerates over the set of instances represented by the current step.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public IEnumerable<ModelInstance> GetInstances(ModelInstance instance)
		{
			// Stop loading if the step is null or represents a value
			if (Property is ModelValueProperty || !((ModelReferenceProperty)Property).DeclaringType.IsInstanceOfType(instance))
				yield break;

			// Cast the property to the correct type
			var reference = (ModelReferenceProperty)Property;

			// Return each instance exposed by a list property
			if (reference.IsList)
			{
				ModelInstanceList children = instance.GetList(reference);
				if (children != null)
				{
					foreach (ModelInstance child in instance.GetList(reference))
					{
						if (Filter == null || Filter.IsInstanceOfType(child))
							yield return child;
					}
				}
			}

			// Return the instance exposed by a reference property
			else
			{
				ModelInstance child = instance.GetReference(reference);
				if (child != null && (Filter == null || Filter.IsInstanceOfType(child)))
					yield return child;
			}
		}

		/// <summary>
		/// Returns the name of the property and all child steps.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if (Property == null)
				return "?";
			else if (NextSteps == null || NextSteps.Count == 0)
				return Property.Name + "<" + (Filter == null ? Property.DeclaringType.Name : Filter.Name) + ">";
			else if (NextSteps.Count == 1)
				return Property.Name + "<" + (Filter == null ? Property.DeclaringType.Name : Filter.Name) + ">" + "." + NextSteps.First();
			else
				return Property.Name + "<" + (Filter == null ? Property.DeclaringType.Name : Filter.Name) + ">" + "{" + NextSteps.Aggregate("", (p, s) => p.Length > 0 ? p + "," + s : s.ToString()) + "}";
		}

		#endregion
	}
}