using FluentAssertions;
using GoApi.Data.Dtos;
using GoApi.Data.Models;
using GoApi.Services.Implementations;
using System;
using System.Collections.Generic;
using Xunit;
using GoApi.Extensions;

namespace GoApi.Tests.Unit
{
    public class UpdateServiceTests
    {
        [Fact]
        public void Diff_ShouldIndicateCorrectUpdate()
        {
            // Arrange
            var preUpdate = new SiteUpdateRequestDto { Title = "TestTitle", Description = "TestDescription", EndDate = new DateTime(2020, 1, 1) };
            var postUpdate = new SiteUpdateRequestDto { Title = "ReTestTitle", Description = "TestDescription", EndDate = new DateTime(2020, 1, 1) };

            // Act
            var updateService = new UpdateService(null,null,null);
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
            var updateService = new UpdateService(null, null, null);
            var actualDiffDict = updateService.Diff(preUpdate, postUpdate);

            // Assert
            Assert.Empty(actualDiffDict);

        }

        [Fact]
        public void AssembleSyntaxFromDiff_GivesCorrectSyntaxSingleton()
        {
            // Arrange
            var diff = new Dictionary<string, string> { { "Title", "ReTestTitle" } };
            var expected = " updated the Title to ReTestTitle";

            // Act
            var updateService = new UpdateService(null, null, null);
            var actual = updateService.AssembleSyntaxFromDiff(diff);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AssembleSyntaxFromDiff_GivesCorrectSyntaxMultiple()
        {
            // Arrange
            var diff = new Dictionary<string, string> { { "Title", "ReTestTitle" }, { "Description", "ReTestDescription" } };
            var expected = " updated the Title to ReTestTitle, updated the Description to ReTestDescription";

            // Act
            var updateService = new UpdateService(null, null, null);
            var actual = updateService.AssembleSyntaxFromDiff(diff);

            // Assert
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void UpdateToStringExtension_GivesCorrectStringSiteUpdateCase()
        {
            // Arrange
            var updateList = new List<UpdateDetail> { new UpdateDetail { Resource = new ResourceUpdateDetail { Id = "", Location = "", Name = "Bob" }, Syntax = null }, new UpdateDetail { Resource = null, Syntax = " updated the Title to ReTestTitle."} };
            var update = new Update { UpdatedResourceId = Guid.NewGuid(), Time = new DateTime(2016, 6, 15, 13, 30, 0), UpdateList = updateList };
            var expected = "15/06/2016 13:30: Bob updated the Title to ReTestTitle.";

            // Act
            var actual = update.ToString();

            // Assert
            Assert.Equal(expected, actual);
        }

    }
}
