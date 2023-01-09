using WebDavFilesRepository.Server.Database;
using WebDavFilesRepository.Shared.Entitys;
using WebDavFilesRepository.Shared.Enums;

namespace WebDavFilesRepository.Server.Services
{
    public class FoldersAccessesService : IFoldersAccessesService
    {
        public FoldersAccessesService(AppDbContext dbContext)
        {
            db = dbContext;
        }

        private readonly AppDbContext db;


        // доступы на просмотр
        public async Task AddOrUpdateAccessView(int userId, string folderUri)
        {
            // получаем все доступы к папкам пользователя
            IQueryable<FolderAccessEntity> foldersAccesses =
                db.FoldersAccesses.Where(fa => fa.UserId == userId &&
                                    fa.PermissionId == (int)Permissions.AllowedToViewFolder);

            // удаляем все вложенные доступы
            IQueryable<FolderAccessEntity> nestedAccessesWithCurrentAccess =
                foldersAccesses.Where(fa => fa.FolderUri.IndexOf(folderUri) == 0);

            foreach (FolderAccessEntity access in nestedAccessesWithCurrentAccess)
            {
                db.FoldersAccesses.Remove(access);
            }

            await db.SaveChangesAsync();

            // если есть доступы у родительских папок то доступ к этой не даем
            if (CheckAccessFromParentFolders(foldersAccesses.ToArray(), folderUri)) return;

            // добавляем доступ к текущей папке
            FolderAccessEntity newAccess = new()
            {
                UserId = userId,
                FolderUri = folderUri,
                PermissionId = (int)Permissions.AllowedToViewFolder
            };

            db.FoldersAccesses.Add(newAccess);

            await db.SaveChangesAsync();
        }
        public async Task RemoveAccessView(int userId, string folderUri)
        {
            // получаем все доступы к папкам пользователя
            IQueryable<FolderAccessEntity> foldersAccesses =
                db.FoldersAccesses.Where(fa => fa.UserId == userId &&
                                    fa.PermissionId == (int)Permissions.AllowedToViewFolder);

            // удаляем все вложенные доступы
            IQueryable<FolderAccessEntity> nestedAccessesWithCurrentAccess =
                foldersAccesses.Where(fa => fa.FolderUri.IndexOf(folderUri) == 0);

            foreach (FolderAccessEntity access in nestedAccessesWithCurrentAccess)
            {
                db.FoldersAccesses.Remove(access);
            }

            await db.SaveChangesAsync();

            // получаем все доступы для редактирования ко вложенным папкам включая текущую
            IQueryable<FolderAccessEntity> foldersAccessesToEdit =
                db.FoldersAccesses.Where(fa => fa.UserId == userId &&
                                    fa.PermissionId == (int)Permissions.AllowedToEditContentFolder);

            // даем доступ ко вложенным папкам у которых разрешено редактирование
            // так как редактирование имеет выше приоритет (если разрешено редактирование то должен быть доступ)
            foreach (FolderAccessEntity access in foldersAccessesToEdit)
            {
                access.PermissionId = (int)Permissions.AllowedToViewFolder;

                db.FoldersAccesses.Add(access);
            }

            await db.SaveChangesAsync();
        }
        public FolderAccessEntity[] GetAccessesToViewInCurrentBranch(int userId, string folderUri)
        {
            // получаем первую родительскую папку в корневой директории
            string firstParentFolderInRootFolder = folderUri;

            string[] parentsUri = Shared.Additions.Uri.GetParentsUris(folderUri);

            // если директория не корневая то берем ее
            if (parentsUri.Length > 1) firstParentFolderInRootFolder = parentsUri[1];

            // получаем все доступы на просмотр к папкам пользователя в текущей ветке
            return db.FoldersAccesses.Where(fa => fa.UserId == userId &&
                                    fa.FolderUri.IndexOf(firstParentFolderInRootFolder) == 0 &&
                                    fa.PermissionId == (int)Permissions.AllowedToViewFolder).ToArray();
        }

