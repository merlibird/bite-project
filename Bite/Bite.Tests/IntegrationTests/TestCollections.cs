using System;
using System.Collections.Generic;
using System.Text;

namespace Bite.Tests.IntegrationTests;

[CollectionDefinition("Database", DisableParallelization = true)]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
