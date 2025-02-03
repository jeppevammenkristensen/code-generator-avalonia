using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Client.Services;

// <summary>
/// Represents a class that provides lazy-loading of various type symbols.
/// </summary>
public class LazyTypes
{
    private readonly Lazy<INamedTypeSymbol?> _listGeneric;
    private readonly Lazy<INamedTypeSymbol?> _hashSet;
    private readonly Lazy<INamedTypeSymbol?> _collection;
    private readonly Lazy<INamedTypeSymbol?> _task;
    private readonly Lazy<INamedTypeSymbol?> _genericTask;
    private readonly Lazy<INamedTypeSymbol?> _memoryCache;
    private readonly Lazy<INamedTypeSymbol?> _cacheEntry;
    private readonly Lazy<INamedTypeSymbol> _ignoreKey;


    public INamedTypeSymbol? ListGeneric => _listGeneric.Value;
    public INamedTypeSymbol? HashSet => _hashSet.Value;
    public INamedTypeSymbol? Collection => _collection.Value;
    public INamedTypeSymbol? Task => _task.Value;

    public INamedTypeSymbol? GenericTask => _genericTask.Value;
    public INamedTypeSymbol? MemoryCache => _memoryCache.Value;
    public INamedTypeSymbol? CacheEntry => _cacheEntry.Value;
    public INamedTypeSymbol IgnoreKeyAttribute => _ignoreKey.Value;

    public LazyTypes(Compilation compilation)
    {
        _listGeneric = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Collections.Generic.List`1"));
        _hashSet = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName("System.Collections.Generic.HashSet`1"));
        _collection = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypeByMetadataName("System.Collections.Generic.Collection`1"));
        _task = new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Threading.Tasks.Task"));
        _genericTask =
            new Lazy<INamedTypeSymbol?>(() => compilation.GetTypeByMetadataName("System.Threading.Tasks.Task`1"));
        _memoryCache = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypesByMetadataName("Microsoft.Extensions.Caching.Memory.IMemoryCache").FirstOrDefault());
        _cacheEntry = new Lazy<INamedTypeSymbol?>(() =>
            compilation.GetTypesByMetadataName("Microsoft.Extensions.Caching.Memory.ICacheEntry").FirstOrDefault());
        _ignoreKey = new Lazy<INamedTypeSymbol>(() =>
            compilation.GetTypesByMetadataName("CacheSourceGenerator.IgnoreKeyAttribute").First());
    }

    /// <summary>
    /// For testing purposes
    /// </summary>
    /// <returns></returns>
    internal static LazyTypes Empty()
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        return new LazyTypes(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}

public static class Extensions
{
    
    public static T? GetAttributePropertyValue<T>(this AttributeData attributeData, string name, T? valueIfNotPresent = default) where T : notnull
    {
        if (attributeData == null) throw new ArgumentNullException(nameof(attributeData));

        var named = attributeData.NamedArguments.Select(x => new {x.Key, x.Value})
            .FirstOrDefault(x => x.Key == name);
        if (named != null)
        {
            return named.Value switch
            {
                {Value: not { }} => default(T),
                {Kind: TypedConstantKind.Enum} x => (T) Enum.ToObject(typeof(T), x.Value),
                _ => (T?) named.Value.Value
            };
        }
        return valueIfNotPresent;
    }
    
    

    public static bool IsAsyncWithResult(this IMethodSymbol methodSymbol, LazyTypes _types)
    {
        return methodSymbol.ReturnType.OriginalDefinition.Equals(_types.GenericTask, SymbolEqualityComparer.Default);
    }

    public static bool IsAsync(this ITypeSymbol typeSymbol, LazyTypes types)
    {
        return typeSymbol.Equals(types.Task, SymbolEqualityComparer.Default) ||
               typeSymbol.OriginalDefinition.Equals(types.GenericTask, SymbolEqualityComparer.Default);
    }

