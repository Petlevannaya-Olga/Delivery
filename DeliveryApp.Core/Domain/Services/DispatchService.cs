using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Services
{
	public class DispatchService : IDispatchService
	{
		public Result<Courier, Error> Dispatch(Order order, List<Courier> couriers)
		{
			if (order is null) return GeneralErrors.ValueIsRequired(nameof(order));

			if (couriers is null || couriers.Count == 0)
				return GeneralErrors.ValueIsRequired(nameof(couriers));

			// Заказ должен быть свободен
			if (order.Status != OrderStatus.Created)
				return Errors.CantAssignOrderWhenStatusIsNotCreated();

			// Курьер должен быть свободен
			var freeCouriers = couriers.Where(x => x.Status == CourierStatus.Free);
			if (freeCouriers.Any() is false) return Errors.NoCouriersAvailable();

			var minTime = double.MaxValue;
			Courier winner = null;

			foreach (var courier in couriers)
			{
				var timeToLocation = courier.GetTimeToLocation(order.Location);
				if (timeToLocation.IsFailure) return timeToLocation.Error;

				if (timeToLocation.Value < minTime)
				{
					minTime = timeToLocation.Value;
					winner = courier;
				}
			}

			if (winner is null) return Errors.CourierWasNotFound();

			var assignResult = order.Assign(winner);
			if (assignResult.IsFailure) return assignResult.Error;

			var busyResult = winner.SetBusy();
			if (busyResult.IsFailure) return busyResult.Error;

			return winner;
		}

		/// <summary>
		/// Ошибки, которые может возвращать сущность
		/// </summary>
		[ExcludeFromCodeCoverage]
		public static class Errors
		{
			public static Error CantAssignOrderWhenStatusIsNotCreated()
			{
				return new Error($"{nameof(Order).ToLowerInvariant()}.cant.assign.order.when.status.is.not.created",
					$"Нельзя назначить заказ, статус которого не {OrderStatus.Created.Name.ToLowerInvariant()}");
			}

			public static Error CourierWasNotFound()
			{
				return new Error("courier.was.not.found", "Курьер не был найден");
			}

			public static Error NoCouriersAvailable()
			{
				return new Error("no.couriers.available", "Нет свободных курьеров");
			}
		}
	}
}
