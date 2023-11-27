using Email_Handler;
using Email_Database;

namespace Email_Config
{
    class FilterEmail
    {
        public static void Filter(Email email, ConfigJson config, EmailDbContext dbContext)
        {
            bool ok = false;
            foreach (var filter in config.Filters)
            {
                if (FilterMatches(email, filter))
                {
                    // Assign the email to the corresponding folder
                    dbContext.AddEmail(email, filter.Folder);
                    ok = true;
                    // Move on to the next email if a match is found
                    break;
                }
            }
            if (!ok)
                dbContext.AddEmail(email, "MessageBox");
        }
        private static bool FilterMatches(Email email, Filter filter)
        {
            switch (filter.Criteria)
            {
                case "From":
                    return filter.Values.Contains(email.From);

                case "Subject":
                    return filter.Values.Contains(email.Subject, StringComparer.OrdinalIgnoreCase);

                case "Content":
                    return filter.Values.Any(value => email.Body.Contains(value, StringComparison.OrdinalIgnoreCase));

                case "Spam":
                    // Example: Check if any of the values appear in the email body
                    return filter.Values.Any(value => email.Body.Contains(value, StringComparison.OrdinalIgnoreCase));

                default:
                    // Handle unknown criteria
                    return false;
            }
        }
    }
}