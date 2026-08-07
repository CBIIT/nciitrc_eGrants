#region FileHeader

// /****************************** Module Header ******************************\
// Module Name:  EntraIdUserResolver.cs
// Solution: eGrants
// Project:  eGrants
// Created: 2025-08-01
// Contributors:
//      - Dehuff, Daryl (NIH/NCI) [C] - dehuffdc
// Copyright (c) National Institute of Health
// 
// <Description of the file>
// Encapsulates resolution of the eGrants user identity (and IC/Institute code)
// from a Microsoft Entra ID (Azure AD) ClaimsPrincipal. This logic is extracted
// from the request pipeline so it can be unit tested in isolation.
// 
// This source is subject to the NIH Softwre License.
// See https://ncihub.org/resources/899/download/Guidelines_for_Releasing_Research_Software_04062015.pdf
// All other rights reserved.
// \***************************************************************************/

#endregion

#region

using System.Security.Claims;

#endregion

namespace eGrants.Common
{
    /// <summary>
    /// Resolves the eGrants user identity from Microsoft Entra ID (Azure AD) claims.
    /// </summary>
    public static class EntraIdUserResolver
    {
        /// <summary>
        /// The default Institute/Center (IC) code used when no <c>department</c> claim is present.
        /// </summary>
        public const string DefaultIc = "NCI";

        /// <summary>
        /// Resolves the eGrants user id (network id) from an Entra ID <see cref="ClaimsPrincipal"/>.
        /// <para>
        /// Preference order matches the request pipeline:
        /// <c>preferred_username</c> ? <see cref="ClaimTypes.Upn"/> ? <see cref="ClaimTypes.Email"/>.
        /// </para>
        /// The username portion is extracted from the UPN (e.g. <c>"dehuffdc@nih.gov"</c> ? <c>"dehuffdc"</c>).
        /// </summary>
        /// <param name="user">The authenticated principal. May be <see langword="null"/>.</param>
        /// <returns>
        /// The resolved user id, or <see langword="null"/> if no usable identity claim is present.
        /// </returns>
        public static string? ResolveUserId(ClaimsPrincipal? user)
        {
            if (user is null)
            {
                return null;
            }

            // Use the first claim in preference order that has a non-empty value.
            // A raw "?? " chain would let an empty (but non-null) claim value shadow a
            // valid later claim, so each candidate is filtered through FirstNonEmpty.
            var upn = FirstNonEmpty(
                user.FindFirst("preferred_username")?.Value,
                user.FindFirst(ClaimTypes.Upn)?.Value,
                user.FindFirst(ClaimTypes.Email)?.Value);

            if (string.IsNullOrEmpty(upn))
            {
                return null;
            }

            // Extract username from UPN (e.g., "dehuffdc@nih.gov" -> "dehuffdc")
            return upn.Contains('@') ? upn.Split('@')[0] : upn;
        }

        /// <summary>
        /// Returns the first supplied value that is neither <see langword="null"/> nor empty,
        /// or <see langword="null"/> when every value is null or empty.
        /// </summary>
        private static string? FirstNonEmpty(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves the Institute/Center (IC) code from the Entra ID <c>department</c> claim,
        /// falling back to <see cref="DefaultIc"/> when the claim is absent or empty.
        /// </summary>
        /// <param name="user">The authenticated principal. May be <see langword="null"/>.</param>
        /// <returns>The resolved IC code; never <see langword="null"/> or empty.</returns>
        public static string ResolveIc(ClaimsPrincipal? user)
        {
            var department = user?.FindFirst("department")?.Value;
            return string.IsNullOrEmpty(department) ? DefaultIc : department;
        }

        /// <summary>
        /// Indicates whether the supplied principal represents an authenticated Entra ID user
        /// from which a usable user id can be resolved.
        /// </summary>
        /// <param name="user">The principal to evaluate. May be <see langword="null"/>.</param>
        /// <returns>
        /// <see langword="true"/> when the principal is authenticated and a non-empty user id can be resolved;
        /// otherwise <see langword="false"/>.
        /// </returns>
        public static bool HasResolvableIdentity(ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            return !string.IsNullOrEmpty(ResolveUserId(user));
        }
    }
}
