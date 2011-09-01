﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ExoGraph
{
	/// <summary>
	/// Base class for classes that represent specific events with an object graph.
	/// </summary>
	[DataContract]
	[KnownType(typeof(GraphSaveEvent))]
	[KnownType(typeof(GraphInitEvent))]
	[KnownType(typeof(GraphInitEvent.InitNew))]
	[KnownType(typeof(GraphInitEvent.InitExisting))]
	[KnownType(typeof(GraphDeleteEvent))]
	[KnownType(typeof(GraphPropertyGetEvent))]
	[KnownType(typeof(GraphValueChangeEvent))]
	[KnownType(typeof(GraphReferenceChangeEvent))]
	[KnownType(typeof(GraphListChangeEvent))]
	public abstract class GraphEvent : EventArgs
	{
		string id;
		GraphInstance instance;

		/// <summary>
		/// Creates a new <see cref="GraphEvent"/> for the specified <see cref="GraphType"/> and id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="type"></param>
		internal GraphEvent(GraphType type, string id)
		{
			this.Instance = new GraphInstance(type, id);
		}

		/// <summary>
		/// Creates a new <see cref="GraphEvent"/> for the specified <see cref="GraphInstance"/>.
		/// </summary>
		/// <param name="instance">The instance the event is for</param>
		internal GraphEvent(GraphInstance instance)
		{
			this.Instance = instance;
		}

		/// <summary>
		/// Gets the <see cref="GraphInstance"/> the event is for.
		/// </summary>
		public GraphInstance Instance
		{
			get
			{
				return instance;
			}
			internal set
			{
				instance = value;
				id = instance.Id;
			}
		}

		/// <summary>
		/// Gets the id of the instance at the moment the event occurred, which may be different than the current id of the instance.
		/// </summary>
		public string InstanceId
		{
			get
			{
				return id;
			}
		}

		/// <summary>
		/// Starts a new <see cref="GraphEventScope"/>, allows subclasses to perform
		/// event specific notifications by overriding <see cref="OnNotify"/>, and
		/// notifies the context that the event has occurred.
		/// </summary>
		internal void Notify()
		{
			using (new GraphEventScope(this))
			{
				if (OnNotify())
					instance.Type.Context.Notify(this);
			}
		}

		/// <summary>
		/// Allows subclasses to perform event specific notification logic.
		/// </summary>
		protected abstract bool OnNotify();

		/// <summary>
		/// Verifies that the specified <see cref="GraphInstance"/> refers to a valid real instance
		/// and if not, uses the type and id information to look up the real instance.
		/// </summary>
		/// <param name="transaction"></param>
		/// <param name="instance"></param>
		/// <returns></returns>
		protected GraphInstance EnsureInstance(GraphTransaction transaction, GraphInstance instance)
		{
			return instance.Instance == null ? transaction.GetInstance(instance.Type, instance.Id) : instance;
		}

		/// <summary>
		/// Attempts to merge two graph events into a single event.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		internal bool Merge(GraphEvent e)
		{
			return Instance == e.Instance && GetType() == e.GetType() && OnMerge(e);
		}

		protected virtual bool OnMerge(GraphEvent e)
		{
			return false;
		}

		/// <summary>
		/// Indicates whether the current event is valid and represents a real change to the model.
		/// </summary>
		internal virtual bool IsValid
		{
			get
			{
				return true;
			}
		}
	}
}
