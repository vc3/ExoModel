﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ExoGraph
{
	public class DescriptorGraphTypeProvider<T> : DynamicGraphTypeProvider<ICustomTypeDescriptor, PropertyDescriptor>
		where T : class, ICustomTypeDescriptor
	{
		#region Fields

		Func<string, T> create;
		Func<GraphInstance, string> getScopeName;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new <see cref="DescriptorGraphTypeProvider"/> based on the specified types.
		/// </summary>
		/// <param name="namespace"></param>
		/// <param name="create"></param>
		public DescriptorGraphTypeProvider(string @namespace, Func<string, T> create, Func<GraphInstance, string> getScopeName)
			: base(@namespace, GraphContext.Current.GetGraphType<T>().Name)
		{
			this.create = create;
			this.getScopeName = getScopeName;
		}

		#endregion

		#region Methods

		protected override object CreateInstance(ICustomTypeDescriptor type)
		{
			return create(type.GetClassName());
		}

		protected override IEnumerable<PropertyDescriptor> GetProperties(ICustomTypeDescriptor type)
		{
			return type.GetProperties().Cast<PropertyDescriptor>();
		}

		protected override ICustomTypeDescriptor GetTypeSource(string typeName)
		{
			return create(typeName);
		}

		protected override ICustomTypeDescriptor GetTypeSource(object instance)
		{
			return (ICustomTypeDescriptor)instance;
		}

		protected override Attribute[] GetTypeAttributes(ICustomTypeDescriptor type)
		{
			return type.GetAttributes().Cast<Attribute>().ToArray();
		}

		protected override string GetClassName(ICustomTypeDescriptor instance)
		{
			return instance.GetClassName();
		}

		protected override string GetPropertyName(PropertyDescriptor property)
		{
			return property.Name;
		}

		protected override Attribute[] GetPropertyAttributes(PropertyDescriptor property)
		{
			return property.Attributes.Cast<Attribute>().ToArray();
		}

		protected override bool IsList(PropertyDescriptor property)
		{
			Type itemType;
			GraphContext.Current.GetGraphType<T>().TryGetListItemType(property.PropertyType, out itemType);
			return itemType != null;
		}

		protected override GraphType GetReferenceType(PropertyDescriptor property)
		{
			Type itemType;
			GraphContext.Current.GetGraphType<T>().TryGetListItemType(property.PropertyType, out itemType);
			return GraphContext.Current.GetGraphType(itemType ?? property.PropertyType);
		}

		protected override string GetScopeName(GraphInstance instance)
		{
			return getScopeName(instance);
		}

		protected override TypeConverter GetValueConverter(PropertyDescriptor property)
		{
			return property.Converter;
		}

		protected override Type GetValueType(PropertyDescriptor property)
		{
			return property.PropertyType;
		}

		protected override object GetPropertyValue(object instance, PropertyDescriptor property)
		{
			return property.GetValue(instance);
		}

		protected override void SetPropertyValue(object instance, PropertyDescriptor property, object value)
		{
			property.SetValue(instance, value);
		}

		#endregion
	}
}
