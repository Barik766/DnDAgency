using Hangfire.Dashboard;

namespace DnDAgency.EmailWorker.Infrastructure;


public class AllowAllAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // in production, restrict access to admin users
        // return context.GetHttpContext().User.IsInRole("Admin");

        return true; // only for development
    }
}