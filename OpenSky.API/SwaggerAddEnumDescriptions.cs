// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwaggerAddEnumDescriptions.cs" company="OpenSky">
// sushi.at for OpenSky 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.OpenApi.Any;
    using Microsoft.OpenApi.Models;

    using Swashbuckle.AspNetCore.SwaggerGen;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Add enum value descriptions to swagger documentation.
    /// </summary>
    /// <remarks>
    /// sushi.at, 03/06/2021.
    /// </remarks>
    /// <seealso cref="T:Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter"/>
    /// -------------------------------------------------------------------------------------------------
    public class SwaggerAddEnumDescriptions : IDocumentFilter
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Applies the document filter.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
        /// </remarks>
        /// <param name="swaggerDoc">
        /// The swagger documentation.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // add enum descriptions to result models
            foreach (var (key, value) in swaggerDoc.Components.Schemas.Where(x => x.Value?.Enum?.Count > 0))
            {
                var propertyEnums = value.Enum;
                if (propertyEnums is { Count: > 0 })
                {
                    value.Description += DescribeEnum(propertyEnums, key);
                }
            }

            // add enum descriptions to input parameters
            foreach (var (key, value) in swaggerDoc.Paths)
            {
                this.DescribeEnumParameters(value.Operations, swaggerDoc, context.ApiDescriptions, key);
            }
        }

        private static string DescribeEnum(IEnumerable<IOpenApiAny> enums, string proprtyTypeName)
        {
            var enumType = GetEnumTypeByName(proprtyTypeName);
            if (enumType == null)
                return null;

            return " " + string.Join(", ", (from OpenApiInteger enumOption in enums select enumOption.Value into enumInt select $"{enumInt} = {Enum.GetName(enumType, enumInt)}").ToArray());
        }

        private static Type GetEnumTypeByName(string enumTypeName)
        {
            return AppDomain.CurrentDomain
                            .GetAssemblies()
                            .SelectMany(x => x.GetTypes())
                            .FirstOrDefault(x => x.Name == enumTypeName);
        }

        private static Type GetTypeIEnumerableType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var underlyingType = type.GetGenericArguments()[0];
                if (underlyingType.IsEnum)
                {
                    return underlyingType;
                }
            }

            return null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Describe enum parameters.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
        /// </remarks>
        /// <param name="operations">
        /// The operations.
        /// </param>
        /// <param name="swaggerDoc">
        /// The swagger documentation.
        /// </param>
        /// <param name="apiDescriptions">
        /// The API descriptions.
        /// </param>
        /// <param name="path">
        /// Full pathname of the file.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void DescribeEnumParameters(IDictionary<OperationType, OpenApiOperation> operations, OpenApiDocument swaggerDoc, IEnumerable<ApiDescription> apiDescriptions, string path)
        {
            path = path.Trim('/');
            if (operations != null)
            {
                var pathDescriptions = apiDescriptions.Where(a => a.RelativePath == path).ToList();
                foreach (var (key, value) in operations)
                {
                    var operationDescription = pathDescriptions.FirstOrDefault(a => a.HttpMethod != null && a.HttpMethod.Equals(key.ToString(), StringComparison.InvariantCultureIgnoreCase));
                    foreach (var param in value.Parameters)
                    {
                        var parameterDescription = operationDescription?.ParameterDescriptions.FirstOrDefault(a => a.Name == param.Name);
                        if (parameterDescription != null && this.TryGetEnumType(parameterDescription.Type, out var enumType))
                        {
                            var paramEnum = swaggerDoc.Components.Schemas.FirstOrDefault(x => x.Key == enumType.Name);
                            if (paramEnum.Value != null)
                            {
                                param.Description += DescribeEnum(paramEnum.Value.Enum, paramEnum.Key);
                            }
                        }
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Try to get enum value.
        /// </summary>
        /// <remarks>
        /// sushi.at, 03/06/2021.
        /// </remarks>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="enumType">
        /// [out] Type of the enum.
        /// </param>
        /// <returns>
        /// True if it succeeds, false if it fails.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        private bool TryGetEnumType(Type type, out Type enumType)
        {
            if (type.IsEnum)
            {
                enumType = type;
                return true;
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType is { IsEnum: true })
                {
                    enumType = underlyingType;
                    return true;
                }
            }
            else
            {
                var underlyingType = GetTypeIEnumerableType(type);
                if (underlyingType is { IsEnum: true })
                {
                    enumType = underlyingType;
                    return true;
                }
                else
                {
                    var interfaces = type.GetInterfaces();
                    foreach (var interfaceType in interfaces)
                    {
                        underlyingType = GetTypeIEnumerableType(interfaceType);
                        if (underlyingType is { IsEnum: true })
                        {
                            enumType = underlyingType;
                            return true;
                        }
                    }
                }
            }

            enumType = null;
            return false;
        }
    }
}