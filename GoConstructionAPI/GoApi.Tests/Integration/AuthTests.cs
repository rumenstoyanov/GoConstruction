using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GoApi.Tests
{
    public class AuthTests : IntegrationTest
    {
        [Fact]
        public async Task RegisterContractor_IsSuccessful()
        {
            // Arrange

            // Act
            var responseMessage = await RegisterContractorAsync();

            // Assert
            (await responseMessage.Content.ReadAsStringAsync()).Should().Be("");
            responseMessage.StatusCode.Should().Be(HttpStatusCode.OK);
        }

    }
}
