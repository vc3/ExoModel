﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExoGraph.UnitTest
{
	public interface ICategory<TUser, TCategory, TPriority, TRequest, TRequestList, TCategoryList>
		where TUser : IUser<TUser, TCategory, TPriority, TRequest, TRequestList, TCategoryList>
		where TCategory : ICategory<TUser, TCategory, TPriority, TRequest, TRequestList, TCategoryList>
		where TPriority : IPriority<TUser, TCategory, TPriority, TRequest, TRequestList, TCategoryList>
		where TRequest : IRequest<TUser, TCategory, TPriority, TRequest, TRequestList, TCategoryList>
		where TRequestList : ICollection<TRequest>
		where TCategoryList : ICollection<TCategory>
	{
		string Name { get; set; }
		TCategory ParentCategory { get; set; }
		TCategoryList ChildCategories { get; }
	}
}