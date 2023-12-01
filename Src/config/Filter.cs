using Email_Handler;
using Email_Database;

namespace Email_Config
{
    class FilterEmail
    {
        public static void Filter(Email email, ConfigJson config, EmailDbContext dbContext)
        {
            bool ok = false;
            foreach (var filter in config.Filters!)
            {
                if (FilterMatches(email, filter))
                {
                    // Assign the email to the corresponding folder
                    dbContext.AddEmail(email, filter.Folder!);
                    ok = true;
                    // Move on to the next email if a match is found
                    break;
                }
            }
            if (!ok)
                dbContext.AddEmail(email, "Inbox");
        }
        private static bool FilterMatches(Email email, Filter filter)
        {
            switch (filter.Criteria)
            {
                case "From":
                    return filter.Values!.Any(value => email.From!.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);

                case "Subject":
                    return filter.Values!.Any(value => email.Subject!.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);

                case "Content":
                    return filter.Values!.Any(value => email.Body!.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);

                case "Spam":
                    // Example: Check if any of the values appear in the email body or subject
                    return filter.Values!.Any(value => email.Body!.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0) ||
                           filter.Values!.Any(value => email.Subject!.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0);

                default:
                    // Handle unknown criteria
                    return false;
            }
        }
    }
}