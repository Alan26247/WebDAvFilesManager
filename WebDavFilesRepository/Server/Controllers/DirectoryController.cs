using Microsoft.AspNetCore.Mvc;
using WebDav;
using WebDavFilesRepository.Server.Additions;
using WebDavFilesRepository.Server.Services;
using WebDavFilesRepository.Shared.Forms;
using WebDavFilesRepository.Shared.Models;

namespace WebDavFilesRepository.Server.Controllers
{
    /// <summary>
    /// контроллер для работы с директориями
    /// все директории должны заканчиваться на слеш
    /// все файлы должны быть без слешей
    /// </summary>
    //[Authorize]    // тестовая версия авторизацию выключаем
    [ApiController]
    public class DirectoryController : ControllerBase
    {
        public DirectoryController(IFoldersAccessesService foldersAccessesService,
                                    IWebDAVService webDAVService,
                                    ILogsService logsService)
        {
            this.foldersAccessesService = foldersAccessesService;
            this.webDAVService = webDAVService;
            this.logsService = logsService;
        }

        private readonly IFoldersAccessesService foldersAccessesService;
        private readonly IWebDAVService webDAVService;
        private readonly ILogsService logsService;



        /// <summary>
        /// получить данные директории по url
        /// 
        /// логика работы следующая
        /// если админ то отображаются все директории
        /// если пользователь то он должен иметь доступ к директории
        /// а также выводятся только те папки к которым пользователь
        /// имеет доступ
        /// </summary>
        /// <param name="form">форма запроса директории</param>
        /// <returns>возвращает содержимое директории</returns>
        [HttpPost]
        [Route("api/get-directory")]
        public async Task<ActionResult> GetDirectory(FormUrl form)
        {
            try
            {
                // проверим и преобразуем входной параметр
                if (!SqlInjectionProtect.Check(form.Uri))
                    return BadRequest("Введенно некорректное значение");
                if (form.Uri == "") form.Uri = "/";
                if (form.Uri[^1] != '/') form.Uri += "/";

                // в тестовой версии проверку не проводим
                //string role = User.Identity?.GetRole();
                //FolderAccessEntity[] accessesToView = 
                //    foldersAccessesService.GetAccessesToViewInCurrentBranch(User.Identity.GetId(), form.Uri);
                //FolderAccessEntity[] accessesToEdit =
                //    foldersAccessesService.GetAccessesToEditInCurrentBranch(User.Identity.GetId(), form.Uri);

                //if (role != "admin")
                //{
                //    if (!foldersAccessesService.CheckFolderAccess(accessesToView, form.Uri)) 
                //                                                return BadRequest("Нет доступа к папке");
                //}

                // получаем все папки с данной директории
                WebDavResource[] items = await webDAVService.List(form.Uri);
                if (items == null) return BadRequest("Нет директории с данной Url");
                if (!items[0].IsCollection) return BadRequest("Url указывает на файл");

                FolderDataModel response = new();
                response.Url = form.Uri;
                List<FolderModel> foldersList = new();
                List<Shared.Models.FileModel> filesList = new();

                for (int i = 0; i < items.Length; i++)
                {
                    if (i == 0) // данные о самой папке
                    {
                        response.Name = items[i].DisplayName;

                        // в тестовой версии проверку не проводим
                        //if (role == "admin") response.AbilityToEditContent = true;
                        //else
                        //{
                        //    response.AbilityToEditContent =
                        //        foldersAccessesService.CheckFolderAccess(accessesToEdit, form.Uri);
                        //}

                        response.AbilityToEditContent = true;

                        continue;
                    }

                    if (items[i].IsCollection)
                    {
                        // в тестовой версии проверку не проводим
                        //if (role != "admin")
                        //{
                        //    if (!foldersAccessesService.CheckFolderAccess(accessesToView, 
                        //                            form.Uri + items[i].DisplayName + "/"))
                        //                                    continue;
                        //}

                        FolderModel folder = new()
                        {
                            Url = form.Uri + items[i].DisplayName + "/",
                            Name = items[i].DisplayName
                        };

                        foldersList.Add(folder);
                    }
                    else
                    {
                        Shared.Models.FileModel file = new()
                        {
                            Url = form.Uri + items[i].DisplayName,
                            Name = items[i].DisplayName,
                            Size = items[i].ContentLength
                        };
                        file.Ext = Additions.File.GetExtentionFromFileName(file.Name);

                        filesList.Add(file);
                    }
                }
                response.Folders = foldersList.ToArray();
                response.Files = filesList.ToArray();

                return Ok(response);
            }
            catch
            {
                return StatusCode(500, "WebDav сервис не отвечает");
            }
        }

