using FluentAssertions;
using GoApi.Data.Dtos;
using GoApi.Services.Implementations;
using System;
using Xunit;

namespace GoApi.Tests
{
    public class UpdateServiceTest
    {
        [Fact]
        public void Diff_ShouldIndicateCorrectUpdate()
        {
            // Arrange
            var preUpdate = new SiteUpdateRequestDto { Title = "TestTitle", Description = "TestDescription", EndDate = new DateTime(2020, 1, 1) };
            var postUpdate = new SiteUpdateRequestDto { Title = "ReTestTitle", Description = "TestDescription", EndDate = new DateTime(2020, 1, 1) };

            // Act
            var updateService = new UpdateService();
            var actualDiffDict = updateService.Diff(preUpdate, postUpdate);

            // Assert
            Assert.Single(actualDiffDict);
            Assert.Contains("Title", actualDiffDict.Keys);
            Assert.Equal("ReTestTitle", actualDiffDict["Title"]);
        }

        [Fact]
        public void Diff_ShouldBeEmptyIfEqual()
        {
            // Arrange
            var preUpdate = new SiteUpdateRequestDto { Title = "TestTitle", Description = "TestDescription", EndDate = new DateTime(2020, 1, 1) };
            var postUpdate = new SiteUpdateRequestDto { Title = "TestTitle", Description = "TestDescription", EndDate = new DateTime(2020, 1, 1) };

            // Act
            var updateService = new UpdateService();
            var actualDiffDict = updateService.Diff(preUpdate, postUpdate);

            // Assert
            Assert.Empty(actualDiffDict);

        }

    }
}
