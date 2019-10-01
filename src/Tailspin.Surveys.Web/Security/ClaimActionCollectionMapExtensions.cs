using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using System;

namespace Tailspin.Surveys.Web.Security
{
    public static class ClaimActionCollectionMapExtensions
    {
        /// <summary>
        /// Clears any current ClaimsActions and maps all values from the json user data as claims, excluding duplicates.
        /// </summary>
        /// <param name="collection"></param>
        public static void MapAll(this ClaimActionCollection collection)
        {
            collection.Clear();
            collection.Add(new MapAllClaimsAction());
        }

        /// <summary>
        /// Clears any current ClaimsActions and maps all values from the json user data as claims, excluding the specified types.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="exclusions"></param>
        public static void MapAllExcept(this ClaimActionCollection collection, params string[] exclusions)
        {
            collection.MapAll();
            collection.DeleteClaims(exclusions);
        }

        /// <summary>
        /// Delete all claims from the given ClaimsIdentity with the given ClaimType.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="claimType"></param>
        public static void DeleteClaim(this ClaimActionCollection collection, string claimType)
        {
            collection.Add(new DeleteClaimAction(claimType));
        }

        /// <summary>
        /// Delete all claims from the ClaimsIdentity with the given claimTypes.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="claimTypes"></param>
        public static void DeleteClaims(this ClaimActionCollection collection, params string[] claimTypes)
        {
            if (claimTypes == null)
            {
                throw new ArgumentNullException(nameof(claimTypes));
            }

            foreach (var claimType in claimTypes)
            {
                collection.Add(new DeleteClaimAction(claimType));
            }
        }
    }
}
