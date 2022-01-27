// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PocoLoadingExtensions.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Helpers
{
    using System;
    using System.Runtime.CompilerServices;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// POCO loading extension methods.
    /// </summary>
    /// <remarks>
    /// sushi.at, 11/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public static class PocoLoadingExtensions
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// An extension method that loads a lazy loading navigation property.
        /// </summary>
        /// <remarks>
        /// sushi.at, 11/05/2021.
        /// </remarks>
        /// <typeparam name="TRelated">
        /// Type of the related.
        /// </typeparam>
        /// <param name="loader">
        /// The loader to act on.
        /// </param>
        /// <param name="entity">
        /// The entity.
        /// </param>
        /// <param name="navigationField">
        /// [in,out] The navigation field.
        /// </param>
        /// <param name="navigationName">
        /// Name of the navigation property.
        /// </param>
        /// <returns>
        /// A TRelated.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        public static TRelated Load<TRelated>(this Action<object, string> loader, object entity, ref TRelated navigationField, [CallerMemberName] string navigationName = null) where TRelated : class
        {
            loader?.Invoke(entity, navigationName);
            return navigationField;
        }
    }
}