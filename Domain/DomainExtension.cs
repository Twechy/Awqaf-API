using Domain.Db;
using Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public static class DomainExtension
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services) => services.AddAwqafDb()
            .AddTransient<IGuardianService, GuardianService>()
            .AddTransient<IMosqueService, MosqueService>()
            .AddTransient<IRegionService, RegionService>()
            .AddTransient<ICityService, CityService>()
            .AddTransient<IStudentService, StudentService>()
            .AddTransient<IMemorizationService, MemorizationService>()
            .AddTransient<IWorkerService, WorkerService>();
    }
}