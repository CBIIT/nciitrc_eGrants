namespace egrants_new.Egrants.Models
{
        /// <summary>
        ///     The notification.
        /// </summary>
        public class Notification
        {
            /// <summary>
            ///     Gets or sets the notification name.
            /// </summary>
            public string notificationName { get; set; }

            /// <summary>
            ///     Gets or sets the id.
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            ///     Gets or sets the description.
            /// </summary>
            public string description { get; set; }

            /// <summary>
            ///     Gets or sets the sent date.
            /// </summary>
            public string sentDate { get; set; }

            /// <summary>
            ///     Gets or sets the from address.
            /// </summary>
            public string fromAddress { get; set; }

            /// <summary>
            ///     Gets or sets the to address.
            /// </summary>
            public string toAddress { get; set; }

            /// <summary>
            ///     Gets or sets the cc address.
            /// </summary>
            public string ccAddress { get; set; }

            /// <summary>
            ///     Gets or sets the subject.
            /// </summary>
            public string subject { get; set; }

            /// <summary>
            ///     Gets or sets the email content.
            /// </summary>
            public string emailContent { get; set; }
        }
    }
