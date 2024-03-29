﻿using Amazon.SecurityToken.Model;
using CloudinaryDotNet.Actions;
using CulinaryRecipes.API.Models;
using CulinaryRecipes.API.Services.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Services
{
    public class RecipesService : IRecipesService
    {
        private readonly IMongoCollection<Recipes> _recipesCollection;

        public RecipesService(IOptions<CulinaryRecipesDatabaseSettings> options)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(options.Value.DatabaseName);


            _recipesCollection = mongoDatabase.GetCollection<Recipes>(
            options.Value.RecipesCollectionName);
        }

        public async Task<List<Recipes>> GetAsync(string[]? tags = null, string? category = null, bool? publishedOnly = false)
        {
            var filterList = new List<FilterDefinition<Recipes>>();

            if(tags != null && tags.Length > 0)
            {
                filterList.Add(Builders<Recipes>.Filter.All(x => x.tags, tags));
            }

            if (!string.IsNullOrEmpty(category))
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.category, category));
            }

            if(publishedOnly ?? false)
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.published, true));
            }
        
            filterList.Add(Builders<Recipes>.Filter.Eq(x => x.isActive, true));


            var filter = Builders<Recipes>.Filter.And(filterList);

            return await _recipesCollection.Find(filter).ToListAsync();
        }
              

        public async Task<Recipes?> GetAsync(string id) =>
            await _recipesCollection.Find(x => x.id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(Recipes newRecipes, ImageUploadResult imageUploadResult) 
        {
            newRecipes.createdAt = DateTime.UtcNow;
            newRecipes.createdBy = "TODO: Admin development";
            newRecipes.isActive = true;

            if (imageUploadResult?.SecureUrl?.AbsoluteUri != null)
            {
                newRecipes.photo = new Photo
                {
                    url = imageUploadResult.SecureUrl.AbsoluteUri,
                    publicId = imageUploadResult.PublicId,
                    mainColor = imageUploadResult.Colors[0][0]
                };
            }
            else
            {
                newRecipes.photo = new Photo
                {
                    url = "",
                    publicId = "",
                    mainColor = ""
                };
            }

            await _recipesCollection.InsertOneAsync(newRecipes); 
        }

        public async Task UpdateAsync(string id, Recipes updatedRecipes, ImageUploadResult imageUploadResult)
        {
            updatedRecipes.updatedAt = DateTime.UtcNow;
            updatedRecipes.updatedBy = "TODO: Admin development";
            updatedRecipes.isActive = true;

            if (imageUploadResult?.SecureUrl?.AbsoluteUri != null)
            {
                updatedRecipes.photo = new Photo
                {
                    url = imageUploadResult.SecureUrl.AbsoluteUri,
                    publicId = imageUploadResult.PublicId,
                    mainColor = imageUploadResult.Colors[0][0]
                };
            }
        
            await _recipesCollection.ReplaceOneAsync(x => x.id == id, updatedRecipes);
        }

        public async Task RemoveAsync(string id, Recipes recipe )
        {
            recipe.isActive = false;
            await _recipesCollection.ReplaceOneAsync(x => x.id == id, recipe);
        }

        public List<string> GetCategories(string searchText)
        {
            var categories = new List<string>();
            if(!string.IsNullOrEmpty(searchText))
            {
                categories = _recipesCollection.AsQueryable().Where(x => x.category == searchText).Select(x => x.category).Distinct().ToList();
            }
            else
            {
                categories = _recipesCollection.AsQueryable().Select(x => x.category).Distinct().ToList();
            }
            return categories;
       
        }

        public async Task<List<string>> GetTags(bool? publishedOnly = false)
        {
            var filterList = new List<FilterDefinition<Recipes>>();

            if(publishedOnly ?? false)
            {
                filterList.Add(Builders<Recipes>.Filter.Eq(x => x.published, true));
            }
        
            filterList.Add(Builders<Recipes>.Filter.Eq(x => x.isActive, true));

            var filter = Builders<Recipes>.Filter.And(filterList);
            if (filter == null)
            {
                return new List<string>();
            }

            var tagList = await _recipesCollection.Distinct<string>("tags", filter).ToListAsync();
            return tagList;
        }
    }
}
