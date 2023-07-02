namespace StructureApp.Models
{
    public class Folder
    {
        public long Id { get; set; }
        public long ParentId { get; set; }
        public string Name { get; set; }
    }
}
