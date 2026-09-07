using System.Reflection;
using Catalog.Application.Abstractions;
using Catalog.Contracts;
using Catalog.Domain;
using SharedKernal.Entities;
using Users.Application.Abstractions;
using Users.Domain;
using Users.Presentation;

namespace CleanModular.ArchTests;

/// <summary>
/// Provides assembly anchors for all architecture tests.
///
/// Each anchor is a concrete type from the target assembly.
/// If a project is renamed or removed, the compiler catches it here
/// instead of failing silently at test runtime.
///
/// HOW TO EXTEND:
/// When you add a new module (e.g. Posts), add its anchor types here:
///
///   protected static readonly Assembly PostsDomainAssembly      = typeof(Posts.Domain.Post).Assembly;
///   protected static readonly Assembly PostsApplicationAssembly = typeof(Posts.Application.Abstractions.IPostRepository).Assembly;
///   ...
///   protected static readonly IEnumerable<Assembly> PostsModuleAssemblies = [ PostsDomainAssembly, ... ];
/// </summary>
public abstract class TestBase
{
    // ── Building Blocks ──────────────────────────────────────────
    protected static readonly Assembly SharedKernalAssembly = typeof(BaseEntity).Assembly;

    protected static readonly Assembly CatalogDomainAssembly = typeof(Actor).Assembly;
    protected static readonly Assembly CatalogContractsAssembly = typeof(ActorProfile).Assembly;
    protected static readonly Assembly CatalogApplicationAssembly = typeof(ICatalogReader).Assembly;
    protected static readonly Assembly CatalogInfraAssembly = typeof(Catalog.Infrastructure.CatalogModule).Assembly;
    protected static readonly Assembly CatalogPresentationAssembly = typeof(Catalog.Presentation.CatalogPresentationExtensions).Assembly;
    protected static readonly IEnumerable<Assembly> CatalogModuleAssemblies =
    [
        CatalogDomainAssembly,
        CatalogContractsAssembly,
        CatalogApplicationAssembly,
        CatalogInfraAssembly,
        CatalogPresentationAssembly
    ];

    // ── Users Module ─────────────────────────────────────────────
    protected static readonly Assembly UsersDomainAssembly       = typeof(User).Assembly;
    protected static readonly Assembly UsersApplicationAssembly  = typeof(IUserRepository).Assembly;
    protected static readonly Assembly UsersInfraAssembly        = typeof(Users.Infrastracture.DependencyInjection).Assembly;
    protected static readonly Assembly UsersPresentationAssembly = typeof(Extension).Assembly;

    // All Users assemblies together — used in module isolation tests
    protected static readonly IEnumerable<Assembly> UsersModuleAssemblies =
    [
        UsersDomainAssembly,
        UsersApplicationAssembly,
        UsersInfraAssembly,
        UsersPresentationAssembly
    ];
}
