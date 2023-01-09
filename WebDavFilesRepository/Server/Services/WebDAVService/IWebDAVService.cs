using WebDav;

namespace WebDavFilesRepository.Server.Services
{
    public interface IWebDAVService
    {
        /// <summary>
        /// получить информацию по ресурсу
        /// </summary>
        /// <param name="url">url ресурса</param>
        /// <returns>возвращает информацию по ресурсу</returns>
        Task<WebDavResource> GetInfo(string url);

        /// <summary>
        /// возвращает контент в папке
        /// </summary>
        /// <param name="urlFolder">url папки</param>
        /// <returns>возвращает список контента папки</returns>
        Task<WebDavResource[]> List(string urlFolder);

        /// <summary>
        /// создает директорию
        /// </summary>
        /// <param name="url">id папки</param>
        /// <param name="name">имя папки</param>
        /// <returns>возвращает результат выполнения</returns>
        Task<WebDavResponse> CreateDirectory(string url, string name);

        /// <summary>
        /// переместить ресурс
        /// </summary>
        /// <param name="urlFrom">url ресурса</param>
        /// <param name="urlTo">url назначения</param>
        Task<bool> Move(string urlFrom, string urlTo);

        /// <summary>
        /// загрузить файл
        /// </summary>
        /// <param name="path">путь</param>
        /// <param name="name">имя файла</param>
        /// <param name="stream">поток</param>
        /// <returns>возвращает результат выполнения</returns>
        Task<WebDavResponse> Upload(string path, string name, Stream stream);

        /// <summary>
        /// загрузка файла через поток
        /// </summary>
        /// <param name="url">url файла</param>
        /// <returns>возврвщает поток</returns>
        Task<Stream> DownloadFileFromStream(string url);

        /// <summary>
        /// загрузить файл в виде массива байт
        /// </summary>
        /// <param name="url">url файла</param>
        /// <returns>массив содержимого файла в байтах</returns>
        Task<byte[]> DownloadFile(string url);

        /// <summary>
        /// удалить ресурс
        /// </summary>
        /// <param name="url">url ресурса</param>
        /// <returns>возвращает результат</returns>
        Task<WebDavResponse> Delete(string url);
    }
}
