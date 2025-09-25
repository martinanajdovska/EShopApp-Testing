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
    public class ShoppingCartServiceTests
    {
        private Mock<IRepository<ShoppingCart>> _shoppingCartRepoMock;
        private Mock<IRepository<ShoppingCartItem>> _shoppingCartItemRepoMock;
        private Mock<IRepository<Order>> _orderRepoMock;
        private Fixture _fixture;
        private IShoppingCartService _shoppingCartService;
        private List<ShoppingCart> _shoppingCartList;

        public ShoppingCartServiceTests()
        {
            _fixture = new Fixture();
            _shoppingCartRepoMock = new Mock<IRepository<ShoppingCart>>();
            _shoppingCartItemRepoMock = new Mock<IRepository<ShoppingCartItem>>();
            _orderRepoMock = new Mock<IRepository<Order>>();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _shoppingCartList = _fixture.CreateMany<ShoppingCart>(5).ToList();

            _shoppingCartRepoMock.Setup(repo => repo.Get(
             It.IsAny<Expression<Func<ShoppingCart, ShoppingCart>>>(),
             It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
             It.IsAny<Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>>>(),
             It.IsAny<Func<IQueryable<ShoppingCart>, IIncludableQueryable<ShoppingCart, object>>>()))
         .Returns((Expression<Func<ShoppingCart, ShoppingCart>> selector,
                   Expression<Func<ShoppingCart, bool>>? predicate,
                   Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>>? orderBy,
                   Func<IQueryable<ShoppingCart>, IIncludableQueryable<ShoppingCart, object>>? include) =>
         {
             var query = _shoppingCartList.AsQueryable();
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

            _shoppingCartRepoMock
                .Setup(repo => repo.Insert(It.IsAny<ShoppingCart>()))
                .Returns((ShoppingCart sc) => sc);

            _shoppingCartService = new ShoppingCartService(
                _shoppingCartRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _orderRepoMock.Object
            );
        }

        [TestMethod]
        public void GetByUserId_Works()
        {
            var target = _shoppingCartList[0];
            target.OwnerId = Guid.NewGuid().ToString();

            var result = _shoppingCartService.GetByUserId(Guid.Parse(target.OwnerId));

            Assert.AreEqual(target.OwnerId, result.OwnerId);
            _shoppingCartRepoMock.Verify(r => r.Get(
             It.IsAny<Expression<Func<ShoppingCart, ShoppingCart>>>(),
             It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
             It.IsAny<Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>>>(),
             It.IsAny<Func<IQueryable<ShoppingCart>, IIncludableQueryable<ShoppingCart, object>>>()), Times.Once);
        }

        [TestMethod]
        public void GetByUserId_ReturnsNull()
        {
            _shoppingCartRepoMock.Setup(repo => repo.Get(
             It.IsAny<Expression<Func<ShoppingCart, ShoppingCart>>>(),
             It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
             It.IsAny<Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>>>(),
             It.IsAny<Func<IQueryable<ShoppingCart>, IIncludableQueryable<ShoppingCart, object>>>()))
                .Returns(() => null);

            _shoppingCartService = new ShoppingCartService(
                _shoppingCartRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _orderRepoMock.Object
            );

            var nonExistingId = Guid.NewGuid();

            var result = _shoppingCartService.GetByUserId(nonExistingId);

            Assert.IsNull(result);
            _shoppingCartRepoMock.Verify(r => r.Get(
             It.IsAny<Expression<Func<ShoppingCart, ShoppingCart>>>(),
             It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
             It.IsAny<Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>>>(),
             It.IsAny<Func<IQueryable<ShoppingCart>, IIncludableQueryable<ShoppingCart, object>>>()), Times.Once);
        }

        [TestMethod]
        public void Insert_Works()
        {
            var result = _shoppingCartService.Insert();

            Assert.IsNotNull(result);
            _shoppingCartRepoMock.Verify(r => r.Insert(It.IsAny<ShoppingCart>()), Times.Once);
        }

        [TestMethod]
        public void GetByUserIdWithIncludedProducts_Works()
        {
            var target = _shoppingCartList[0];
            target.OwnerId = Guid.NewGuid().ToString();
            target.Id = Guid.NewGuid();

            var artPiece = _fixture.Create<ArtPiece>();

            var shoppingCartItemList = _fixture.CreateMany<ShoppingCartItem>(3).ToList();
            shoppingCartItemList[0].ArtPiece = artPiece;
            shoppingCartItemList[0].ShoppingCart = target;
            shoppingCartItemList[0].ShoppingCart.Id = target.Id;
            shoppingCartItemList[1].ArtPiece = artPiece;
            shoppingCartItemList[1].ShoppingCart = target;
            shoppingCartItemList[1].ShoppingCart.Id = target.Id;
            shoppingCartItemList[2].ArtPiece = artPiece;
            shoppingCartItemList[2].ShoppingCart = target;
            shoppingCartItemList[2].ShoppingCart.Id = target.Id;

            _shoppingCartItemRepoMock.Setup(repo => repo.GetAll(
              It.IsAny<Expression<Func<ShoppingCartItem, ShoppingCartItemDTO>>>(),
              It.IsAny<Expression<Func<ShoppingCartItem, bool>>>(),
              It.IsAny<Func<IQueryable<ShoppingCartItem>, IOrderedQueryable<ShoppingCartItem>>>(),
              It.IsAny<Func<IQueryable<ShoppingCartItem>, IIncludableQueryable<ShoppingCartItem, object>>>()))
          .Returns((Expression<Func<ShoppingCartItem, ShoppingCartItemDTO>> selector,
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

            _shoppingCartService = new ShoppingCartService(
                _shoppingCartRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _orderRepoMock.Object
            );

            var result = _shoppingCartService.GetByUserIdWithIncludedProducts(Guid.Parse(target.OwnerId));

            Assert.AreEqual(3, result.ShoppingCartItemsDTO.Count);
            _shoppingCartRepoMock.Verify(r => r.Get(
             It.IsAny<Expression<Func<ShoppingCart, ShoppingCart>>>(),
             It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
             It.IsAny<Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>>>(),
             It.IsAny<Func<IQueryable<ShoppingCart>, IIncludableQueryable<ShoppingCart, object>>>()), Times.Once);
            _shoppingCartItemRepoMock.Verify(r => r.GetAll(
              It.IsAny<Expression<Func<ShoppingCartItem, ShoppingCartItemDTO>>>(),
              It.IsAny<Expression<Func<ShoppingCartItem, bool>>>(),
              It.IsAny<Func<IQueryable<ShoppingCartItem>, IOrderedQueryable<ShoppingCartItem>>>(),
              It.IsAny<Func<IQueryable<ShoppingCartItem>, IIncludableQueryable<ShoppingCartItem, object>>>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GetByUserIdWithIncludedProducts_ShoppingCartDoesntExistException()
        {
            var target = _shoppingCartList[0];
            target.OwnerId = Guid.NewGuid().ToString();
            target.Id = Guid.NewGuid();

            _shoppingCartRepoMock.Setup(repo => repo.Get(
                It.IsAny<Expression<Func<ShoppingCart, ShoppingCart>>>(),
                 It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                 It.IsAny<Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>>>(),
                 It.IsAny<Func<IQueryable<ShoppingCart>, IIncludableQueryable<ShoppingCart, object>>>()))
                .Returns(() => null);


            _shoppingCartService = new ShoppingCartService(
                _shoppingCartRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _orderRepoMock.Object
            );

            _shoppingCartService.GetByUserIdWithIncludedProducts(Guid.Parse(target.OwnerId));
        }

        [TestMethod]
        public void GetByUserIdWithIncludedProducts_EmptyShoppingCartWorks()
        {
            var target = _shoppingCartList[0];
            target.OwnerId = Guid.NewGuid().ToString();
            target.Id = Guid.NewGuid();

            var artPiece = _fixture.Create<ArtPiece>();

            var shoppingCartItemList = _fixture.CreateMany<ShoppingCartItem>(3).ToList();

            _shoppingCartItemRepoMock.Setup(repo => repo.GetAll(
              It.IsAny<Expression<Func<ShoppingCartItem, ShoppingCartItemDTO>>>(),
              It.IsAny<Expression<Func<ShoppingCartItem, bool>>>(),
              It.IsAny<Func<IQueryable<ShoppingCartItem>, IOrderedQueryable<ShoppingCartItem>>>(),
              It.IsAny<Func<IQueryable<ShoppingCartItem>, IIncludableQueryable<ShoppingCartItem, object>>>()))
                .Returns(() => Enumerable.Empty<ShoppingCartItemDTO>());


            _shoppingCartService = new ShoppingCartService(
                _shoppingCartRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _orderRepoMock.Object
            );

            var result = _shoppingCartService.GetByUserIdWithIncludedProducts(Guid.Parse(target.OwnerId));

            Assert.AreEqual(0, result.TotalPrice);
            _shoppingCartRepoMock.Verify(r => r.Get(
            It.IsAny<Expression<Func<ShoppingCart, ShoppingCart>>>(),
            It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
            It.IsAny<Func<IQueryable<ShoppingCart>, IOrderedQueryable<ShoppingCart>>>(),
            It.IsAny<Func<IQueryable<ShoppingCart>, IIncludableQueryable<ShoppingCart, object>>>()), Times.Once);
            _shoppingCartItemRepoMock.Verify(r => r.GetAll(
              It.IsAny<Expression<Func<ShoppingCartItem, ShoppingCartItemDTO>>>(),
              It.IsAny<Expression<Func<ShoppingCartItem, bool>>>(),
              It.IsAny<Func<IQueryable<ShoppingCartItem>, IOrderedQueryable<ShoppingCartItem>>>(),
              It.IsAny<Func<IQueryable<ShoppingCartItem>, IIncludableQueryable<ShoppingCartItem, object>>>()), Times.Once);
        }

    }
}
