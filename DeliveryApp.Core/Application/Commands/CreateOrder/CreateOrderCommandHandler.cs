using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, UnitResult<Error>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IGeoClient _geoClient;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    ///     Ctr
    /// </summary>
    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IGeoClient geoClient)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _geoClient = geoClient;
    }

    public async Task<UnitResult<Error>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Если заказ существует - ничего не делаем
        var order = await _orderRepository.GetAsync(request.OrderId);
        if (order.HasValue)
            return UnitResult.Success<Error>();

        // Получаем координаты
        var location = await _geoClient.GetLocation(request.Street, cancellationToken);
        if (location.IsFailure)
            return Errors.LocationIsInvalid();

        // Создаем заказ
        var createOrderResult = Order.Create(request.OrderId, location.Value);
        if (createOrderResult.IsFailure)
            return createOrderResult.Error;

        // Сохраняем заказ
        await _orderRepository.Add(createOrderResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    private static class Errors
    {
        public static Error LocationIsInvalid()
        {
            return new Error("location.is.invalid",
                $"Ошибка при получении координат заказа");
        }
    }
}