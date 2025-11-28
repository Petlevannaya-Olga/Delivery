using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application;

public class CreateOrderCommandShould
{
    private readonly IOrderRepository _orderRepositoryMock = Substitute.For<IOrderRepository>();
    private readonly IUnitOfWork _unitOfWorkMock = Substitute.For<IUnitOfWork>();

    private Maybe<Order> EmptyOrder => null;

    private Maybe<Order> ExistingOrder => Order.Create(Guid.NewGuid(), Location.Create(1,1).Value).Value;

    private Result<Location, Error> DefaultLocation => Location.Create(1, 1).Value;
    
    [Fact]
    public async Task ReturnTrueWhenOrderExists()
    {
        // Arrange
        _orderRepositoryMock.GetAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(ExistingOrder));
        
        _unitOfWorkMock.SaveChangesAsync()
            .Returns(Task.FromResult(true));

        // Act
        var command = new CreateOrderCommand(Guid.NewGuid(), "Название улицы", 10);
        var handler = new CreateOrderCommandHandler(_unitOfWorkMock, _orderRepositoryMock);
        var result  = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ReturnTrueWhenOrderCreatedSuccessfully()
    {
        // Arrange
        _orderRepositoryMock.GetAsync(Arg.Any<Guid>())
            .Returns(Task.FromResult(EmptyOrder));
        
        _unitOfWorkMock.SaveChangesAsync()
            .Returns(Task.FromResult(true));

        // Act
        var command = new CreateOrderCommand(Guid.NewGuid(), "Название улицы", 10);
        var handler = new CreateOrderCommandHandler(_unitOfWorkMock, _orderRepositoryMock);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        _orderRepositoryMock.Received(1);
        _unitOfWorkMock.Received(1);
        result.IsSuccess.Should().BeTrue();
    }
}