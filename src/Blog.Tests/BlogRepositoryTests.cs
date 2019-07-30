using Blog.DataAccess;
using Blog.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BlogRepositoryTests
{
    [TestClass]
    public class BlogRepositoryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullContextThrowsException()
        {
            var blogRepository = new BlogRepository(null);
            Assert.Fail();
        }

        [TestMethod]
        public void ReturnAllBlogObjects_Mock_DbContext()
        {
            var blogContextMock = new Mock<BlogContext>(new DbContextOptionsBuilder().Options);
            var blogs = new List<Blog.DataAccess.Blog>
            {
                new Blog.DataAccess.Blog(),
                new Blog.DataAccess.Blog()
            };
            var dbSetMock = new DbQueryMock<Blog.DataAccess.Blog>(blogs);
            blogContextMock
                .Setup(b => b.Blogs)
                .Returns(dbSetMock.Object);
            var blogRepository = new BlogRepository(blogContextMock.Object);

            var blogEntries = blogRepository.GetAllBlogEntries();
            Assert.AreEqual(2, blogEntries.Count());
        }

        [TestMethod]
        public void InMemory_Repository_Integration()
        {
            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using (var context = new BlogContext(options))
            {
                var blogRepo = new BlogRepository(context);
                var blogEntries = blogRepo.GetAllBlogEntries();
                Assert.AreEqual(0, blogEntries.Count());
                context.Blogs.Add(new Blog.DataAccess.Blog());
                context.SaveChanges();
                blogEntries = blogRepo.GetAllBlogEntries();
                Assert.AreEqual(1, blogEntries.Count());
            }
        }

        [TestMethod]
        public void InMemory_Repository_IntegrationTwo()
        {
            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using (var context = new BlogContext(options))
            {
                var blogRepo = new BlogRepository(context);
                var blogEntries = blogRepo.GetAllBlogEntries();
                Assert.AreEqual(0, blogEntries.Count());
                context.Blogs.Add(new Blog.DataAccess.Blog());
                context.SaveChanges();
                blogEntries = blogRepo.GetAllBlogEntries();
                Assert.AreEqual(1, blogEntries.Count());
            }
        }

        [TestMethod]
        public void ReturnAllBlogObjects_Mock_Repository()
        {
            var blogRepository = new Mock<IBlogRepository>();
            blogRepository
                .Setup(x => x.GetAllBlogEntries())
                .Returns(new List<Blog.DataAccess.Blog> { new Blog.DataAccess.Blog() });
            var blogController = new BlogController(blogRepository.Object);
            var blogResult = blogController.Get().Result as OkObjectResult;
        }

        [TestMethod]
        public void Inmemory_Controller_Integration()
        {
            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(databaseName: "controllerDb")
                .Options;
            using(var context = new BlogContext(options))
            {
                var blogRepository = new BlogRepository(context);
                var controller = new BlogController(blogRepository);
                var res = controller.Get().Result as OkObjectResult;
                var blogEntries = res.Value as IEnumerable<Blog.DataAccess.Blog>;
                Assert.AreEqual(0, blogEntries.Count());
                context.Blogs.Add(new Blog.DataAccess.Blog());
                context.SaveChanges();
                res = controller.Get().Result as OkObjectResult;
                blogEntries = res.Value as IEnumerable<Blog.DataAccess.Blog>;
                Assert.AreEqual(1, blogEntries.Count());
            }
        }
    }


    public class DbQueryMock<TEntity> : Mock<DbSet<TEntity>> where TEntity : class
    {
        private readonly IEnumerable<TEntity> _entities;
        public DbQueryMock(IEnumerable<TEntity> entities)
        {
            _entities = (entities ?? Enumerable.Empty<TEntity>()).ToList();
            var data = _entities.AsQueryable();
            As<IQueryable<TEntity>>().Setup(x => x.Provider).Returns(data.Provider);
            As<IQueryable<TEntity>>().Setup(x => x.Expression).Returns(data.Expression);
            As<IQueryable<TEntity>>().Setup(x => x.ElementType).Returns(data.ElementType);
            As<IQueryable<TEntity>>().Setup(x => x.GetEnumerator()).Returns(() => data.GetEnumerator());
            As<IEnumerable>().Setup(x => x.GetEnumerator()).Returns(() => data.GetEnumerator());
        }
    }
}
