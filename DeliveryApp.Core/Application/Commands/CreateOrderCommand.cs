using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.Commands;

public class CreateOrderCommand : IRequest<UnitResult<Error>>
{
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    public Guid OrderId { get; }

    /// <summary>
    /// Улица
    /// </summary>
    /// <remarks>Корзина содержала полный Address, но для упрощения мы будем использовать только Street из Address</remarks>
    public string Street { get; }

    /// <summary>
    /// Объем
    /// </summary>
    public int Volume { get; }

    /// <summary>
    /// Конструктор
    /// </summary>
    public CreateOrderCommand(Guid orderId, string street, int volume)
    {
        OrderId = orderId;
        Street = street;
        Volume = volume;
    }
    
    /// <summary>
    /// Fabric Method
    /// </summary>
    /// <param name="orderId">ID заказа</param>
    /// <param name="street"> Аддрес</param>
    /// <param name="volume">Размер заказа</param>
    /// <returns></returns>
    public static Result<CreateOrderCommand, Error> Create(Guid orderId, string street, int volume)
    {
        if (orderId == Guid.Empty) 
            return GeneralErrors.ValueIsRequired(nameof(OrderId));
        
        if (string.IsNullOrEmpty(street)) 
            return GeneralErrors.ValueIsRequired(nameof(Street));
        
        if (volume <= 0) 
            return Errors.CantAssignCreateOrderWithWrongVolume();

        return new CreateOrderCommand(orderId, street, volume);
    }
    
    /// <summary>
    /// Ошибки, которые может возвращать сущность
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Errors
    {
        public static Error CantAssignCreateOrderWithWrongVolume()
        {
            return new Error($"{nameof(Volume).ToLowerInvariant()}.cant.create.order.with.wrong.volume",
                $"Нельзя создать заказ, у которого вес <= 0");
        }
    }
}