using NUnit.Framework;

// Preserve deterministic isolation during migration; opt fixtures into parallelism only after review.
[assembly: NonParallelizable]
[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[assembly: SetCulture("en-GB")]
[assembly: SetUICulture("en-GB")]
