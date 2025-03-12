using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate
{
	public class CourierShould
	{
		public static IEnumerable<object[]> GetTransports()
		{
			// Пешеход, заказ X:совпадает, Y: совпадает
			yield return
			[
				Transport.Pedestrian, Location.Create(1, 1).Value, Location.Create(1, 1).Value, Location.Create(1, 1).Value
			];
			yield return
			[
				Transport.Pedestrian, Location.Create(5, 5).Value, Location.Create(5, 5).Value, Location.Create(5, 5).Value
			];

			// Пешеход, заказ X:совпадает, Y: выше
			yield return
			[
				Transport.Pedestrian, Location.Create(1, 1).Value, Location.Create(1, 2).Value, Location.Create(1, 2).Value
			];
			yield return
			[
				Transport.Pedestrian, Location.Create(1, 1).Value, Location.Create(1, 5).Value, Location.Create(1, 2).Value
			];

			// Пешеход, заказ X:правее, Y: совпадает
			yield return
			[
				Transport.Pedestrian, Location.Create(2, 2).Value, Location.Create(3, 2).Value, Location.Create(3, 2).Value
			];
			yield return
			[
				Transport.Pedestrian, Location.Create(5, 5).Value, Location.Create(6, 5).Value, Location.Create(6, 5).Value
			];

			// Пешеход, заказ X:правее, Y: выше
			yield return
			[
				Transport.Pedestrian, Location.Create(2, 2).Value, Location.Create(3, 3).Value, Location.Create(3, 2).Value
			];
			yield return
			[
				Transport.Pedestrian, Location.Create(1, 1).Value, Location.Create(5, 5).Value, Location.Create(2, 1).Value
			];

			// Пешеход, заказ X:совпадает, Y: ниже
			yield return
			[
				Transport.Pedestrian, Location.Create(1, 2).Value, Location.Create(1, 1).Value, Location.Create(1, 1).Value
			];
			yield return
			[
				Transport.Pedestrian, Location.Create(5, 5).Value, Location.Create(5, 1).Value, Location.Create(5, 4).Value
			];

			// Пешеход, заказ X:левее, Y: совпадает
			yield return
			[
				Transport.Pedestrian, Location.Create(2, 2).Value, Location.Create(1, 2).Value, Location.Create(1, 2).Value
			];
			yield return
			[
				Transport.Pedestrian, Location.Create(5, 5).Value, Location.Create(1, 5).Value, Location.Create(4, 5).Value
			];

			// Пешеход, заказ X:левее, Y: ниже
			yield return
			[
				Transport.Pedestrian, Location.Create(2, 2).Value, Location.Create(1, 1).Value, Location.Create(1, 2).Value
			];
			yield return
			[
				Transport.Pedestrian, Location.Create(5, 5).Value, Location.Create(1, 1).Value, Location.Create(4, 5).Value
			];


			// Велосипедист, заказ X:совпадает, Y: совпадает
			yield return
			[
				Transport.Bicycle, Location.Create(1, 1).Value, Location.Create(1, 1).Value, Location.Create(1, 1).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(5, 5).Value, Location.Create(5, 5).Value, Location.Create(5, 5).Value
			];

			// Велосипедист, заказ X:совпадает, Y: выше
			yield return
			[
				Transport.Bicycle, Location.Create(1, 1).Value, Location.Create(1, 3).Value, Location.Create(1, 3).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(1, 1).Value, Location.Create(1, 5).Value, Location.Create(1, 3).Value
			];

			// Велосипедист, заказ X:правее, Y: совпадает
			yield return
			[
				Transport.Bicycle, Location.Create(2, 2).Value, Location.Create(4, 2).Value, Location.Create(4, 2).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(5, 5).Value, Location.Create(8, 5).Value, Location.Create(7, 5).Value
			];

			// Велосипедист, заказ X:правее, Y: выше
			yield return
			[
				Transport.Bicycle, Location.Create(2, 2).Value, Location.Create(4, 4).Value, Location.Create(4, 2).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(1, 1).Value, Location.Create(5, 5).Value, Location.Create(3, 1).Value
			];

			// Велосипедист, заказ X:совпадает, Y: ниже
			yield return
			[
				Transport.Bicycle, Location.Create(1, 3).Value, Location.Create(1, 1).Value, Location.Create(1, 1).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(5, 5).Value, Location.Create(5, 1).Value, Location.Create(5, 3).Value
			];

			// Велосипедист, заказ X:левее, Y: совпадает
			yield return
			[
				Transport.Bicycle, Location.Create(3, 2).Value, Location.Create(1, 2).Value, Location.Create(1, 2).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(5, 5).Value, Location.Create(1, 5).Value, Location.Create(3, 5).Value
			];

			// Велосипедист, заказ X:левее, Y: ниже
			yield return
			[
				Transport.Bicycle, Location.Create(3, 3).Value, Location.Create(1, 1).Value, Location.Create(1, 3).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(5, 5).Value, Location.Create(1, 1).Value, Location.Create(3, 5).Value
			];

			// Велосипедист, заказ ближе чем скорость
			yield return
			[
				Transport.Bicycle, Location.Create(1, 1).Value, Location.Create(1, 2).Value, Location.Create(1, 2).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(1, 1).Value, Location.Create(2, 1).Value, Location.Create(2, 1).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(5, 5).Value, Location.Create(5, 4).Value, Location.Create(5, 4).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(5, 5).Value, Location.Create(4, 5).Value, Location.Create(4, 5).Value
			];

			// Велосипедист, заказ с шагами по 2 осям
			yield return
			[
				Transport.Bicycle, Location.Create(1, 1).Value, Location.Create(2, 2).Value, Location.Create(2, 2).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(5, 5).Value, Location.Create(4, 4).Value, Location.Create(4, 4).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(1, 1).Value, Location.Create(1, 2).Value, Location.Create(1, 2).Value
			];
			yield return
			[
				Transport.Bicycle, Location.Create(5, 5).Value, Location.Create(5, 4).Value, Location.Create(5, 4).Value
			];
		}

		[Fact]
		public void ConstructorShouldBePrivate()
		{
			// Arrange
			var typeInfo = typeof(Courier).GetTypeInfo();

			// Act

			// Assert
			typeInfo.DeclaredConstructors.All(x => x.IsPrivate).Should().BeTrue();
		}

		[Fact]
		public void BeCorrectWhenParamsAreCorrect()
		{
			//Arrange
			var transport = Transport.Pedestrian;

			//Act
			var result = Courier.Create("Ваня", transport, Location.MinLocation);

			//Assert
			result.IsSuccess.Should().BeTrue();
			result.Value.Id.Should().NotBeEmpty();
			result.Value.Name.Should().Be("Ваня");
			result.Value.Transport.Should().Be(transport);
			result.Value.Location.Should().Be(Location.MinLocation);
		}

		[Fact]
		public void ReturnValueIsRequiredErrorWhenNameIsEmpty()
		{
			//Arrange
			var name = "";
			var transport = Transport.Pedestrian;

			//Act
			var result = Courier.Create(name, transport, Location.MinLocation);

			//Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().BeEquivalentTo(GeneralErrors.ValueIsRequired(nameof(name)));
		}

		[Fact]
		public void ReturnValueIsRequiredErrorWhenLocationIsNull()
		{
			//Arrange
			var name = "Иван";
			Location location = null;
			var transport = Transport.Pedestrian;

			//Act
			var result = Courier.Create(name, transport, location);

			//Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().BeEquivalentTo(GeneralErrors.ValueIsRequired(nameof(location)));
		}

		[Fact]
		public void ReturnValueIsRequiredErrorWhenTransportIsNull()
		{
			//Arrange
			var name = "Иван";
			Transport transport = null;
			var location = Location.MinLocation;

			//Act
			var result = Courier.Create(name, transport, location);

			//Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().BeEquivalentTo(GeneralErrors.ValueIsRequired(nameof(transport)));
		}

		[Fact]
		public void CanSetCourierBusy()
		{
			// Arrange
			var courier = Courier.Create("Иван", Transport.Bicycle, Location.MinLocation);

			// Act
			var result = courier.Value.SetBusy();

			// Assert
			result.IsSuccess.Should().BeTrue();
			courier.Value.Status.Should().BeEquivalentTo(CourierStatus.Busy);
		}

		[Fact]
		public void CantSetCourierBusyTwice()
		{
			// Arrange
			var courier = Courier.Create("Иван", Transport.Bicycle, Location.MinLocation);

			// Act
			courier.Value.SetBusy();
			var result = courier.Value.SetBusy();

			// Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().BeEquivalentTo(Courier.Errors.CantAssignOrderWhenCourierIsBusy());
		}

		[Fact]
		public void CanSetCourierFree()
		{
			// Arrange
			var courier = Courier.Create("Иван", Transport.Bicycle, Location.MinLocation);

			// Act
			courier.Value.SetFree();

			// Assert
			courier.Value.Status.Should().BeEquivalentTo(CourierStatus.Free);
		}

		[Theory]
		[MemberData(nameof(GetTransports))]
		public void CanMove(
			Transport transport, 
			Location currentLocation, 
			Location targetLocation,
			Location locationAfterMove)
		{
			//Arrange
			var courier = Courier.Create("Ваня", transport, currentLocation).Value;

			//Act
			var result = courier.Go(targetLocation);

			//Assert
			result.IsSuccess.Should().BeTrue();
			courier.Location.Should().Be(locationAfterMove);
		}

		[Fact]
		public void CantGoToIncorrectLocation()
		{
			//Arrange
			var courier = Courier.Create("Ваня", Transport.Pedestrian, Location.MinLocation).Value;

			//Act
			var result = courier.Go(null);

			//Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().BeEquivalentTo(GeneralErrors.ValueIsRequired("location"));
		}

		[Fact]
		public void CanGetTimeToLocation()
		{
			//Arrange
			var location = Location.Create(5, 10).Value;
			var courier = Courier.Create("Ваня", Transport.Pedestrian, Location.MinLocation).Value;

			//Act
			var result = courier.GetTimeToLocation(location);

			//Assert
			result.IsSuccess.Should().BeTrue();
			result.Value.Should().Be(13);
		}

		[Fact]
		public void CantGetTimeToIncorrectLocation()
		{
			// Arrange
			var courier = Courier.Create("Ваня", Transport.Pedestrian, Location.MinLocation).Value;
			
			// Act
			var result = courier.GetTimeToLocation(null);

			// Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().BeEquivalentTo(GeneralErrors.ValueIsRequired("location"));
		}
	}
}
