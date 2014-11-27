using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests.Source
{
    [Category("xunit1")]
    public class Xunit1SourceTest : XunitSourceTest
    {
        protected override string RelativeTestDataPathSuffix
        {
            get { return "xunit1"; }
        }

      protected override IEnumerable<string> GetReferencedAssemblies()
      {
        foreach (var assembly in base.GetReferencedAssemblies())
        {
          yield return assembly;
        }
        yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.dll");
        yield return Environment.ExpandEnvironmentVariables(EnvironmentVariables.XUNIT_ASSEMBLIES + @"\xunit191\xunit.extensions.dll");
      }
    }
}