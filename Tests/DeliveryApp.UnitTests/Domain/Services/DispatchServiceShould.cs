using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Domain.Services;
using FluentAssertions;
using Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Services
{
	public class DispatchServiceShould
	{
		[Fact]
		public void ReturnValueIsRequiredErrorWhenOrderIsNull()
		{
			//Arrange
			Order order = null;
			var couriers = new List<Courier>()
			{
				Courier.Create("Вася", Transport.Bicycle, Location.MaxLocation).Value,
				Courier.Create("Петя", Transport.Pedestrian, Location.MinLocation).Value
			};

			//Act
			var dispatchService = new DispatchService();
			var result = dispatchService.Dispatch(order, couriers);

			//Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().BeEquivalentTo(GeneralErrors.ValueIsRequired("order"));
		}

		[Fact]
		public void ReturnValueIsRequiredErrorWhenCourierListIsNull()
		{
			//Arrange
			Order order = Order.Create(Guid.NewGuid(), Location.MinLocation).Value;
			List<Courier> couriers = null;

			//Act
			var dispatchService = new DispatchService();
			var result = dispatchService.Dispatch(order, couriers);

			//Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().BeEquivalentTo(GeneralErrors.ValueIsRequired("couriers"));
		}

		[Fact]
		public void ReturnValueIsRequiredErrorWhenCourierListIsEmpty()
		{
			//Arrange
			Order order = Order.Create(Guid.NewGuid(), Location.MinLocation).Value;
			var couriers = new List<Courier>();

			//Act
			var dispatchService = new DispatchService();
			var result = dispatchService.Dispatch(order, couriers);

			//Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().BeEquivalentTo(GeneralErrors.ValueIsRequired("couriers"));
		}

		[Fact]
		public void ReturnCantAssignOrderWhenStatusIsNotCreatedErrorWhenOrderIsNotFree()
		{
			//Arrange
			var order = Order.Create(Guid.NewGuid(), Location.MinLocation).Value;
			var courier = Courier.Create("Ваня", Transport.Pedestrian, Location.MinLocation).Value;
			order.Assign(courier);
			var couriers = new List<Courier>()
			{
				Courier.Create("Вася", Transport.Bicycle, Location.MaxLocation).Value
			};

			//Act
			var dispatchService = new DispatchService();
			var result = dispatchService.Dispatch(order, couriers);

			//Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().BeEquivalentTo(DispatchService.Errors.CantAssignOrderWhenStatusIsNotCreated());
		}

		[Fact]
		public void ReturnNoCouriersAvailableErrorWhenAllCouriersAreBusy()
		{
			// Arrange
			Order order = Order.Create(Guid.NewGuid(), Location.MinLocation).Value;
			var couriers = new List<Courier>()
			{
				Courier.Create("Вася", Transport.Bicycle, Location.MaxLocation).Value
			};
			couriers[0].SetBusy();

			// Act
			var dispatchService = new DispatchService();
			var result = dispatchService.Dispatch(order, couriers);

			//Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().BeEquivalentTo(DispatchService.Errors.NoCouriersAvailable());
		}

		[Fact]
		public void CanFindFastestCourierForOrder()
		{
			// Arrange
			Order order = Order.Create(Guid.NewGuid(), Location.MinLocation).Value;
			var couriers = new List<Courier>()
			{
				Courier.Create("Вася", Transport.Car, Location.MaxLocation).Value,
				Courier.Create("Петя", Transport.Bicycle, Location.Create(3,3).Value).Value,
				Courier.Create("Степа", Transport.Pedestrian, Location.Create(4,4).Value).Value,
			};

			// Act
			var dispatchService = new DispatchService();
			var result = dispatchService.Dispatch(order, couriers);

			// Assert
			result.IsSuccess.Should().BeTrue();
			result.Value.Should().Be(couriers[1]);
		}
	}
}
