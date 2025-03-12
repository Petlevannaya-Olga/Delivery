using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using System.Collections.Generic;
using System.Security;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate
{
	public class TransportShould
	{
		public static IEnumerable<object[]> GetTransports()
		{
			yield return [Transport.Pedestrian, 1, "pedestrian", 1];
			yield return [Transport.Bicycle, 2, "bicycle", 2];
			yield return [Transport.Car, 3, "car", 3];
		}

		[Theory]
		[MemberData(nameof(GetTransports))]
		public void ReturnCorrectIdAndName(Transport transport, int id, string name, int speed)
		{
			//Arrange

			//Act

			//Assert
			transport.Id.Should().Be(id);
			transport.Name.Should().Be(name);
			transport.Speed.Should().Be(speed);
		}

		[Theory]
		[MemberData(nameof(GetTransports))]
		public void CanBeFoundByName(Transport transport, int id, string name, int speed)
		{
			//Arrange

			//Act
			var result = Transport.FromName(name).Value;

			//Assert
			result.Should().Be(transport);
			result.Id.Should().Be(id);
			result.Name.Should().Be(name);
			result.Speed.Should().Be(speed);
		}

		[Theory]
		[MemberData(nameof(GetTransports))]
		public void CanBeFoundById(Transport transport, int id, string name, int speed)
		{
			//Arrange

			//Act
			var result = Transport.FromId(id).Value;

			//Assert
			result.Should().Be(transport);
			result.Id.Should().Be(id);
			result.Name.Should().Be(name);
			result.Speed.Should().Be(speed);
		}


		[Theory]
		[InlineData("Skier")]
		[InlineData("Motorcyclist")]
		public void ReturnsErrorWhenTransportNotFoundByName(string name)
		{
			//Arrange

			//Act
			var result = Transport.FromName(name);

			//Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().Be(Transport.Errors.TransportIsWrong());
		}

		[Theory]
		[InlineData(10)]
		[InlineData(0)]
		[InlineData(4)]
		public void ReturnsErrorWhenTransportNotFoundById(int id)
		{
			// Arrange

			// Act
			var result = Transport.FromId(id);

			// Assert
			result.IsSuccess.Should().BeFalse();
			result.Error.Should().Be(Transport.Errors.TransportIsWrong());
		}

		[Fact]
		public void ReturnListOfStatuses()
		{
			//Arrange

			//Act
			var allStatuses = Transport.List();

			//Assert
			allStatuses.Should().NotBeEmpty();
		}

		[Fact]
		public void DerivedEntity()
		{
			//Arrange

			//Act
			var isDerivedEntity = typeof(Transport).IsSubclassOf(typeof(Entity<int>));

			//Assert
			isDerivedEntity.Should().BeTrue();
		}

		[Fact]
		public void BeEqualWhenIdsAreEqual()
		{
			// Arrange
			var transport1 = Transport.Car;
			var transport2 = Transport.Car;
			

			// Act
			var result = transport1 == transport2;

			// Assert
			result.Should().BeTrue();
			transport1.Id.Should().Be(transport2.Id);
		}

		[Fact]
		public void NotBeEqualWhenIdsAreNotEqual()
		{
			//Arrange
			var pedestrian = Transport.Pedestrian;
			var car = Transport.Car;
			
			//Act
			var result = pedestrian == car;

			//Assert
			pedestrian.Id.Should().NotBe(car.Id);
			result.Should().BeFalse();
		}
	}
}
