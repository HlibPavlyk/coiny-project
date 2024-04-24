using IdentityServer4;
using IdentityServer4.Models;

namespace CoinyProject.IdentityServer
{
    public static class Configuration
    {
        public static IEnumerable<ApiResource> GetApis() =>
            new List<ApiResource>
            {};
        public static IEnumerable<IdentityResource> GetIdentityResources() =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),

            };

        public static IEnumerable<Client> GetClients() =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "client_id_mvc",
                    ClientSecrets = { new Secret("client_secret_mvc".Sha256()) },
                    //7184
                    //7115 mvc
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:7115/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:7115/Home/Index" },
                    AllowedScopes = { 
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    },
                    /*AlwaysIncludeUserClaimsInIdToken = true,*/
                    RequireConsent = false
                },
                new Client
                {
                    ClientId = "client_id_ui",
                    ClientSecrets = { new Secret("client_secret_ui".Sha256()) },
                    //7184
                    //7115 mvc
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:7184/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:7184/Home/Index" },
                    AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile
                    },
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequireConsent = false
                }
            };
    }
}
