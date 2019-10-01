// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Tailspin.Surveys.Common;

namespace Tailspin.Surveys.Web.Security
{
    /// <summary>
    /// Extension methods for the ASP.NET Context.
    /// </summary>
    internal static class ContextExtensions
    {
        /// <summary>
        /// Extension method for RedirectContext to see if the current process flow is the sign up process.
        /// </summary>
        /// <param name="context">PropertiesContext from ASP.NET.</param>
        /// <returns>true if the user is signing up a tenant, otherwise, false.</returns>
        internal static bool IsSigningUp(this RedirectContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            return IsSigningUp(context.Properties);
        }

        /// <summary>
        /// Extension method for TokenValidatedContext to see if the current process flow is the sign up process.
        /// </summary>
        /// <param name="context">RemoteAuthenticationContext from ASP.NET.</param>
        /// <returns>true if the user is signing up a tenant, otherwise, false.</returns>
        internal static bool IsSigningUp(this TokenValidatedContext context)
        {
            Guard.ArgumentNotNull(context, nameof(context));
            return IsSigningUp(context.Properties);
        }

        private static bool IsSigningUp(AuthenticationProperties properties)
        {
            // Check the HTTP context and convert to string
            if ((properties == null) ||
                (!properties.Items.TryGetValue("signup", out string signupValue)))
            {
                return false;
            }

            // We have found the value, so see if it's valid
            if (!bool.TryParse(signupValue, out bool isSigningUp))
            {
                // The value for signup is not a valid boolean, throw                
                throw new InvalidOperationException($"'{signupValue}' is an invalid boolean value");
            }

            return isSigningUp;
        }
    }
}
