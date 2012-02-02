﻿using ExoModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExoModel.UnitTest
{
	/// <summary>
	/// Base validation test that ensures a model provider correctly supports all of the requisite model events.
	/// </summary>
	[TestClass]
	public abstract class ModelProviderTest<TUser, TCategory, TPriority, TRequest, TRequestList, TCategoryList>
		where TUser : IUser<TUser, TCategory, TPriority, TRequest, TRequestList, TCategoryList>, new()
		where TCategory : ICategory<TUser, TCategory, TPriority, TRequest, TRequestList, TCategoryList>, new()
		where TPriority : IPriority<TUser, TCategory, TPriority, TRequest, TRequestList, TCategoryList>, new()
		where TRequest : IRequest<TUser, TCategory, TPriority, TRequest, TRequestList, TCategoryList>, new()
		where TRequestList : ICollection<TRequest>
		where TCategoryList : ICollection<TCategory>
	{
		/// <summary>
		/// Verifies that an <see cref="IModelContextProvider"/> has been assigned.
		///</summary>
		[TestMethod]
		public virtual void ProviderTest()
		{
			// Verify that a provider has been set
			Assert.IsNotNull(ModelContext.Provider);
		}

		/// <summary>
		/// Verifies that a current <see cref="ModelContext"/> exists.
		///</summary>
		[TestMethod()]
		public virtual void CurrentTest()
		{
			// Verify that the current context is available
			Assert.IsNotNull(ModelContext.Current);
		}

		/// <summary>
		/// Verifies that base types have been correctly assigned for the test model.
		///</summary>
		[TestMethod()]
		public virtual void SetBaseTypeTest()
		{

		}

		/// <summary>
		/// Verify that the model is saved when <see cref="ModelContext.Save"/> is called.
		///</summary>
		[TestMethod()]
		public virtual void SaveTest()
		{
			ModelInstance userInstance = null;
			ModelSaveEvent saveEvent = null;

			// Start a transaction to track identity changes that occur 
			new ModelTransaction().Record(() =>
			{
				// Create a new user
				TUser user = CreateNewUser();

				// Get the model instance for the new user
				userInstance = ModelContext.Current.GetModelType(user).GetModelInstance(user);

				// Set the username for the new user
				user.UserName = "New User";

				// Ensure that the model instance is new
				Assert.IsTrue(userInstance.IsNew, "Newly created instance was not marked as new.");

				// Save the new user instance
				saveEvent = Perform<ModelSaveEvent>(() => userInstance.Save()).FirstOrDefault();
			});

			// Ensure that the model instance has been saved correctly
			Assert.IsFalse(userInstance.IsNew, "New instance was not saved.");
			Assert.IsTrue(saveEvent != null && saveEvent.Instance == userInstance && saveEvent.Added.Count() == 1, 
				"The save event was not correctly raised during a save operation.");
		}

		/// <summary>
		/// Verify that <see cref="ModelContext.OnPropertyGet"/> is called when a property is accessed.
		///</summary>
		[TestMethod()]
		public virtual void OnPropertyGetTest()
		{
			var category = CreateNewCategory();

			// Test getting a value property
			var propertyGet = Perform<ModelPropertyGetEvent>(() => NoOp(category.Name)).FirstOrDefault();
			Assert.IsNotNull(propertyGet, "The property get event was not raised for a value property.");
			Assert.AreEqual<string>("Name", propertyGet.Property.Name, "The name of the value property that was changed is incorrect.");
			Assert.AreEqual(category, propertyGet.Instance.Instance, "The instance the value property change occured on did not match the event");
			Assert.IsTrue(propertyGet.IsFirstAccess, "The event did not indicate that this was the first access for this value property.");

			// Test second access to a value property
			propertyGet = Perform<ModelPropertyGetEvent>(() => NoOp(category.Name)).FirstOrDefault();
			Assert.IsNotNull(propertyGet, "The property get event was not raised for a value property.");
			Assert.AreEqual<string>("Name", propertyGet.Property.Name, "The name of the value property that was changed is incorrect.");
			Assert.AreEqual(category, propertyGet.Instance.Instance, "The instance the value property change occured on did not match the event");
			Assert.IsFalse(propertyGet.IsFirstAccess, "The event did not indicate that this was not the first access for this value property.");

			// Test getting a reference property
			propertyGet = Perform<ModelPropertyGetEvent>(() => NoOp(category.ParentCategory)).FirstOrDefault();
			Assert.IsNotNull(propertyGet, "The property get event was not raised for a value property.");
			Assert.AreEqual<string>("ParentCategory", propertyGet.Property.Name, "The name of the value property that was changed is incorrect.");
			Assert.AreEqual(category, propertyGet.Instance.Instance, "The instance the value property change occured on did not match the event");
			Assert.IsTrue(propertyGet.IsFirstAccess, "The event did not indicate that this was the first access for this reference property.");

			// Test second access to a reference property
			propertyGet = Perform<ModelPropertyGetEvent>(() => NoOp(category.ParentCategory)).FirstOrDefault();
			Assert.IsNotNull(propertyGet, "The property get event was not raised for a value property.");
			Assert.AreEqual<string>("ParentCategory", propertyGet.Property.Name, "The name of the value property that was changed is incorrect.");
			Assert.AreEqual(category, propertyGet.Instance.Instance, "The instance the value property change occured on did not match the event");
			Assert.IsFalse(propertyGet.IsFirstAccess, "The event did not indicate that this was not the first access for this reference property.");

			// Test getting a list property
			propertyGet = Perform<ModelPropertyGetEvent>(() => NoOp(category.ChildCategories)).FirstOrDefault();
			Assert.IsNotNull(propertyGet, "The property get event was not raised for a value property.");
			Assert.AreEqual<string>("ChildCategories", propertyGet.Property.Name, "The name of the value property that was changed is incorrect.");
			Assert.AreEqual(category, propertyGet.Instance.Instance, "The instance the value property change occured on did not match the event");
			Assert.IsTrue(propertyGet.IsFirstAccess, "The event did not indicate that this was the first access for this list property.");

			// Test second access to a reference property
			propertyGet = Perform<ModelPropertyGetEvent>(() => NoOp(category.ChildCategories)).FirstOrDefault();
			Assert.IsNotNull(propertyGet, "The property get event was not raised for a value property.");
			Assert.AreEqual<string>("ChildCategories", propertyGet.Property.Name, "The name of the value property that was changed is incorrect.");
			Assert.AreEqual(category, propertyGet.Instance.Instance, "The instance the value property change occured on did not match the event");
			Assert.IsFalse(propertyGet.IsFirstAccess, "The event did not indicate that this was not the first access for this list property.");
		}

		// Stub to allow properties to be treated as actions
		void NoOp(object o)
		{ }

		/// <summary>
		/// Verify that <see cref="ModelContext.OnPropertyChanged"/> is called when a property value is changed.
		///</summary>
		[TestMethod()]
		public virtual void OnPropertyChangedTest()
		{
			// Get the relevant model types and create real instances
			ModelType userType = ModelContext.Current.GetModelType<TUser>();
			ModelType requestType = ModelContext.Current.GetModelType<TRequest>();
			TRequest request = CreateNewRequest();
			TUser user = CreateNewUser();

			// Test a value property
			var valueChange = Perform<ModelValueChangeEvent>(() => request.Description = "My New Description").FirstOrDefault();
			Assert.IsTrue(valueChange != null && valueChange.Property.Name == "Description" &&
				String.IsNullOrEmpty((string)valueChange.OldValue) &&	(string)valueChange.NewValue == "My New Description", 
				"Property change was not correctly raised on value property.");

			// Test a reference property
			var referenceChange = Perform<ModelReferenceChangeEvent>(() => request.AssignedTo = user).FirstOrDefault();
			Assert.IsTrue(referenceChange != null && referenceChange.Property.Name == "AssignedTo" &&
				referenceChange.OldValue == null &&	referenceChange.NewValue.Instance == (object)user, 
				"Property change was not correctly raised on reference property.");
		}

		/// <summary>
		/// Verify that <see cref="ModelContext.OnListChanged"/> is called with items are added or removed from a list.
		///</summary>
		[TestMethod()]
		public virtual void OnListChangedTest()
		{
			// Get the relevant model types and create real instances
			ModelType userType = ModelContext.Current.GetModelType<TUser>();
			ModelType requestType = ModelContext.Current.GetModelType<TRequest>();
			TRequest request = CreateNewRequest();
			TUser user = CreateNewUser();

			// Test adding to a list
			var listChange = Perform<ModelListChangeEvent>(() => user.Requests.Add(request)).FirstOrDefault();
			Assert.IsTrue(listChange != null && listChange.Property.Name == "Requests" &&
				listChange.Added.Any() && listChange.Added.FirstOrDefault().Instance == (object)request &&
				!listChange.Removed.Any(), 
				"List change was not correctly raised when adding an item.");

			// Test removing from a list
			listChange = Perform<ModelListChangeEvent>(() => user.Requests.Remove(request)).FirstOrDefault();
			Assert.IsTrue(listChange != null && listChange.Property.Name == "Requests" &&
				listChange.Removed.Any() && listChange.Removed.FirstOrDefault().Instance == (object)request &&
				!listChange.Added.Any(),
				"List change was not correctly raised when removing an item.");
	
			// Test clears
			user.Requests.Add(CreateNewRequest());
			user.Requests.Add(CreateNewRequest());
			user.Requests.Add(CreateNewRequest());
			var listChanges = Perform<ModelListChangeEvent>(() => user.Requests.Clear());
			int removed = 0;
			foreach (var change in listChanges)
			{
				Assert.IsTrue(change.Property.Name == "Requests" && listChange.Removed.Any() && !listChange.Added.Any(),
				"List change was not correctly raised when clearing a list.");
				removed += listChange.Removed.Count();
			}
			Assert.AreEqual<int>(1, removed, "The clear operation did not raise change events for all of the items in the list.");
		}

		/// <summary>
		/// Verify that <see cref="ModelContext.OnInit"/> is called when a new instance is created.
		///</summary>
		[TestMethod()]
		public virtual void OnInitTest()
		{
			// Test init not raise just due to construction
			ModelInitEvent init = Perform<ModelInitEvent>(() => CreateNewPriority()).FirstOrDefault();
			Assert.IsNull(init, "Init event was raised prematurely when a new object was constructed.");

			// Test init raised after first access
			init = Perform<ModelInitEvent>(() => NoOp(CreateNewPriority().Name)).FirstOrDefault();
			Assert.IsNotNull(init, "Init event was not raised when a new object was constructed and accessed.");
	
		}

		/// <summary>
		/// Verifies that <see cref="ModelContext.GetInstance"/> returns a value new or existing instance.
		///</summary>
		[TestMethod()]
		public virtual void GetInstanceTest()
		{
			ModelType userType = ModelContext.Current.GetModelType<TUser>();
			ModelType requestType = ModelContext.Current.GetModelType<TRequest>();
			ModelType priorityType = ModelContext.Current.GetModelType<TPriority>();
			ModelType categoryType = ModelContext.Current.GetModelType<TCategory>();
			
			// Test creating new instance
			ModelInstance request = requestType.Create();
			Assert.IsNotNull(request, "Did not successfully create a new request instance.");
			Assert.IsTrue(request.Instance is TRequest, "Newly created request instance was not the correct type.");

			ModelInstance user = userType.Create();
			Assert.IsNotNull(user, "Did not successfully create a new user instance.");
			Assert.IsTrue(user.Instance is TUser, "Newly created user instance was not the correct type.");

			// Test creating existing instance
			ModelInstance category = categoryType.Create("1");
			Assert.IsNotNull(category, "Did not successfully create a existing category instance.");
			Assert.IsTrue(category.Instance is TCategory, "Existing category instance was not the correct type.");

			ModelInstance priority = priorityType.Create("1");
			Assert.IsNotNull(priority, "Did not successfully create a existing priority instance.");
			Assert.IsTrue(priority.Instance is TPriority, "Existing priority instance was not the correct type.");

			// Save a new instance in order to validate cached loading of existing instances that were just saved
			request["User"] = user.Instance;
			request["Category"] = category.Instance;
			request["Priority"] = priority.Instance;
			request["Description"] = "The is a test request";
			user["UserName"] = "Test User";
			request.Save();
			Assert.IsFalse(request.IsNew, "The request was not saved.");
			Assert.IsFalse(user.IsNew, "The user was not saved.");
			
			// Test getting cached reference to existing instance
			ModelInstance categoryClone = categoryType.Create("1");
			Assert.AreSame(category, categoryClone, "The categories have the same id but were not the same instance as expected.");

			ModelInstance requestClone = requestType.Create(request.Id);
			Assert.AreSame(request, requestClone, "The requests have the same id but were not the same instance as expected.");

		}

		/// <summary>
		/// Verifies that <see cref="ModelContext.GetId"/> returns a valid string identifier for a model instance.
		///</summary>
		[TestMethod()]
		public virtual void GetIdTest()
		{

		}

		/// <summary>
		/// Verifies that <see cref="ModelContext.GetModelType"/> correctly returns the requested <see cref="ModelType"/>.
		///</summary>
		[TestMethod()]
		public virtual void GetModelTypeTest()
		{
			VerifyType(ModelContext.Current.GetModelType("User"));
			VerifyType(ModelContext.Current.GetModelType("Request"));
			VerifyType(ModelContext.Current.GetModelType("Category"));
			VerifyType(ModelContext.Current.GetModelType("Priority"));
		}

		/// <summary>
		/// Verifies that <see cref="ModelContext.GetModelInstance"/> returns the <see cref="ModelInstance"/>
		/// associated with the specified real model object.
		///</summary>
		[TestMethod()]
		public virtual void GetModelInstanceTest()
		{

		}

		/// <summary>
		/// Verifies that <see cref="ModelContext.DeleteInstance"/> successfully marks the
		/// specified instance for deletion.
		///</summary>
		[TestMethod()]
		public virtual void DeleteInstanceTest()
		{

		}

		/// <summary>
		/// Verifies that <see cref="ModelContext.CreateModelType"/> returns a new <see cref="ModelType"/>
		/// that corresponds to the specified type name.
		///</summary>
		[TestMethod()]
		public virtual void CreateModelTypeTest()
		{

		}

		/// <summary>
		/// Verifies that <see cref="ModelContext.ConvertToList"/> returns a valid <see cref="IList"/> instance
		/// given the underlying value of a list property.
		///</summary>
		[TestMethod()]
		public virtual void ConvertToListTest()
		{

		}

		/// <summary>
		/// Verifies that <see cref="ModelContext.BeginTransaction"/> creates a valid <see cref="ModelTransaction"/>
		/// and that the transaction successfully commits and rolls back when requested.
		///</summary>
		[TestMethod()]
		public virtual void BeginTransactionTest()
		{

		}

		public virtual TUser CreateNewUser()
		{
			return new TUser();
		}

		public virtual TCategory CreateNewCategory()
		{
			return new TCategory();
		}

		public virtual TPriority CreateNewPriority()
		{
			return new TPriority();
		}

		public virtual TRequest CreateNewRequest()
		{
			return new TRequest();
		}

		/// <summary>
		/// Verifies that the specified <see cref="ModelType"/> is not null and has the correct meta data.
		/// </summary>
		/// <param name="type"></param>
		void VerifyType(ModelType type)
		{
			Assert.IsNotNull(type, "The model type was not found");
			switch (type.Name)
			{
				case "Request":
					Assert.AreEqual<int>(5, type.Properties.Count, "The Request type does not have the correct number of properties.");
					Assert.IsTrue(type.Properties["User"] is ModelReferenceProperty, "Request model type does not have expected User value property.");
					Assert.IsTrue(type.Properties["Description"] is ModelValueProperty, "Request model type does not have expected Description value property.");
					break;
			}
		}

		/// <summary>
		/// Performs the specified action and returns the last model event raised as
		/// a result of the action or null if not events where raised or the last event
		/// was not of the specified event type.
		/// </summary>
		/// <typeparam name="TEvent"></typeparam>
		/// <param name="action"></param>
		/// <returns></returns>
		IEnumerable<TEvent> Perform<TEvent>(Action action)
			where TEvent : ModelEvent
		{
			events = new List<ModelEvent>();
			try
			{
				ModelContext.Current.Event += UpdateLastEvent;
				action();
				return events.OfType<TEvent>();
			}
			finally
			{
				ModelContext.Current.Event -= UpdateLastEvent;
			}
		}

		List<ModelEvent> events;

		void UpdateLastEvent(object sender, ModelEvent e)
		{
			events.Add(e);
		}
	}

	[TestClass]
	public class DefaultModelProviderTest : ModelProviderTest<User, Category, Priority, Request, ICollection<Request>, ICollection<Category>>
	{
		[TestInitialize]
		public void CreateContext()
		{
			ModelContext.Init(new TestModelTypeProvider());

			foreach (var entity in CreatePersistedEntities())
				ModelContext.Current.GetModelInstance(entity).Save();
		}

		IEnumerable<TestEntity> CreatePersistedEntities()
		{
			yield return new Category() { Name = "Client" };
			yield return new Priority() { Name = "High" };
		}

		[TestCleanup]
		public void DisposeContext()
		{
			ModelContext.Current = null;
		}
	}
}
