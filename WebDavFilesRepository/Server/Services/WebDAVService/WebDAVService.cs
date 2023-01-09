using System.Net;
using WebDav;

namespace WebDavFilesRepository.Server.Services
{
    public class WebDavService : IWebDAVService
    {
        public WebDavService(IConfiguration configuration,
                                IWebDAVConnectionStringService webDAVConnectionStringService)
        {
            // начальная инициализация
            string connectioinString = webDAVConnectionStringService.ConnectionString;

            clientParams = new WebDavClientParams
            {
                BaseAddress = new Uri(configuration[connectioinString + "URL"]),
                Credentials = new NetworkCredential(configuration[connectioinString + "Login"],
                                                        configuration[connectioinString + "Password"])
            };

            rootFolder = configuration[connectioinString + "RootFolder"];
        }

        private static WebDavClientParams clientParams;
        private readonly string rootFolder;

        public async Task<WebDavResource> GetInfo(string url)
        {
            using HttpClientHandler httpHandler = CreateHttpHandler();
            using HttpClient httpClient = new HttpClient(httpHandler, true)
            { BaseAddress = clientParams.BaseAddress };
            using var client = new WebDav.WebDav(httpClient);
            if (url == "") url = "/";
            PropfindResponse responce = await client.Propfind(rootFolder + url);

            if (!responce.IsSuccessful) return null;

            IEnumerator<WebDavResource> resources = responce.Resources.GetEnumerator();

            resources.MoveNext();

            WebDavResource webDavResource = resources.Current;

            // если нет имени то получаем имя из url
            if (webDavResource.DisplayName == null)
                webDavResource.DisplayName = GetNameFromUri(webDavResource.Uri);

            return webDavResource;
        }
        public async Task<WebDavResource[]> List(string urlFolder)
        {
            using HttpClientHandler httpHandler = CreateHttpHandler();
            using HttpClient httpClient = new HttpClient(httpHandler, true)
            { BaseAddress = clientParams.BaseAddress };
            using var client = new WebDav.WebDav(httpClient);

            PropfindResponse responce = await client.Propfind(rootFolder + urlFolder);

            if (!responce.IsSuccessful) return null;

            IEnumerator<WebDavResource> resources = responce.Resources.GetEnumerator();

            List<WebDavResource> items = new();
            //resources.MoveNext();           // пропускаем первый элемент так как это описание самой папки
            while (resources.MoveNext())
            {
                WebDavResource webDavResource = resources.Current;

                // если нет имени то получаем имя из url
                if (webDavResource.DisplayName == null)
                    webDavResource.DisplayName = GetNameFromUri(webDavResource.Uri);

                items.Add(webDavResource);
            }

            return items.ToArray();
        }
        public async Task<WebDavResponse> CreateDirectory(string url, string name)
        {
            using HttpClientHandler httpHandler = CreateHttpHandler();
            using HttpClient httpClient = new HttpClient(httpHandler, true)
            { BaseAddress = clientParams.BaseAddress };
            using var client = new WebDav.WebDav(httpClient);
            return await client.Mkcol(rootFolder + url + "/" + name);
        }
        public async Task<bool> Move(string urlFrom, string urlTo)
        {
            using HttpClientHandler httpHandler = CreateHttpHandler();
            using HttpClient httpClient = new HttpClient(httpHandler, true)
            { BaseAddress = clientParams.BaseAddress };
            using var client = new WebDav.WebDav(httpClient);
            if (urlFrom == "") urlFrom = "/";
            if (urlTo == "") urlTo = "/";
            await client.Move(rootFolder + urlFrom, rootFolder + urlTo);

            return true;
        }
        public async Task<WebDavResponse> Upload(string path, string name, Stream stream)
        {
            using HttpClientHandler httpHandler = CreateHttpHandler();
            using HttpClient httpClient = new HttpClient(httpHandler, true)
            { BaseAddress = clientParams.BaseAddress };

            using var client = new WebDav.WebDav(httpClient);
            stream.Position = 0;
            return await client.PutFile(rootFolder + path + "/" + name, stream);
        }
        public async Task<Stream> DownloadFileFromStream(string url)
        {
            using HttpClientHandler httpHandler = CreateHttpHandler();
            using HttpClient httpClient = new HttpClient(httpHandler, true)
            { BaseAddress = clientParams.BaseAddress };
            using var client = new WebDav.WebDav(httpClient);
            return (await client.GetRawFile(rootFolder + url)).Stream;
        }
        public async Task<byte[]> DownloadFile(string url)
        {
            using HttpClientHandler httpHandler = CreateHttpHandler();
            using HttpClient httpClient = new HttpClient(httpHandler, true)
            { BaseAddress = clientParams.BaseAddress };
            using var client = new WebDav.WebDav(httpClient);
            using var response = await client.GetRawFile(rootFolder + url);
            // считываем поток до конца
            List<byte> bytes = new List<byte>();
            int b = 0;
            // если не -1 то поток не окончен
            while (b != -1)
            {
                b = response.Stream.ReadByte();
                if (b != -1) bytes.Add((byte)b);
            }

            return bytes.ToArray();
        }
        public async Task<WebDavResponse> Delete(string url)
        {
            using HttpClientHandler httpHandler = CreateHttpHandler();
            using HttpClient httpClient = new HttpClient(httpHandler, true)
            { BaseAddress = clientParams.BaseAddress };
            using var client = new WebDav.WebDav(httpClient);
            return await client.Delete(rootFolder + url);
        }


        // получает название файла из uri
        private string GetNameFromUri(string uri)
        {
            if (uri.Length > 0)
            {
                string[] words = uri.Split('/');

                if (uri[uri.Length - 1] == '/' && uri.Length > 1) return words[words.Length - 2];
                else if (uri[uri.Length - 1] == '/' && uri.Length == 1) return String.Empty;
                return words[words.Length - 1];
            }
            else return String.Empty;
        }
        private HttpClientHandler CreateHttpHandler()
        {
            HttpClientHandler httpHandler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                PreAuthenticate = clientParams.PreAuthenticate,
                UseDefaultCredentials = clientParams.UseDefaultCredentials,
                UseProxy = clientParams.UseProxy
            };

            if (clientParams.Credentials != null)
            {
                httpHandler.Credentials = clientParams.Credentials;
                httpHandler.UseDefaultCredentials = false;
            }
            if (clientParams.Proxy != null)
            {
                httpHandler.Proxy = clientParams.Proxy;
            }

            // отключение проверки ssl сертификата чтоб не вы....
            httpHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };

            return httpHandler;
        }
    }
}