        /// <summary>
        /// создает указанную директории
        /// 
        /// логика работы следующая
        /// если админ то создание директории однозначно разрешено
        /// если пользователь то должен иметь доступ к родительской
        /// директории и иметь разрешение на редактирование 
        /// родительской директории
        /// </summary>
        /// <param name="form">форма создания директории</param>
        /// <returns>возвращает результат выполнения</returns>
        [HttpPost]
        [Route("api/create-directory")]
        public async Task<ActionResult> CreateDirectory(FormCreateDirectory form)
        {
            try
            {
                // проверим и преобразуем входной параметр
                if (!SqlInjectionProtect.Check(form.Uri))
                    return BadRequest("Введен некорректный url");
                if (form.Uri == "") form.Uri = "/";
                if (form.Uri[^1] != '/') form.Uri += "/";

                if (!SqlInjectionProtect.Check(form.Name))
                    return BadRequest("Введено некорректное имя");

                // проверяем есть ли такая директория
                WebDavResource[] items = await webDAVService.List(form.Uri);
                if (items == null) return BadRequest("Нет родительской директории");

                // в тестовой версии проверку не проводим
                //FolderAccessEntity[] accessesToEdit =
                //    foldersAccessesService.GetAccessesToEditInCurrentBranch(User.Identity.GetId(), form.Uri);

                //// если админ то не проверяем потомучто у админа есть доступ ко всему
                //if (User.Identity?.GetRole() != "admin")
                //{
                //    if (!foldersAccessesService.CheckFolderAccess(accessesToEdit, form.Uri))
                //        return BadRequest("Нет доступа к редактированию контента директории");
                //}

                WebDavResponse wdResponce = await webDAVService.CreateDirectory(form.Uri, form.Name);

                if (wdResponce.StatusCode == 201)
                {
                    return Ok($"Папка ({form.Name}) успешно создана");
                }
                else if (wdResponce.StatusCode == 405) return StatusCode(409, "Директория с таким именем уже существует");
                else return StatusCode(wdResponce.StatusCode);
            }
            catch
            {
                return StatusCode(500, "WebDav сервис не отвечает");
            }
        }

        /// <summary>
        /// удаляет указанную директорию
        /// 
        /// логика работы следующая
        /// если админ то удаление директории однозначно разрешено
        /// если пользователь то должен иметься доступ к родительской
        /// директории, к текущей директории, а также быть доступ к
        /// редактированию в этих директориях
        /// </summary>
        /// <param name="url">url директории</param>
        /// <returns>возаращает результат выполнения</returns>
        [HttpPost]
        [Route("api/delete-directory")]
        public async Task<ActionResult> DeleteDirectory(FormUrl form)
        {
            try
            {
                // проверим и преобразуем входной параметр
                if (!SqlInjectionProtect.Check(form.Uri))
                    return BadRequest("Введен некорректный url");
                if (form.Uri == "") form.Uri = "/";
                if (form.Uri[^1] != '/') form.Uri += "/";

                // проверяем есть ли такая директория
                WebDavResource[] items = await webDAVService.List(form.Uri);
                if (items == null) return BadRequest("Указанной директории не существует");

                // если не директория то не удаляем
                if (!items[0].IsCollection) return BadRequest("Отказано. Произведена попытка удаления файла.");

                // в тестовой версии проверку не проводим
                //FolderAccessEntity[] accessesToEdit =
                //    foldersAccessesService.GetAccessesToEditInCurrentBranch(User.Identity.GetId(), form.Uri);

                //// если админ то не проверяем потомучто у админа есть доступ ко всему
                //if (User.Identity?.GetRole() != "admin")
                //{
                //    string parentFolderUrl = Shared.Additions.Uri.GetParentUri(form.Uri);

                //    if (!foldersAccessesService.CheckFolderAccess(accessesToEdit, parentFolderUrl))
                //        return BadRequest("Нет доступа на редактирование к родительской папке");
                //}

                WebDavResponse wdResponce = await webDAVService.Delete(form.Uri);

                if (wdResponce.StatusCode == 200)
                {
                    // даем все права к удаленной директории
                    await foldersAccessesService.RemoveAllAccessesToFolder(form.Uri);

                    return Ok("Папка удалена");
                }
                else return StatusCode(wdResponce.StatusCode);
            }
            catch
            {
                return StatusCode(500, "WebDav сервис не отвечает");
            }
        }
    }
}
