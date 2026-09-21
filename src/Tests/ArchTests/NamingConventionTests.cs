using NetArchTest.Rules;
using SharedKernal.Messaging;

namespace CleanModular.ArchTests;

/// <summary>
/// Enforces consistent naming conventions for CQRS types.
///
/// CQRS works through MediatR dispatch — the framework resolves handlers
/// by interface, not by name. But consistent naming makes the codebase
/// readable at scale. When a developer sees "CreateUserCommandHandler",
/// they immediately know: it is a handler, it handles a command, the
/// command is about creating a user.
///
/// Rules:
///   ICommand / ICommand<T>     → name must end with "Command"
///   IQuery<T>                  → name must end with "Query"
///   ICommandHandler<T>         → name must end with "CommandHandler"
///   IQueryHandler<T, R>        → name must end with "QueryHandler"
/// </summary>
public class NamingConventionTests : TestBase
{
    [Fact]
    public void PostsCommandsAndQueries_Should_FollowNamingConventions()
    {
        Types.InAssembly(PostsApplicationAssembly)
            .That().ImplementInterface(typeof(ICommand)).Or().ImplementInterface(typeof(ICommand<>))
            .Should().HaveNameEndingWith("Command").GetResult().ShouldBeSuccessful();

        Types.InAssembly(PostsApplicationAssembly)
            .That().ImplementInterface(typeof(IQuery<>))
            .Should().HaveNameEndingWith("Query").GetResult().ShouldBeSuccessful();
    }

    [Fact]
    public void PostsHandlers_Should_FollowNamingConventions()
    {
        Types.InAssembly(PostsApplicationAssembly)
            .That().ImplementInterface(typeof(ICommandHandler<>)).Or().ImplementInterface(typeof(ICommandHandler<,>))
            .Should().HaveNameEndingWith("CommandHandler").GetResult().ShouldBeSuccessful();

        Types.InAssembly(PostsApplicationAssembly)
            .That().ImplementInterface(typeof(IQueryHandler<,>))
            .Should().HaveNameEndingWith("QueryHandler").GetResult().ShouldBeSuccessful();
    }

    [Fact]
    public void Commands_Should_EndWith_Command()
    {
        Types.InAssembly(UsersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommand))
            .Or()
            .ImplementInterface(typeof(ICommand<>))
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void Queries_Should_EndWith_Query()
    {
        Types.InAssembly(UsersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQuery<>))
            .Should()
            .HaveNameEndingWith("Query")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void CommandHandlers_Should_EndWith_CommandHandler()
    {
        Types.InAssembly(UsersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(ICommandHandler<>))
            .Or()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void QueryHandlers_Should_EndWith_QueryHandler()
    {
        Types.InAssembly(UsersApplicationAssembly)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