    /// <summary>
    /// Gets the underlying type of the given type symbol.
    /// </summary>
    /// <param name="typeSymbol">The type symbol.</param>
    /// <param name="types">The lazy-loaded type symbols.</param>
    /// <returns>The underlying type of the given type symbol.</returns>
    public static ITypeSymbol GetUnderlyingType(this ITypeSymbol typeSymbol, LazyTypes types)
    { 
        if (typeSymbol is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol )
        {
            if (namedTypeSymbol.OriginalDefinition.Equals(types.GenericTask, SymbolEqualityComparer.Default))
            {
                return GetUnderlyingType(namedTypeSymbol.TypeArguments[0], types);    
            }

            if (namedTypeSymbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
            {
                return GetUnderlyingType(namedTypeSymbol.TypeArguments[0], types);
            }

            return typeSymbol;
        }

        return typeSymbol;


    }
 
    
    public static (bool isEnumerable, ITypeSymbol underlyingType) IsEnumerableOfTypeButNotString(
        this ITypeSymbol typeSymbol)
    {
        if (typeSymbol == null) throw new ArgumentNullException(nameof(typeSymbol));
        if (typeSymbol.SpecialType == SpecialType.System_String)
            return (false, default!);

        return IsEnumerableOfType(typeSymbol);
    }
    
    public static (bool isEnumerable, ITypeSymbol underlyingType) IsEnumerableOfType(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            return (true, arrayType.ElementType);
        }

        if (typeSymbol is INamedTypeSymbol namedType)
        {
            if (namedType.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T)
                return (true, namedType.TypeArguments.First());

            var candidates = namedType.AllInterfaces.Where(x =>
                x.OriginalDefinition?.SpecialType == SpecialType.System_Collections_Generic_IEnumerable_T);

            foreach (var namedTypeSymbol in candidates)
            {
                if (namedTypeSymbol.TypeArguments.FirstOrDefault() is { } result)
                    return (true, result);
            }
        }

        return (false, default!);
    }
    
    internal static bool IsPublic(this ISymbol source)
    {
        return source.DeclaredAccessibility == Accessibility.Public;
    }
    
    /// <summary>
    /// Gets all members and members of base types that are not interfaces
    /// It will return all types of members. But will have a special focus on Properties
    /// and Methods. If the method of a base class is virtual and it matches the signature of an existing members
    /// in a derived class it will not be returned
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static List<(ISymbol member, bool isInherited)> GetAllMembers(this INamedTypeSymbol source)
    {
        List<(ISymbol,bool)> members = new(source.GetMembers().Select(x => (x,false)));

        if (source.BaseType is {TypeKind: TypeKind.Class, SpecialType: SpecialType.None} baseType) 
        {
            foreach ((ISymbol member, _) in baseType.GetAllMembers())
            {
                if (member.IsAbstract)
                    continue;
                if (member.IsVirtual && members.Exists(x => x.Item1.IsApproximateMemberMatch(member)))
                    continue;
                members.Add((member, false));
            }
        }

        return members;
    }

    internal static bool IsApproximateMemberMatch(this ISymbol source, ISymbol other)
    {
        if (source.Kind == other.Kind && source.Name == other.Name)
        {
            if (source is IMethodSymbol methodSymbol && other is IMethodSymbol otherMethodSymbol)
            {
                if (methodSymbol.Parameters.Length != otherMethodSymbol.Parameters.Length)
                    return false;

                for (int i = 0; i < methodSymbol.Parameters.Length; i++)
                {
                    if (!methodSymbol.Parameters[i].Type.Equals(otherMethodSymbol.Parameters[i].Type,
                            SymbolEqualityComparer.Default))
                        return false;
                }

                return true;
            }
            else if (source is IPropertySymbol propertySymbol && other is IPropertySymbol otherPropertySymbol)
            {
                return propertySymbol.Type.Equals(otherPropertySymbol.Type, SymbolEqualityComparer.Default);
            }
            else if (source is IFieldSymbol fieldSymbol && other is IFieldSymbol otherFieldSymbol)
            {
                return fieldSymbol.Type.Equals(otherFieldSymbol.Type, SymbolEqualityComparer.Default);
            }
        }

        // We currently only have interest in properties and symbols
        return false;
    }

}