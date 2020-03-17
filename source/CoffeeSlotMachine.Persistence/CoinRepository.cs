using CoffeeSlotMachine.Core.Contracts;
using CoffeeSlotMachine.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeSlotMachine.Persistence
{
    public class CoinRepository : ICoinRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CoinRepository(ApplicationDbContext dbContext)
        { 
            _dbContext = dbContext;
        }

        public IEnumerable<Coin> GetAll()
        {
            return _dbContext.Coins;
        }
        public void AddCoins(Coin[] coins)
        {
            throw new System.NotImplementedException();
        }

     

        public void RemoveCoins(Coin[] coins)
        {
            throw new System.NotImplementedException();
        }
    }
}
