// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Tailspin.Surveys.Common;

namespace Tailspin.Surveys.TokenStorage
{
    /// <summary>Token cache abstract provider</summary>
    /// <seealso cref="ITokenCacheProvider" />
    public abstract class TokenCacheProvider: ITokenCacheProvider
    {
        private readonly ILogger _logger;
        private ClaimsPrincipal _claimsPrincipal;

        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.TokenStorage.TokenCacheProvider"/>
        /// </summary>
        /// <param name="loggerFactory"></param>
        protected TokenCacheProvider(ILoggerFactory loggerFactory)
        {
            Guard.ArgumentNotNull(loggerFactory, nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger(this.GetType().FullName);
        }

        /// <summary>
        /// Initializes the token cache serialization.
        /// </summary>
        /// <param name="tokenCache">Token cache to serialize/deserialize</param>
        /// <param name="claimsPrincipal">Current claims principal for this token cache</param>
        /// <returns></returns>
        public Task InitializeAsync(ITokenCache tokenCache, ClaimsPrincipal claimsPrincipal)
        {
            _claimsPrincipal = claimsPrincipal;

            tokenCache.SetBeforeAccessAsync(OnBeforeAccessAsync);
            tokenCache.SetAfterAccessAsync(OnAfterAccessAsync);
            tokenCache.SetBeforeWriteAsync(OnBeforeWriteAsync);

            return Task.CompletedTask;
        }

        private static string BuildCacheKey(ClaimsPrincipal claimsPrincipal)
        {
            Guard.ArgumentNotNull(claimsPrincipal, nameof(claimsPrincipal));
            var clientId = claimsPrincipal.FindFirstValue("aud", true);
            var objectId = claimsPrincipal.GetObjectIdentifierValue();
            var cacheKey = $"UserId:{objectId}::ClientId:{clientId}";
            return cacheKey;
        }

        /// <summary>
        /// Raised AFTER MSAL added the new token in its in-memory copy of the cache.
        /// This notification is called every time MSAL accessed the cache, not just when a write took place:
        /// If MSAL's current operation resulted in a cache change, the property TokenCacheNotificationArgs.HasStateChanged will be set to true.
        /// If that is the case, we call the TokenCache.SerializeMsalV3() to get a binary blob representing the latest cache content – and persist it.
        /// </summary>
        /// <param name="args">Contains parameters used by the MSAL call accessing the cache.</param>
        private async Task OnAfterAccessAsync(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                try
                {
                    var cacheKey = BuildCacheKey(_claimsPrincipal);
                    if (!string.IsNullOrWhiteSpace(cacheKey))
                    {
                        await WriteCacheBytesAsync(cacheKey, args.TokenCache.SerializeMsalV3()).ConfigureAwait(false);
                        _logger.TokensWrittenToStore(args.ClientId, args.Account.Username);
                    }
                }
                catch (Exception exp)
                {
                    _logger.WriteToCacheFailed(exp);
                    throw;
                }
            }
        }

        private async Task OnBeforeAccessAsync(TokenCacheNotificationArgs args)
        {
            var cacheKey = BuildCacheKey(_claimsPrincipal);
            if (!string.IsNullOrEmpty(cacheKey))
            {
                byte[] tokenCacheBytes = await ReadCacheBytesAsync(cacheKey).ConfigureAwait(false);
                args.TokenCache.DeserializeMsalV3(tokenCacheBytes, shouldClearExistingCache: true);
                _logger.TokensRetrievedFromStore(cacheKey);
            }
        }

        // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        protected virtual Task OnBeforeWriteAsync(TokenCacheNotificationArgs args)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clears the cache
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal to clear cache</param>
        /// <returns></returns>
        public async Task ClearAsync(ClaimsPrincipal claimPrincipal)
        {
            var cacheKey = BuildCacheKey(claimPrincipal);
            await RemoveKeyAsync(cacheKey).ConfigureAwait(false);
            _logger.TokenCacheCleared(cacheKey);
        }

        protected abstract Task WriteCacheBytesAsync(string cacheKey, byte[] bytes);

        protected abstract Task<byte[]> ReadCacheBytesAsync(string cacheKey);

        protected abstract Task RemoveKeyAsync(string cacheKey);
    }
}

