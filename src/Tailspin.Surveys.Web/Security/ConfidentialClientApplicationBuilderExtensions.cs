// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Identity.Client;

namespace Tailspin.Surveys.Web.Security
{
    public static class ConfidentialClientApplicationBuilderExtensions
    {
        /// <summary>
        /// Adds client credentials information to the <see cref="ConfidentialClientApplicationBuilder"/>
        /// </summary>
        /// <param name="applicationBuilder">The <see cref="ConfidentialClientApplicationBuilder"/> to initialize</param>
        /// <param name="credentialProvider">The <see cref="ICredentialsProvider"/> to use for initialization</param>
        /// <returns></returns>
        public static ConfidentialClientApplicationBuilder WithCredentials(
            this ConfidentialClientApplicationBuilder applicationBuilder,
            ICredentialsProvider credentialProvider)
        {
            return credentialProvider.Initialize(applicationBuilder);
        }
    }
}
