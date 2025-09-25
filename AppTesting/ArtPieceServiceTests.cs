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
    public class ArtPieceServiceTests
    {
        private Mock<IRepository<ArtPiece>> _artPieceRepoMock;
        private Mock<IRepository<ShoppingCartItem>> _shoppingCartItemRepoMock;
        private Mock<IShoppingCartService> _shoppingCartServiceMock;
        private Fixture _fixture;
        private IArtPieceService _artPieceService;
        private List<ArtPiece> _artPieceList;
        private ArtPiece? _capturedArtPieceUpdate;

        public ArtPieceServiceTests()
        {
            _fixture = new Fixture();
            _artPieceRepoMock = new Mock<IRepository<ArtPiece>>();
            _shoppingCartItemRepoMock = new Mock<IRepository<ShoppingCartItem>>();
            _shoppingCartServiceMock = new Mock<IShoppingCartService>();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _artPieceList = _fixture.CreateMany<ArtPiece>(5).ToList();

            _artPieceRepoMock.Setup(repo => repo.Get(
             It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
             It.IsAny<Expression<Func<ArtPiece, bool>>>(),
             It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
             It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()))
         .Returns((Expression<Func<ArtPiece, ArtPiece>> selector,
                   Expression<Func<ArtPiece, bool>>? predicate,
                   Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>? orderBy,
                   Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>? include) =>
         {
             var query = _artPieceList.AsQueryable();
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

            _artPieceRepoMock.Setup(repo => repo.GetAll(
                    It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                    It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                    It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()))
                .Returns((Expression<Func<ArtPiece, ArtPiece>> selector,
                          Expression<Func<ArtPiece, bool>>? predicate,
                          Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>? orderBy,
                          Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>? include) =>
                {
                    var query = _artPieceList.AsQueryable();

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

            _artPieceRepoMock
                .Setup(repo => repo.Insert(It.IsAny<ArtPiece>()))
                .Returns((ArtPiece ap) => ap);


            _artPieceRepoMock
                .Setup(repo => repo.Update(It.IsAny<ArtPiece>()))
                .Callback<ArtPiece>(ap => _capturedArtPieceUpdate = ap)
                .Returns((ArtPiece ap) => ap);

            _artPieceRepoMock
               .Setup(repo => repo.Delete(It.IsAny<ArtPiece>()))
               .Returns((ArtPiece ap) => ap);

            _artPieceService = new ArtPieceService(
                _artPieceRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _shoppingCartServiceMock.Object
            );
        }

        [TestMethod]
        public void GetAll_Works()
        {
            var result = _artPieceService.GetAll();

            Assert.AreEqual(5, result.Count);
            _artPieceRepoMock.Verify(r => r.GetAll(
                    It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                    It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                    It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                    It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()), Times.Once);
        }


        [TestMethod]
        public void GetAll_ReturnsEmptyList()
        {
            _artPieceRepoMock.Setup(repo => repo.GetAll(
                It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()))
                .Returns(Enumerable.Empty<ArtPiece>());

            _artPieceService = new ArtPieceService(
                _artPieceRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _shoppingCartServiceMock.Object
            );

            var result = _artPieceService.GetAll();

            Assert.AreEqual(0, result.Count);
            _artPieceRepoMock.Verify(r => r.GetAll(
                   It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                   It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()), Times.Once);
        }

        [TestMethod]
        public void GetAllByName_Works()
        {
            _artPieceList[0].Name = "name";
            _artPieceList[1].Name = "name";

            var result = _artPieceService.GetAllByName("name");

            Assert.AreEqual(2, result.Count);
            _artPieceRepoMock.Verify(r => r.GetAll(
                   It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                   It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()), Times.Once);
        }

        [TestMethod]
        public void GetById_Works()
        {
            var target = _artPieceList[0];

            var result = _artPieceService.GetById(target.Id);

            Assert.AreEqual(target.Id, result.Id);
            _artPieceRepoMock.Verify(r => r.Get(
                   It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                   It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()), Times.Once);
        }

        [TestMethod]
        public void GetById_ReturnsNull()
        {
            _artPieceRepoMock.Setup(repo => repo.Get(
                It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()))
                .Returns(() => null);

            _artPieceService = new ArtPieceService(
                _artPieceRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _shoppingCartServiceMock.Object
            );

            var nonExistingId = Guid.NewGuid();

            var result = _artPieceService.GetById(nonExistingId);

            Assert.IsNull(result);
            _artPieceRepoMock.Verify(r => r.Get(
                  It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                  It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                  It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                  It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()), Times.Once);
        }

        [TestMethod]
        public void Insert_Works()
        {
            var mockFormFile = new Mock<IFormFile>();
            mockFormFile.Setup(f => f.FileName).Returns("test.png");

            var artPieceDTO = new ArtPieceDTO()
            {
                Name = "ArtName",
                Price = 200,
                ImageFile = mockFormFile.Object
            };

            string fileName = "test.png";

            var result = _artPieceService.Insert(artPieceDTO, fileName);

            Assert.AreEqual(artPieceDTO.Name, result.Name);
            Assert.AreEqual(artPieceDTO.Price, result.Price);
            Assert.AreEqual(fileName, result.Image);
            Assert.IsTrue(result.IsAvailable);
            _artPieceRepoMock.Verify(r => r.Insert(It.IsAny<ArtPiece>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Insert_ThrowsExceptionWhenDtoIsNull()
        {
            _artPieceService.Insert(null, "filename.png");
        }

        [TestMethod]
        public void Update_Works()
        {
            var target = _artPieceList[0];
            var updatedArtPiece = new ArtPiece()
            {
                Name = target.Name + "aaaa",
                Price = target.Price + 100,
                Image = target.Image,
                Id = target.Id,
                IsAvailable = target.IsAvailable
            };

            var result = _artPieceService.Update(updatedArtPiece);

            Assert.AreEqual(updatedArtPiece.Id, result.Id);
            Assert.AreEqual(updatedArtPiece.Name, result.Name);
            Assert.AreEqual(updatedArtPiece.Price, result.Price);
            Assert.AreEqual(updatedArtPiece.IsAvailable, result.IsAvailable);
            Assert.AreEqual(updatedArtPiece.Image, result.Image);
            _artPieceRepoMock.Verify(r => r.Get(
                   It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                   It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()), Times.Once);
            _artPieceRepoMock.Verify(r => r.Update(It.IsAny<ArtPiece>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Update_ThrowsExceptionWhenArtPieceDoesNotExist()
        {
            var artPiece = _fixture.Create<ArtPiece>();

            _artPieceRepoMock.Setup(repo => repo.Get(
                It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()))
                .Returns(() => null);

            _artPieceService = new ArtPieceService(
                _artPieceRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _shoppingCartServiceMock.Object
            );

            _artPieceService.Update(artPiece);
        }

        [TestMethod]
        public void ChangeAvailable_Works()
        {
            var artPiece = _fixture.Create<ArtPiece>();
            artPiece.Id = Guid.NewGuid();
            artPiece.IsAvailable = true;

            _artPieceService.ChangeAvailable(artPiece);

            Assert.IsFalse(_capturedArtPieceUpdate.IsAvailable);
            _artPieceRepoMock.Verify(r => r.Update(It.IsAny<ArtPiece>()), Times.Once);
        }


        [TestMethod]
        public void DeleteById_Works()
        {
            var target = _artPieceList[0];

            var result = _artPieceService.DeleteById(target.Id);

            Assert.AreEqual(result.Id, target.Id);
            _artPieceRepoMock.Verify(r => r.Get(
                   It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                   It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()), Times.Once);
            _artPieceRepoMock.Verify(r => r.Delete(It.IsAny<ArtPiece>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]

        public void DeleteById_ThrowsException()
        {
            var artPiece = _fixture.Create<ArtPiece>();

            _artPieceRepoMock.Setup(repo => repo.Get(
                It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()))
                .Returns(() => null);

            _artPieceService = new ArtPieceService(
                _artPieceRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _shoppingCartServiceMock.Object
            );

            _artPieceService.DeleteById(artPiece.Id);
        }

        [TestMethod]
        public void AddProductToShoppingCart_WorksWithNewItem()
        {
            var artPiece = _artPieceList[0];
            var shoppingCart = _fixture.Create<ShoppingCart>();
            shoppingCart.OwnerId = Guid.NewGuid().ToString();

            _shoppingCartServiceMock.Setup(s => s.GetByUserId(Guid.Parse(shoppingCart.OwnerId))).Returns(shoppingCart);

            _shoppingCartItemRepoMock.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCartItem, ShoppingCartItem>>>(),
                It.IsAny<Expression<Func<ShoppingCartItem, bool>>>(),
                null, null)).Returns<ShoppingCartItem>(null);

            ShoppingCartItem? insertedItem = null;

            _shoppingCartItemRepoMock
                .Setup(repo => repo.Insert(It.IsAny<ShoppingCartItem>()))
                .Callback<ShoppingCartItem>(ap => insertedItem = ap)
                .Returns((ShoppingCartItem ap) => ap);

            _artPieceService = new ArtPieceService(
                _artPieceRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _shoppingCartServiceMock.Object
            );

            _artPieceService.AddProductToShoppingCart(artPiece.Id, Guid.Parse(shoppingCart.OwnerId), 1);

            Assert.IsNotNull(insertedItem);
            Assert.AreEqual(artPiece.Id, insertedItem.ArtPiece.Id);
            Assert.AreEqual(shoppingCart.Id, insertedItem.ShoppingCart.Id);
            Assert.AreEqual(1, insertedItem.Quantity);

            _artPieceRepoMock.Verify(r => r.Get(
                   It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                   It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()), Times.Once);
            _shoppingCartServiceMock.Verify(s => s.GetByUserId(It.IsAny<Guid>()), Times.Once);
            _shoppingCartItemRepoMock.Verify(r => r.Insert(It.IsAny<ShoppingCartItem>()), Times.Once);
            _shoppingCartItemRepoMock.Verify(r => r.Update(It.IsAny<ShoppingCartItem>()), Times.Never);
        }

        [TestMethod]
        public void AddProductToShoppingCart_WorksWithExistingItem()
        {
            var artPiece = _artPieceList[0];
            var shoppingCart = _fixture.Create<ShoppingCart>();
            shoppingCart.OwnerId = Guid.NewGuid().ToString();

            var shoppingCartItemList = _fixture.CreateMany<ShoppingCartItem>(5).ToList();
            var shoppingCartItem = shoppingCartItemList[0];
            shoppingCartItem.ArtPiece = artPiece;
            shoppingCartItem.ShoppingCart = shoppingCart;

            _shoppingCartServiceMock.Setup(s => s.GetByUserId(Guid.Parse(shoppingCart.OwnerId))).Returns(shoppingCart);

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

            ShoppingCartItem? insertedItem = null;

            _shoppingCartItemRepoMock
                .Setup(repo => repo.Update(It.IsAny<ShoppingCartItem>()))
                .Callback<ShoppingCartItem>(ap => insertedItem = ap)
                .Returns((ShoppingCartItem ap) => ap);

            _artPieceService = new ArtPieceService(
                _artPieceRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _shoppingCartServiceMock.Object
            );


            int? quantity = shoppingCartItem.Quantity;
            _artPieceService.AddProductToShoppingCart(artPiece.Id, Guid.Parse(shoppingCart.OwnerId), 1);

            Assert.IsNotNull(insertedItem);
            Assert.AreEqual(artPiece.Id, insertedItem.ArtPiece.Id);
            Assert.AreEqual(shoppingCart.Id, insertedItem.ShoppingCart.Id);
            Assert.AreEqual(quantity + 1, insertedItem.Quantity);
            _artPieceRepoMock.Verify(r => r.Get(
                   It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                   It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                   It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()), Times.Once);
            _shoppingCartServiceMock.Verify(s => s.GetByUserId(It.IsAny<Guid>()), Times.Once);
            _shoppingCartItemRepoMock.Verify(r => r.Insert(It.IsAny<ShoppingCartItem>()), Times.Never);
            _shoppingCartItemRepoMock.Verify(r => r.Update(It.IsAny<ShoppingCartItem>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void AddProductToShoppingCart_ArtPieceNotFoundException()
        {
            var artPiece = _fixture.Create<ArtPiece>();
            var shoppingCart = _fixture.Create<ShoppingCart>();
            shoppingCart.OwnerId = Guid.NewGuid().ToString();


            _artPieceRepoMock.Setup(repo => repo.Get(
                It.IsAny<Expression<Func<ArtPiece, ArtPiece>>>(),
                It.IsAny<Expression<Func<ArtPiece, bool>>>(),
                It.IsAny<Func<IQueryable<ArtPiece>, IOrderedQueryable<ArtPiece>>>(),
                It.IsAny<Func<IQueryable<ArtPiece>, IIncludableQueryable<ArtPiece, object>>>()))
                .Returns(() => null);

            _artPieceService = new ArtPieceService(
                _artPieceRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _shoppingCartServiceMock.Object
            );

            _artPieceService.AddProductToShoppingCart(artPiece.Id, Guid.Parse(shoppingCart.OwnerId), 1);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void AddProductToShoppingCart_ShoppingCartNotFoundException()
        {
            var artPieceList = _fixture.CreateMany<ArtPiece>(5).ToList();
            var artPiece = artPieceList[0];
            var shoppingCart = _fixture.Create<ShoppingCart>();
            shoppingCart.OwnerId = Guid.NewGuid().ToString();

            _shoppingCartServiceMock.Setup(s => s.GetByUserId(Guid.Parse(shoppingCart.OwnerId))).Returns<ShoppingCart>(null);

            _artPieceService = new ArtPieceService(
                _artPieceRepoMock.Object,
                _shoppingCartItemRepoMock.Object,
                _shoppingCartServiceMock.Object
            );

            _artPieceService.AddProductToShoppingCart(artPiece.Id, Guid.Parse(shoppingCart.OwnerId), 1);
        }
    }
}
