namespace eGrants.Models
{

        /// <summary>
        ///     The docs unidentified.
        /// </summary>
        public class DocsUnidentified
        {
            /// <summary>
            ///     Gets or sets the document_id.
            /// </summary>
            public string document_id { get; set; }

            /// <summary>
            ///     Gets or sets the document_name.
            /// </summary>
            public string document_name { get; set; }

            /// <summary>
            ///     Gets or sets the document_date.
            /// </summary>
            public DateOnly? document_date { get; set; }

            /// <summary>
            ///     Gets or sets the created_by.
            /// </summary>
            public string created_by { get; set; }

            /// <summary>
            ///     Gets or sets the created_date.
            /// </summary>
            public DateOnly? created_date { get; set; }

            /// <summary>
            ///     Gets or sets the category_id.
            /// </summary>
            public string category_id { get; set; }

            /// <summary>
            ///     Gets or sets the qc_date.
            /// </summary>
            public DateOnly? qc_date { get; set; }

            /// <summary>
            ///     Gets or sets the url.
            /// </summary>
            public string url { get; set; }
        }
    }
