using Xunit;

namespace eGrants.Tests.Integration
{
    /// <summary>
    /// Collection that serializes every smoke test class.
    ///
    /// The smoke tests all share the single process-wide <see cref="SmokeTestHost.Factory"/>.
    /// Because <c>AddSystemWebAdapters()</c> permits only one web host per process, these
    /// classes must never run in parallel with one another. Placing them all in this one
    /// collection keeps them serialized, while allowing the (host-independent) live-database
    /// test classes to run in parallel with each other now that
    /// parallelizeTestCollections is enabled.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class SmokeTestCollection
    {
        public const string Name = "Smoke tests (shared single host)";
    }
}
