using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application;

public class AssignOrderCommandShould
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly ICourierRepository _courierRepositoryMock = Substitute.For<ICourierRepository>();
    private readonly IDispatchService _dispatchServiceMock = Substitute.For<IDispatchService>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private Maybe<Order> ExistingOrder => Order.Create(Guid.NewGuid(), Location.CreateRandom()).Value;

    private Maybe<Order> EmptyOrder => null;

    private List<Courier> FreeCouriers => [Courier.Create("Тест", Transport.Car, Location.CreateRandom()).Value];

    private List<Courier> EmptyCouriersList => [];

    [Fact]
    public async Task ReturnTrueWhenOrderAssignedSuccessfully()
    {
        // Arrange
        _orderRepositoryMock.GetFirstInCreatedStatus()
            .Returns(Task.FromResult(ExistingOrder));

        _courierRepositoryMock.GetAllFree()
            .Returns(FreeCouriers);

        _dispatchServiceMock.Dispatch(ExistingOrder.Value, Arg.Any<List<Courier>>())
            .Returns(Result.Failure<Courier, Error>(AssignOrderCommandHandler.Errors.AvailableOrdersWereNotFound()));

        _unitOfWorkMock.SaveChangesAsync().Returns(true);

        //Act
        var command = new AssignOrderCommand();
        var handler = new AssignOrderCommandHandler(
            _orderRepositoryMock,
            _courierRepositoryMock,
            _unitOfWorkMock,
            _dispatchServiceMock);
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnFailureWhenNoAvailableOrders()
    {
        // Arrange
        _orderRepositoryMock.GetFirstInCreatedStatus()
            .Returns(Task.FromResult(EmptyOrder));

        _courierRepositoryMock.GetAllFree()
            .Returns(FreeCouriers);

        _dispatchServiceMock.Dispatch(ExistingOrder.Value, Arg.Any<List<Courier>>())
            .Returns(Result.Success<Courier, Error>(FreeCouriers[0]));

        _unitOfWorkMock.SaveChangesAsync().Returns(true);
        
        // Act
        var command = new AssignOrderCommand();
        var handler = new AssignOrderCommandHandler(
            _orderRepositoryMock,
            _courierRepositoryMock,
            _unitOfWorkMock,
            _dispatchServiceMock);
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        _orderRepositoryMock.Received(1);
        _unitOfWorkMock.Received(1);
        _courierRepositoryMock.Received(1);
        _dispatchServiceMock.Received(1);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnFailureIfCouriersWereNotFound()
    {
        //Arrange
        _orderRepositoryMock.GetFirstInCreatedStatus()
            .Returns(Task.FromResult(ExistingOrder));
        
        _courierRepositoryMock.GetAllFree()
            .Returns(EmptyCouriersList);
        
        _dispatchServiceMock
            .Dispatch(Arg.Any<Order>(), Arg.Any<List<Courier>>())
            .Returns(Result.Failure<Courier, Error>(GeneralErrors.NotFound()));
        
        _unitOfWorkMock.SaveChangesAsync().Returns(true);
        
        //Act
        var command = new AssignOrderCommand();
        var handler = new AssignOrderCommandHandler(
            _orderRepositoryMock,
            _courierRepositoryMock,
            _unitOfWorkMock,
            _dispatchServiceMock);
        var result = await handler.Handle(command, CancellationToken.None);
        
        //Assert
        result.IsFailure.Should().BeTrue();
        _orderRepositoryMock.Received(1);
        _courierRepositoryMock.Received(1).GetAllFree();
        _dispatchServiceMock.Received(0).Dispatch(Arg.Any<Order>(), Arg.Any<List<Courier>>());
    }
}