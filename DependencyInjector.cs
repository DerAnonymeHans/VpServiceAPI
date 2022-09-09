using Microsoft.Extensions.DependencyInjection;
using System;
using VpServiceAPI.Interfaces;
using VpServiceAPI.Jobs.Analysing;
using VpServiceAPI.Jobs.Checking;
using VpServiceAPI.Jobs.Notification;
using VpServiceAPI.Jobs.Routines;
using VpServiceAPI.Jobs.StatExtraction;
using VpServiceAPI.Jobs.StatProviding;
using VpServiceAPI.Repositories;
using VpServiceAPI.Tools;

namespace VpServiceAPI
{
    public class DependencyInjector
    {
        private IServiceCollection Services { get; init; }
        private UTestDependencyInjector? UTestInjector { get; init; }

        private static Func<string, string?> GetEnvVar = Environment.GetEnvironmentVariable;
        private static Func<bool> IsProduction = () => GetEnvVar("ASPNETCORE_ENVIRONMENT") == "Production";

        private bool _allUsersWithTestNotificator = false; // or test users with prod notificator
        private bool _forceTestUsers = false;
        private bool _forceTestNotificator = false;

        // ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG
        // ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG
        // ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG
        private bool _forceProdNotificator = false;
        // ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG
        // ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG
        // ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG ACHTUNG

        public DependencyInjector(ref IServiceCollection services)
        {
            if (GetEnvVar("MODE") == "UTesting")
            {                
                UTestInjector = new UTestDependencyInjector(ref services);
            }
            Services = services;
        }
        public void Inject()
        {
            Console.WriteLine($"Environment: {GetEnvVar("ASPNETCORE_ENVIRONMENT")}");
            if(UTestInjector is not null)
            {
                UTestInjector.Inject();
                return;
            }
            InjectTools();
            InjectUpdateChecking();
            InjectNotification();
            InjectRepositories();
            InjectStatisticCreation();
            InjectStatisticProviding();
            InjectAuthentication();


        }
        private void InjectTools()
        {
            Services
                .AddSingleton<IMyLogger, Logger>()
                .AddSingleton<IRoutine, Routine>()
                .AddSingleton<IDBAccess, DBAccess>()
                .AddSingleton<IDataQueries, DataQueries>()
                .AddSingleton<IWebScraper, WebScraper>();
            if (IsProduction())
            {
                Services.AddSingleton<IOutputter, ProdOutputter>();
            }
            else
            {
                Services.AddSingleton<IOutputter, ConsoleOutputter>();
            }
        }
        private void InjectUpdateChecking()
        {
            Services
                .AddSingleton<IUpdateChecker, UpdateChecker>()
                .AddSingleton<IRoutine, Routine>();

            Console.WriteLine($"PlanSource: {GetEnvVar("VP_SOURCE")}");
            switch (GetEnvVar("VP_SOURCE"))
            {
                case "VP24":
                    Services
                        .AddSingleton<IPlanHTMLProvider, ProdPlanProviderVP24>()
                        .AddSingleton<IPlanConverter, PlanConverterVP24>();
                    break;
                case "KEPLER":
                    Services
                        .AddSingleton<IPlanHTMLProvider, ProdPlanProviderKEPLER>()
                        .AddSingleton<IPlanConverter, PlanConverterKEPLER>();
                    break;
                case "STATIC":
                    Services
                        .AddSingleton<IPlanHTMLProvider, TestPlanProvider>()
                        .AddSingleton<IPlanConverter, PlanConverterVP24>();
                    break;
            }
        }
        private void InjectNotification()
        {
            Services
                .AddSingleton<INotificationJob, NotificationJob>()
                .AddSingleton<IEmailBuilder, EmailBuilder>();


            if (IsProduction() || _forceProdNotificator)
            {
                Services
                    .AddSingleton<IEmailJob, ProdEmailJob>()
                    .AddSingleton<IPushJob, ProdPushJob>();
            }
            else
            {
                InjectWithCondition<IEmailJob, TestEmailJob, ProdEmailJob>(_allUsersWithTestNotificator || _forceTestNotificator);
                InjectWithCondition<IPushJob, TestPushJob, ProdPushJob>(_allUsersWithTestNotificator || _forceTestNotificator);
            }
        }
        private void InjectRepositories()
        {
            Services
                .AddSingleton<IArtworkRepository, ArtworkRepository>()
                .AddSingleton<IStatExtractor, StatExtractor>()
                .AddSingleton<IExtraRepository, ExtraRepository>()
                .AddSingleton<ITeacherRepository, TeacherRepository>();

            if (IsProduction())
            {
                Services.AddSingleton<IUserRepository, ProdUserRepository>();
            }
            else
            {
                InjectWithCondition<IUserRepository, ProdUserRepository, TestUserRepository>(_allUsersWithTestNotificator && !_forceTestUsers);
            }
        }
        private void InjectStatisticCreation()
        {
            Services
                .AddSingleton<IPlanAnalyser, PlanAnalyser>()
                .AddSingleton<IAnalysedPlanSaver, AnalysedPlanSaver>();
        }
        private void InjectStatisticProviding()
        {
            Services
                .AddSingleton<IByCountProvider, ByCountProvider>()
                .AddSingleton<IByTimeProvider, ByTimeProvider>()
                .AddSingleton<IByWhoProvider, ByWhoProvider>()
                .AddSingleton<IByComparisonProvider, ByComparisonProvider>()
                .AddSingleton<IByGeneralProvider, GeneralProvider>()
                .AddSingleton<IByMetaProvider, ByMetaProvider>();
        }
        private void InjectAuthentication()
        {

        }


