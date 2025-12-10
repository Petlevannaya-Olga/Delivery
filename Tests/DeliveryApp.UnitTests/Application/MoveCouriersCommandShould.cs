using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application;

public class MoveCouriersCommandShould
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly ICourierRepository _courierRepositoryMock = Substitute.For<ICourierRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private Maybe<Courier> ExistingCourier => Courier.Create("Иван", Transport.Car, Location.CreateRandom()).Value;

    private List<Order> EmptyOrderList => [];

    private List<Order> GetAssignedOrders()
    {
        var order = Order.Create(Guid.NewGuid(), Location.Create(1, 1).Value).Value;
        order.Assign(ExistingCourier.Value);
        return [order];
    }

    [Fact]
    public async Task ReturnTrueWhenAssignedOrderExists()
    {
        // Arrange
        _orderRepositoryMock.GetAllAssigned().Returns(GetAssignedOrders());
        _courierRepositoryMock.Get(Arg.Any<Guid>()).Returns(ExistingCourier);
        _unitOfWorkMock.SaveChangesAsync().Returns(true);

        //Act
        var command = new MoveCouriersCommand();
        var handler = new MoveCouriersCommandHandler(
            _courierRepositoryMock,
            _unitOfWorkMock,
            _orderRepositoryMock);
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
        _orderRepositoryMock.Received(2);
        _courierRepositoryMock.Received(2);
        _unitOfWorkMock.Received(1);
    }

    [Fact]
    public async Task ReturnFailureWhenAssignedOrderDoesNotExist()
    {
        // Arrange
        _orderRepositoryMock.GetAllAssigned().Returns(EmptyOrderList);
        _courierRepositoryMock.Get(Arg.Any<Guid>()).Returns(ExistingCourier);
        _unitOfWorkMock.SaveChangesAsync().Returns(true);

        //Act
        var command = new MoveCouriersCommand();
        var handler = new MoveCouriersCommandHandler(
            _courierRepositoryMock,
            _unitOfWorkMock,
            _orderRepositoryMock);
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("no.assigned.orders");
        _orderRepositoryMock.Received(1);
    }
}