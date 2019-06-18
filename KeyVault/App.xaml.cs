using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Security.AccessControl;
using System.Security.Principal;


namespace KeyVault
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Powered by https://www.uuidgenerator.net/
        private const string MUTEXID = @"Local\{927c35c9-d26c-4aba-8358-ab801579f843}";
        private Mutex _mutex;

        internal static string SALT = "";
        internal static string DB_PATH = "keyvault.db";

        internal static string STARTUP_ARGS= "";

        public App()
        {
            bool isSingleton;
            _mutex = new Mutex(true, MUTEXID, out isSingleton);
            if (!isSingleton)
            {
                MessageBox.Show("Another instance is already there. Please try to close it and try again.", ":(", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Environment.Exit(0);
            }
            else
            {
                // Need to Project > Add Reference > Check System.Confiration.dll
                string salt = ConfigurationManager.AppSettings["SALT"];
                if (salt != null)
                {
                    App.SALT = salt;
                }
                string dbpath = ConfigurationManager.AppSettings["DB_PATH"];
                if (dbpath != null)
                {
                    App.DB_PATH = dbpath;
                }

                DBHelper.InitDB();
            }
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            STARTUP_ARGS = String.Join(" ", e.Args);

                // Application is running
                // Process command line args
                //bool startMinimized = false;
                //for (int i = 0; i != e.Args.Length; ++i)
                //{
                //    if (e.Args[i] == "/StartMinimized")
                //    {
                //        startMinimized = true;
                //    }
                //}

                //using (var appLock = new SingleInstanceApplicationLock())
                //{
                //if (!appLock.TryAcquireExclusiveLock()) {
                //    MessageBox.Show("test");
                //this.Shutdown();
                //    Environment.Exit(1);
                //}

                // Create main application window, starting minimized if specified
                MainWindow window = new MainWindow();
                //if (startMinimized)
                //{
                //    window.WindowState = WindowState.Minimized;
                //}

                //window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Top = System.Windows.SystemParameters.WorkArea.Height - window.Height;
                window.Left = (System.Windows.SystemParameters.WorkArea.Width - window.Width) / 2;
                window.Show();
            //}
        }
    }

    sealed class SingleInstanceApplicationLock : IDisposable
    {
        ~SingleInstanceApplicationLock()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool TryAcquireExclusiveLock()
        {
            try
            {
                if (!_mutex.WaitOne(1000, false))
                    return false;
            }
            catch (AbandonedMutexException)
            {
                // Abandoned mutex, just log? Multiple instances
                // may be executed in this condition...
            }

            return _hasAcquiredExclusiveLock = true;
        }

        // Powered by https://www.uuidgenerator.net/
        private const string MutexId = @"Local\{927c35c9-d26c-4aba-8358-ab801579f843}";
        private readonly Mutex _mutex = CreateMutex();
        private bool _hasAcquiredExclusiveLock, _disposed;

        private void Dispose(bool disposing)
        {
            if (disposing && !_disposed && _mutex != null)
            {
                try
                {
                    if (_hasAcquiredExclusiveLock)
                        _mutex.ReleaseMutex();

                    _mutex.Dispose();
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        private static Mutex CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var allowEveryoneRule = new MutexAccessRule(sid,
                MutexRights.FullControl, AccessControlType.Allow);

            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            var mutex = new Mutex(false, MutexId);
            mutex.SetAccessControl(securitySettings);

            return mutex;
        }
    }
}
