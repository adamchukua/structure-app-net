using Microsoft.AspNetCore.Mvc;
using StructureApp.Models;

namespace StructureApp.Controllers
{
    public class FolderController : Controller
    {
        private readonly FolderContext _context;

        public FolderController(FolderContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var path = Request.Path.Value;
            var pathFolderNames = path
                .Split('/')
                .Where(folder => !string.IsNullOrEmpty(folder))
                .ToArray();
            ViewBag.Url = path;

            if (pathFolderNames.Length == 0)
            {
                var rootFolder = _context.Folders.FirstOrDefault(folder => folder.ParentId == null);
                rootFolder.ChildFolders = _context.Folders.Where(folder => folder.ParentId == rootFolder.Id).ToList();
                ViewBag.Url = rootFolder.Name.Trim();

                return View(rootFolder);
            }
            else if (pathFolderNames.Length == 1)
            {
                var rootFolder = _context.Folders.FirstOrDefault(folder => folder.Name == pathFolderNames[0]);
                rootFolder.ChildFolders = _context.Folders.Where(folder => folder.ParentId == rootFolder.Id).ToList();

                if (rootFolder.ParentId == null)
                {
                    return View(rootFolder);
                }

                return NotFound();
            }
            else
            {
                Folder currentFolder;
                Folder nextFolder = new Folder();

                int i = 0;
                while (i != pathFolderNames.Length - 1)
                {
                    currentFolder = _context.Folders.FirstOrDefault(folder => folder.Name == pathFolderNames[i]);
                    nextFolder = _context.Folders.FirstOrDefault(folder => folder.Name == pathFolderNames[i + 1]);

                    if (nextFolder.ParentId != currentFolder.Id)
                    {
                        return NotFound();
                    }

                    i++;
                }

                nextFolder.ChildFolders = _context.Folders.Where(folder => folder.ParentId == nextFolder.Id).ToList();

                return View(nextFolder);
            }
        }
    }
}
