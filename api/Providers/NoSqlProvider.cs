﻿using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using api.Models;
using Microsoft.Extensions.Options;

namespace api.Providers
{
    public class NoSqlProvider
    {
        private readonly AmazonDynamoDBClient _client;
        private readonly AwsConfig _config;
        private readonly PutItemOperationConfig _putConfig;
        private readonly DeleteItemOperationConfig _deleteConfig;
        private const string TableName = "ubisoft";

        public NoSqlProvider(IOptions<AwsConfig> config)
        {
            _config = config.Value;
            var credentials = new BasicAWSCredentials(_config.AccessKey, _config.SecretKey);
            _client = new AmazonDynamoDBClient(credentials, RegionEndpoint.EUWest1);
        }

        public async Task EnsureProvisionedAsync(CancellationToken token)
        {
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

        public async Task<Document> GetItemAsync(string id)
        {
            if (!Table.TryLoadTable(_client, TableName, out var table ))
                return null;

            var response = await table.GetItemAsync(id);
            return response;
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

        public async Task<bool> UpsertItemAsync(Ticket ticket, CancellationToken token)
        {
            await EnsureProvisionedAsync(token);

            if (!Table.TryLoadTable(_client, TableName, out var table))
                return false;

            var attributes = new Dictionary<string, AttributeValue>
            {
                {"id", new AttributeValue(ticket.Id.ToString())},
                {"value", new AttributeValue(ticket.Value)},
                {"created", new AttributeValue(ticket.Created.ToString(CultureInfo.InvariantCulture))}
            };

            var item = Document.FromAttributeMap(attributes);

            var config = new PutItemOperationConfig
            {
                ReturnValues = ReturnValues.AllOldAttributes
            };

            var response = await table.PutItemAsync(item, config , token);
            return response != null;
        } 
    }
}
