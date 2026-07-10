using System.Reflection;
using AutoMapper;

namespace ThriveERP.Application.Common.Mappings;

/// <summary>
/// AutoMapper profile that discovers and applies all <see cref="IMapFrom{T}"/> mappings
/// in the Application assembly.
/// </summary>
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
    }

    private void ApplyMappingsFromAssembly(Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);

        var types = assembly.GetExportedTypes()
            .Where(t => t.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType))
            .ToList();

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);

            var methodInfo = type.GetMethod("Mapping")
                ?? type.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == mapFromType)
                    .GetMethod("Mapping");

            methodInfo?.Invoke(instance, [this]);
        }
    }
}
