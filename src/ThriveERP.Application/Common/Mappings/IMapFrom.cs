using AutoMapper;

namespace ThriveERP.Application.Common.Mappings;

/// <summary>
/// Interface that DTOs implement to declare they can be mapped from <typeparamref name="T"/>.
/// Provides a default AutoMapper mapping configuration.
/// </summary>
/// <typeparam name="T">The source type to map from.</typeparam>
public interface IMapFrom<T>
{
    /// <summary>
    /// Creates a default mapping from <typeparamref name="T"/> to the implementing type.
    /// Override to customize the mapping configuration.
    /// </summary>
    /// <param name="profile">The AutoMapper profile to configure.</param>
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}
