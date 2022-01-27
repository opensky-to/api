// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumSchemaFilter.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API
{
    using System;
    using System.Linq;

    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Models;

    using Swashbuckle.AspNetCore.SwaggerGen;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Enum schema filter for swagger to use strings for enums.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/06/2021.
    /// </remarks>
    /// <seealso cref="T:Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter"/>
    /// -------------------------------------------------------------------------------------------------
    public class EnumSchemaFilter : ISchemaFilter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Applies this filter.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
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
            if (context.Type.IsEnum)
            {
                var array = new OpenApiArray();
                array.AddRange(Enum.GetNames(context.Type).Select(n => new OpenApiString(n)));
                
                // NSwag
                schema.Extensions.Add("x-enumNames", array);
                
                // OpenApi-generator
                schema.Extensions.Add("x-enum-varnames", array);
            }
        }
    }
}