using System;
using System.Diagnostics;
using JetBrains.ReSharper.TaskRunnerFramework;

namespace XunitContrib.Runner.ReSharper.RemoteRunner
{
    public partial class RemoteTaskServer
    {
        private readonly ISimpleRemoteTaskServer server;
        private readonly ISimpleClientController clientController;

        public RemoteTaskServer(IRemoteTaskServer server, TaskExecutorConfiguration configuration)
        {
            this.server = SimpleRemoteTaskServerFactory.Create(server);
            Configuration = configuration;
            clientController = SimpleClientControllerFactory.Create(server);

            ShouldContinue = true;
        }

        public TaskExecutorConfiguration Configuration { get; private set; }

        public bool ShouldContinue
        {
            get { return server.ShouldContinue; }
            set { server.ShouldContinue = value; }
        }

        public void SetTempFolderPath(string path)
        {
            server.SetTempFolderPath(path);
        }

        public void TaskRunStarting()
        {
            clientController.TaskRunStarting();
        }

        public void TaskStarting(RemoteTask remoteTask)
        {
            clientController.TaskStarting(remoteTask);
            server.TaskStarting(remoteTask);
        }

        public void TaskException(RemoteTask remoteTask, TaskException[] exceptions)
        {
            server.TaskException(remoteTask, exceptions);
        }

        public void TaskOutput(RemoteTask remoteTask, string text, TaskOutputType outputType)
        {
            server.TaskOutput(remoteTask, text, outputType);
        }

        public void TaskFinished(RemoteTask remoteTask, string message, TaskResult result, decimal durationInSeconds)
        {
            var timeSpan = durationInSeconds != 0 ? TimeSpan.FromSeconds((double) durationInSeconds) : TimeSpan.Zero;
            TaskFinished(remoteTask, message, result, timeSpan);
        }

        public void TaskFinished(RemoteTask remoteTask, string message, TaskResult result, TimeSpan duration)
        {
            Debug.Assert(result != TaskResult.Inconclusive);

            clientController.TaskFinished(remoteTask);
            if (result == TaskResult.Skipped)
                server.TaskExplain(remoteTask, message);
            if (duration >= TimeSpan.Zero)
                server.TaskDuration(remoteTask, duration);
            server.TaskFinished(remoteTask, message, result);
        }

        public void TaskRunFinished()
        {
            clientController.TaskRunFinished();
        }

        public void CreateDynamicElement(RemoteTask remoteTask)
        {
            server.CreateDynamicElement(remoteTask);
        }

        public void ShowNotification(string message, string description)
        {
            server.ShowNotification(message, description);
        }
    }
}