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

namespace VpServiceAPI.Tools
{
    public class DependencyInjector
    {
        private IServiceCollection Services { get; init; }

        private static Func<string, string?> GetEnvVar = Environment.GetEnvironmentVariable;
        private static Func<bool> IsProduction = () => GetEnvVar("ASPNETCORE_ENVIRONMENT") == "Production";

        private bool _allUsersWithTestNotificator = false;
        private bool _forceTestUsers = true;
        private bool _forceTestNotificator = true;

        public DependencyInjector(ref IServiceCollection services)
        {
            Services = services;            
        }
        public void Inject()
        {
            Console.WriteLine($"Mode: {GetEnvVar("ASPNETCORE_ENVIRONMENT")}");
            InjectTools();
            InjectUpdateChecking();
            InjectNotification();
            InjectRepositories();
            InjectStatisticCreation();
            InjectStatisticProviding();
            
        }
        private void InjectTools()
        {
            Services
                .AddSingleton<IMyLogger, Logger>()
                .AddSingleton<IRoutine, Routine>()
                .AddSingleton<IDataAccess, DBAccess>()
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
                .AddSingleton<INotificationBuilder, NotificationBuilder>();


            if (IsProduction())
            {
                Services
                    .AddSingleton<INotificator, ProdNotificator>();
            }
            else
            {
                InjectWithCondition<INotificator, TestNotificator, ProdNotificator>(_allUsersWithTestNotificator || _forceTestNotificator);
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



        private void InjectWithCondition<TInterface, TTrue, TFalse>(bool mode) 
            where TInterface : class 
            where TTrue : class, TInterface 
            where TFalse : class, TInterface
        {
            if (mode)
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
