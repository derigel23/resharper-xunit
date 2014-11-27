using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using JetBrains.Application.Components;
using JetBrains.Application.platforms;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.TestFramework.Components.UnitTestSupport;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using XunitContrib.Runner.ReSharper.RemoteRunner;
using XunitContrib.Runner.ReSharper.UnitTestProvider;
using PlatformID = JetBrains.Application.platforms.PlatformID;

namespace XunitContrib.Runner.ReSharper.Tests.AcceptanceTests
{
    public abstract class XunitTaskRunnerTestBase : UnitTestTaskRunnerTestBase
    {
        private Action<IProjectFile, UnitTestSessionTestImpl, List<IList<UnitTestTask>>, Lifetime> execute;

        protected XunitTaskRunnerTestBase(string environmentId)
        {
            XunitEnvironment = GetAllEnvironments().Single(e => e.Id == environmentId);
        }

        protected IXunitEnvironment XunitEnvironment { get; private set; }

        private static IEnumerable<IXunitEnvironment> GetAllEnvironments()
        {
            // TODO: Add in all old versions of xunit?
            yield return new Xunit1Environment();
            yield return new Xunit2Environment();
        }

        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();

            EnvironmentVariables.SetUp(BaseTestDataPath);
            EnsureReferences();
        }

        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
            CleanupReferences();
        }

        private void EnsureReferences()
        {
            // Copy the xunit dlls to the current dir
            foreach (var assembly in GetReferencedAssemblies())
            {
                var assemblyPath = FileSystemPath.Parse(assembly);
                var destination = TestDataPath2.CombineWithShortName(assemblyPath.Name);

                if (assemblyPath.IsAbsolute && assemblyPath.ExistsFile && !destination.ExistsFile)
                    assemblyPath.CopyFile(destination, true);
            }
        }

        private void CleanupReferences()
        {
            foreach (var assembly in GetReferencedAssemblies())
            {
                var assemblyPath = FileSystemPath.Parse(assembly);
                var destination = TestDataPath2.CombineWithShortName(assemblyPath.Name);

                if (assemblyPath.IsAbsolute && assemblyPath.ExistsFile && destination.ExistsFile)
                    destination.DeleteFile();
            }
        }

        protected override string RelativeTestDataPath
        {
            get { return @"Runner\"; }
        }

        protected override RemoteTaskRunnerInfo GetRemoteTaskRunnerInfo()
        {
            return new RemoteTaskRunnerInfo(XunitTaskRunner.RunnerId, typeof(XunitTaskRunner));
        }

        protected override IEnumerable<string> GetReferencedAssemblies()
        {
            return XunitEnvironment.GetReferences(GetPlatformID(), TestDataPath2);
        }

        protected override PlatformID GetPlatformID()
        {
            return XunitEnvironment.GetPlatformID();
        }

        // You normally call DoOneTest, DoSolution, DoTest, etc from a test
        // method. This will run the test, save the output and compare against
        // the gold file. This doesn't work with xunit, which runs tests in an
        // arbitrary order. I'd rather not reorder the output, instead, capture
        // the output and assert over it
        protected override void DoOneTest(string testName)
        {
            DoTestSolution(EnsureTestDll(testName));
        }

        protected void DoOneTestWithStrictOrdering(string testName)
        {
            execute = ExecuteWithGold;
            DoOneTest(testName);
        }

        protected IList<XElement> DoOneTestWithCapturedOutput(string testName)
        {
            IList<XElement> messages = null;
            execute = (projectFile, session, sequences, lifetime) =>
            {
                messages = ExecuteWithCapture(session, sequences, lifetime);
            };
            DoOneTest(testName);
            return messages;
        }

        private string EnsureTestDll(string testName)
        {
            // Get dll + cs file (use TestFileExtensionAttribute to use other
            // than .cs). Check existence + file stamps. If missing or out of
            // date, rebuild. Then call DoTestSolution with dll
            var source = GetTestDataFilePath2(testName + Extension);
            var dll = GetTestDataFilePath2(testName + "." + XunitEnvironment.Id + ".dll");

            var references = GetReferencedAssemblies().Select(Environment.ExpandEnvironmentVariables).ToArray();
            if (!dll.ExistsFile || source.FileModificationTimeUtc > dll.FileModificationTimeUtc || ReferencesAreNewer(references, dll.FileModificationTimeUtc))
            {
                CompileUtil.CompileCs(ShellInstance.GetComponent<IFrameworkDetectionHelper>(), source, dll, references, generateXmlDoc: false, generatePdb: false, 
                    framework: GetPlatformID().Version);
            }
            return dll.Name;
        }

        private static bool ReferencesAreNewer(IEnumerable<string> references, DateTime dateTime)
        {
            return references.Any(r => FileSystemPath.Parse(r).FileModificationTimeUtc > dateTime);
        }

        protected override void Execute(IProjectFile projectFile, UnitTestSessionTestImpl session, List<IList<UnitTestTask>> sequences, Lifetime lt)
        {
            execute(projectFile, session, sequences, lt);
        }

        private void ExecuteWithGold(IProjectFile projectFile, UnitTestSessionTestImpl session,
            List<IList<UnitTestTask>> sequences, Lifetime lt)
        {
            ExecuteWithGold(projectFile.Location.FullPath, output => Execute(session, sequences, lt, output));
        }

        private IList<XElement> ExecuteWithCapture(UnitTestSessionTestImpl session,
            List<IList<UnitTestTask>> sequences, Lifetime lt)
        {
            using (var output = new StringWriter())
            {
                Execute(session, sequences, lt, output);
                var messages = output.ToString();
                Console.WriteLine(messages);
                return CaptureMessages(messages);
            }
        }

        private IList<XElement> CaptureMessages(string output)
        {
            // Now we have to reparse xml fragments. Sheesh.
            var xml = string.Format("<messages>{0}</messages>", output);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xml);

            var xDocument = XDocument.Parse(xml);
            return xDocument.Element("messages").Elements().ToList();
        }

        protected override XunitTestMetadataExplorer MetadataExplorer
        {
            get
            {
                return new XunitTestMetadataExplorer(Solution.GetComponent<XunitTestProvider>(),
                    Solution.GetComponent<UnitTestElementFactory>());
            }
        }
    }
}
