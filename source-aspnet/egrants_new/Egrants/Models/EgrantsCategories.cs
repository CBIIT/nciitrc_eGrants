namespace egrants_new.Egrants.Models
{

        /// <summary>
        ///     The egrants categories.
        /// </summary>
        public class EgrantsCategories
        {
            /// <summary>
            ///     Gets or sets the category_id.
            /// </summary>
            public string category_id { get; set; }

            /// <summary>
            ///     Gets or sets the category_name.
            /// </summary>
            public string category_name { get; set; }

            /// <summary>
            ///     Gets or sets the package.
            /// </summary>
            public string package { get; set; }

            /// <summary>
            ///     Gets or sets the input_type.
            /// </summary>
            public string input_type { get; set; }

            /// <summary>
            ///     Gets or sets the input_constraint.
            /// </summary>
            public string input_constraint { get; set; }
        }
    }
