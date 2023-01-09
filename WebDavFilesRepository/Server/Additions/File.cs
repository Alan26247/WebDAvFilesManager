using System.Text.RegularExpressions;

namespace WebDavFilesRepository.Server.Additions
{
    /// <summary>
    /// файловый помощник
    /// </summary>
    public static class File
    {
        /// <summary>
        /// получить расширение файла из названия
        /// </summary>
        /// <param name="fileName">название файла</param>
        /// <returns>возвращает расширение файла</returns>
        public static string GetExtentionFromFileName(string fileName)
        {
            // для поиска раширений
            Regex regexExt = new Regex(@"[^\.][A-z]{1,}$");

            string ext = "";

            // ищем расширение файла
            MatchCollection matchCollection = regexExt.Matches(fileName);

            if (matchCollection.Count > 0)
                ext = regexExt.Matches(fileName).First().Value;
            return ext.ToLower();
        }

        /// <summary>
        /// вырезать из названия файла его расширение
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFileNameWithoutExtension(string fileName)
        {
            string ext = GetExtentionFromFileName(fileName);

            return fileName.Replace("." + ext, "");
        }
    }
}
