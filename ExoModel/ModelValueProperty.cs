﻿using System;
using System.ComponentModel;

namespace ExoModel
{
	/// <summary>
	/// Represents a property that exposes strongly-typed data as leaves of a model hierarchy.
	/// </summary>
	public abstract class ModelValueProperty : ModelProperty
	{
		#region Constructors

		protected internal ModelValueProperty(ModelType declaringType, string name, string label, string helptext, string format, bool isStatic, Type propertyType, TypeConverter converter, bool isList, bool isReadOnly, bool isPersisted, Attribute[] attributes)
			: base(declaringType, name, label, helptext, format, isStatic, isList, isReadOnly, isPersisted, attributes)
		{
			this.PropertyType = propertyType;
			this.Converter = converter;
			this.AutoConvert = converter != null && converter.CanConvertTo(typeof(object));
			this.FormatProvider = declaringType.GetFormatProvider(propertyType);
		}

		#endregion

		#region Properties

		public Type PropertyType { get; private set; }

		public TypeConverter Converter { get; private set; }

		internal IFormatProvider FormatProvider { get; private set; }

		internal bool AutoConvert { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Attempts to coerce the specific value into the appropriate object
		/// representation for the current property type.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public object CoerceValue(object value)
		{
			if (value == null)
				return null;
			if (AutoConvert)
			{
				if (Converter.CanConvertFrom(value.GetType()))
					value = Converter.ConvertFrom(value);
				else if (value.GetType() != PropertyType)
					value = Convert.ChangeType(value, PropertyType);

				return Converter.ConvertTo(value, typeof(object));
			}
			else if (Converter != null && Converter.CanConvertFrom(value.GetType()))
				return Converter.ConvertFrom(value);
			else if (value.GetType() == PropertyType)
				return value;
			else if (PropertyType.IsGenericType && PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
				return Convert.ChangeType(value, PropertyType.GetGenericArguments()[0]);
			else
				return Convert.ChangeType(value, PropertyType);
		}

		/// <summary>
		/// Gets the formatted representation of a value based on the formatting rules
		/// defined for the current property.
		/// </summary>
		/// <param name="value">The correct value type to format</param>
		/// <returns>The formatted value</returns>
		public string FormatValue(object value)
		{
			return FormatValue(value, null);
		}

		/// <summary>
		/// Gets the formatted representation of a value based on the formatting rules
		/// defined for the current property.
		/// </summary>
		/// <param name="value">The correct value type to format</param>
		/// <param name="format">The format specifier, or null to use the default property format</param>
		/// <returns>The formatted value</returns>
		public string FormatValue(object value, string format)
		{
			if (value == null)
				return "";
			format = format ?? Format;
			if (format == null || format == "")
			{
				if (FormatProvider == null)
					return value.ToString();
				else
					return String.Format(FormatProvider, "{0}", value);
			}
			return String.Format(FormatProvider, "{0:" + format + "}", value);
		}

		/// <summary>
		/// Gets the formatted value of the property for the specified instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		internal override string GetFormattedValue(ModelInstance instance, string format)
		{
			return FormatValue(GetValue(instance.Instance), format ?? Format);
		}

		#endregion
	}
}
