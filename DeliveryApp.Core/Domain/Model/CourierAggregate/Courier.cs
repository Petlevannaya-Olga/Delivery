using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Primitives;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate
{
	public class Courier : Aggregate<Guid>
	{
		/// <summary>
		/// Конструктор без параметров
		/// </summary>
		[ExcludeFromCodeCoverage]
		private Courier() { }

		/// <summary>
		/// Конструктор с параметрами
		/// </summary>
		/// <param name="id">Идентификатор</param>
		/// <param name="Name">Имя курьера</param>
		/// <param name="transport">Транспорт курьера</param>
		/// <param name="location">Местоположение</param>
		private Courier(
			string name,
			Transport transport,
			Location location)
		{
			Id = Guid.NewGuid();
			Name = name;
			Transport = transport;
			Location = location;

			Status = CourierStatus.Free;
		}

		/// <summary>
		/// Имя курьера
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Транспорт курьера
		/// </summary>
		public Transport Transport { get; }

		/// <summary>
		/// Местоположение
		/// </summary>
		public Location Location { get; private set; }

		/// <summary>
		/// Статус курьера
		/// </summary>
		public CourierStatus Status { get; private set; }

		/// <summary>
		/// Фабричный метод
		/// </summary>
		/// <param name="name">Имя курьера</param>
		/// <param name="transport">Транспорт курьера</param>
		/// <param name="location">Местоположение</param>
		public static Result<Courier, Error> Create(
			string name,
			Transport transport,
			Location location)
		{
			if (string.IsNullOrEmpty(name)) return GeneralErrors.ValueIsRequired(nameof(name));
			if (location is null) return GeneralErrors.ValueIsRequired(nameof(location));
			if (transport is null) return GeneralErrors.ValueIsRequired(nameof(transport));

			return new Courier(name, transport, location);
		}

		/// <summary>
		/// Сделать курьера занятым
		/// </summary>
		public UnitResult<Error> SetBusy()
		{
			if (Status == CourierStatus.Busy) return Errors.CantAssignOrderWhenCourierIsBusy();

			Status = CourierStatus.Busy;
			return UnitResult.Success<Error>();
		}

		/// <summary>
		/// Установить курьеру статус Free, если он завершил и больше не назначен на заказ
		/// </summary>
		public void SetFree()
		{
			Status = CourierStatus.Free;
		}

		/// <summary>
		/// Посчитать время в пути до места назначения
		/// </summary>
		/// <param name="location">Место назначения</param>
		public Result<double, Error> GetTimeToLocation(Location location)
		{
			if (location is null) return GeneralErrors.ValueIsRequired(nameof(location));

			var distance = Location.DistanceTo(location);
			if (distance.IsFailure) return distance.Error;

			return (double)distance.Value / Transport.Speed;
		}

		/// <summary>
		/// Переместить курьера на один шаг с учетом транспорта
		/// </summary>
		/// <param name="location">Место назначения</param>
		public UnitResult<Error> Go(Location location)
		{
			if (location is null) return GeneralErrors.ValueIsRequired(nameof(location));

			// Расстояние до места назначения по х
			var dx = location.X - Location.X;

			// Расстояние до места назначения по y
			var dy = location.Y - Location.Y;

			// Количество клеток, которые нужно пройти курьеру (по x и/или по y)
			var cellsCount = Transport.Speed;

			// Перемещаемся по x
			// Еще пройден не весь путь (все клетки можно пройти по х)
			if (cellsCount <= Math.Abs(dx))
			{
				Location = Location.Create(GetX(), Location.Y).Value;
				return UnitResult.Success<Error>();
			}
			else // Не все клетки можно пройти по х
			{
				Location = Location.Create(Location.X + dx, Location.Y).Value;

				// Прибыли в пункт назначения
				if (Location == location)
					return UnitResult.Success<Error>();

				cellsCount -= Math.Abs(dx);
			}

			// Перемещаемся по y
			if (cellsCount <= Math.Abs(dy))
			{
				Location = Location.Create(Location.X, GetY()).Value;
			}
			else // Заканчиваем поездку
			{
				Location = Location.Create(Location.X, Location.Y + dy).Value;
			}

			return UnitResult.Success<Error>();

			int GetX()
			{
				if (dx > 0) return Location.X + cellsCount;
				else return Location.X - cellsCount;
			}

			int GetY()
			{
				if (dy > 0) return Location.Y + cellsCount;
				else return Location.Y - cellsCount;
			}
		}

		/// <summary>
		/// Ошибки, которые может возвращать сущность
		/// </summary>
		[ExcludeFromCodeCoverage]
		public static class Errors
		{
			public static Error CantAssignOrderWhenCourierIsBusy()
			{
				return new Error($"{nameof(Courier).ToLowerInvariant()}.cant.assign.order.when.courier.is.busy",
					"Нельзя назначить заказ на занятого курьера");
			}
		}
	}
}
