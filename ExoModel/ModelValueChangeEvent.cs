﻿namespace ExoModel
{
	/// <summary>
	/// Represents a change to a value property in the model.
	/// </summary>
	public class ModelValueChangeEvent : ModelEvent, ITransactedModelEvent
	{
		public ModelValueChangeEvent(ModelInstance instance, ModelValueProperty property, object oldValue, object newValue)
			: base(instance)
		{
			this.Property = property;

			if (property.AutoConvert)
			{
				this.OldValue = oldValue == null ? null : property.Converter.ConvertTo(oldValue, typeof(object));
				this.NewValue = newValue == null ? null : property.Converter.ConvertTo(newValue, typeof(object));
			}
			else
			{
				this.OldValue = oldValue;
				this.NewValue = newValue;
			}
		}

		public ModelValueProperty Property { get; private set; }

		public object OldValue { get; private set; }
	
		public object NewValue { get; private set; }
		
		/// <summary>
		/// Indicates whether the current event is valid and represents a real change to the model.
		/// </summary>
		internal override bool IsValid
		{
			get
			{
				return (OldValue == null ^ NewValue == null) || (OldValue != null && !OldValue.Equals(NewValue));
			}
		}

		/// <summary>
		/// Notify subscribers that the property value has changed.
		/// </summary>
		protected override void OnNotify()
		{
			Property.NotifyPathChange(Instance);

			// Raise value change on all types in the inheritance hierarchy
			for (ModelType type = Instance.Type; type != null; type = type.BaseType)
			{
				type.RaiseValueChange(this);

				// Stop walking the type hierarchy if this is the type that declares the property that was accessed
				if (type == Property.DeclaringType)
					break;
			}
		}

		/// <summary>
		/// Merges a <see cref="ModelValueChangeEvent"/> into the current event.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		protected override bool OnMerge(ModelEvent e)
		{
			// Ensure the events are for the same value property
			if (((ModelValueChangeEvent)e).Property != Property)
				return false;

			NewValue = ((ModelValueChangeEvent)e).NewValue;
			return true;
		}

		/// <summary>
		/// Returns the description of the property value change.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("Changed {0} on '{1}' from '{2}' to '{3}'", Property, Instance, OldValue, NewValue);
		}

		#region ITransactedModelEvent Members

		void ITransactedModelEvent.Perform(ModelTransaction transaction)
		{
			Instance = EnsureInstance(transaction, Instance);
			Instance.SetValue(Property, NewValue);
		}

		/// <summary>
		/// Restores the property to the old value.
		/// </summary>
		void ITransactedModelEvent.Rollback(ModelTransaction transaction)
		{
			Instance = EnsureInstance(transaction, Instance);
			Instance.SetValue(Property, OldValue);
		}

		#endregion
	}
}
