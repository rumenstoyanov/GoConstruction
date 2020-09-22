using GoApi.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace GoApi.Tests.Unit
{
    public class RedisCacheServiceTests
    {
        [Fact]
        public void BuildCacheKeyFromUrl_UppercaseBecomesLowercase()
        {
            var testOid = Guid.NewGuid();
            var testUrl = "https://localhost/api/Sites/";
            // Arrange
            var expectedCacheKey = $"{testOid}|/api/sites/";

            // Act
            var cacheService = new RedisCacheService(null, new Data.Constants.RedisSettings { IsEnabled = true, TimeToLiveSeconds = 0});
            var actualCacheKey = cacheService.BuildCacheKeyFromUrl(testUrl, testOid);

            // Assert
            Assert.Equal(expectedCacheKey, actualCacheKey);
        }


        [Fact]
        public void BuildCacheKeyFromUrl_AppendsForwardSlash()
        {
            var testOid = Guid.NewGuid();
            var testUrl = "https://localhost/api/Sites";
            // Arrange
            var expectedCacheKey = $"{testOid}|/api/sites/";

            // Act
            var cacheService = new RedisCacheService(null, new Data.Constants.RedisSettings { IsEnabled = true, TimeToLiveSeconds = 0 });
            var actualCacheKey = cacheService.BuildCacheKeyFromUrl(testUrl, testOid);

            // Assert
            Assert.Equal(expectedCacheKey, actualCacheKey);
        }

    }
}
