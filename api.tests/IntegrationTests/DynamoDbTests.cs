using System;
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
        private readonly Mock<IOptions<AwsConfig>> _configMock;

        public DynamoDbTests()
        {
            _configMock = new Mock<IOptions<AwsConfig>>();
            _configMock.Setup(m => m.Value).Returns(new AwsConfig
            {
                AccessKey = "AKIAR7NVIYZ2N7ULZSU7",
                SecretKey = "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",
                Region = "eu-west-1"
            });

        }

        [Fact]
        public async void UpsertTicket()
        {
            // Arrange
            var provider = new NoSqlProvider(_configMock.Object);

            var ticket = new TestTicket
            {
                Created = DateTime.Now,
                Content = "test2"
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
            var provider = new NoSqlProvider(_configMock.Object);
            var ticket = new TestTicket
            {
                Created = DateTime.Now,
                Content = "test2"
            };
            await provider.UpsertItemAsync(ticket, CancellationToken.None);

            // Act
            var result = await provider.GetItemAsync<Ticket>(ticket.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(string.Empty, result.Content.Replace("Ubi_v1 t=", ""));
            Assert.True(result.Valid);

            // Cleanup
            await provider.DeleteItemAsync(ticket.Id, CancellationToken.None);
        }
    }
}
