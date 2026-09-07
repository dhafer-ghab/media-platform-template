using NetArchTest.Rules;

namespace CleanModular.ArchTests;

/// <summary>
/// Enforces Clean Architecture dependency direction within each module.
///
/// The allowed dependency flow is strictly inward:
///
///   Presentation → Application → Domain
///                ↗
///   Infrastructure
///
/// - Domain:         knows nothing outside itself (only SharedKernal)
/// - Application:    orchestrates use cases; defines contracts (IUserRepository)
/// - Infrastructure: implements Application contracts; never leaked upward
/// - Presentation:   maps HTTP → MediatR commands/queries; knows only Application
/// </summary>
public class LayerDependencyTests : TestBase
{
    [Fact]
    public void CatalogLayers_ShouldRespect_DependencyDirection()
    {
        Types.InAssembly(CatalogContractsAssembly).Should()
            .NotHaveDependencyOnAny(
                CatalogDomainAssembly.GetName().Name,
                CatalogApplicationAssembly.GetName().Name,
                CatalogInfraAssembly.GetName().Name,
                CatalogPresentationAssembly.GetName().Name)
            .GetResult().ShouldBeSuccessful();
        Types.InAssembly(CatalogDomainAssembly).Should()
            .NotHaveDependencyOnAny(
                CatalogApplicationAssembly.GetName().Name,
                CatalogInfraAssembly.GetName().Name,
                CatalogPresentationAssembly.GetName().Name)
            .GetResult().ShouldBeSuccessful();
        Types.InAssembly(CatalogApplicationAssembly).Should()
            .NotHaveDependencyOnAny(
                CatalogInfraAssembly.GetName().Name,
                CatalogPresentationAssembly.GetName().Name)
            .GetResult().ShouldBeSuccessful();
        Types.InAssembly(CatalogPresentationAssembly).Should()
            .NotHaveDependencyOnAny(
                CatalogDomainAssembly.GetName().Name,
                CatalogInfraAssembly.GetName().Name)
            .GetResult().ShouldBeSuccessful();
    }

    // ── DOMAIN ───────────────────────────────────────────────────

    [Fact]
    public void Domain_ShouldNot_Reference_Application()
    {
        Types.InAssembly(UsersDomainAssembly)
            .Should()
            .NotHaveDependencyOn(UsersApplicationAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Domain_ShouldNot_Reference_Infrastructure()
    {
        Types.InAssembly(UsersDomainAssembly)
            .Should()
            .NotHaveDependencyOn(UsersInfraAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Domain_ShouldNot_Reference_Presentation()
    {
        Types.InAssembly(UsersDomainAssembly)
            .Should()
            .NotHaveDependencyOn(UsersPresentationAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    // ── APPLICATION ──────────────────────────────────────────────

    [Fact]
    public void Application_ShouldNot_Reference_Infrastructure()
    {
        // Application defines IUserRepository.
        // Infrastructure implements it.
        // If Application referenced Infrastructure, the abstraction would be pointless.
        Types.InAssembly(UsersApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(UsersInfraAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Application_ShouldNot_Reference_Presentation()
    {
        Types.InAssembly(UsersApplicationAssembly)
            .Should()
            .NotHaveDependencyOn(UsersPresentationAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    // ── PRESENTATION ─────────────────────────────────────────────

    [Fact]
    public void Presentation_ShouldNot_Reference_Infrastructure()
    {
        // Presentation dispatches commands through MediatR.
        // It should never know which database or ORM is being used.
        Types.InAssembly(UsersPresentationAssembly)
            .Should()
            .NotHaveDependencyOn(UsersInfraAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Presentation_ShouldNot_Reference_Domain()
    {
        // Presentation works with DTOs returned by Application.
        // It should never reach into the Domain model directly.
        Types.InAssembly(UsersPresentationAssembly)
            .Should()
            .NotHaveDependencyOn(UsersDomainAssembly.GetName().Name)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
