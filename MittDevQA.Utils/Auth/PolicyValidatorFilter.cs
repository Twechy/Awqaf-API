using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Utils.Auth.JwtHelper;
using Utils.Others;
using static Utils.Auth.PolicyValidatorAuthorize;

namespace Utils.Auth
{
    public static class ServicePolicyExtenstion
    {
        public static IServiceCollection AddAuthPolicyValidator(this IServiceCollection services)
        {
            services.AddJwt();
            services.TryAddSingleton<IAuthorizationHandler, PolicyValidatorHandler>();
            services.TryAddTransient<IAuthorizationPolicyProvider, PolicyValidatorPolicy>();
            services.AddAuthorization();
            services.AddHttpContextAccessor();
            return services;
        }
    }

    public class PolicyValidatorAuthorize : AuthorizeAttribute
    {
        public PolicyValidatorAuthorize(params object[] policies)
        {
            PolicyType = policies;
        }

        private object[] _policies;

        public object[] PolicyType
        {
            get => _policies;
            set
            {
                _policies = value;
                var finalPolicy = string.Empty;
                foreach (var item in _policies)
                    finalPolicy += $"{item.GetType().AssemblyQualifiedName}-{(long)item };";
                Policy = finalPolicy;
            }
        }

        public class PolicyValidatorRequirement : IAuthorizationRequirement
        {
            public PolicyValidatorRequirement(Type[] policyTypes, string[] requirements)
            {
                Requirments = requirements;
                PolicyTypes = policyTypes;
            }

            public string[] Requirments { get; private set; }
            public Type[] PolicyTypes { get; private set; }
        }

        public class PolicyValidatorHandler : AuthorizationHandler<PolicyValidatorRequirement>
        {
            protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                PolicyValidatorRequirement requirement)
            {
                foreach (var value in requirement.Requirments)
                {
                    var i = Array.IndexOf(requirement.Requirments, value);
                    var type = requirement.PolicyTypes[i];
                    var req = type.Name;
                    if (!context.User.HasClaim(c => c.Type == req))
                        continue;
                    var user = context?.User?.FindFirst(c => c.Type == req)?.Value;
                    var userState = long.Parse(user);
                    if (userState == 0) continue;
                    if (userState.Has(long.Parse(requirement.Requirments[i])))
                    {
                        context.Succeed(requirement);
                    }
                }
                return Task.FromResult(0);
            }
        }

        public class PolicyValidatorPolicy : IAuthorizationPolicyProvider
        {
            private DefaultAuthorizationPolicyProvider DefaultPolicyProvider { get; }

            public PolicyValidatorPolicy(IOptions<AuthorizationOptions> options)
            {
                DefaultPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
            }

            public async Task<AuthorizationPolicy> GetDefaultPolicyAsync()
            {
                return await DefaultPolicyProvider.GetDefaultPolicyAsync();
            }

            public async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
            {
                List<Type> policiesTypes = new List<Type>();
                List<string> policiesValues = new List<string>();
                foreach (var item in policyName.Split(";").Where(x => !string.IsNullOrEmpty(x)))
                {
                    var policySplit = item.Split("-");
                    var policyValue = policySplit[1];
                    var policyType = Type.GetType(policySplit[0]);
                    if (policySplit.Length <= 1 || policyType == null)
                        return await DefaultPolicyProvider.GetPolicyAsync(policyName);
                    //  var enumValue = Enum.Parse(policyType, policyValue);
                    policiesTypes.Add(policyType);
                    policiesValues.Add(policyValue);
                }
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new PolicyValidatorRequirement(policiesTypes.ToArray(), policiesValues.ToArray()));
                return policy.Build();
            }

            public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
            => DefaultPolicyProvider.GetDefaultPolicyAsync();
        }
    }
}