using Domain.Abstracts;
using Domain.Db;
using Domain.Models;

namespace Domain.Services;

public interface IRegionService : IManagementService<Region>
{
}

public class RegionService : ManagementService<Region>, IRegionService
{
    public RegionService(AwqafDb dataContext) : base(dataContext)
    {
    }
}

public interface ICityService : IManagementService<City>
{
}

public class CityService : ManagementService<City>, ICityService
{
    public CityService(AwqafDb dataContext) : base(dataContext)
    {
    }
}