﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace ExoGraph.UnitTest
{
	public class User : TestEntity, IUser<User, Category, Priority, Request, ICollection<Request>, ICollection<Category>>
	{
		public string UserName
		{
			get { return Get(() => UserName); }
			set { Set(() => UserName, value); }
		}

		public ICollection<Request> Requests
		{
			get { return Get(() => Requests); }
			set { Set(() => Requests, value); }
		}

		public ICollection<Request> Assignments
		{
			get { return Get(() => Assignments); }
			set { Set(() => Assignments, value); }
		}
	}
}
