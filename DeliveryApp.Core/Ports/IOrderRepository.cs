using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Ports;

/// <summary>
/// Repository для Aggregate Order
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    /// <summary>
    /// Добавить заказ
    /// </summary>
    /// <param name="order">Заказ</param>
    Task Add(Order order);

    /// <summary>
    /// Обновить заказ
    /// </summary>
    /// <param name="order">Заказ</param>
    void Update(Order order);

    /// <summary>
    /// Получить заказ по идентификатору
    /// </summary>
    /// <param name="orderId">Идентификатор</param>
    Task<Maybe<Order>> GetAsync(Guid orderId);

    /// <summary>
    /// Получить 1 новый заказ
    /// </summary>
    /// <returns>Заказы</returns>
    Task<Maybe<Order>> GetFirstInCreatedStatus();

    /// <summary>
    /// Получить все назначенные заказы
    /// </summary>
    /// <returns>Заказы</returns>
    List<Order> GetAllAssigned();
}
