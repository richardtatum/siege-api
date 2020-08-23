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
                AccessKey = "AKIAR7NVIYZ2J2HCRDLB",
                SecretKey = "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",
                Region = "eu-west-1"
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
            var result = await provider.GetItemAsync(ticket.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Keys.Count);
            Assert.Equal(3, result.Values.Count);
            Assert.NotNull(result.Keys.FirstOrDefault());
            Assert.NotNull(result.Values.FirstOrDefault());

            // Cleanup
            await provider.DeleteItemAsync(ticket.Id, CancellationToken.None);
        }
    }
}
