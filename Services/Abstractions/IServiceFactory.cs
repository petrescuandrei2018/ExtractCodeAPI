namespace ExtractCodeAPI.Services.Abstractions
{
    public interface IServiceFactory
    {
        T CreateService<T>() where T : class;
    }
}
