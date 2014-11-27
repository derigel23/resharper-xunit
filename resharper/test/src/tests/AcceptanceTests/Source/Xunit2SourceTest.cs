using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Source
{
    [Category("xunit2")]
    public class Xunit2SourceTest : XunitSourceTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit2"; }
        }

      protected override IEnumerable<string> GetReferencedAssemblies()
      {
        foreach (var assembly in base.GetReferencedAssemblies())
        {
          yield return assembly;
        }
        yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.abstractions.dll");
        yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.core.dll");
        yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit2\xunit.execution.dll");
      }
    }
}