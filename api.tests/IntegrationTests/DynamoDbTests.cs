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
        public async void Insert()
        {
            // Arrange
            var provider = new NoSqlProvider(_config.Object);
            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Value = "test"
            };

            // Act
            var x = await provider.PutItemAsync(ticket, CancellationToken.None);

            // Assert
            Assert.NotNull(x);
        }
    }
}
