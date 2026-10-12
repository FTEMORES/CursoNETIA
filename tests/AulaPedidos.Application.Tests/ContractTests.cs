using System.Reflection;
using AulaPedidos.Application;
using AulaPedidos.Application.Abstractions;
using AulaPedidos.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AulaPedidos.Application.Tests;

public class ContractTests
{
    [Fact]
    public void AddApplication_devuelve_la_misma_coleccion_sin_registrar_servicios()
    {
        var services = new ServiceCollection();

        var result = services.AddApplication();

        Assert.Same(services, result);
        Assert.Empty(services);
    }

    [Fact]
    public void IAggregateRoot_expone_Id_de_tipo_Guid()
    {
        var property = typeof(IAggregateRoot).GetProperty(nameof(IAggregateRoot.Id));

        Assert.NotNull(property);
        Assert.Equal(typeof(Guid), property!.PropertyType);
    }

    [Fact]
    public void IRepository_restringe_T_a_class_e_IAggregateRoot()
    {
        var genericArgument = typeof(IRepository<>).GetGenericArguments()[0];

        Assert.True(genericArgument.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint));
        Assert.Contains(typeof(IAggregateRoot), genericArgument.GetGenericParameterConstraints());
    }

    [Fact]
    public void IRepository_define_solo_las_operaciones_previstas()
    {
        var methods = typeof(IRepository<>).GetMethods().Select(m => m.Name).OrderBy(n => n).ToArray();

        Assert.Equal(new[] { "Add", "GetByIdAsync", "GetByIdsAsync" }, methods);
    }
}
