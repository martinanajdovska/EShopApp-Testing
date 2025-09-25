using ArtEShop.Domain.DomainModels;
using ArtEShop.Domain.DTO;
using ArtEShop.Repository.Interface;
using ArtEShop.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtEShop.Service.Implementation
{
    public class RequestService : IRequestService
    {
        private readonly IRepository<Request> _requestRepository;

        public RequestService(IRepository<Request> requestRepository)
        {
            _requestRepository = requestRepository;
        }

        public List<Request>? GetAllByEmail(string email)
        {
            if (email == null) throw new ArgumentNullException();
            return _requestRepository.GetAll(selector: x => x, predicate: x => x.Email.Equals(email)).ToList();
        }

        public Request? GetById(Guid id)
        {
            if (id == null || id == Guid.Empty) throw new ArgumentNullException();
            return _requestRepository.Get(selector: x => x,
                                          predicate: x => x.Id.Equals(id));
        }

        public List<Request> GetAll()
        {
            return _requestRepository.GetAll(selector: x => x).ToList();
        }
        public Request RequestPiece(CreateRequestDTO model, string fileName)
        {
            if (model == null) throw new ArgumentNullException();

            Request request = new Request
            {
                Id = Guid.NewGuid(),
                Description = model.Description,
                ReferenceImage = fileName,
                Price = null,
                ArtistNotes = null,
                IsAnswered = false,
                Email = model.Email,
                Subject = model.Subject,
            };

            _requestRepository.Insert(request);

            return request;
        }

        public Request UpdateFromDTO(RequestDTO requestDTO, bool artistUpdated)
        {
            if (requestDTO == null) throw new ArgumentNullException();

            Request request = GetById(requestDTO.RequestId);
            if (request == null) throw new Exception("Id not found");

            request.IsAnswered = artistUpdated;

            if (artistUpdated)
            {
                request.ArtistNotes = requestDTO.ArtistNotes;
                request.Price = requestDTO.Price;
            }
            else
            {
                request.Subject = requestDTO.Subject;
                request.Description = requestDTO.Description;
            }
            _requestRepository.Update(request);

            return request;
        }

        public RequestDTO EntityToDTO(Request request)
        {
            if (request == null) throw new ArgumentNullException();

            RequestDTO model = new RequestDTO
            {
                RequestId = request.Id,
                Subject = request.Subject,
                Description = request.Description,
                Price = request.Price,
                ReferenceImage = request.ReferenceImage,
                ArtistNotes = request.ArtistNotes,
                IsAnswered = request.IsAnswered
            };

            return model;
        }

    }
}
