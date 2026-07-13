namespace SchoolProject.Services.Search.Interfaces
{
    public interface IQueryNormalizer
    {
        List<string> Normalize(string query);
    }
}