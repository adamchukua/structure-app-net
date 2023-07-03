using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace StructureApp.Models
{
    public class Folder
    {
        public long Id { get; set; }
        [Column("parent_id")]
        public long? ParentId { get; set; }
        public string Name { get; set; }
        [NotMapped]
        public List<Folder> ChildFolders { get; set; }

        public Folder() {}

        public Folder(long? parentId, string name)
        {
            ParentId = parentId;
            Name = name;
        }
    }
}
