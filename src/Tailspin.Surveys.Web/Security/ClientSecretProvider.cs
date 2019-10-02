// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Tailspin.Surveys.Common.Configuration;
using Tailspin.Surveys.Web.Configuration;

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// Initializes <see cref="ConfidentialClientApplicationBuilder"/> with client secret.
    /// </summary>
    public class ClientSecretProvider : ICredentialsProvider
    {
        private readonly AzureAdOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tailspin.Surveys.Web.Security.ClientSecretProvider"/>.
        /// </summary>
        /// <param name="options">The current application configuration options.</param>
        public ClientSecretProvider(IOptions<ConfigurationOptions> options)
        {
            _options = options?.Value?.AzureAd;
        }

        /// <summary>
        /// Initializes application builder with the choosen authentication method
        /// </summary>
        /// <param name="applicationBuilder">An instance of <see cref="ConfidentialClientApplicationBuilder"/> to initialize</param>
        /// <returns>The <see cref="ConfidentialClientApplicationBuilder"/> initialized</returns>
        public ConfidentialClientApplicationBuilder Initialize(ConfidentialClientApplicationBuilder applicationBuilder)
        {
            return applicationBuilder.WithClientSecret(_options.ClientSecret);
        }
    }
}
