namespace Jack.RemoteLog.WebApi.Domains
{
    public interface ISourceContextCollection : IEnumerable<string>
    {
        void Add(string sourceContext);
        int GetId(string sourceContext);
        string GetSourceContext(int id);
    }
}
