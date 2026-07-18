using System.Collections.Generic;
using RestaurantPOS.Services;
using Xunit;

namespace RestaurantPOS.Tests
{
    public class AIServiceTests
    {
        [Fact]
        public void GetRecommendedDishes_EmptyCart_ReturnsThreeDishes()
        {
            // Arrange
            var aiService = new AIService();
            var currentDishIds = new List<int>();

            // Act
            var recommendations = aiService.GetRecommendedDishes(currentDishIds);

            // Assert
            Assert.NotNull(recommendations);
            Assert.True(recommendations.Count > 0, "Recommendations list should not be empty");
            Assert.Equal(3, recommendations.Count);
        }

        [Fact]
        public void GetRecommendedDishes_WithItems_ReturnsRecommendations()
        {
            // Arrange
            var aiService = new AIService();
            var currentDishIds = new List<int> { 1, 2 };

            // Act
            var recommendations = aiService.GetRecommendedDishes(currentDishIds);

            // Assert
            Assert.NotNull(recommendations);
            Assert.True(recommendations.Count > 0);
            Assert.Equal(3, recommendations.Count);
        }
    }
}
