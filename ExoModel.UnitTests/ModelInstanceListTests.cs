﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExoModel.UnitTests.Models;
using ExoModel.UnitTests.Models.Requests;

namespace ExoModel.UnitTests
{
	[TestClass]
	[TestModel(Name = "Requests")]
	public class ModelInstanceListTests : TestModelBase
	{
		private void UpdateChildren(Category parent, IEnumerable<Category> children)
		{
			ModelContext.Current.GetModelInstance(parent).GetList("ChildCategories").Update(children.Select(c => ModelContext.Current.GetModelInstance(c)));
		}

		/// <summary>
		/// Simply update the state of the list from empty to the target list.
		/// </summary>
		[TestMethod]
		public void UpdateCollectionBasic()
		{
			// Set up data
			var parent = new Category { Name = "Sports" };
			var children = new List<Category>
			{
				new Category {Name = "Football"},
				new Category {Name = "Baseball"},
				new Category {Name = "Basketball"},
			};

			// Update the list
			UpdateChildren(parent, children);

			// Assert the expected state of the list
			Assert.AreEqual(3, parent.ChildCategories.Count, "All 3 children should be in the source list.");
			Assert.AreEqual("Football", parent.ChildCategories.ToArray()[0].Name, "The first sport should be 'Football'.");
			Assert.AreEqual("Baseball", parent.ChildCategories.ToArray()[1].Name, "The second sport should be 'Baseball'.");
			Assert.AreEqual("Basketball", parent.ChildCategories.ToArray()[2].Name, "The third sport should be 'Basketball'.");
		}

		/// <summary>
		/// Duplicates are emoved if the values are not duplicated in the target set.
		/// </summary>
		[TestMethod]
		public void UpdateCollectionWithDupesRemoved()
		{
			// Set up data
			var parent = new Category { Name = "Sports" };
			var children = new List<Category>
			{
				new Category {Name = "Football"},
				new Category {Name = "Baseball"},
				new Category {Name = "Basketball"},
			};

			// Add the children twice
			foreach (var child in children)
				parent.ChildCategories.Add(child);
			foreach (var child in children)
				parent.ChildCategories.Add(child);

			// Assert the expected state of the list
			Assert.AreEqual(6, parent.ChildCategories.Count, "All 3 children should be duplicated in the source list.");
			Assert.AreEqual("Football", parent.ChildCategories.ToArray()[0].Name, "The first sport should be 'Football'.");
			Assert.AreEqual("Baseball", parent.ChildCategories.ToArray()[1].Name, "The second sport should be 'Baseball'.");
			Assert.AreEqual("Basketball", parent.ChildCategories.ToArray()[2].Name, "The third sport should be 'Basketball'.");
			Assert.AreEqual("Football", parent.ChildCategories.ToArray()[3].Name, "The fourth sport should be 'Football'.");
			Assert.AreEqual("Baseball", parent.ChildCategories.ToArray()[4].Name, "The fifth sport should be 'Baseball'.");
			Assert.AreEqual("Basketball", parent.ChildCategories.ToArray()[5].Name, "The sixth sport should be 'Basketball'.");

			// Update the list
			UpdateChildren(parent, children);

			// Assert the expected state of the list
			Assert.AreEqual(3, parent.ChildCategories.Count, "All 3 children should be duplicated in the source list.");
			Assert.AreEqual("Football", parent.ChildCategories.ToArray()[0].Name, "The first sport should be 'Football'.");
			Assert.AreEqual("Baseball", parent.ChildCategories.ToArray()[1].Name, "The second sport should be 'Baseball'.");
			Assert.AreEqual("Basketball", parent.ChildCategories.ToArray()[2].Name, "The third sport should be 'Basketball'.");
		}

		/// <summary>
		/// When an item shows up more than once all occurances should be removed if it is not in the target set.
		/// </summary>
		[TestMethod]
		public void UpdateCollectionWithDupesCleared()
		{
			// Set up data
			var parent = new Category { Name = "Sports" };
			var children = new List<Category>
			{
				new Category {Name = "Football"},
				new Category {Name = "Baseball"},
				new Category {Name = "Basketball"},
			};

			// Add the first child category twice
			parent.ChildCategories.Add(children[0]);
			parent.ChildCategories.Add(children[0]);

			// Assert the expected state of the list
			Assert.AreEqual(2, parent.ChildCategories.Count, "There should be 2 children in the source list.");
			Assert.AreEqual("Football", parent.ChildCategories.ToArray()[0].Name, "The first sport should be 'Football'.");
			Assert.AreEqual("Football", parent.ChildCategories.ToArray()[1].Name, "The second sport should be 'Football'.");

			// Update the list
			UpdateChildren(parent, new Category[0]);

			// Assert the expected state of the list
			Assert.AreEqual(0, parent.ChildCategories.Count, "Should no longer be any children.");
		}
	}
}
