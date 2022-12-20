// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using FranceConnectFacade.Identity.Helpers;
using FranceConnectFacade.Identity.Middleware;
using FranceConnectFacade.Identity.Services;
using Microsoft.AspNetCore.Mvc;

namespace FranceConnectFacade.Identity.Controllers
{
    /// <summary>
    /// Points d'entr� d'autentification et d'autorisation
    /// </summary>
    [ApiController]
    [Route("/api/beta")]
    public class FranceConnectFacadeOpenIdConnectController : ControllerBase
    {
        private readonly ILogger<FranceConnectFacadeOpenIdConnectController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpFranceConnectClient _httpFcClient;

        public FranceConnectFacadeOpenIdConnectController(ILogger<FranceConnectFacadeOpenIdConnectController> logger,
                                      IConfiguration configuration,
                                      IHttpFranceConnectClient httpFcClient)
        {
            _logger = logger;
            _configuration = configuration;
            _httpFcClient = httpFcClient;
            
        }

        //[HttpGet()]
        //[Route("userinfo")]        
        //public async Task<IActionResult> UserInfo()
        //{

        //    //Use the FC's AccessToken to get the user info
        //    var authorizationHeader = Request.Headers["Authorization"];   
            
        //    var result=await _httpFcClient.GetFranceConnectUserInfo(authorizationHeader);
        //    if (result != null)
        //    {
        //        _logger.LogInformation($"UserInfo");
        //        return Ok(result);
        //    }

        //    return new StatusCodeResult(StatusCodes.Status401Unauthorized);
        //}

        /// <summary>
        /// Pas encore impl�ment�e
        /// </summary>
        /// <returns></returns>
        [HttpGet()]
        [Route("logout")]
        public IActionResult Logout()
        {
            //TODO : Logout
            return Ok();
        }

        /// <summary>
        /// Point de terminaison afin d'obtenir un jeton compatible portal/page
        /// </summary>
        /// <remarks>
        /// Ce point d'entr� est appel� par l'application portal/page une fois 
        /// l'utilisateur autentifi�.
        ///  Le middleware aura pour charge d'obtenir le jeton FC et de le 
        ///  transformer afin qu'il soit compatible avec portal/page.
        /// </remarks>
        /// <returns></returns>
        [HttpPost]        
        [Route("token")]
        [Produces("application/json")]
#if TEST_FC_IN_PORTAL
        [FranceConnectFacadeEndPoint(EndPoint = "token:testinportal")]
#else
        [FranceConnectFacadeEndPoint(EndPoint = "token")]
#endif
        public IActionResult Token()
        {
            
            var fcfResult = HttpContext.Items["token"];
            
            if (fcfResult == null)
            {
                return new UnauthorizedResult();
            }
            _logger.LogInformation(fcfResult.ToString());
            _logger.LogInformation("Controller : token");
            // Retourne le nouveau jeton � l'application Portal/Page
            return new OkObjectResult(fcfResult);

                        
        }

        /// <summary>
        /// Point de terminaison pour l'autentification aupr�s 
        /// de FranceConnect.
        /// </summary>
        /// <remarks> 
        /// Lorque l'application portal/page invoque ce point d'entr�,             
        /// Le middleware construit une requ�te compatible avec 
        /// FranceConnect et redirige uniquement l'appel.
        /// A ce stade, l'authentification ce fait � l'aide des
        /// m�canismes FranceConnect.
        /// </remarks>
        /// <returns></returns>
        [HttpGet()]        
        [Route("authorize")]
#if TEST_FC_IN_PORTAL
        [FranceConnectFacadeEndPoint(EndPoint = "authorize:testinportal")]
#else
        [FranceConnectFacadeEndPoint(EndPoint = "authorize")]        
#endif
        public RedirectResult Authorize()
        {
            string baseAddress = _configuration["FranceConnect:AuthorizationEndpoint"];
            if (string.IsNullOrEmpty(baseAddress))
            {
                throw new ArgumentNullException("AuthorizationEndpoint", "Vous devez rajouter le point d'entr�e FranceConnect dans le fichier appsettings.json");
            }
            // Contient la requ�te compatible FranceConnect
            string? query = HttpContext.Items["query"] as string;
           
            string redirectUri = $"{baseAddress}/{query}";
            var redirectReponse = this.Redirect(redirectUri);

            _logger.LogInformation($"Controller : authorize");
            // Redirige l'appel vers FranceConnect
            return redirectReponse;
        }
    }
}