namespace EList.FilestorageClient
{
    public class SetFilesVisibilityRequest
    {
        public List<Guid> FileIds { get; set; }

        public FileVisibility Visibility { get; set; }
    }
}
