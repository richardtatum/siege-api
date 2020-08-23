using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using api.Interfaces;
using api.Models;
using Microsoft.Extensions.Options;

namespace api.Providers
{
    public class NoSqlProvider : INoSqlProvider
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly AwsConfig _config;
        private const string TableName = "ubisoft";

        public NoSqlProvider(IOptions<AwsConfig> config)
        {
            _config = config.Value;
            var credentials = new BasicAWSCredentials(_config.AccessKey, _config.SecretKey);
            _client = new AmazonDynamoDBClient(credentials, RegionEndpoint.EUWest1);
        }

        private async Task EnsureProvisionedAsync(CancellationToken token)
        {
            // This currently still passes the Exception, even though its caught
            try
            {
                await _client.DescribeTableAsync(TableName, token);
                return;
            }
            catch (ResourceNotFoundException e)
            {
                // TODO: Log
            }

            // Default table information for the Ubisoft key table
            var table = new CreateTableRequest(
                TableName,
                new List<KeySchemaElement> { new KeySchemaElement("id", KeyType.HASH) },
                new List<AttributeDefinition> { new AttributeDefinition("id", ScalarAttributeType.S) },
                new ProvisionedThroughput(5, 5)
                );

            await _client.CreateTableAsync(table, token);
        }

        public async Task<T> GetItemAsync<T>(string id, CancellationToken token) where T : class
        {
            if (!Table.TryLoadTable(_client, TableName, out var table ))
                return null;

            var response = await table.GetItemAsync(id, token);

            if (response == null) return null;

            var item = JsonSerializer.Deserialize<T>(response.ToJson(),
                new JsonSerializerOptions{PropertyNameCaseInsensitive = true});

            return item;
        }

        public async Task<bool> DeleteItemAsync(string id, CancellationToken token)
        {
            if (!Table.TryLoadTable(_client, TableName, out var table))
                return false;

            var primitive = new Primitive(id, false);

            var config = new DeleteItemOperationConfig
            {
                ReturnValues = ReturnValues.AllOldAttributes
            };

            var response = await table.DeleteItemAsync(primitive, config, token);
            return response != null;
        }

        public async Task<bool> UpsertItemAsync<T>(T item, CancellationToken token) where T : class
        {
            await EnsureProvisionedAsync(token);

            if (!Table.TryLoadTable(_client, TableName, out var table))
                return false;

            var json = JsonSerializer.Serialize(item, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var document = Document.FromJson(json);

            var config = new PutItemOperationConfig
            {
                ReturnValues = ReturnValues.AllOldAttributes
            };

            var response = await table.PutItemAsync(document, config , token);
            return response != null;
        } 
    }
}
