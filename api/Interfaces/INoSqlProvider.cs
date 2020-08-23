using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using api.Models;

namespace api.Interfaces
{
    public interface INoSqlProvider
    {
        Task<T> GetItemAsync<T>(string id, CancellationToken token) where T : class;
        Task<bool> DeleteItemAsync(string id, CancellationToken token);
        Task<bool> UpsertItemAsync<T>(T item, CancellationToken token) where T : class;
    }
}
