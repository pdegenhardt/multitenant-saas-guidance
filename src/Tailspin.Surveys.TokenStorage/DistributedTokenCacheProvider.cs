// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Tailspin.Surveys.TokenStorage
{
    public class DistributedTokenCacheProvider : TokenCacheProvider
    {
        private readonly IDistributedCache _distributedCache;

        /// <summary>
        /// Initializes a new instance of <see cref="Tailspin.Surveys.TokenStorage.DistributedTokenCacheProvider"/>
        /// </summary>
        /// <param name="distributedCache"></param>
        /// <param name="loggerFactory"></param>
        public DistributedTokenCacheProvider(IDistributedCache distributedCache, ILoggerFactory loggerFactory) :
            base(loggerFactory)
        {
            _distributedCache = distributedCache;
        }

        protected override async Task RemoveKeyAsync(string cacheKey)
        {
            await _distributedCache.RemoveAsync(cacheKey).ConfigureAwait(false);
        }

        protected override async Task<byte[]> ReadCacheBytesAsync(string cacheKey)
        {
            return await _distributedCache.GetAsync(cacheKey).ConfigureAwait(false);
        }

        protected override async Task WriteCacheBytesAsync(string cacheKey, byte[] bytes)
        {
            await _distributedCache.SetAsync(cacheKey, bytes).ConfigureAwait(false);
        }
    }
}
