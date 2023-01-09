using WebDavFilesRepository.Shared.Entitys;

namespace WebDavFilesRepository.Server.Services
{
    /// <summary>
    /// папка должна всегда заканчиваться слешем, 
    /// файл должен заканчиваться всегда без слеша
    /// </summary>
    public interface IFoldersAccessesService
    {
        /// <summary>
        /// добавить доступ к просмотру папки
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <param name="folderUri">uri папки</param>
        Task AddOrUpdateAccessView(int userId, string folderUri);

        /// <summary>
        /// удалить доступ к просмотру папки
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <param name="folderUri">uri папки</param>
        Task RemoveAccessView(int userId, string folderUri);

        /// <summary>
        /// получить все доступы на просмотр в текущей ветке
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <param name="folderUri">uri папки</param>
        /// <returns>возвращает все доступы к папкам на просмотр</returns>
        FolderAccessEntity[] GetAccessesToViewInCurrentBranch(int userId, string folderUri);

        /// <summary>
        ///  добавить доступ к редактированию содержимого папки
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <param name="folderUri">uri папки</param>
        Task AddOrUpdateAccessEditContent(int userId, string folderUri);

        /// <summary>
        /// удалить доступ к редактированию папки
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <param name="folderUri">uri папки</param>
        Task RemoveAccessEditContent(int userId, string folderUri);

        /// <summary>
        /// получить все доступы на редактирование в текущей ветке
        /// </summary>
        /// <param name="userId">id пользователя</param>
        /// <param name="folderUri">uri папки</param>
        /// <returns>возвращает все доступы к папкам на редактирование</returns>
        FolderAccessEntity[] GetAccessesToEditInCurrentBranch(int userId, string folderUri);

        /// <summary>
        /// проверить доступна ли папка для просмотра пользователю
        /// </summary>
        /// <param name="accesses">доступы в которых проверяем</param>
        /// <param name="folderUri">uri папки</param>
        /// <returns>возвращает true если доступ к просмотру есть иначе false</returns>
        bool CheckFolderAccess(FolderAccessEntity[] accesses, string folderUri);

        /// <summary>
        /// удалить все доступы к текущей папке
        /// </summary>
        /// <param name="folderUri">uri папки</param>
        /// <returns></returns>
        Task RemoveAllAccessesToFolder(string folderUri);
    }
}
