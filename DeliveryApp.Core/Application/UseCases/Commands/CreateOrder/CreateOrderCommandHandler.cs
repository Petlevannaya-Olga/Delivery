using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, UnitResult<Error>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Конструктор
    /// </summary>
    public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
    }

    public async Task<UnitResult<Error>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Если заказ существует - ничего не делаем
        var order = await _orderRepository.GetAsync(request.OrderId);
        if (order.HasValue)
            return UnitResult.Success<Error>();

        // Получаем координаты
        var location = Location.CreateRandom();

        // Создаем заказ
        var createOrderResult = Order.Create(request.OrderId, location);
        if (createOrderResult.IsFailure)
            return createOrderResult;

        // Сохраняем заказ
        await _orderRepository.Add(createOrderResult.Value);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return UnitResult.Success<Error>();
    }
}