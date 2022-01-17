// --------------------------------------------------------------------------------------------------------------------
// <copyright file="S2CellIDSchemaFilter.cs" company="OpenSky">
// OpenSky project 2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API
{
    using Microsoft.OpenApi.Models;

    using Swashbuckle.AspNetCore.SwaggerGen;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Swagger schema filter for S2 Cell IDs (uint64->string)
    /// </summary>
    /// <remarks>
    /// sushi.at, 17/01/2022.
    /// </remarks>
    /// <seealso cref="T:Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter"/>
    /// -------------------------------------------------------------------------------------------------
    public class S2CellIDSchemaFilter : ISchemaFilter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Applies this filter.
        /// </summary>
        /// <remarks>
        /// sushi.at, 17/01/2022.
        /// </remarks>
        /// <param name="schema">
        /// The OpenAPI schema.
        /// </param>
        /// <param name="context">
        /// The filter context.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(ulong) && context.MemberInfo.Name.ToLowerInvariant().StartsWith("s2cell"))
            {
                schema.Type = "string";
            }
        }
    }
}