using ArtEShop.Domain.DomainModels;
using ArtEShop.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtEShop.Service.Interface
{
    public interface IRequestService
    {
        List<Request>? GetAllByEmail(string email);
        Request? GetById(Guid id);
        Request UpdateFromDTO(RequestDTO request, bool artistUpdated);
        List<Request> GetAll();
        RequestDTO EntityToDTO(Request request);

        Request RequestPiece(CreateRequestDTO model, string fileName);
    }
}
