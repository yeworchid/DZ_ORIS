using System.Collections.Generic;
using Xunit;

namespace MyORMLibrary.Tests
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class ORMContextTests
    {
        private string connectionString = "Host=localhost;Port=5432;Database=usersdb;Username=postgres;Password=password";

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            var context = new ORMContext(connectionString);
            Assert.NotNull(context);
        }

        [Fact]
        public void Create_Test()
        {
            var context = new ORMContext(connectionString);
            var newUser = new User 
            { 
                Name = "TestUser", 
                Age = 25 
            };

            var result = context.Create(newUser, "users");
            
            Assert.NotNull(result);
        }

        [Fact]
        public void ReadById_Test()
        {
            var context = new ORMContext(connectionString);
            var user = context.ReadById<User>(1, "users");
            
            Assert.NotNull(user);
        }

        [Fact]
        public void ReadAll_Test()
        {
            var context = new ORMContext(connectionString);
            var users = context.ReadAll<User>("users");
            
            Assert.NotNull(users);
        }

        [Fact]
        public void Read_WithoutFilter_Test()
        {
            var context = new ORMContext(connectionString);
            var users = context.Read<User>("users");
            
            Assert.NotNull(users);
        }

        [Fact]
        public void Read_WithSimpleFilter_Test()
        {
            var context = new ORMContext(connectionString);
            var parameters = new Dictionary<string, object> { { "@age", 25 } };
            var users = context.Read<User>("users", "Age = @age", parameters);
            
            Assert.NotNull(users);
        }

        [Fact]
        public void Read_WithComplexFilter_Test()
        {
            var context = new ORMContext(connectionString);
            var parameters = new Dictionary<string, object> { { "@minAge", 18 }, { "@maxAge", 30 } };
            var users = context.Read<User>("users", "Age >= @minAge AND Age <= @maxAge", parameters);
            
            Assert.NotNull(users);
        }

        [Fact]
        public void Read_WithLikeFilter_Test()
        {
            var context = new ORMContext(connectionString);
            var parameters = new Dictionary<string, object> { { "@name", "%John%" } };
            var users = context.Read<User>("users", "Name LIKE @name", parameters);
            
            Assert.NotNull(users);
        }

        [Fact]
        public void Update_Test()
        {
            var context = new ORMContext(connectionString);
            var user = new User 
            { 
                Id = 1,
                Name = "Updated", 
                Age = 30 
            };

            context.Update(1, user, "users");
        }

        [Fact]
        public void Delete_Test()
        {
            var context = new ORMContext(connectionString);

            context.Delete(999, "users");
        }
    }
}
