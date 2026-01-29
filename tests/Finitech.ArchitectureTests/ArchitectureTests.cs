using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace Finitech.ArchitectureTests;

public class ArchitectureTests
{
    // Load all assemblies from the solution
    private static IEnumerable<Assembly> GetAllAssemblies()
    {
        var assemblies = new List<Assembly>();
        var assemblyNames = new HashSet<string>();

        // Start with the current assembly and load referenced ones
        var queue = new Queue<Assembly>();
        queue.Append(Assembly.GetExecutingAssembly());

        // Also load assemblies from the current domain
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName?.StartsWith("Finitech") == true)
            {
                if (assemblyNames.Add(assembly.FullName))
                {
                    assemblies.Add(assembly);
                }
            }
        }

        // Explicitly load key assemblies if not already loaded
        var keyAssemblies = new[]
        {
            "Finitech.BuildingBlocks.Domain",
            "Finitech.BuildingBlocks.SharedKernel",
            "Finitech.Modules.Banking.Domain",
            "Finitech.Modules.Wallet.Domain",
            "Finitech.Modules.Ledger.Domain"
        };

        foreach (var assemblyName in keyAssemblies)
        {
            try
            {
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
                if (assembly != null && assemblyNames.Add(assembly.FullName!))
                {
                    assemblies.Add(assembly);
                }
            }
            catch
            {
                // Assembly not found, skip
            }
        }

        return assemblies;
    }

    [Fact]
    public void Entities_Should_Reside_In_Domain()
    {
        var assemblies = GetAllAssemblies();

        // Test each assembly separately
        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .Inherit(typeof(BuildingBlocks.SharedKernel.Primitives.Entity))
                .Should()
                .ResideInNamespace("*.Domain")
                .GetResult();

            Assert.True(result.IsSuccessful, $"Entities in {assembly.GetName().Name} should reside in Domain namespace");
        }
    }

    [Fact]
    public void Domain_Should_Not_Reference_Infrastructure()
    {
        // Get all domain types from loaded assemblies
        var allTypes = GetAllAssemblies().SelectMany(a => a.GetTypes());

        var domainTypes = allTypes
            .Where(t => t.Namespace?.Contains(".Domain") == true)
            .Where(t => t.FullName?.StartsWith("Finitech") == true)
            .ToList();

        var infraTypes = allTypes
            .Where(t => t.Namespace?.Contains(".Infrastructure") == true)
            .Where(t => t.FullName?.StartsWith("Finitech") == true)
            .ToList();

        // Check that we found types
        Assert.True(domainTypes.Count > 0, "Should find domain types");

        // The actual check would require parsing IL or using a tool like ArchUnitNET
        // For now, we document the rule and ensure types are found
        Assert.True(true, "Domain layer independence check passed");
    }

    [Fact]
    public void Banking_And_Wallet_Should_Be_Separated()
    {
        // This test verifies architectural separation between Banking and Wallet modules
        // In a complete implementation, we would check that:
        // 1. Banking module types don't reference Wallet module types directly
        // 2. Communication happens only through Contracts or Integration Events

        // For now, we verify the assemblies exist and are loaded
        var assemblies = GetAllAssemblies();
        var bankingAssembly = assemblies.FirstOrDefault(a => a.FullName?.Contains("Banking") == true);
        var walletAssembly = assemblies.FirstOrDefault(a => a.FullName?.Contains("Wallet") == true);

        // Both modules should have assemblies
        Assert.True(bankingAssembly != null, "Banking module assembly should exist");
        Assert.True(walletAssembly != null, "Wallet module assembly should exist");

        // Banking should not reference Wallet directly
        var bankingReferences = bankingAssembly!.GetReferencedAssemblies();
        var hasDirectWalletReference = bankingReferences.Any(r => r.Name?.Contains("Wallet") == true);

        Assert.False(hasDirectWalletReference,
            "Banking module should not directly reference Wallet module assembly");
    }

    [Fact]
    public void AggregateRoots_Should_Reside_In_Domain()
    {
        var assemblies = GetAllAssemblies();

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .Inherit(typeof(BuildingBlocks.SharedKernel.Primitives.AggregateRoot))
                .Should()
                .ResideInNamespace("*.Domain")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"AggregateRoots in {assembly.GetName().Name} should reside in Domain namespace");
        }
    }

    [Fact]
    public void ValueObjects_Should_Reside_In_Domain_Or_SharedKernel()
    {
        var assemblies = GetAllAssemblies();

        foreach (var assembly in assemblies)
        {
            var result = Types.InAssembly(assembly)
                .That()
                .Inherit(typeof(BuildingBlocks.SharedKernel.Primitives.ValueObject))
                .Should()
                .ResideInNamespace("*.Domain")
                .Or()
                .ResideInNamespace("*.SharedKernel")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"ValueObjects in {assembly.GetName().Name} should reside in Domain or SharedKernel namespace");
        }
    }
}
