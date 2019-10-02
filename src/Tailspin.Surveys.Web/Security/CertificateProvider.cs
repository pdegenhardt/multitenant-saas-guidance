// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using System.Security.Cryptography.X509Certificates;
using Tailspin.Surveys.Common;
using Tailspin.Surveys.Common.Configuration;
using Tailspin.Surveys.Web.Configuration;

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// Initializes <see cref="ConfidentialClientApplicationBuilder"/> with certificate.
    /// </summary>
    public class CertificateProvider : ICredentialsProvider
    {
        private readonly AzureAdOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tailspin.Surveys.Web.Security.CertificateProvider"/>.
        /// </summary>
        /// <param name="options">The current application configuration options.</param>
        public CertificateProvider(IOptions<ConfigurationOptions> options)
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
            Guard.ArgumentNotNull(_options, "configOptions.AzureAd");
            Guard.ArgumentNotNull(_options.Asymmetric, "configOptions.AzureAd.Assymetric");

            X509Certificate2 cert = CertificateUtility.FindCertificateByThumbprint(
                _options.Asymmetric.StoreName,
                _options.Asymmetric.StoreLocation,
                _options.Asymmetric.CertificateThumbprint,
                _options.Asymmetric.ValidationRequired);
            var certBytes = CertificateUtility.ExportCertificateWithPrivateKey(cert, out string password);

            return applicationBuilder.WithCertificate(new X509Certificate2(certBytes, password));
        }
    }
}
