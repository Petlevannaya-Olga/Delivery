using CSharpFunctionalExtensions;
using System.Diagnostics.CodeAnalysis;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.SharedKernel
{
	public class Location : ValueObject
	{
		public static readonly Location MinLocation = new(1, 1);
		public static readonly Location MaxLocation = new(10, 10);

		/// <summary>
		/// Конструктор
		/// </summary>
		[ExcludeFromCodeCoverage]
		private Location()
		{

		}

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="x">Координата x</param>
		/// <param name="y">Координана y</param>
		private Location(int x, int y) : this()
		{
			X = x;
			Y = y;
		}

		/// <summary>
		/// Координана X
		/// </summary>
		public int X { get; }

		/// <summary>
		/// Координана Y
		/// </summary>
		public int Y { get; }

		/// <summary>
		/// Factory method
		/// </summary>
		/// <param name="x">Координата х</param>
		/// <param name="y">Координата y</param>
		public static Result<Location, Error> Create(int x, int y)
		{
			if (x < MinLocation.X || x > MaxLocation.X)
			{
				return GeneralErrors.ValueIsRequired(nameof(x));
			}

			if (y < MinLocation.Y || y > MaxLocation.Y)
			{
				return GeneralErrors.ValueIsRequired(nameof(y));
			}

			return new Location(x, y);
		}

		/// <summary>
		/// Создать Location со случайными координатами
		/// </summary>
		public static Location CreateRandom()
		{
			var random = new Random();
			var x = random.Next(MinLocation.X, MaxLocation.X + 1);
			var y = random.Next(MinLocation.Y, MaxLocation.Y + 1);
			return new Location(x, y);
		}

		/// <summary>
		/// Вычисление расстояния между пунктами назначения
		/// </summary>
		/// <param name="location">Пункт назначения</param>
		public Result<int, Error> DistanceTo(Location location)
		{
			if (location is null)
			{
				return GeneralErrors.ValueIsRequired(nameof(location));
			}

			var dx = Math.Abs(location.X - X);
			var dy = Math.Abs(location.Y - Y);

			return dx + dy;
		}

		/// <summary>
		/// Перегрузка для определения идентичности
		/// </summary>
		[ExcludeFromCodeCoverage]
		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return X;
			yield return Y;
		}
	}
}
