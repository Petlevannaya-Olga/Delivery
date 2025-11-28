using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrder;

public class AssignOrderCommandHandler : IRequestHandler<AssignOrderCommand, UnitResult<Error>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICourierRepository _courierRepository;
    private readonly IDispatchService _dispatchService;
    private readonly IUnitOfWork _unitOfWork;

    public AssignOrderCommandHandler(IOrderRepository orderRepository,
        ICourierRepository courierRepository,
        IUnitOfWork unitOfWork,
        IDispatchService dispatchService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _dispatchService = dispatchService ?? throw new ArgumentNullException(nameof(dispatchService));
    }

    public async Task<UnitResult<Error>> Handle(AssignOrderCommand request, CancellationToken cancellationToken)
    {
        // Получить первый заказ в статусе Created
        var order = await _orderRepository.GetFirstInCreatedStatus();

        // Если такого заказа нет - ничего не делаем
        if (order.HasNoValue)
        {
            return UnitResult.Success<Error>();
        }

        // Получаем всех свободных курьеров
        var couriers = _courierRepository.GetAllFree();

        if (couriers.Count == 0)
        {
            return Errors.FreeCouriersWereNotFound();
        }

        // Выбираем подходящего курьера
        var dispatchResult = _dispatchService.Dispatch(order.Value, couriers);

        if (dispatchResult.IsFailure)
        {
            return dispatchResult;
        }

        // Сохраняем изменения в БД
        try
        {
            _courierRepository.Update(dispatchResult.Value);
            _orderRepository.Update(order.Value);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
        }

        return UnitResult.Success<Error>();
    }

    /// <summary>
    /// Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    private static class Errors
    {
        public static Error OrderWithIdWasNotFound(string orderId)
        {
            return new Error($"order.with.id.{orderId}.was.not.found",
                $"Заказ с id = {orderId} не найден");
        }

        public static Error FreeCouriersWereNotFound()
        {
            return new Error($"free.couriers.was.not.found",
                "Свободные курьеры не найдены");
        }
    }
}