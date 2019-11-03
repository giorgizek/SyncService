using System;
using System.Collections;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using HibernatingRhinos.Profiler.Appender.EntityFramework;
using Zek.Extensions;
using Zek.Threading;

namespace Sync.Win
{
    class Program
    {
        private static ProjectInstaller GetProjectInstaller()
        {
            var pi = new ProjectInstaller();
            if (!string.IsNullOrWhiteSpace(AppConfig.ServiceName))
                pi.serviceInstaller.ServiceName = AppConfig.ServiceName;
            if (!string.IsNullOrWhiteSpace(AppConfig.ServiceDisplayName))
                pi.serviceInstaller.DisplayName = AppConfig.ServiceDisplayName;
            if (!string.IsNullOrWhiteSpace(AppConfig.ServiceDescription))
                pi.serviceInstaller.Description = AppConfig.ServiceDescription;
            return pi;
        }

        public static void Install()
        {
            var ti = new TransactedInstaller();
            ti.Installers.Add(GetProjectInstaller());
            ti.Context = new InstallContext("", null);
            var path = Assembly.GetExecutingAssembly().Location;
            ti.Context.Parameters["assemblypath"] = path;
            ti.Install(new Hashtable());
        }

        public static void Uninstall()
        {
            var ti = new TransactedInstaller();
            ti.Installers.Add(GetProjectInstaller());
            ti.Context = new InstallContext("", null);
            var path = Assembly.GetExecutingAssembly().Location;
            ti.Context.Parameters["assemblypath"] = path;
            ti.Uninstall(null);
        }

        static void Main(string[] args)
        {
            if (args.Contains("/install", StringComparer.InvariantCultureIgnoreCase))
            {
                Install();
                return;
            }

            if (args.Contains("/uninstall", StringComparer.InvariantCultureIgnoreCase))
            {
                Uninstall();
                return;
            }



            if (MutexHelper.IsAlreadyRunning(false, true))
                return;

            if (Debugger.IsAttached)
                InitProf();

            var service = new SyncService();
            if (args.Contains("task", StringComparer.InvariantCultureIgnoreCase))
            {
                service.TaskScheduler = true;
            }

            service.Execute(args);
        }

        //[Conditional("DEBUG")]
        private static void InitProf()
        {
            //#if DEBUG
            //EntityFrameworkProfiler.Initialize();
            //#endif
        }
    }
}
