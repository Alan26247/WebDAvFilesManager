using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using WebDav;
using WebDavFilesRepository.Server.Additions;
using WebDavFilesRepository.Server.Services;
using WebDavFilesRepository.Shared.Forms;

namespace WebDavFilesRepository.Server.Controllers
{
    /// <summary>
    /// контроллер для работы с директориями
    /// все директории должны заканчиваться на слеш
    /// все файлы должны быть без слешей
    /// </summary>
    //[Authorize]       // тестовая версия авторизацию выключам
    [ApiController]
    public class FileController : ControllerBase
    {
        public FileController(IConfiguration configuration,
                                    IFoldersAccessesService foldersAccessesService,
                                    IWebDAVService webDAVService,
                                    ILogsService logsService)
        {
            this.configuration = configuration;
            this.foldersAccessesService = foldersAccessesService;
            this.webDAVService = webDAVService;
            this.logsService = logsService;
        }

        private readonly IConfiguration configuration;
        private readonly IFoldersAccessesService foldersAccessesService;
        private readonly IWebDAVService webDAVService;
        private readonly ILogsService logsService;



        /// <summary>
        /// загрузить файл или папку архивом
        /// 
        /// логика работы следующая
        /// если админ скачивание разрешено
        /// если пользователь то он должен иметь доступ 
        /// к директории
        /// </summary>
        /// <param name="form">форма загрузки</param>
        /// <returns>возвращает файл или архив с папкой</returns>
        [HttpPost]
        [Route("api/get-content/")]
        public async Task<ActionResult> GetContent(FormUrl form)
        {
            try
            {
                // проверим и преобразуем входной параметр
                if (!SqlInjectionProtect.Check(form.Uri))
                    return BadRequest("Введен некорректное значение");

                WebDavResource[] items = await webDAVService.List(form.Uri);
                if (items == null) return BadRequest("Нет ресурса с данным url");

                // в тестовой версии проверку не проводим
                //FolderAccessEntity[] accessesToView =
                //    foldersAccessesService.GetAccessesToViewInCurrentBranch(User.Identity.GetId(), form.Uri);

                //// если админ то не проверяем потомучто у админа есть доступ ко всему
                //if (User.Identity?.GetRole() != "admin")
                //{
                //    if (!foldersAccessesService.CheckFolderAccess(accessesToView, form.Uri))
                //        return BadRequest("Нет доступа к папке");
                //}

                // если контент является файлом то возвращаем файл
                if (!items[0].IsCollection)
                {
                    using Stream stream = await webDAVService.DownloadFileFromStream(form.Uri);

                    MemoryStream memoryStream = new();
                    await stream.CopyToAsync(memoryStream);

                    memoryStream.Position = 0;
                    byte[] buffer = memoryStream.GetBuffer();

                    return Ok("data:" + items[0].ContentType + ";base64," + Convert.ToBase64String(buffer));
                }

                // если контент является папкой то возвращаем архив
                string nameTempFolder = "Temp/" + GeneratorRandom.GenerateName(32);
                string nameTempArchive = $"Archive_" + DateTime.Now.Hour + DateTime.Now.Minute + "_" +
                                            DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + ".zip";

                try
                {
                    // скачиваем папку
                    await DownloadFolder(nameTempFolder + "/" + items[0].DisplayName, form.Uri);

                    // создаем архив
                    ZipFile.CreateFromDirectory(nameTempFolder + "/" + items[0].DisplayName,
                                                                        "Temp/" + nameTempArchive);

                    // возвращаем архив
                    List<byte> bytes = new();
                    using (FileStream stream = System.IO.File.OpenRead("Temp/" + nameTempArchive))
                    {
                        // считываем поток до конца
                        int b = 0;
                        // если не -1 то поток не окончен
                        while (b != -1)
                        {
                            b = stream.ReadByte();
                            if (b != -1) bytes.Add((byte)b);
                        }
                    }

                    // удаляем временную папку и архив 
                    Directory.Delete(nameTempFolder, true);
                    System.IO.File.Delete("Temp/" + nameTempArchive);

                    // возвращаем base64
                    return Ok("data:application/zip;base64," + Convert.ToBase64String(bytes.ToArray()));
                }
                catch
                {
                    // если временная папка создана то удаляем ее
                    if (Directory.Exists(nameTempFolder)) Directory.Delete(nameTempFolder, true);

                    // если архив создан то удаляем его
                    if (System.IO.File.Exists("Temp/" + nameTempArchive))
                        System.IO.File.Delete("Temp/" + nameTempArchive);

                    return BadRequest("Ошибка скачивания папки.");
                }
            }
            catch
            {
                return StatusCode(500, "WebDav сервис не отвечает");
            }
        }
        private async Task<bool> DownloadFolder(string url, string urlSource)
        {
            try
            {
                WebDavResource[] items = (await webDAVService.List(urlSource)).ToArray();

                // если временной папки не существует то создаем ее
                if (!Directory.Exists(url)) Directory.CreateDirectory(url);

                // загружаем контент
                for (int i = 1; i < items.Count(); i++)
                {
                    if (items[i].IsCollection)
                    {
                        await DownloadFolder(url + "/" + items[i].DisplayName, urlSource + "/" + items[i].DisplayName);
                    }
                    else
                    {
                        await DownloadFile(url, urlSource, items[i].DisplayName);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        private async Task<bool> DownloadFile(string urlTempFolder, string urlSource, string fileName)
        {
            try
            {
                // если папки не существует то создаем ее
                if (!Directory.Exists(urlTempFolder)) Directory.CreateDirectory(urlTempFolder);

                // создаем файл
                using (var tempFile = System.IO.File.OpenWrite(urlTempFolder + "/" + fileName))
                using (var stream = await webDAVService.DownloadFileFromStream(urlSource + "/" + fileName))
                    await stream.CopyToAsync(tempFile);

                return true;
            }
            catch
            {
                return false;
            }
        }



        /// <summary>
        /// загрузить файл в указанную директорию
        /// 
        /// логика работы следующая
        /// если админ то загрузка разрешена
        /// если пользователь то должен иметь доступ к директории
        /// и разрешение на редактирование директории
        /// </summary> 
        /// <param name="form">форма загрузки файла</param>
        /// <returns>возвращает результат выполнения</returns>
        [HttpPost]
        [Route("api/upload-content")]
        public async Task<ActionResult> UploadContent([FromForm] FormUploadFiles form)
        {
            try
            {
                // проверим и преобразуем входной параметр
                if (!SqlInjectionProtect.Check(form.Uri))
                    return BadRequest("Введен некорректный url");
                form.Uri = form.Uri.Replace("%2F", "/");

                // проверяем есть ли такая директория
                WebDavResource[] items = await webDAVService.List(form.Uri);
                if (items == null) return BadRequest("Нет родительской директории");
                if (!items[0].IsCollection) return BadRequest("Ссылка указывает на файл");

                // в тестовой версии проверку не проводим
                //FolderAccessEntity[] accessesToEdit =
                //    foldersAccessesService.GetAccessesToEditInCurrentBranch(User.Identity.GetId(), form.Uri);

                //// если админ то не проверяем потомучто у админа есть доступ ко всему
                //if (User.Identity?.GetRole() != "admin")
                //{
                //    if (!foldersAccessesService.CheckFolderAccess(accessesToEdit, form.Uri))
                //        return BadRequest("Нет доступа к редактированию контента директории");
                //}

                int maxSizeUploadFile = int.Parse(configuration["MaxSizeUploadFile"]);
                int maxSizeNameFile = int.Parse(configuration["MaxSizeNameFile"]);

                if (form.File != null)
                {
                    string result = string.Empty;

                    // проверяем размер файла
                    if (form.File.Length > maxSizeUploadFile * 1048576)
                        return BadRequest($" --- Файл ({form.File.Name}) не добавлен. Размер файла не должен превышать {maxSizeUploadFile} мб\n");

                    // обрезаем название если оно слишком длинное
                    if (form.File.FileName.Length > 64)
                        return BadRequest($" --- Файл ({form.File.Name}) не добавлен. Название файла не должено превышать {maxSizeNameFile} символа\n");

                    using var stream = new MemoryStream();
                    await form.File.CopyToAsync(stream);

                    WebDavResponse wdResponce = await webDAVService.Upload(form.Uri, form.File.FileName, stream);

                    if (wdResponce.StatusCode != 201)
                        result = $" --- Файл ({form.File.FileName}) не добавлен. Файл с таким именем уже имеется.\n";

                    // если все файлы добавлены
                    if (result == "") return Ok("Файл успешно добавлен");

                    return BadRequest(result);
                }

                return BadRequest("Нет файлов для добавления");
            }
            catch
            {
                return StatusCode(500, "WebDav сервис не отвечает");
            }
        }

        /// <summary>
        /// удаляет файл по указанному url
        /// 
        /// логика работы следующая
        /// если админ то удаление разрешено
        /// если пользователь то он должен иметь доступ к родительской директории
        /// и разрешение на редактирование директории
        /// </summary>
        /// <param name="form">форма удаления файла</param>
        /// <returns>возаращает результат выполнения</returns>
        [HttpPost]
        [Route("api/delete-file")]
        public async Task<ActionResult> Delete(FormUrl form)
        {
            try
            {
                // проверим и преобразуем входной параметр
                if (!SqlInjectionProtect.Check(form.Uri))
                    return BadRequest("Введен некорректный url");

                // проверяем есть ли такой ресурс
                WebDavResource[] items = await webDAVService.List(form.Uri);
                if (items == null) return BadRequest("Указанного ресурса не существует");

                // если директория то не удаляем
                if (items[0].IsCollection) return BadRequest("Отказано. Произведена попытка удаления директории.");

                // в тестовой версии проверку не проводим
                //FolderAccessEntity[] accessesToEdit =
                //    foldersAccessesService.GetAccessesToEditInCurrentBranch(User.Identity.GetId(), form.Uri);

                //// если админ то не проверяем потомучто у админа есть доступ ко всему
                //if (User.Identity?.GetRole() != "admin")
                //{
                //    string parentFolderUrl = Shared.Additions.Uri.GetParentUri(form.Uri);

                //    if (!foldersAccessesService.CheckFolderAccess(accessesToEdit, form.Uri))
                //        return BadRequest("Нет доступа на редактирование к родительской папке");
                //}

                WebDavResponse wdResponce = await webDAVService.Delete(form.Uri);

                if (wdResponce.StatusCode == 200) return Ok("Файл удален");
                else return StatusCode(wdResponce.StatusCode);
            }
            catch
            {
                return StatusCode(500, "WebDav сервис не отвечает");
            }
        }
    }
}
