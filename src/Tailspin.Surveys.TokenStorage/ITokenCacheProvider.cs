// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Identity.Client;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Tailspin.Surveys.TokenStorage
{
    /// <summary>
    /// Token cache provider interface.
    /// </summary>
    public interface ITokenCacheProvider
    {
        /// <summary>
        /// Initializes the token cache serialization.
        /// </summary>
        /// <param name="tokenCache">Token cache to serialize/deserialize</param>
        /// <param name="claimsPrincipal">Current claims principal for this token cache</param>
        /// <returns></returns>
        Task InitializeAsync(ITokenCache tokenCache, ClaimsPrincipal claimsPrincipal);

        /// <summary>
        /// Clears the cache
        /// </summary>
        /// <param name="claimsPrincipal">Claims principal to clear cache</param>
        /// <returns></returns>
        Task ClearAsync(ClaimsPrincipal claimPrincipal);
    }
}