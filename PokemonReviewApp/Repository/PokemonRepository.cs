using Microsoft.EntityFrameworkCore;
using PokemonReviewApp.Data;
using PokemonReviewApp.Interfaces;
using PokemonReviewApp.Models;

namespace PokemonReviewApp.Repository
{
    public class PokemonRepository : IPokemonRepository
    {
        private readonly DataContext _context;

        public PokemonRepository(DataContext context)
        {
            _context = context;
        }

        public bool CreatePokemon(int ownerId, int categoryId, Pokemon pokemon)
        {
            var pokemonOwnerEntity = _context.Owners.Where(a => a.Id == ownerId).FirstOrDefault();
            var category = _context.Categories.Where(a => a.Id == categoryId).FirstOrDefault();

            var pokemonOwner = new PokemonOwner()
            {
                Owner = pokemonOwnerEntity,
                Pokemon = pokemon
            };
            _context.Add(pokemonOwner);

            var pokemonCategory = new PokemonCategory()
            {
                Category = category,
                Pokemon = pokemon
            };
            _context.Add(pokemonCategory);

            _context.Add(pokemon);

            return Save();
        }

        public bool DeletePokemon(Pokemon pokemon)
        {
            _context.Remove(pokemon);
            return Save();
        }

        public Pokemon GetPokemon(int id)
        {
            return _context.Pokemon.Where(p => p.Id == id).FirstOrDefault();
        }

        public Pokemon GetPokemon(string name)
        {
            return _context.Pokemon.Where(p => p.Name == name).FirstOrDefault();
        }

        public decimal GetPokemonRating(int pokeId)
        {
            var review = _context.Reviews.Where(p => p.Pokemon.Id == pokeId);

            if(review.Count() <= 0) 
                return 0;

            return ((decimal)review.Sum(r =>  r.Rating) / review.Count());
        }

        public ICollection<Pokemon> GetPokemons()
        {
            return _context.Pokemon.OrderBy(p => p.Id).ToList();
        }

        public bool PokemonExists(int pokeId)
        {
            return _context.Pokemon.Any(p => p.Id == pokeId);
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

        public bool UpdatePokemon(int ownerId, int categoryId, Pokemon pokemon)
        {
            // Retrieve the existing pokemon from the database
            var existingPokemon = _context.Pokemon
                .Include(o => o.PokemonOwners)
                .Include(c => c.PokemonCategories)
                .FirstOrDefault();

            // Update scalar properties of the pokemon
            existingPokemon.Name = pokemon.Name;
            existingPokemon.BirthDate = pokemon.BirthDate;

            // Update the Owners (many-to-many relationship)
            if (ownerId != 0)
            {
                existingPokemon.PokemonOwners.Clear();
                var pokemonOwner = new PokemonOwner()
                {
                    Owner = _context.Owners.Where(a => a.Id == ownerId).FirstOrDefault(),
                    Pokemon = pokemon
                };
                existingPokemon.PokemonOwners.Add(pokemonOwner);
            }

            // Update the Category (many-to-many relationship)
            if (categoryId != 0)
            {
                existingPokemon.PokemonCategories.Clear();
                var pokemonCategory = new PokemonCategory()
                {
                    Category = _context.Categories.Where(a => a.Id == categoryId).FirstOrDefault(),
                    Pokemon = pokemon
                };
                existingPokemon.PokemonCategories.Add(pokemonCategory);
            }

            _context.Update(existingPokemon);
            return Save();
        }
    }
}
