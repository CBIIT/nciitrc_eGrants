namespace egrants_new.Egrants.Models
{

        /// <summary>
        ///     The doc transaction history.
        /// </summary>
        public class DocTransactionHistory
        {
            /// <summary>
            ///     Gets or sets the transaction_type.
            /// </summary>
            public string transaction_type { get; set; }

            /// <summary>
            ///     Gets or sets the document_id.
            /// </summary>
            public string document_id { get; set; }

            /// <summary>
            ///     Gets or sets the full_grant_num.
            /// </summary>
            public string full_grant_num { get; set; }

            /// <summary>
            ///     Gets or sets the category_name.
            /// </summary>
            public string category_name { get; set; }

            /// <summary>
            ///     Gets or sets the person_name.
            /// </summary>
            public string person_name { get; set; }

            /// <summary>
            ///     Gets or sets the url.
            /// </summary>
            public string url { get; set; }

            /// <summary>
            ///     Gets or sets the transaction_date.
            /// </summary>
            public string transaction_date { get; set; }
        }
    }