        private void InjectWithCondition<TInterface, TTrue, TFalse>(bool condition)
            where TInterface : class
            where TTrue : class, TInterface
            where TFalse : class, TInterface
        {
            if (condition)
            {
                Services.AddSingleton<TInterface, TTrue>();
            }
            else
            {
                Services.AddSingleton<TInterface, TFalse>();
            }

        }
    }



    public class UTestDependencyInjector
    {
        private IServiceCollection Services { get; init; }

        private const string VpSource = "STATIC";


        public UTestDependencyInjector(ref IServiceCollection services)
        {
            Services = services;
        }
        public void Inject()
        {
            Console.WriteLine($"Environment: UnitTesting");
            InjectTools();
            InjectUpdateChecking();
            InjectNotification();
            InjectRepositories();
            InjectStatisticCreation();
            InjectStatisticProviding();
            InjectAuthentication();
        }
        private void InjectTools()
        {
            Services
                .AddSingleton<IMyLogger, Logger>()
                .AddSingleton<IRoutine, Routine>()
                .AddSingleton<IDBAccess, DBAccess>()
                .AddSingleton<IDataQueries, UTestingDataQueries>()
                .AddSingleton<IWebScraper, WebScraper>()
                .AddSingleton<IOutputter, ConsoleOutputter>();
        }
        private void InjectUpdateChecking()
        {
            Services
                .AddSingleton<IUpdateChecker, UpdateChecker>()
                .AddSingleton<IRoutine, Routine>();

            switch (VpSource)
            {
                case "VP24":
                    Services
                        .AddSingleton<IPlanHTMLProvider, ProdPlanProviderVP24>()
                        .AddSingleton<IPlanConverter, PlanConverterVP24>();
                    break;
                case "KEPLER":
                    Services
                        .AddSingleton<IPlanHTMLProvider, ProdPlanProviderKEPLER>()
                        .AddSingleton<IPlanConverter, PlanConverterKEPLER>();
                    break;
                case "STATIC":
                    Services
                        .AddSingleton<IPlanHTMLProvider, TestPlanProvider>()
                        .AddSingleton<IPlanConverter, PlanConverterVP24>();
                    break;
            }
        }
        private void InjectNotification()
        {
            Services
                .AddSingleton<INotificationJob, NotificationJob>()
                .AddSingleton<IEmailBuilder, EmailBuilder>()
                .AddSingleton<IEmailJob, TestEmailJob>()
                .AddSingleton<IPushJob, TestPushJob>();

        }
        private void InjectRepositories()
        {
            Services
                .AddSingleton<IArtworkRepository, ArtworkRepository>()
                .AddSingleton<IStatExtractor, StatExtractor>()
                .AddSingleton<IExtraRepository, ExtraRepository>()
                .AddSingleton<ITeacherRepository, TeacherRepository>()
                .AddSingleton<IUserRepository, ProdUserRepository>();
        }
        private void InjectStatisticCreation()
        {
            Services
                .AddSingleton<IPlanAnalyser, PlanAnalyser>()
                .AddSingleton<IAnalysedPlanSaver, AnalysedPlanSaver>();
        }
        private void InjectStatisticProviding()
        {
            Services
                .AddSingleton<IByCountProvider, ByCountProvider>()
                .AddSingleton<IByTimeProvider, ByTimeProvider>()
                .AddSingleton<IByWhoProvider, ByWhoProvider>()
                .AddSingleton<IByComparisonProvider, ByComparisonProvider>()
                .AddSingleton<IByGeneralProvider, GeneralProvider>()
                .AddSingleton<IByMetaProvider, ByMetaProvider>();
        }
        private void InjectAuthentication()
        {

        }


        private void InjectWithCondition<TInterface, TTrue, TFalse>(bool condition)
            where TInterface : class
            where TTrue : class, TInterface
            where TFalse : class, TInterface
        {
            if (condition)
            {
                Services.AddSingleton<TInterface, TTrue>();
            }
            else
            {
                Services.AddSingleton<TInterface, TFalse>();
            }

        }
    }
}
