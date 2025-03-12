using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate
{
    public class CourierStatusShould
    {
		[Fact]
		public void ReturnCorrectName()
		{
			//Arrange

			//Act

			//Assert
			CourierStatus.Free.Name.Should().Be("free");
			CourierStatus.Busy.Name.Should().Be("busy");
		}

		[Fact]
		public void BeEqualWhenAllPropertiesAreEqual()
		{
			//Arrange

			//Act
			// ReSharper disable once EqualExpressionComparison
			var result = CourierStatus.Free == CourierStatus.Free;

			//Assert
			result.Should().BeTrue();
		}

		[Fact]
		public void BeNotEqualWhenAllPropertiesAreNotEqual()
		{
			//Arrange

			//Act
			var result = CourierStatus.Free == CourierStatus.Busy;

			//Assert
			result.Should().BeFalse();
		}

	}
}
