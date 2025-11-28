using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands.MoveCouriers;

public class MoveCouriersCommandHandler : IRequestHandler<MoveCouriersCommand, UnitResult<Error>>
{
    private readonly ICourierRepository _courierRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MoveCouriersCommandHandler(
        ICourierRepository courierRepository,
        IUnitOfWork unitOfWork,
        IOrderRepository orderRepository)
    {
        _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task<UnitResult<Error>> Handle(MoveCouriersCommand request, CancellationToken cancellationToken)
    {
        var assignedOrders = _orderRepository.GetAllAssigned();
        if (assignedOrders.Count == 0)
            return Errors.NoAssignedOrders();

        foreach (var order in assignedOrders)
        {
            if (order.CourierId == Guid.Empty || order.CourierId.HasValue is false)
                return Errors.CourierIdIsNullOrEmpty();

            var courier = await _courierRepository.Get(order.CourierId.Value);
            if (courier.HasNoValue)
            {
                return Errors.CourierWithIdWasNotFound(order.CourierId.ToString());
            }

            var goResult = courier.Value.Go(order.Location);
            if (goResult.IsFailure)
                return goResult;

            // Если курьер дошел до точки заказа - завершаем заказ, освобождаем курьера
            if (courier.Value.Location == order.Location)
            {
                var completeOrderResult = order.Complete();
                if (completeOrderResult.IsFailure)
                    return completeOrderResult.Error;
                
                courier.Value.SetFree();
                _orderRepository.Update(order);
            }

            _courierRepository.Update(courier.Value);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    private static class Errors
    {
        public static Error NoAssignedOrders()
        {
            return new Error("no.assigned.orders",
                $"Нет назначенных заказов");
        }

        public static Error CourierIdIsNullOrEmpty()
        {
            return new Error("courier.id.is.null.or.empty",
                $"Неверно указан идентификатор курьера");
        }

        public static Error CourierWithIdWasNotFound(string courierId)
        {
            return new Error($"courier.with.id.{courierId}.was.not.found",
                $"Курьер с id = {courierId} не найден");
        }
    }
}