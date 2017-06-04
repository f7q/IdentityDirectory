namespace IdentityDirectory.Scim.Endpoints
{
    using Filters;
    using Microsoft.AspNetCore.Mvc;

    [Produces(ScimConstants.ScimMediaType)]
	[ScimExceptionFilter]
	public class ScimEndpoint : Controller
	{
	}
}