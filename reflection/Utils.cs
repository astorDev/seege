using System.Reflection;

namespace Seege.Reflection;

public static class InheritanceExtensions {
    public static bool IsImplementorOf(this Type inspectedType, Type baseType) => 
        !inspectedType.IsAbstract && !inspectedType.IsInterface && baseType.IsAssignableFrom(inspectedType);
}

public record AssemblyCollection(Assembly[] Items) {
    public static AssemblyCollection EntryAndReferenced() {
        var entryAssembly = Assembly.GetEntryAssembly()!;
        var referencedAssemblies = entryAssembly!.GetReferencedAssemblies().Select(Assembly.Load);
        var allAssemblies = referencedAssemblies.Union(new[] { entryAssembly });
        return new(allAssemblies.ToArray());
    }
}