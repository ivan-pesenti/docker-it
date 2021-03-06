using DockerIt.Entities;
using DockerIt.Service.Tests.Platform;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;

namespace DockerIt.Service.Tests
{
    public class OrderServiceTest : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _databaseFixture;
        private readonly IOrderService _orderService;
        public OrderServiceTest(DatabaseFixture databaseFixture)
        {
            _databaseFixture = databaseFixture;
            _orderService = new OrderService(new SqlConnection($"Data Source=localhost,{_databaseFixture.SqlPort};Initial Catalog=WideWorldImporters;Integrated Security=False;User ID=sa;Password=abcd1234ABCD!"));
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(2, 15)]
        [InlineData(500, 15)]
        public async Task GetAll_Returns_Exact_Rows_Num(int page, int take)
        {
            // arrange            

            // act
            var orders = await _orderService.GetAll(page: page, take: take);

            // assert            
            Assert.True(orders.Count <= take);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2000)]
        public async Task GetById_Returns_Exact_Row(int orderId)
        {
            // arrange

            // act
            var order = await _orderService.GetbyId(orderId);

            // assert            
            Assert.Equal(orderId, order.OrderID);
        }

        [Theory]
        [InlineData(100000)]
        public async Task GetById_Returns_Null_When_Not_Found(int orderId)
        {
            // arrange

            // act
            var order = await _orderService.GetbyId(orderId);

            // assert            
            Assert.Null(order);
        }

        [Fact]
        public async Task Create_Order_Return_One_Rows_Affected()
        {
            // arrange
            var order = new Order()
            {
                CustomerID = 1,
                SalespersonPersonID = 1,
                ContactPersonID = 1,
                OrderDate = DateTime.Today,
                ExpectedDeliveryDate = DateTime.Today.AddDays(3),
                IsUndersupplyBackordered = false,
                LastEditedBy = 1
            };

            // act
            var affRows = await _orderService.Create(order);

            // assert
            Assert.Equal(1, affRows);
        }

        [Fact]
        public async Task Create_Order_Throws_Exc_When_Info_Are_Missing()
        {
            // arrange
            var order = new Order()
            {
                SalespersonPersonID = 1,
                ContactPersonID = 1,
                OrderDate = DateTime.Today,
                ExpectedDeliveryDate = DateTime.Today.AddDays(3),
                LastEditedBy = 1
            };

            // act

            // assert
            await Assert.ThrowsAsync<SqlException>(async () => 
            {
                await _orderService.Create(order);
            });
        }

        [Fact]
        public async Task Update_Order_Must_Set_Correct_Values()
        {
            // arrange
            var order = new Order()
            {
                OrderID = 1,
                CustomerID = 2,
                IsUndersupplyBackordered = true,
                SalespersonPersonID = 1,
                ContactPersonID = 1,
                OrderDate = DateTime.Today.AddDays(-10),
                ExpectedDeliveryDate = DateTime.Today.AddDays(3),
                LastEditedBy = 1
            };

            // act
            var affRows = await _orderService.Update(order);

            // assert
            var updatedOrder = await _orderService.GetbyId(order.OrderID);
            Assert.Equal(1, affRows);
            Assert.Equal(order.CustomerID, updatedOrder.CustomerID);
            Assert.Equal(order.OrderDate, updatedOrder.OrderDate);
        }

        [Fact]
        public async Task Delete_Order_Must_Remove_From_Db()
        {
            // arrange
            int id = 4000;

            // act
            var affRows = await _orderService.Delete(id);

            // assert
            var deletedOrder = await _orderService.GetbyId(id);
            Assert.Equal(1, affRows);
            Assert.Null(deletedOrder);
        }
        
        [Fact]
        public async Task Delete_Not_Existing_Order_Must_Affect_Zero_Rows()
        {
            // arrange
            int id = 100000;

            // act
            var affRows = await _orderService.Delete(id);

            // assert
            Assert.Equal(0, affRows);
        }
    }
}
