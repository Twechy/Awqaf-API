using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Utils.HangFire
{
    public class HangFireAuthorization : IDashboardAuthorizationFilter
    {
        private readonly IAuthorizationService _authorizationService;

        private readonly IHttpContextAccessor _httpContextAccessor;
        /*private readonly IOptions<MonitorAdminVm> _admin;*/

        public HangFireAuthorization(IAuthorizationService authorizationService,
            IHttpContextAccessor httpContextAccessor
            /*, IOptions<MonitorAdminVm> admin*/)
        {
            _authorizationService = authorizationService;
            _httpContextAccessor = httpContextAccessor;
            /*_admin = admin;*/
        }

        public bool Authorize([NotNull] DashboardContext context)
        {
            return true;
            /* if (_httpContextAccessor is null) return false;
             var name = _httpContextAccessor.HttpContext.Session.GetString("name");
             var pin = _httpContextAccessor.HttpContext.Session.GetString("pin");
             return name == _admin.Value.Name && pin == _admin.Value.Pin;*/
        }
    }

    public class MonitorAdminDto
    {
        public string Name { get; set; }
        public string Pin { get; set; }
    }
}