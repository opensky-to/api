// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AirlineMember.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.API.Model.Airline
{
    using System.ComponentModel.DataAnnotations;

    using OpenSky.API.DbModel;
    using OpenSky.API.DbModel.Enums;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Airline member model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 26/10/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class AirlineMember
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirlineMember"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/10/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        public AirlineMember()
        {
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="AirlineMember"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/10/2021.
        /// </remarks>
        /// <param name="user">
        /// The OpenSky user.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public AirlineMember(OpenSkyUser user)
        {
            this.MemberID = user.Id;
            this.Name = user.UserName;
            this.Rank = user.AirlineRank ?? PilotRank.Cadet;
            this.AirlineIncomeShare = user.AirlineIncomeShare ?? 0;
            this.AirlineSalary = user.AirlineSalary ?? 0;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline income share (in percent, 20=>20% of job income).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int AirlineIncomeShare { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the airline salary (SkyBucks per flight hour).
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public int AirlineSalary { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the identifier of the member.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        [StringLength(255)]
        [Required]
        public string MemberID { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the name of the member.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string Name { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the pilot rank.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PilotRank Rank { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/10/2021.
        /// </remarks>
        /// <param name="obj">
        /// The object to compare with the current object.
        /// </param>
        /// <returns>
        /// <see langword="true" /> if the specified object  is equal to the current object; otherwise,
        /// <see langword="false" />.
        /// </returns>
        /// <seealso cref="M:System.Object.Equals(object)"/>
        /// -------------------------------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((AirlineMember)obj);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/10/2021.
        /// </remarks>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        /// <seealso cref="M:System.Object.GetHashCode()"/>
        /// -------------------------------------------------------------------------------------------------
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyMemberInGetHashCode
            return (this.MemberID != null ? this.MemberID.GetHashCode() : 0);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Tests if this AirlineMember is considered equal to another.
        /// </summary>
        /// <remarks>
        /// sushi.at, 26/10/2021.
        /// </remarks>
        /// <param name="other">
        /// The airline member to compare to this object.
        /// </param>
        /// <returns>
        /// True if the objects are considered equal, false if they are not.
        /// </returns>
        /// -------------------------------------------------------------------------------------------------
        protected bool Equals(AirlineMember other)
        {
            return this.MemberID == other.MemberID;
        }
    }
}