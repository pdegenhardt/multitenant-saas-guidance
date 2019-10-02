// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Identity.Client;

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// Interface implemented by services that provides credentials information for <see cref="ConfidentialClientApplicationBuilder"/>.
    /// </summary>
    public interface ICredentialsProvider
    {
        /// <summary>
        /// Initializes application builder with the right credentials
        /// </summary>
        /// <param name="applicationBuilder">An instance of <see cref="ConfidentialClientApplicationBuilder"/> to initialize</param>
        /// <returns>The <see cref="ConfidentialClientApplicationBuilder"/> initialized</returns>
        ConfidentialClientApplicationBuilder Initialize(ConfidentialClientApplicationBuilder applicationBuilder);
    }
}
