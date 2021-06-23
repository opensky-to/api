// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiResponse.cs" company="OpenSky">
// OpenSky project 2021
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model
{
    using System;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// API standard response model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 05/05/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class ApiResponse<T>
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse&lt;T&gt;"></see> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/05/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public ApiResponse()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse&lt;T&gt;"></see> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/05/2021.
        /// </remarks>
        /// <param name="ex">
        /// The exception to report.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public ApiResponse(Exception ex)
        {
            this.Status = "Error";
            this.IsError = true;
            this.Message = ex.Message;
            this.ErrorDetails = ex.ToString();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse&lt;T&gt;"></see> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/05/2021.
        /// </remarks>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="ex">
        /// The exception to report.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public ApiResponse(string message, Exception ex)
        {
            this.Status = "Error";
            this.IsError = true;
            this.Message = message;
            this.ErrorDetails = ex.ToString();
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse&lt;T&gt;"></see> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 05/05/2021.
        /// </remarks>
        /// <param name="message">
        /// The message (status will be "Success").
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public ApiResponse(string message)
        {
            this.Status = "Success";
            this.Message = message;
            this.IsError = false;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse&lt;T&gt;"></see> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/06/2021.
        /// </remarks>
        /// <param name="data">
        /// The data object to embed (status will be "Success").
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public ApiResponse(T data)
        {
            this.Status = "Success";
            this.IsError = false;
            this.Message = string.Empty;
            this.Data = data;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the embedded data of type T.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public T Data { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the error details (NULL if no error).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string ErrorDetails { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether this response is reporting an error.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public bool IsError { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Message { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Status { get; set; }
    }
}