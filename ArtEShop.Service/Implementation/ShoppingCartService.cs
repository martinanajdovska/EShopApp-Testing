using ArtEShop.Domain.DomainModels;
using ArtEShop.Domain.DTO;
using ArtEShop.Repository.Interface;
using ArtEShop.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtEShop.Service.Implementation
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IRepository<ShoppingCart> _shoppingCartRepository;
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemRepository;
        private readonly IRepository<Order> _orderRepository;

        public ShoppingCartService(IRepository<ShoppingCart> shoppingCartRepository, IRepository<ShoppingCartItem> shoppingCartItemRepository, IRepository<Order> orderRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _shoppingCartItemRepository = shoppingCartItemRepository;
            _orderRepository = orderRepository;
        }

        public void DeleteItemFromShoppingCart(Guid artPieceId, Guid shoppingCartId)
        {
            var shoppingCartItem = _shoppingCartItemRepository.Get(selector: x => x,
                                                                             predicate: x => x.ShoppingCart.Id.Equals(shoppingCartId) && x.ArtPiece.Id.Equals(artPieceId));

            if (shoppingCartItem == null)
            {
                throw new Exception("Product in shopping cart not found");
            }

            _shoppingCartItemRepository.Delete(shoppingCartItem);
        }

        public ShoppingCartDTO GetByUserIdWithIncludedProducts(Guid userId)
        {
            var shoppingCart = GetByUserId(userId);
            if (shoppingCart == null)
            {
                throw new Exception("Shopping cart not found");
            }

            List<ShoppingCartItemDTO> shoppingCartItemsDTO = _shoppingCartItemRepository.GetAll(selector: x => new ShoppingCartItemDTO
            {
                ArtPiece = x.ArtPiece,
                Quantity = x.Quantity,
                TotalPrice = x.Quantity * x.ArtPiece.Price
            },
                predicate: x => x.ShoppingCart.Id.Equals(shoppingCart.Id),
                include: x => x.Include(z => z.ArtPiece)).ToList();

            ShoppingCartDTO shoppingCartDTO = new ShoppingCartDTO();

            if (shoppingCartItemsDTO.IsNullOrEmpty())
            {
                shoppingCartDTO.TotalPrice = 0;
            }
            else
            {
                shoppingCartDTO.TotalPrice = shoppingCartItemsDTO.Sum(x => x.TotalPrice);
            }

            shoppingCartDTO.ShoppingCartItemsDTO = shoppingCartItemsDTO;

            return shoppingCartDTO;
        }

        public ShoppingCart? GetByUserId(Guid userId)
        {
            return _shoppingCartRepository.Get(selector: x => x, predicate: x => x.OwnerId.Equals(userId.ToString()));
        }



        public ShoppingCart Insert()
        {
            return _shoppingCartRepository.Insert(new ShoppingCart());
        }
    }
}
