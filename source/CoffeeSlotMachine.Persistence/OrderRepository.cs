using System;
using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CoffeeSlotMachine.Persistence
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public OrderRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<Order> GetAllWithProduct()
        {
            return _dbContext.Orders
                 .Include(p => p.Product);
        }

        public Order AddOrder(Product p)
        {
            Order o = new Order
            {
                Product = p
            };
            _dbContext.Orders
                .Add(o);
            _dbContext.SaveChanges();
            return o;
        }


    }
}
