using ArtEShop.Domain.DomainModels;
using ArtEShop.Domain.DTO;
using ArtEShop.Repository.Interface;
using ArtEShop.Service.Implementation;
using ArtEShop.Service.Interface;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualStudio.TestPlatform.Common;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
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
    public class RequestServiceTests
    {
        private Mock<IRepository<Request>> _requestRepoMock;
        private Fixture _fixture;
        private IRequestService _requestService;
        private List<Request> _requestList;

        public RequestServiceTests()
        {
            _fixture = new Fixture();
            _requestRepoMock = new Mock<IRepository<Request>>();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _requestList = _fixture.CreateMany<Request>(5).ToList();

            _requestRepoMock.Setup(repo => repo.Get(
            It.IsAny<Expression<Func<Request, Request>>>(),
                   It.IsAny<Expression<Func<Request, bool>>>(),
                   It.IsAny<Func<IQueryable<Request>, IOrderedQueryable<Request>>>(),
                   It.IsAny<Func<IQueryable<Request>, IIncludableQueryable<Request, object>>>()))
               .Returns((Expression<Func<Request, Request>> selector,
                         Expression<Func<Request, bool>>? predicate,
                         Func<IQueryable<Request>, IOrderedQueryable<Request>>? orderBy,
                         Func<IQueryable<Request>, IIncludableQueryable<Request, object>>? include) =>
               {
                   var query = _requestList.AsQueryable();
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

            _requestRepoMock.Setup(repo => repo.GetAll(
                    It.IsAny<Expression<Func<Request, Request>>>(),
                    It.IsAny<Expression<Func<Request, bool>>>(),
                    It.IsAny<Func<IQueryable<Request>, IOrderedQueryable<Request>>>(),
                    It.IsAny<Func<IQueryable<Request>, IIncludableQueryable<Request, object>>>()))
                .Returns((Expression<Func<Request, Request>> selector,
                          Expression<Func<Request, bool>>? predicate,
                          Func<IQueryable<Request>, IOrderedQueryable<Request>>? orderBy,
                          Func<IQueryable<Request>, IIncludableQueryable<Request, object>>? include) =>
                {
                    var query = _requestList.AsQueryable();

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

            _requestRepoMock
                .Setup(repo => repo.Insert(It.IsAny<Request>()))
                .Returns((Request req) => req);

            _requestRepoMock
                .Setup(repo => repo.Update(It.IsAny<Request>()))
                .Returns((Request req) => req);

            _requestService = new RequestService(
                _requestRepoMock.Object
            );
        }

        [TestMethod]
        public void GetAllByEmail_Works()
        {
            _requestList[0].Email = "email@email.com";
            _requestList[1].Email = "temp@email.com";

            var result = _requestService.GetAllByEmail("email@email.com");

            Assert.AreEqual(1, result.Count);
            _requestRepoMock.Verify(r => r.GetAll(
                    It.IsAny<Expression<Func<Request, Request>>>(),
                    It.IsAny<Expression<Func<Request, bool>>>(),
                    It.IsAny<Func<IQueryable<Request>, IOrderedQueryable<Request>>>(),
                    It.IsAny<Func<IQueryable<Request>, IIncludableQueryable<Request, object>>>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetAllByEmail_ThrowsExceptionWhenEmailIsNull()
        {
            _requestService.GetAllByEmail(null);
        }

        [TestMethod]
        public void GetById_Works()
        {
            var target = _requestList[0];

            var result = _requestService.GetById(target.Id);

            Assert.AreEqual(target.Id, result.Id);
            _requestRepoMock.Verify(r => r.Get(
             It.IsAny<Expression<Func<Request, Request>>>(),
                    It.IsAny<Expression<Func<Request, bool>>>(),
                    It.IsAny<Func<IQueryable<Request>, IOrderedQueryable<Request>>>(),
                    It.IsAny<Func<IQueryable<Request>, IIncludableQueryable<Request, object>>>()), Times.Once);
        }

        [TestMethod]
        public void GetById_ReturnsNull()
        {
            _requestRepoMock.Setup(repo => repo.Get(
             It.IsAny<Expression<Func<Request, Request>>>(),
                    It.IsAny<Expression<Func<Request, bool>>>(),
                    It.IsAny<Func<IQueryable<Request>, IOrderedQueryable<Request>>>(),
                    It.IsAny<Func<IQueryable<Request>, IIncludableQueryable<Request, object>>>()))
                .Returns(() => null);

            _requestService = new RequestService(
                  _requestRepoMock.Object
              );

            var nonExistingId = Guid.NewGuid();

            var result = _requestService.GetById(nonExistingId);

            Assert.IsNull(result);
            _requestRepoMock.Verify(r => r.Get(
             It.IsAny<Expression<Func<Request, Request>>>(),
                    It.IsAny<Expression<Func<Request, bool>>>(),
                    It.IsAny<Func<IQueryable<Request>, IOrderedQueryable<Request>>>(),
                    It.IsAny<Func<IQueryable<Request>, IIncludableQueryable<Request, object>>>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetById_ThrowsExceptionWhenIdIsNullOrEmpty()
        {
            _requestService.GetById(Guid.Empty);
        }

        [TestMethod]
        public void GetAll_Works()
        {
            var result = _requestService.GetAll();

            Assert.AreEqual(5, result.Count);
            _requestRepoMock.Verify(r => r.GetAll(
             It.IsAny<Expression<Func<Request, Request>>>(),
                    It.IsAny<Expression<Func<Request, bool>>>(),
                    It.IsAny<Func<IQueryable<Request>, IOrderedQueryable<Request>>>(),
                    It.IsAny<Func<IQueryable<Request>, IIncludableQueryable<Request, object>>>()), Times.Once);
        }


        [TestMethod]
        public void GetAll_ReturnsEmptyList()
        {
            _requestRepoMock.Setup(repo => repo.GetAll(
            It.IsAny<Expression<Func<Request, Request>>>(),
                   It.IsAny<Expression<Func<Request, bool>>>(),
                   It.IsAny<Func<IQueryable<Request>, IOrderedQueryable<Request>>>(),
                   It.IsAny<Func<IQueryable<Request>, IIncludableQueryable<Request, object>>>()))
               .Returns(Enumerable.Empty<Request>());

            _requestService = new RequestService(
               _requestRepoMock.Object
           );

            var result = _requestService.GetAll();

            Assert.AreEqual(0, result.Count);
            _requestRepoMock.Verify(r => r.GetAll(
            It.IsAny<Expression<Func<Request, Request>>>(),
                   It.IsAny<Expression<Func<Request, bool>>>(),
                   It.IsAny<Func<IQueryable<Request>, IOrderedQueryable<Request>>>(),
                   It.IsAny<Func<IQueryable<Request>, IIncludableQueryable<Request, object>>>()), Times.Once);
        }

        [TestMethod]
        public void RequestPiece_Works()
        {
            var mockFormFile = new Mock<IFormFile>();
            mockFormFile.Setup(f => f.FileName).Returns("test.png");

            var createRequestDTO = new CreateRequestDTO()
            {
                Subject = "subject",
                Description = "description",
                ReferenceImageFile = mockFormFile.Object,
                Email = "email@email.com"
            };

            var result = _requestService.RequestPiece(createRequestDTO, "test.png");

            Assert.AreEqual(result.Subject, createRequestDTO.Subject);
            Assert.AreEqual(result.Description, createRequestDTO.Description);
            Assert.AreEqual(result.Email, createRequestDTO.Email);
            Assert.AreEqual(result.ReferenceImage, "test.png");

            _requestRepoMock.Verify(r => r.Insert(It.IsAny<Request>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RequestPiece_ThrowsExceptionWhenModelIsNull()
        {
            _requestService.RequestPiece(null, "test.png");
        }

        [TestMethod]
        public void UpdateFromDTO_WorksArtistUpdatedTrue()
        {
            var requestDTO = _fixture.Create<RequestDTO>();
            var target = _requestList[0];
            target.Id = Guid.NewGuid();
            requestDTO.RequestId = target.Id;
            var artistUpdated = true;

            var result = _requestService.UpdateFromDTO(requestDTO, artistUpdated);

            Assert.AreEqual(result.ArtistNotes, requestDTO.ArtistNotes);
            Assert.AreEqual(result.Price, requestDTO.Price);
            Assert.IsNotNull(result.Subject);
            Assert.IsNotNull(result.Description);

            _requestRepoMock.Verify(r => r.Update(It.IsAny<Request>()), Times.Once);
        }

        [TestMethod]
        public void UpdateFromDTO_WorksArtistUpdatedFalse()
        {
            var requestDTO = _fixture.Create<RequestDTO>();
            var target = _requestList[0];
            target.Id = Guid.NewGuid();
            requestDTO.RequestId = target.Id;
            var artistUpdated = false;

            var result = _requestService.UpdateFromDTO(requestDTO, artistUpdated);

            Assert.IsNotNull(result.ArtistNotes);
            Assert.IsNotNull(result.Price);
            Assert.AreEqual(result.Subject, requestDTO.Subject);
            Assert.AreEqual(result.Description, requestDTO.Description);

            _requestRepoMock.Verify(r => r.Update(It.IsAny<Request>()), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateFromDTO_ThrowsExceptionWhenModelIsNull()
        {
            _requestService.UpdateFromDTO(null, true);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void UpdateFromDTO_ThrowsExceptionWhenRequestNotFound()
        {
            var requestDTO = _fixture.Create<RequestDTO>();
            requestDTO.RequestId = Guid.NewGuid();

            _requestRepoMock.Setup(repo => repo.Get(
             It.IsAny<Expression<Func<Request, Request>>>(),
                    It.IsAny<Expression<Func<Request, bool>>>(),
                    It.IsAny<Func<IQueryable<Request>, IOrderedQueryable<Request>>>(),
                    It.IsAny<Func<IQueryable<Request>, IIncludableQueryable<Request, object>>>()))
                .Returns(() => null);

            _requestService = new RequestService(
              _requestRepoMock.Object
          );

            _requestService.UpdateFromDTO(requestDTO, true);
        }

        [TestMethod]
        public void EntityToDTO_Works()
        {
            var request = _fixture.Create<Request>();
            request.Id = Guid.NewGuid();

            var result = _requestService.EntityToDTO(request);

            Assert.AreEqual(request.Id, result.RequestId);
            Assert.AreEqual(request.Subject, result.Subject);
            Assert.AreEqual(request.Description, result.Description);
            Assert.AreEqual(request.Price, result.Price);
            Assert.AreEqual(request.ReferenceImage, result.ReferenceImage);
            Assert.AreEqual(request.ArtistNotes, result.ArtistNotes);
            Assert.AreEqual(request.IsAnswered, result.IsAnswered);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EntityToDTO_ThrowsExceptionWhenRequestIsNull()
        {
            _requestService.EntityToDTO(null);
        }
    }
}
