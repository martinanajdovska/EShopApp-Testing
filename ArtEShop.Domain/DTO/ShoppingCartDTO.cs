﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtEShop.Domain.DTO
{
    public class ShoppingCartDTO
    {
        public List<ShoppingCartItemDTO>? ShoppingCartItemsDTO { get; set; }
        public int? TotalPrice { get; set; }
    }
}
