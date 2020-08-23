using System;
using System.Linq;
using System.Threading;
using api.Models;
using api.Providers;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace api.tests.IntegrationTests
{
    public class DynamoDbTests
    {
        private readonly Mock<IOptions<AwsConfig>> _config;

        public DynamoDbTests()
        {
            _config = new Mock<IOptions<AwsConfig>>();
            _config.Setup(m => m.Value).Returns(new AwsConfig
            {
                AccessKey = "",
                SecretKey = "",
                Region = ""
            });

        }

        [Fact]
        public async void UpsertTicket()
        {
            // Arrange
            var provider = new NoSqlProvider(_config.Object);

            var ticket = new TestTicket
            {
                Created = DateTime.Now,
                Value = "test2"
            };

            // Act
            var result = await provider.UpsertItemAsync(ticket, CancellationToken.None);

            // Assert
            Assert.True(result);

            // Cleanup
            await provider.DeleteItemAsync(ticket.Id, CancellationToken.None);
        }

        [Fact]
        public async void GetTicket()
        {
            // Arrange
            var provider = new NoSqlProvider(_config.Object);
            var ticket = new TestTicket
            {
                Created = DateTime.Now,
                Value = "test2"
            };
            await provider.UpsertItemAsync(ticket, CancellationToken.None);

            // Act
            var result = await provider.GetItemAsync<Ticket>(ticket.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(string.Empty, result.Value.Replace("Ubi_v1 t=", ""));
            Assert.True(result.Valid);

            // Cleanup
            await provider.DeleteItemAsync(ticket.Id, CancellationToken.None);
        }
    }
}
