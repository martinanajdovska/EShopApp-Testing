using ArtEShop.Domain.DomainModels;
using ArtEShop.Domain.DTO;
using ArtEShop.Repository.Interface;
using ArtEShop.Service.Implementation;
using ArtEShop.Service.Interface;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AppTesting
{
    [TestClass]
    public class OrderServiceTests
    {
        private Mock<IRepository<Order>> _orderRepoMock;
        private Mock<IRepository<ShoppingCartItem>> _shoppingCartItemRepoMock;
        private Mock<IShoppingCartService> _shoppingCartServiceMock;
        private Fixture _fixture;
        private IOrderService _orderService;
        private List<Order> _orderList;

        public OrderServiceTests()
        {
            _fixture = new Fixture();
            _orderRepoMock = new Mock<IRepository<Order>>();
            _shoppingCartItemRepoMock = new Mock<IRepository<ShoppingCartItem>>();
            _shoppingCartServiceMock = new Mock<IShoppingCartService>();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _orderList = _fixture.CreateMany<Order>(5).ToList();

            _orderRepoMock.Setup(repo => repo.Get(
                    It.IsAny<Expression<Func<Order, Order>>>(),
                    It.IsAny<Expression<Func<Order, bool>>>(),
                    It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                    It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                .Returns((Expression<Func<Order, Order>> selector,
                          Expression<Func<Order, bool>>? predicate,
                          Func<IQueryable<Order>, IOrderedQueryable<Order>>? orderBy,
                          Func<IQueryable<Order>, IIncludableQueryable<Order, object>>? include) =>
                {
                    var query = _orderList.AsQueryable();

                    if (include != null)
                    {
                        query = include(query);
                    }

                    if (predicate != null)
                        query = query.Where(predicate.Compile()).AsQueryable();

                    if (orderBy != null)
                        query = orderBy(query);

                    return query.Select(selector.Compile()).FirstOrDefault();
                });

            _orderRepoMock.Setup(repo => repo.GetAll(
                    It.IsAny<Expression<Func<Order, Order>>>(),
                    It.IsAny<Expression<Func<Order, bool>>>(),
                    It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                    It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                .Returns((Expression<Func<Order, Order>> selector,
                          Expression<Func<Order, bool>>? predicate,
                          Func<IQueryable<Order>, IOrderedQueryable<Order>>? orderBy,
                          Func<IQueryable<Order>, IIncludableQueryable<Order, object>>? include) =>
                {
                    var query = _orderList.AsQueryable();

                    if (include != null)
                    {
                        query = include(query);
                    }

                    if (predicate != null)
                        query = query.Where(predicate.Compile()).AsQueryable();

                    if (orderBy != null)
                        query = orderBy(query);

                    return query.Select(selector.Compile()).AsEnumerable();
                });

            _orderRepoMock
                .Setup(repo => repo.Insert(It.IsAny<Order>()))
                .Returns((Order o) => o);

            _orderService = new OrderService(
                _orderRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _shoppingCartServiceMock.Object
            );

        }

        [TestMethod]
        public void GetAll_Works()
        {
            var result = _orderService.GetAll();

            Assert.AreEqual(5, result.Count);

            _orderRepoMock.Verify(r => r.GetAll(
                    It.IsAny<Expression<Func<Order, Order>>>(),
                    It.IsAny<Expression<Func<Order, bool>>>(),
                    It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                    It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()), Times.Once);
        }

        [TestMethod]
        public void GetAll_ReturnsEmptyList()
        {
            _orderRepoMock.Setup(repo => repo.GetAll(
                    It.IsAny<Expression<Func<Order, Order>>>(),
                    It.IsAny<Expression<Func<Order, bool>>>(),
                    It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                    It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()))
                .Returns(Enumerable.Empty<Order>());

            _orderService = new OrderService(
                _orderRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _shoppingCartServiceMock.Object
            );

            var result = _orderService.GetAll();

            Assert.AreEqual(0, result.Count);

            _orderRepoMock.Verify(r => r.GetAll(
                    It.IsAny<Expression<Func<Order, Order>>>(),
                    It.IsAny<Expression<Func<Order, bool>>>(),
                    It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                    It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()), Times.Once);
        }

        [TestMethod]
        public void GetById_Works()
        {
            var target = _orderList[0];

            var result = _orderService.GetById(target.Id);

            Assert.AreEqual(target.Id, result.Id);

            _orderRepoMock.Verify(r => r.Get(
                    It.IsAny<Expression<Func<Order, Order>>>(),
                    It.IsAny<Expression<Func<Order, bool>>>(),
                    It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                    It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()), Times.Once);
        }

        [TestMethod]
        public void GetById_ReturnsNullWhenIdNotFound()
        {
            var result = _orderService.GetById(Guid.NewGuid());

            Assert.IsNull(result);

            _orderRepoMock.Verify(r => r.Get(
                    It.IsAny<Expression<Func<Order, Order>>>(),
                    It.IsAny<Expression<Func<Order, bool>>>(),
                    It.IsAny<Func<IQueryable<Order>, IOrderedQueryable<Order>>>(),
                    It.IsAny<Func<IQueryable<Order>, IIncludableQueryable<Order, object>>>()), Times.Once);
        }

        [TestMethod]
        public void Insert_Works()
        {
            var order = new Order()
            {
                OwnerId = Guid.NewGuid(),
                TotalPrice = 200,
                PurchasedItems = _fixture.CreateMany<ShoppingCartItem>(2).ToList()
            };

            var result = _orderService.Insert(order);

            Assert.IsNotNull(result.Id);
            Assert.AreEqual(order.OwnerId, result.OwnerId);
            Assert.AreEqual(order.TotalPrice, result.TotalPrice);
            Assert.AreEqual(order.PurchasedItems[0], result.PurchasedItems[0]);
            Assert.AreEqual(order.PurchasedItems[1], result.PurchasedItems[1]);

            _orderRepoMock.Verify(r => r.Insert(It.IsAny<Order>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Insert_OrderIsNullException()
        {
            _orderService.Insert(null);
        }

        [TestMethod]
        public void CreateOrder_WorksWithOneArtPiece()
        {
            var artPiece = _fixture.Create<ArtPiece>();
            artPiece.Id = Guid.NewGuid();

            var shoppingCart = _fixture.Create<ShoppingCart>();
            shoppingCart.OwnerId = Guid.NewGuid().ToString();

            var shoppingCartItemList = _fixture.CreateMany<ShoppingCartItem>(5).ToList();
            shoppingCartItemList[0].ArtPiece = artPiece;
            shoppingCartItemList[0].ShoppingCart = shoppingCart;

            _shoppingCartServiceMock.Setup(s => s.GetByUserId(It.IsAny<Guid>())).Returns(shoppingCart);

            _shoppingCartItemRepoMock.Setup(repo => repo.Get(
                  It.IsAny<Expression<Func<ShoppingCartItem, ShoppingCartItem>>>(),
                  It.IsAny<Expression<Func<ShoppingCartItem, bool>>>(),
                  It.IsAny<Func<IQueryable<ShoppingCartItem>, IOrderedQueryable<ShoppingCartItem>>>(),
                  It.IsAny<Func<IQueryable<ShoppingCartItem>, IIncludableQueryable<ShoppingCartItem, object>>>()))
              .Returns((Expression<Func<ShoppingCartItem, ShoppingCartItem>> selector,
                        Expression<Func<ShoppingCartItem, bool>>? predicate,
                        Func<IQueryable<ShoppingCartItem>, IOrderedQueryable<ShoppingCartItem>>? orderBy,
                        Func<IQueryable<ShoppingCartItem>, IIncludableQueryable<ShoppingCartItem, object>>? include) =>
          {
              var query = shoppingCartItemList.AsQueryable();
              if (include != null)
              {
                  query = include(query);
              }

              if (predicate != null)
                  query = query.Where(predicate.Compile()).AsQueryable();

              if (orderBy != null)
                  query = orderBy(query);

              return query.Select(selector.Compile()).FirstOrDefault();
          });

            _orderService = new OrderService(
              _orderRepoMock.Object,
              _shoppingCartItemRepoMock.Object,
              _shoppingCartServiceMock.Object
          );

            var result = _orderService.CreateOrder(artPiece.Id, Guid.Parse(shoppingCart.OwnerId));

            Assert.AreEqual(shoppingCartItemList[0].Id, result.PurchasedItems[0].Id);
            Assert.AreEqual(shoppingCartItemList[0].Quantity * shoppingCartItemList[0].ArtPiece.Price, result.TotalPrice);

            _shoppingCartServiceMock.Verify(s => s.GetByUserId(It.IsAny<Guid>()), Times.Once);
            _shoppingCartItemRepoMock.Verify(r => r.Get(
                  It.IsAny<Expression<Func<ShoppingCartItem, ShoppingCartItem>>>(),
                  It.IsAny<Expression<Func<ShoppingCartItem, bool>>>(),
                  It.IsAny<Func<IQueryable<ShoppingCartItem>, IOrderedQueryable<ShoppingCartItem>>>(),
                  It.IsAny<Func<IQueryable<ShoppingCartItem>, IIncludableQueryable<ShoppingCartItem, object>>>()), Times.Once);
        }

        [TestMethod]
        public void CreateOrder_WorksWithMultipleArtPieces()
        {
            var artPieceList = _fixture.CreateMany<ArtPiece>(5).ToList();

            var shoppingCart = _fixture.Create<ShoppingCart>();
            shoppingCart.OwnerId = Guid.NewGuid().ToString();

            var shoppingCartItemList = _fixture.CreateMany<ShoppingCartItem>(5).ToList();
            shoppingCartItemList[0].ArtPiece = artPieceList[0];
            shoppingCartItemList[0].ShoppingCart = shoppingCart;
            shoppingCartItemList[1].ArtPiece = artPieceList[0];
            shoppingCartItemList[1].ShoppingCart = shoppingCart;
            shoppingCartItemList[2].ArtPiece = artPieceList[1];
            shoppingCartItemList[2].ShoppingCart = shoppingCart;

            _shoppingCartServiceMock.Setup(s => s.GetByUserId(It.IsAny<Guid>())).Returns(shoppingCart);

            _shoppingCartItemRepoMock.Setup(repo => repo.GetAll(
                  It.IsAny<Expression<Func<ShoppingCartItem, ShoppingCartItem>>>(),
                  It.IsAny<Expression<Func<ShoppingCartItem, bool>>>(),
                  It.IsAny<Func<IQueryable<ShoppingCartItem>, IOrderedQueryable<ShoppingCartItem>>>(),
                  It.IsAny<Func<IQueryable<ShoppingCartItem>, IIncludableQueryable<ShoppingCartItem, object>>>()))
              .Returns((Expression<Func<ShoppingCartItem, ShoppingCartItem>> selector,
                        Expression<Func<ShoppingCartItem, bool>>? predicate,
                        Func<IQueryable<ShoppingCartItem>, IOrderedQueryable<ShoppingCartItem>>? orderBy,
                        Func<IQueryable<ShoppingCartItem>, IIncludableQueryable<ShoppingCartItem, object>>? include) =>
          {
              var query = shoppingCartItemList.AsQueryable();
              if (include != null)
              {
                  query = include(query);
              }

              if (predicate != null)
                  query = query.Where(predicate.Compile()).AsQueryable();

              if (orderBy != null)
                  query = orderBy(query);

              return query.Select(selector.Compile()).AsEnumerable();
          });

            _orderService = new OrderService(
              _orderRepoMock.Object,
              _shoppingCartItemRepoMock.Object,
              _shoppingCartServiceMock.Object
          );

            var result = _orderService.CreateOrder(null, Guid.Parse(shoppingCart.OwnerId));

            var sum = shoppingCartItemList[0].Quantity * shoppingCartItemList[0].ArtPiece.Price +
                shoppingCartItemList[1].Quantity * shoppingCartItemList[1].ArtPiece.Price +
                shoppingCartItemList[2].Quantity * shoppingCartItemList[2].ArtPiece.Price;

            Assert.AreEqual(shoppingCartItemList[0].Id, result.PurchasedItems[0].Id);
            Assert.AreEqual(shoppingCartItemList[1].Id, result.PurchasedItems[1].Id);
            Assert.AreEqual(3, result.PurchasedItems.Count);
            Assert.AreEqual(sum, result.TotalPrice);

            _shoppingCartServiceMock.Verify(s => s.GetByUserId(It.IsAny<Guid>()), Times.Once);
            _shoppingCartItemRepoMock.Verify(r => r.GetAll(
                  It.IsAny<Expression<Func<ShoppingCartItem, ShoppingCartItem>>>(),
                  It.IsAny<Expression<Func<ShoppingCartItem, bool>>>(),
                  It.IsAny<Func<IQueryable<ShoppingCartItem>, IOrderedQueryable<ShoppingCartItem>>>(),
                  It.IsAny<Func<IQueryable<ShoppingCartItem>, IIncludableQueryable<ShoppingCartItem, object>>>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void CreateOrder_ShoppingCartNotFoundException()
        {
            _shoppingCartServiceMock.Setup(s => s.GetByUserId(It.IsAny<Guid>())).Returns<ShoppingCart>(null);

            _orderService = new OrderService(
             _orderRepoMock.Object,
             _shoppingCartItemRepoMock.Object,
             _shoppingCartServiceMock.Object
            );

            _orderService.CreateOrder(null, Guid.NewGuid());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void CreateOrder_ShoppingCartItemNotFoundException()
        {
            var artPiece = _fixture.Create<ArtPiece>();
            artPiece.Id = Guid.NewGuid();

            var shoppingCart = _fixture.Create<ShoppingCart>();
            shoppingCart.OwnerId = Guid.NewGuid().ToString();

            _shoppingCartItemRepoMock.Setup(repo => repo.Get(
                  It.IsAny<Expression<Func<ShoppingCartItem, ShoppingCartItem>>>(),
                  It.IsAny<Expression<Func<ShoppingCartItem, bool>>>(),
                  It.IsAny<Func<IQueryable<ShoppingCartItem>, IOrderedQueryable<ShoppingCartItem>>>(),
                  It.IsAny<Func<IQueryable<ShoppingCartItem>, IIncludableQueryable<ShoppingCartItem, object>>>()))
                    .Returns(() => null);

            _shoppingCartServiceMock.Setup(s => s.GetByUserId(It.IsAny<Guid>())).Returns(shoppingCart);

            _orderService = new OrderService(
             _orderRepoMock.Object,
             _shoppingCartItemRepoMock.Object,
             _shoppingCartServiceMock.Object
            );

            _orderService.CreateOrder(artPiece.Id, Guid.Parse(shoppingCart.OwnerId));
        }

        [TestMethod]
        public void PayOrder_Works()
        {
            var order = _fixture.Create<Order>();

            var result = _orderService.PayOrder(order);

            foreach (var item in result.PurchasedItems)
            {
                Assert.IsNull(item.ShoppingCart);
            }

            _orderRepoMock.Verify(r => r.Insert(It.IsAny<Order>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PayOrder_OrderIsNullException()
        {
            _orderService.PayOrder(null);
        }

    }
}
