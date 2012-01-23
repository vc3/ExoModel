﻿using System;
using System.Runtime.Serialization;

namespace ExoGraph
{
	public interface ITransactedGraphEvent
	{
		void Perform(GraphTransaction transaction);

		void Rollback(GraphTransaction transaction);
	}
}
