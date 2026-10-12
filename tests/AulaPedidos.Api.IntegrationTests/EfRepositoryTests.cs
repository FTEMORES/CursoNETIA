using AulaPedidos.Application.Abstractions.Persistence;
using AulaPedidos.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AulaPedidos.Api.IntegrationTests;

public sealed class EfRepositoryTests
{
    [Fact]
    public async Task GetPagedAsync_AppliesPredicateOrderAndPagination()
    {
        await using var fixture = await RepositoryFixture.CreateAsync();
        await using (var setupContext = fixture.CreateContext())
        {
            var repository = new EfRepository<TestItem>(setupContext);
            await repository.AddAsync(new TestItem { Name = "Delta", IsActive = true });
            await repository.AddAsync(new TestItem { Name = "Alpha", IsActive = true });
            await repository.AddAsync(new TestItem { Name = "Beta", IsActive = false });
            await repository.AddAsync(new TestItem { Name = "Gamma", IsActive = true });
            await new EfUnitOfWork(setupContext).SaveChangesAsync();
        }

        await using var queryContext = fixture.CreateContext();
        var repositoryUnderTest = new EfRepository<TestItem>(queryContext);

        var page = await repositoryUnderTest.GetPagedAsync(
            pageNumber: 2,
            pageSize: 2,
            predicate: item => item.IsActive,
            orderBy: items => items.OrderBy(item => item.Name));

        Assert.Equal(["Gamma"], page.Select(item => item.Name));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            repositoryUnderTest.GetPagedAsync(0, 2));
    }

    [Fact]
    public async Task GetAsync_LoadsRequestedIncludeAndRemovePersistsAfterUnitOfWork()
    {
        await using var fixture = await RepositoryFixture.CreateAsync();
        await using (var setupContext = fixture.CreateContext())
        {
            var item = new TestItem
            {
                Name = "Included item",
                Child = new TestChild { Value = "child value" }
            };
            var repository = new EfRepository<TestItem>(setupContext);
            await repository.AddAsync(item);
            await new EfUnitOfWork(setupContext).SaveChangesAsync();
        }

        await using (var queryContext = fixture.CreateContext())
        {
            var repository = new EfRepository<TestItem>(queryContext);
            var item = Assert.Single(await repository.GetAsync(
                predicate: candidate => candidate.Name == "Included item",
                includeStrings: ["Child"]));

            Assert.Equal("child value", item.Child!.Value);
            await repository.RemoveAsync(item);
            await new EfUnitOfWork(queryContext).SaveChangesAsync();
        }

        await using var verifyContext = fixture.CreateContext();
        var verifyRepository = new EfRepository<TestItem>(verifyContext);
        Assert.Empty(await verifyRepository.GetAsync());
    }

    [Fact]
    public async Task GetAsync_ObservesCancellation()
    {
        await using var fixture = await RepositoryFixture.CreateAsync();
        await using var context = fixture.CreateContext();
        var repository = new EfRepository<TestItem>(context);
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            repository.GetAsync(cancellationToken: cancellationSource.Token));
    }

    private sealed class RepositoryFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<AulaPedidosDbContext> _options;

        private RepositoryFixture(SqliteConnection connection, DbContextOptions<AulaPedidosDbContext> options)
        {
            _connection = connection;
            _options = options;
        }

        public static async Task<RepositoryFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<AulaPedidosDbContext>()
                .UseSqlite(connection)
                .Options;
            var fixture = new RepositoryFixture(connection, options);
            await using var context = fixture.CreateContext();
            await context.Database.EnsureCreatedAsync();
            return fixture;
        }

        public TestDbContext CreateContext() => new(_options);

        public ValueTask DisposeAsync() => _connection.DisposeAsync();
    }

    private sealed class TestDbContext(DbContextOptions<AulaPedidosDbContext> options) : AulaPedidosDbContext(options)
    {
        public DbSet<TestItem> Items => Set<TestItem>();

        public DbSet<TestChild> Children => Set<TestChild>();
    }

    private sealed class TestItem
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public TestChild? Child { get; set; }
    }

    private sealed class TestChild
    {
        public int Id { get; set; }

        public string Value { get; set; } = string.Empty;
    }
}