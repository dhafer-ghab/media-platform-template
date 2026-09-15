using NetArchTest.Rules;

namespace CleanModular.ArchTests;

/// <summary>
/// Enforces that modules are completely blind to each other.
///
/// In a modular monolith, each module owns its domain independently.
/// A direct reference from Users to Posts creates coupling that defeats
/// the purpose of modular design — you lose independent deployability,
/// testability, and replaceability of modules.
///
/// ALLOWED:   Users → SharedKernal  (building block, not a module)
/// FORBIDDEN: Users → Posts         (cross-module direct reference)
///            Users → Storage       (cross-module direct reference)
///
/// HOW MODULES COMMUNICATE (without direct references):
///   Option  — MediatR INotification:
///              Users publishes a notification; Posts has a handler for it.
///
/// HOW TO EXTEND:
/// When Posts module is added, mirror the same pattern:
///
///   [Fact]
///   public void PostsModule_ShouldNot_Reference_UsersModule()
///   {
///       Types.InAssemblies(PostsModuleAssemblies)
///           .Should()
///           .NotHaveDependencyOn("Users")
///           .GetResult()
///           .ShouldBeSuccessful();
///   }
/// </summary>
public class ModuleIsolationTests : TestBase
{
    [Fact]
    public void UsersModule_ShouldNot_Reference_PostsModule()
    {
        Types.InAssemblies(UsersModuleAssemblies)
            .Should()
            .NotHaveDependencyOn("Posts")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void UsersModule_ShouldNot_Reference_StorageModule()
    {
        Types.InAssemblies(UsersModuleAssemblies)
            .Should()
            .NotHaveDependencyOn("Storage")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void StorageModule_ShouldNot_Reference_UsersModule()
    {
        Types.InAssemblies(StorageModuleAssemblies)
            .Should()
            .NotHaveDependencyOn("Users")
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void StorageModule_ShouldNot_Reference_PostsModule()
    {
        Types.InAssemblies(StorageModuleAssemblies)
            .Should()
            .NotHaveDependencyOn("Posts")
            .GetResult()
            .ShouldBeSuccessful();
    }
}
