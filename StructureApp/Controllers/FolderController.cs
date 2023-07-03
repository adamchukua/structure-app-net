using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StructureApp.Models;
using System.IO;

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

        public async Task<ActionResult> Import(IFormFile file)
        {
            using (StreamReader reader = new StreamReader(file.OpenReadStream()))
            {
                string line;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    var lineDecoded = line.Split(';');
                    var rootFolderName = lineDecoded[0].Trim();
                    var childFolderName = lineDecoded[1].Trim();

                    var isRootFolderExist = await _context.Folders.AnyAsync(folder => folder.Name == rootFolderName);
                    var isChildFolderExist = await _context.Folders.AnyAsync(folder => folder.Name == childFolderName);

                    Folder rootFolder;

                    if (!isRootFolderExist)
                    { 
                        await _context.Folders.AddAsync(new Folder(null, rootFolderName));
                        await _context.SaveChangesAsync();
                    }

                    rootFolder = await _context.Folders.FirstOrDefaultAsync(folder => folder.Name == rootFolderName);

                    if (!isChildFolderExist)
                    {
                        await _context.Folders.AddAsync(new Folder(rootFolder.Id, childFolderName));
                        await _context.SaveChangesAsync();
                    }
                }
            }

            var oldRootFolder = _context.Folders.FirstOrDefault(folder => folder.ParentId == null);
            var newRootFolder = _context.Folders.OrderBy(f => f.Id).LastOrDefault(folder => folder.ParentId == null);
            var oldFolders = _context.Folders
                .Where(folder => folder.Id >= oldRootFolder.Id && folder.Id < newRootFolder.Id)
                .ToList();

            string exportFilePath = $"export-{newRootFolder.Id}.txt";
            if (!System.IO.File.Exists(exportFilePath))
            {
                using (StreamWriter sw = System.IO.File.CreateText(exportFilePath))
                {
                    foreach (var oldFolder in oldFolders)
                    {
                        var parentFolder = await _context.Folders
                            .FirstOrDefaultAsync(folder => folder.Id == oldFolder.ParentId);

                        if (parentFolder != null)
                        {
                            sw.WriteLine($"{parentFolder.Name.Trim() ?? ""};{oldFolder.Name.Trim()}");
                        }
                    }
                }
            }

            _context.Folders.RemoveRange(oldFolders);
            await _context.SaveChangesAsync();

            return Redirect("/");
        }
    }
}
