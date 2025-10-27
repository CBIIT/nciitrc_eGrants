using System.ComponentModel.DataAnnotations.Schema;

namespace eGrants.ViewModels
{
    public class OrgListViewModel
    {
        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// Gets or sets the org id.
        /// </summary>
        public int OrgId { get; set; }

        /// <summary>
        /// Gets or sets the org name.
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// Gets or sets the sv created by.
        /// </summary>
        public string SVCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the sv created date.
        /// </summary>
        public string SVCreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the sv end date.
        /// </summary>
        public string SVEndDate { get; set; }

        /// <summary>
        /// Gets or sets the sv url.
        /// </summary>
        public string SvUrl { get; set; }

        /// <summary>
        /// Gets or sets the fu created date.
        /// </summary>
        public string FUCreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the fu created by.
        /// </summary>
        public string FUCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the fu end date.
        /// </summary>
        public string FUEndDate { get; set; }

        /// <summary>
        /// Gets or sets the fu url.
        /// </summary>
        public string FUUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any org doc.
        /// </summary>
        public bool AnyOrgDoc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether active.
        /// </summary>
        public bool Active { get; set; }
    }
}
