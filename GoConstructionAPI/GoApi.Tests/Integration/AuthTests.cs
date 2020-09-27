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

namespace GoApi.Tests.Integration
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

        [Fact]
        public async Task RegisterContractorAndAttemptAuthEmailNotConfirmed_YieldsBadRequest()
        {
            // Arrange

            // Act
            await RegisterContractorAsync();
            var responseMessage = await AttemptAuthenticationContractorAsync();


            // Assert
            (await responseMessage.Content.ReadAsStringAsync()).Should().Be(JsonConvert.SerializeObject(new List<IdentityError> { new IdentityError { Code = Messages.EmailNotConfirmedCode, Description = Messages.EmailNotConfirmedDescription } }, serializerSettings));
            responseMessage.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

    }
}
