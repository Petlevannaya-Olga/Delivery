using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate
{
	/// <summary>
	/// Заказ
	/// </summary>
	public class Order : Aggregate<Guid>
	{
		/// <summary>
		/// Конструктор без параметров
		/// </summary>
		[ExcludeFromCodeCoverage]
		private Order() { }

		/// <summary>
		/// Конструктор с параметрами
		/// </summary>
		/// <param name="id">Идентификатор</param>
		/// <param name="location">Местоположение</param>
		private Order(Guid id, Location location) : this()
		{
			Id = id;
			Location = location;
			Status = OrderStatus.Created;
		}

		/// <summary>
		/// Местоположение, куда нужно доставить заказ
		/// </summary>
		public Location Location { get; private set; }

		/// <summary>
		/// Статус заказа
		/// </summary>
		public OrderStatus Status { get; private set; }

		/// <summary>
		/// Идентификатор назначенного курьера
		/// </summary>
		public Guid? CourierId { get; private set; }

		/// <summary>
		/// Фабричный метод
		/// </summary>
		/// <param name="id">Идентификатор заказа</param>
		/// <param name="location">Местоположение</param>
		public static Result<Order, Error> Create(Guid id, Location location)
		{
			if (id == Guid.Empty) return GeneralErrors.ValueIsRequired(nameof(id));
			if (location == null) return GeneralErrors.ValueIsRequired(nameof(location));

			return new Order(id, location);
		}

		/// <summary>
		/// Назначить заказ на курьера
		/// </summary>
		/// <param name="courier">Курьер</param>
		public UnitResult<Error> Assign(Courier courier)
		{
			if (courier == null) return GeneralErrors.ValueIsRequired(nameof(courier));
			if (Status == OrderStatus.Completed) return Errors.CantAssignOrder(Status.Name);
			if (Status == OrderStatus.Assigned) return Errors.CantAssignOrder(Status.Name);
			if (CourierId is not null) return Errors.CantAssignOrderToBusyCourier(CourierId.Value);
			
			CourierId = courier.Id;
			Status = OrderStatus.Assigned;

			return UnitResult.Success<Error>();

		}

		/// <summary>
		/// Завершить выполнение заказа
		/// </summary>
		public UnitResult<Error> Complete()
		{
			if (Status != OrderStatus.Assigned) return Errors.CantCompletedNotAssignedOrder();

			Status = OrderStatus.Completed;

			return UnitResult.Success<Error>();
		}


		/// <summary>
		/// Ошибки, которые может возвращать сущность
		/// </summary>
		[ExcludeFromCodeCoverage]
		public static class Errors
		{
			public static Error CantCompletedNotAssignedOrder()
			{
				return new Error($"{nameof(Order).ToLowerInvariant()}.cant.completed.not.assigned.order",
					"Нельзя завершить заказ, который не был назначен");
			}

			public static Error CantAssignOrderToBusyCourier(Guid courierId)
			{
				return new Error($"{nameof(Order).ToLowerInvariant()}.cant.assign.order.to.busy.courier",
					$"Нельзя назначить заказ на курьера, который занят. Id курьера = {courierId}");
			}

			public static Error CantAssignOrder(string orderStatus)
			{
				return new Error($"{nameof(Order).ToLowerInvariant()}.cant.assign.{orderStatus.ToLowerInvariant()}.order",
					$"Нельзя назначить заказ со статусом {orderStatus}");
			}
		}

	}
}
