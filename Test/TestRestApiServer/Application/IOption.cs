namespace TestRestApiServer.Application
{
    internal interface IOption
    {
        bool Help { get; }
        string? ConfigFile { get; }
        int Port { get; }
    }
}