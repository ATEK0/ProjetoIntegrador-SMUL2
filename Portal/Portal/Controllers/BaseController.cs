using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Portal.Controllers
{
    /// <summary>
    /// Controller base com utilidades partilhadas por todos os controllers autenticados.
    /// </summary>
    public abstract class BaseController : Controller
    {
        protected int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
        }
    }
}
