using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using GoApi.Data.Constants;
using GoApi.Data.Dtos;
using Xunit.Abstractions;

namespace GoApi.Tests.Integration
{
    public class AuthTests : IntegrationTest
    {
        public AuthTests(ITestOutputHelper output) : base(output)
        {

        }

        [Fact]
        public async Task RegisterContractor_IsSuccessful()
        {
            // Arrange
            var expectedMessage = "";
            var expectedStatusCode = HttpStatusCode.OK;

            // Act
            var actualMessage = await RegisterContractorOneAsync();

            // Assert
            (await actualMessage.Content.ReadAsStringAsync()).Should().Be(expectedMessage);
            actualMessage.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public async Task RegisterContractorAndAttemptAuthEmailNotConfirmed_YieldsBadRequest()
        {
            // Arrange
            var expectedMessage = JsonConvert.SerializeObject(new List<IdentityError> { new IdentityError { Code = Messages.EmailNotConfirmedCode, Description = Messages.EmailNotConfirmedDescription } }, serializerSettings);
            var expectedStatusCode = HttpStatusCode.BadRequest;

            // Act
            await RegisterContractorOneAsync();
            var actualMessage = await AttemptAuthenticationContractorOneAsync();


            // Assert
            (await actualMessage.Content.ReadAsStringAsync()).Should().Be(expectedMessage);
            actualMessage.StatusCode.Should().Be(expectedStatusCode);
        }

        [Fact]
        public async Task RegisterContractorAndAttemptAuthEmailConfirmed_IsSuccessful()
        {
            // Arrange
            var expectedMessageType = typeof(LoginResponseDto);
            var expectedStatusCode = HttpStatusCode.OK;

            // Act
            await RegisterContractorOneAsync();
            await ConfirmContractorOneEmailAsync();
            var actualMessage = await AttemptAuthenticationContractorOneAsync();
            var actualMessageContent = await actualMessage.Content.ReadAsStringAsync();


            // Assert
            JsonConvert.DeserializeObject<LoginResponseDto>(actualMessageContent).Should().BeOfType(expectedMessageType);
            actualMessage.StatusCode.Should().Be(expectedStatusCode);
        }

    }
}
