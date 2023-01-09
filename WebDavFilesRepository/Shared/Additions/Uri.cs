using System.Text;

namespace WebDavFilesRepository.Shared.Additions
{
    public static class Uri
    {
        /// <summary>
        /// возвращает url родительской директории
        /// </summary>
        /// <param name="urlSource">url ресурса</param>
        /// <returns>возвращает url родительской директории</returns>
        public static string GetParentUri(string uriSource)
        {
            string[] uris = uriSource.Split('/');

            StringBuilder response = new();

            for (int i = 0; i < uris.Length - 1; i++)
            {
                response.Append(uris[i] + '/');
            }

            return response.ToString();
        }

        /// <summary>
        /// получить все uri родительских директорий начиная от корневой
        /// </summary>
        /// <param name="uriSource">uri ресурса</param>
        /// <returns>возвращает все родительские uri ресурса начиная от корневой</returns>
        public static string[] GetParentsUris(string uriSource)
        {
            // если нет родительских директории возвращаем пустой массив
            if (uriSource == "/" || uriSource == "") return Array.Empty<string>();

            // защита
            if (uriSource[^1] != '/') uriSource += '/';

            string[] uris = uriSource.Split('/');

            List<string> result = new();
            StringBuilder tempUri = new();

            for (int i = 0; i < uris.Length - 1; i++)
            {
                tempUri.Append(uris[i] + '/');

                result.Add(tempUri.ToString());
            }

            return result.ToArray();
        }
    }
}
