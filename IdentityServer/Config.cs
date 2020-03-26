
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public class Config
    {
        // OpenID Connect allowed scopes
        // Scopes represent something you want to protect and that clients want to access.
        // In OpenID Connect though, scopes represent identity data like user id, name or email address and they need to be registered.
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        // APIs to be protected
        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource("SocialAPI", "Social Network API")
            };
        }

        // Clients allowed to request for tokens
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "AspNetCoreIdentity",
                    ClientName = "AspNetCoreIdentity Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RequireClientSecret = false,

                    RedirectUris =           { "http://localhost:5000" },
                    PostLogoutRedirectUris = { "http://localhost:5000" },
                    AllowedCorsOrigins =     { "http://localhost:5000" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "SocialAPI"
                    }
                }
            };
        }
    }
}
