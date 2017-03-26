using System;
using System.Windows;
using Exceptionless;

namespace ImageTransparencyTool
{
    public partial class App : Application
    {
        private static Guid UUID;
        static App() {
            ExceptionlessClient.Default.Register();

            UUID = ImageTransparencyTool.Properties.Settings.Default.UUID;
            if (UUID == Guid.Empty) {
                UUID = ImageTransparencyTool.Properties.Settings.Default.UUID = Guid.NewGuid();
                ImageTransparencyTool.Properties.Settings.Default.Save();
            }

            ExceptionlessClient.Default.CreateSessionStart()
                .SetUserIdentity(UUID.ToString(), Environment.UserName)
                .SetVersion(typeof(App).Assembly.GetName().Version.ToString())
                .Submit();
        }

        protected override void OnExit(ExitEventArgs e) {
            ExceptionlessClient.Default.SubmitSessionEnd(UUID.ToString());
            base.OnExit(e);
        }
    }
}