        // доступы на редактирование
        public async Task AddOrUpdateAccessEditContent(int userId, string folderUri)
        {
            // получаем все доступы на редактирование к папкам пользователя
            IQueryable<FolderAccessEntity> foldersAccesses =
                db.FoldersAccesses.Where(fa => fa.UserId == userId &&
                                    fa.PermissionId == (int)Permissions.AllowedToEditContentFolder);

            // удаляем все вложенные доступы
            IQueryable<FolderAccessEntity> nestedAccessesWithCurrentAccess =
                foldersAccesses.Where(fa => fa.FolderUri.IndexOf(folderUri) == 0);

            foreach (FolderAccessEntity access in nestedAccessesWithCurrentAccess)
            {
                db.FoldersAccesses.Remove(access);
            }

            await db.SaveChangesAsync();

            // если есть доступ к папке то доступ не даем
            FolderAccessEntity[] accessesToView = GetAccessesToViewInCurrentBranch(userId, folderUri);
            if (CheckFolderAccess(accessesToView, folderUri)) return;

            // добавляем доступ к текущей папке
            FolderAccessEntity newAccess = new()
            {
                UserId = userId,
                FolderUri = folderUri,
                PermissionId = (int)Permissions.AllowedToViewFolder
            };

            db.FoldersAccesses.Add(newAccess);

            await db.SaveChangesAsync();
        }
        public async Task RemoveAccessEditContent(int userId, string folderUri)
        {
            // получаем все доступы к папкам пользователя
            IQueryable<FolderAccessEntity> foldersAccesses =
                db.FoldersAccesses.Where(fa => fa.UserId == userId &&
                                    fa.PermissionId == (int)Permissions.AllowedToEditContentFolder);

            // удаляем все вложенные доступы
            IQueryable<FolderAccessEntity> nestedAccessesWithCurrentAccess =
                foldersAccesses.Where(fa => fa.FolderUri.IndexOf(folderUri) == 0);

            foreach (FolderAccessEntity access in nestedAccessesWithCurrentAccess)
            {
                db.FoldersAccesses.Remove(access);
            }

            await db.SaveChangesAsync();
        }
        public FolderAccessEntity[] GetAccessesToEditInCurrentBranch(int userId, string folderUri)
        {
            // получаем первую родительскую папку в корневой директории
            string firstParentFolderInRootFolder = folderUri;

            string[] parentsUri = Shared.Additions.Uri.GetParentsUris(folderUri);

            // если директория не корневая то берем ее
            if (parentsUri.Length > 1) firstParentFolderInRootFolder = parentsUri[1];

            // получаем все доступы на просмотр к папкам пользователя в текущей ветке
            return db.FoldersAccesses.Where(fa => fa.UserId == userId &&
                                    fa.FolderUri.IndexOf(firstParentFolderInRootFolder) == 0 &&
                                    fa.PermissionId == (int)Permissions.AllowedToEditContentFolder).ToArray();
        }

        // удаление всех доступов папки (нужно для удаления папки)
        public async Task RemoveAllAccessesToFolder(string folderUri)
        {
            IQueryable<FolderAccessEntity> foldersAccesses =
                db.FoldersAccesses.Where(fa => fa.FolderUri.IndexOf(folderUri) == 0);

            foreach (FolderAccessEntity folderAccess in foldersAccesses)
            {
                db.FoldersAccesses.Remove(folderAccess);
            }

            await db.SaveChangesAsync();
        }

        // есть ли доступ к папке
        public bool CheckFolderAccess(FolderAccessEntity[] accesses, string folderUri)
        {
            // если у родительских директорий есть доступ то доступ к папке разрешен
            if (CheckAccessFromParentFolders(accesses, folderUri)) return true;

            // если есть доступ к самой папке то возвращаем true
            if (accesses.Where(fa => fa.FolderUri == folderUri).FirstOrDefault() != null) return true;

            return false;
        }



        private bool CheckAccessFromParentFolders(FolderAccessEntity[] accesses, string folderUri)
        {
            string[] parentsUri = Shared.Additions.Uri.GetParentsUris(folderUri);

            foreach (string uri in parentsUri)
            {
                if (accesses.Where(a => a.FolderUri == uri).FirstOrDefault() != null)
                    return true;
            }

            return false;
        }




        //public async Task RemoveAllAccessesToUser(int userId)
        //{
        //    IQueryable<FolderAccessEntity> foldersAccesses =
        //        db.FoldersAccesses.Where(fa => fa.UserId == userId);

        //    foreach (FolderAccessEntity folderAccess in foldersAccesses)
        //    {
        //        db.FoldersAccesses.Remove(folderAccess);
        //    }

        //    await db.SaveChangesAsync();
        //}


        //public FolderAccessEntity[] GetAvailableFolders(int userId)
        //{
        //    return db.FoldersAccesses.Where(fa => fa.UserId == userId).ToArray();
        //}

        //public FolderAccessEntity[] GetAvailableFoldersForFolder(int userId, string parentUrl)
        //{
        //    return db.FoldersAccesses.Where(fa => fa.UserId == userId && fa.GetParentUrl == parentUrl).ToArray();
        //}

        //public FolderAccessEntity? CheckAccessToFolder(int userId, string url)
        //{
        //    return db.FoldersAccesses.Where(
        //        fa => fa.UserId == userId && fa.FolderUrl == url).FirstOrDefault();
        //}
    }
}