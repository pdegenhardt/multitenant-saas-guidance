// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tailspin.Surveys.Common;
using Tailspin.Surveys.Common.Configuration;
using Tailspin.Surveys.Web.Configuration;
using Tailspin.Surveys.Web.Logging;
using Microsoft.Identity.Client;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Tailspin.Surveys.TokenStorage;

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// This service helps with the acquisition of access tokens from Azure Active Directory.
    /// </summary>
    public class SurveysTokenService : ISurveysTokenService
    {
        private readonly AzureAdOptions _adOptions;
        private readonly ICredentialsProvider _credentialsProvider;
        private readonly ILogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenCacheProvider _tokenCacheProvider;

        private HttpContext CurrentHttpContext => _httpContextAccessor.HttpContext;

        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.Web.Security.SurveysTokenService"/>.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="credentialsProvider"></param>
        /// <param name="logger"></param>
        /// <param name="httpContextAccessor"></param>
        public SurveysTokenService(IOptions<ConfigurationOptions> options,
                                   ICredentialsProvider credentialsProvider,
                                   ILogger<SurveysTokenService> logger,
                                   IHttpContextAccessor httpContextAccessor,
                                   ITokenCacheProvider tokenCacheProvider)
        {
            _adOptions = options?.Value?.AzureAd;
            _credentialsProvider = credentialsProvider;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _tokenCacheProvider = tokenCacheProvider;
        }

        /// <summary>
        /// This method retrieves the access token for the WebAPI resource that has previously
        /// been retrieved and cached. This method will fail if an access token for the WebAPI 
        /// resource has not been retrieved and cached. You can use the RequestAccessTokenAsync
        /// method to retrieve and cache access tokens.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> for the user to whom the access token belongs.</param>
        /// <returns>A string access token wrapped in a <see cref="Task"/></returns>
        public async Task<string> GetTokenForWebApiAsync(ClaimsPrincipal user)
        {
            return await GetAccessTokenForResourceAsync(_adOptions.WebApiResourceId, user)
                .ConfigureAwait(false);
        }

        private async Task<string> GetAccessTokenForResourceAsync(string resource, ClaimsPrincipal user)
        {
            AuthenticationResult authenticationResult = null;

            var userId = user.GetObjectIdentifierValue();
            var tenantId = user.GetTenantIdValue();
            var issuerValue = user.GetIssuerValue();
            var userName = user.Identity?.Name;
            var scopes = new string[] { resource + "/.default" };
            var identifier = $"{userId}.{tenantId}";

            try
            {
                _logger.BearerTokenAcquisitionStarted(resource, userName, issuerValue);

                var application = BuildConfidentialClientApplication(user);
                var account = await application.GetAccountAsync(identifier)
                    .ConfigureAwait(false);

                authenticationResult = await application.AcquireTokenSilent(scopes, account).ExecuteAsync()
                    .ConfigureAwait(false);

                _logger.BearerTokenAcquisitionSucceeded(resource, userName, issuerValue);
                return authenticationResult.AccessToken;
            }
            catch (MsalException ex)
            {
                _logger.BearerTokenAcquisitionFailed(resource, userName, issuerValue, ex);
                throw new AuthenticationException($"AcquireTokenSilent failed for user: {userId}", ex);
            }
        }

        /// <summary>
        /// This method acquires an access token using an AcquireTokenOnBehalfOf. The access token is then cached
        /// in a <see cref="TokenCache"/> to be used later (by calls to GetTokenForWebApiAsync).
        /// </summary>
        /// <param name="claimsPrincipal">A <see cref="ClaimsPrincipal"/> for the signed in user</param>
        /// <param name="authorizationCode">a string authorization code obtained when the user signed in</param>
        /// <param name="resource">The resouce identifier of the target resource</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task{AuthenticationResult}"/>.</returns>
        public async Task<AuthenticationResult> RequestTokenAsync(ClaimsPrincipal claimsPrincipal, string authorizationCode, string resource)
        {
            Guard.ArgumentNotNull(claimsPrincipal, nameof(claimsPrincipal));
            Guard.ArgumentNotNullOrWhiteSpace(authorizationCode, nameof(authorizationCode));
            Guard.ArgumentNotNullOrWhiteSpace(resource, nameof(resource));

            var scopes = new string[] { resource + "/.default" };
            try
            {
                var application = BuildConfidentialClientApplication(claimsPrincipal);
                return await application.AcquireTokenByAuthorizationCode(scopes, authorizationCode).ExecuteAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.AuthenticationCodeRedemptionFailed(ex);
                throw;
            }            
        }

        /// <summary>
        /// This method clears the user's <see cref="Microsoft.IdentityModel.Clients.ActiveDirectory.TokenCache"/>.
        /// </summary>
        /// <param name="claimsPrincipal">The <see cref="System.Security.Claims.ClaimsPrincipal"/> for the user</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/></returns>
        public async Task ClearCacheAsync(ClaimsPrincipal claimPrincipal)
        {
            Guard.ArgumentNotNull(claimPrincipal, nameof(claimPrincipal));
            await _tokenCacheProvider?.ClearAsync(claimPrincipal);
        }

        /// <summary>
        /// Creates an MSAL Confidential client application if needed
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        /// <returns></returns>
        private IConfidentialClientApplication BuildConfidentialClientApplication(ClaimsPrincipal claimsPrincipal)
        {
            Guard.ArgumentNotNull(claimsPrincipal, nameof(claimsPrincipal));

            var request = CurrentHttpContext.Request;
            var currentUri = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase, request.Path);

            var application = ConfidentialClientApplicationBuilder.Create(_adOptions.ClientId)
                .WithRedirectUri(currentUri)
                .WithAuthority(Constants.AuthEndpointPrefix)
                .WithCredentials(_credentialsProvider)
                .Build();
            
            // Initialize token cache provider
            _tokenCacheProvider?.InitializeAsync(application.UserTokenCache, claimsPrincipal);

            return application;
        }

    }
}
