using NetArchTest.Rules;
using SharedKernal.Entities;
using SharedKernal.Messaging;

namespace CleanModular.ArchTests;

/// <summary>
/// Enforces design rules for encapsulation and domain purity.
///
/// These rules prevent common mistakes that slowly erode architecture quality:
///
///   1. Handlers must be internal
///      Handlers are resolved and dispatched by MediatR.
///      No external code should ever instantiate or reference them directly.
///      Making them internal enforces this boundary — the only entry point
///      is ISender.Send() or IPublisher.Publish().
///
///   2. Commands and Queries must be sealed
///      They are simple data carriers (records). There is no valid reason
///      to inherit from a command. Sealing prevents accidental inheritance
///      that would create confusing "sub-commands".
///
///   3. Domain entities must not be abstract
///      Entities represent real things in your business domain (a User, an Order).
///      An abstract entity has no business meaning — it would only exist to
///      satisfy a technical pattern, which pollutes the domain model.
/// </summary>
public class DesignRuleTests : TestBase
{
    [Fact]
    public void PostsHandlers_Should_BeInternal()
    {
        Types.InAssembly(PostsApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Or()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void PostsCommandsAndQueries_Should_BeSealed()
    {
        Types.InAssembly(PostsApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>))
            .Or()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    // ── HANDLERS ─────────────────────────────────────────────────

    [Fact]
    public void CommandHandlers_Should_BeInternal()
    {
        Types.InAssembly(UsersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void QueryHandlers_Should_BeInternal()
    {
        Types.InAssembly(UsersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .NotBePublic()
            .GetResult()
            .ShouldBeSuccessful();
    }

    // ── COMMANDS & QUERIES ───────────────────────────────────────

    [Fact]
    public void Commands_Should_BeSealed()
    {
        Types.InAssembly(UsersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Queries_Should_BeSealed()
    {
        Types.InAssembly(UsersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .BeSealed()
            .GetResult()
            .ShouldBeSuccessful();
    }

    // ── DOMAIN ENTITIES ──────────────────────────────────────────

    [Fact]
    public void DomainEntities_Should_NotBeAbstract()
    {
        Types.InAssembly(UsersDomainAssembly)
            .That()
            .Inherit(typeof(BaseEntity))
            .Should()
            .NotBeAbstract()
            .GetResult()
            .ShouldBeSuccessful();
    }
}
