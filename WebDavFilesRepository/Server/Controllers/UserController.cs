using Microsoft.AspNetCore.Mvc;

namespace WebDavFilesRepository.Server.Controllers
{
    /// <summary>
    /// контроллер для получения иконки пользователя
    /// </summary>
    [ApiController]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// получить пользователя
        /// </summary>
        /// <param name="id">id пользователя</param>
        /// <returns>возвращает иконку пользователя</returns>
        [HttpGet]
        [Route("api/get-user-logo/{id}")]
        public async Task<ActionResult> GetUserLogo(int id)
        {
            try
            {
                // возвращаем файл с лого
                FileStream stream = System.IO.File.OpenRead($"Users/{id}/Images/logo.jpg");

                return File(stream, "image/png");
            }
            catch
            {
                throw;
            }
        }
    }
}
