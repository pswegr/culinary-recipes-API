using AspNetCore.Identity.Mongo.Model;
using MongoDbGenericRepository.Attributes;

namespace CulinaryRecipes.API.Models.Identity
{
    // Name this collection Users
    [CollectionName("Roles")]

    public class ApplicationRole: MongoRole
    {
        public ApplicationRole() : base()
        {
        }

        public ApplicationRole(string roleName) : base(roleName)
        {
        }
    }
}
