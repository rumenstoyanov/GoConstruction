using FluentAssertions;
using GoLibrary.Data.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace GoApi.Tests.Integration
{
    public class ResourceTests : IntegrationTest
    {
        public ResourceTests(ITestOutputHelper output) : base(output)
        {

        }

        public override void Dispose()
        {
            base.Dispose();
        }

        [Fact]
        public async Task PostSitesAndGetSitesDetail_ReturnsCorrectData()
        {

            // Arrange
            var expectedTitle = "Test Site Integration";
            var expectedDescription = "Lorum Imsum Test";
            var expectedEndDate = new DateTime(2020, 12, 31);
            var expectedFriendlyId = "4455A";
            await LoginContractorOne();

            // Act - 1
            var contentToPost = new SiteCreateRequestDto
            {
                Title = expectedTitle,
                Description = expectedDescription,
                EndDate = expectedEndDate,
                FriendlyId = expectedFriendlyId
            };
            var postResponse = await TestClient.PostAsync("api/sites/", new StringContent(JsonConvert.SerializeObject(contentToPost), Encoding.UTF8, "application/json"));
            var postResponseContent = JsonConvert.DeserializeObject<SiteReadResponseDto>(await postResponse.Content.ReadAsStringAsync());

            // Act - 2
            var getResponse = await TestClient.GetAsync($"api/sites/{postResponseContent.Id}");
            var getResponseContent = JsonConvert.DeserializeObject<SiteReadResponseDto>(await getResponse.Content.ReadAsStringAsync());

            // Assert
            postResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            getResponseContent.Title.Should().Be(expectedTitle);
            getResponseContent.Description.Should().Be(expectedDescription);
            getResponseContent.EndDate.Should().Be(expectedEndDate);
            getResponseContent.FriendlyId.Should().Be(expectedFriendlyId);
        }

        [Fact]
        public async Task PostSiteWithoutLogin_YieldsUnauthorized()
        {
            // Arrange
            var expectedStatusCode = HttpStatusCode.Unauthorized;
            var expectedTitle = "Test Site Integration";
            var expectedDescription = "Lorum Imsum Test";
            var expectedEndDate = new DateTime(2020, 12, 31);
            var expectedFriendlyId = "4455A";

            // Act
            var contentToPost = new SiteCreateRequestDto
            {
                Title = expectedTitle,
                Description = expectedDescription,
                EndDate = expectedEndDate,
                FriendlyId = expectedFriendlyId
            };
            var postResponse = await TestClient.PostAsync("api/sites/", new StringContent(JsonConvert.SerializeObject(contentToPost), Encoding.UTF8, "application/json"));

            // Assert
            postResponse.StatusCode.Should().Be(expectedStatusCode);

        }



    }
}
