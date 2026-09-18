using PosCs.Domain.Rules;
using Xunit;

namespace PosCs.Tests.Domain
{
    public class DryCleanOrderFlowTests
    {
        [Fact]
        public void Received_Order_Can_Be_Prepared()
        {
            Assert.True(DryCleanOrderFlow.CanPrepare(DryCleanOrderFlow.Received));
        }

        [Theory]
        [InlineData(DryCleanOrderFlow.Prepared)]
        [InlineData(DryCleanOrderFlow.Delivered)]
        public void Only_Received_Order_Can_Be_Prepared(string status)
        {
            Assert.False(DryCleanOrderFlow.CanPrepare(status));
        }

        [Fact]
        public void Only_Prepared_Order_Can_Be_Delivered()
        {
            Assert.True(DryCleanOrderFlow.CanDeliver(DryCleanOrderFlow.Prepared));
            Assert.False(DryCleanOrderFlow.CanDeliver(DryCleanOrderFlow.Received));
            Assert.False(DryCleanOrderFlow.CanDeliver(DryCleanOrderFlow.Delivered));
        }
    }
}
