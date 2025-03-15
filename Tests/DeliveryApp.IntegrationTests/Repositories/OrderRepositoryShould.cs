using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Infrastructure.Adapters.Postgres;
using DeliveryApp.Infrastructure.Adapters.Postgres.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DeliveryApp.IntegrationTests.Repositories
{
	public class OrderRepositoryShould : IAsyncLifetime
	{
		/// <summary>
		/// Настройка Postgres из библиотеки TestContainers
		/// </summary>
		/// <remarks>По сути это Docker контейнер с Postgres</remarks>
		private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
			.WithImage("postgres:14.7")
			.WithDatabase("order")
			.WithUsername("username")
			.WithPassword("secret")
			.WithCleanUp(true)
			.Build();

		private ApplicationDbContext _context;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <remarks>Вызывается один раз перед всеми тестами в рамках этого класса</remarks>
		public OrderRepositoryShould()
		{

		}

		/// <summary>
		/// Инициализируем окружение
		/// </summary>
		/// <remarks>Вызывается перед каждым тестом</remarks>
		public async Task InitializeAsync()
		{
			//Стартуем БД (библиотека TestContainers запускает Docker контейнер с Postgres)
			await _postgreSqlContainer.StartAsync();

			//Накатываем миграции и справочники
			var contextOptions = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(
					_postgreSqlContainer.GetConnectionString(),
					sqlOptions => { sqlOptions.MigrationsAssembly("DeliveryApp.Infrastructure"); })
				.Options;
			_context = new ApplicationDbContext(contextOptions);
			_context.Database.Migrate();
		}

		/// <summary>
		/// Уничтожаем окружение
		/// </summary>
		/// <remarks>Вызывается после каждого теста</remarks>
		public async Task DisposeAsync()
		{
			await _postgreSqlContainer.DisposeAsync().AsTask();
		}

		[Fact]
		public async Task CanAddOrder()
		{
			//Arrange
			var orderId = Guid.NewGuid();
			var order = Order.Create(orderId, Location.MinLocation).Value;

			//Act
			var orderRepository = new OrderRepository(_context);
			var unitOfWork = new UnitOfWork(_context);

			await orderRepository.Add(order);
			await unitOfWork.SaveChangesAsync();

			//Assert
			var getOrderFromDbResult = await orderRepository.GetAsync(order.Id);
			getOrderFromDbResult.HasValue.Should().BeTrue();
			var orderFromDb = getOrderFromDbResult.Value;
			order.Should().BeEquivalentTo(orderFromDb);
		}

		[Fact]
		public async Task CanUpdateOrder()
		{
			//Arrange
			var courier = Courier.Create("Иван", Transport.Pedestrian, Location.MinLocation).Value;

			var orderId = Guid.NewGuid();
			var order = Order.Create(orderId, Location.MinLocation).Value;

			var orderRepository = new OrderRepository(_context);
			await orderRepository.Add(order);

			var unitOfWork = new UnitOfWork(_context);
			await unitOfWork.SaveChangesAsync();

			//Act
			var orderAssignToCourierResult = order.Assign(courier);
			orderAssignToCourierResult.IsSuccess.Should().BeTrue();
			orderRepository.Update(order);
			await unitOfWork.SaveChangesAsync();

			//Assert
			var getOrderFromDbResult = await orderRepository.GetAsync(order.Id);
			getOrderFromDbResult.HasValue.Should().BeTrue();
			var orderFromDb = getOrderFromDbResult.Value;
			order.Should().BeEquivalentTo(orderFromDb);
		}

		[Fact]
		public async Task CanGetById()
		{
			//Arrange
			var orderId = Guid.NewGuid();
			var order = Order.Create(orderId, Location.MinLocation).Value;

			//Act
			var orderRepository = new OrderRepository(_context);
			await orderRepository.Add(order);

			var unitOfWork = new UnitOfWork(_context);
			await unitOfWork.SaveChangesAsync();

			//Assert
			var getOrderFromDbResult = await orderRepository.GetAsync(order.Id);
			getOrderFromDbResult.HasValue.Should().BeTrue();
			var orderFromDb = getOrderFromDbResult.Value;
			order.Should().BeEquivalentTo(orderFromDb);
		}

		[Fact]
		public async Task CanGetFirstInCreatedStatus()
		{
			//Arrange
			var courier = Courier.Create("Иван", Transport.Pedestrian, Location.MinLocation).Value;

			var order1 = Order.Create(Guid.NewGuid(), Location.Create(2, 2).Value).Value;
			var orderAssignToCourierResult = order1.Assign(courier);
			orderAssignToCourierResult.IsSuccess.Should().BeTrue();

			var order2 = Order.Create(Guid.NewGuid(), Location.Create(3, 3).Value).Value;

			var orderRepository = new OrderRepository(_context);
			await orderRepository.Add(order2);
			await orderRepository.Add(order1);

			var unitOfWork = new UnitOfWork(_context);
			await unitOfWork.SaveChangesAsync();

			//Act
			var getFirstOrderInCreatedStatusFromDbResult = await orderRepository.GetFirstInCreatedStatus();

			//Assert
			getFirstOrderInCreatedStatusFromDbResult.HasValue.Should().BeTrue();
			var orderFromDb = getFirstOrderInCreatedStatusFromDbResult.Value;
			order2.Should().BeEquivalentTo(orderFromDb);
		}
	}
}
