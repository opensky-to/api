// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RolesAttribute.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Helpers
{
    using Microsoft.AspNetCore.Authorization;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Multi-roles authorization attribute.
    /// </summary>
    /// <remarks>
    /// sushi.at, 09/06/2021.
    /// </remarks>
    /// <seealso cref="T:Microsoft.AspNetCore.Authorization.AuthorizeAttribute"/>
    /// -------------------------------------------------------------------------------------------------
    public class RolesAttribute : AuthorizeAttribute
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="RolesAttribute"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 09/06/2021.
        /// </remarks>
        /// <param name="roles">
        /// A variable-length parameters list containing roles.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public RolesAttribute(params string[] roles)
        {
            this.Roles = string.Join(",", roles);
        }
    }
}