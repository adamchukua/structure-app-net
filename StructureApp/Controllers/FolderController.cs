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
            List<Folder> folders = _context.Folders.ToList();

            return View(folders);
        }
    }
}
