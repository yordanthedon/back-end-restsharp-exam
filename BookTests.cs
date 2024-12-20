using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class BookTests : IDisposable
    {
        private RestClient client;
        private string token;

        [SetUp]
        public void Setup()
        {
            client = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");

            Assert.That(token, Is.Not.Null.Or.Empty, "Authentication token should not be null or empty");
        }

        [Test]
        public void Test_GetAllBooks()
        {
            var getRequest = new RestRequest("book", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code not OK");
                Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "respon content not ok");

                var books = JArray.Parse(getResponse.Content);

                Assert.That(books.Type, Is.EqualTo(JTokenType.Array), "content not jarray");
                Assert.That(books.Count, Is.GreaterThan(0), "less than zero");

                foreach (var book in books)
                {
                    Assert.That(book["title"]?.ToString(), Is.Not.Null.Or.Empty);
                    Assert.That(book["author"]?.ToString(), Is.Not.Null.Or.Empty);
                    Assert.That(book["description"]?.ToString(), Is.Not.Null.Or.Empty);
                    Assert.That(book["price"]?.ToString(), Is.Not.Null.Or.Empty);
                    Assert.That(book["pages"]?.ToString(), Is.Not.Null.Or.Empty);
                    Assert.That(book["category"]?.ToString(), Is.Not.Null.Or.Empty);
                }
            });
        }

        [Test]
        public void Test_GetBookByTitle()
        {
            var getRequest = new RestRequest("book", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code not OK");
                Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "respon content not ok");

                var books = JArray.Parse(getResponse.Content);
                var book = books.FirstOrDefault(b => b["title"]?.ToString() == "The Great Gatsby");

                Assert.That(book["author"]?.ToString(), Is.EqualTo("F. Scott Fitzgerald"));
            });
        }

        [Test]
        public void Test_AddBook()
        {
            var getCategoriesRequest = new RestRequest("category", Method.Get);
            var getCategoriesResponse = client.Execute(getCategoriesRequest);

            var categories = JArray.Parse(getCategoriesResponse.Content);
            var firstCategory = categories.First();
            var categoryId = firstCategory["_id"]?.ToString();

            var addRequest = new RestRequest("book", Method.Post);
            addRequest.AddHeader("Authorization", $"Bearer {token}");

            var title = "Random Title";
            var author = "Random author";
            var description = "random description";
            var price = 10;
            var pages = 100;

            addRequest.AddJsonBody(new
            {
                title,
                author,
                description,
                price,
                pages,
                category = categoryId
            });

            var addResponse = client.Execute(addRequest);

            Assert.Multiple(() =>
            {
                Assert.That(addResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code not OK");
                Assert.That(addResponse.Content, Is.Not.Null.Or.Empty, "respon content not ok");
            });

            var createdBookId = JObject.Parse(addResponse.Content);
            Assert.That(createdBookId["_id"]?.ToString(), Is.Not.Empty); 

            var createdID = createdBookId["_id"]?.ToString();
            var bookCategoryId = createdBookId["category"]["_id"]?.ToString();

            var getBookRequest = new RestRequest($"/book/{createdID}", Method.Get);
            var getResponse = client.Execute(getBookRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code not OK");
                Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "respon content not ok");
            });

            var book = JObject.Parse(getResponse.Content);

            Assert.That(book["title"]?.ToString(), Is.EqualTo("Random Title"));
            Assert.That(book["author"]?.ToString(), Is.EqualTo("Random author"));
            Assert.That(book["description"]?.ToString(), Is.EqualTo("random description"));
            Assert.That(book["price"]?.ToString(), Is.EqualTo("10"));
            Assert.That(book["pages"]?.ToString(), Is.EqualTo("100"));
            Assert.That(book["category"]?.ToString(), Is.Not.Empty);

            Assert.That(book["category"]["_id"]?.ToString(), Is.EqualTo(bookCategoryId));
        }

        [Test]
        public void Test_UpdateBook()
        {
            var getRequest = new RestRequest("book", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code not OK");
                Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "respon content not ok");
            });

            var books = JArray.Parse(getResponse.Content);
            var bookToUpdate = books.FirstOrDefault(b => b["title"]?.ToString() == "The Catcher in the Rye");

            Assert.That(bookToUpdate, Is.Not.Null);

            var bookId = bookToUpdate["_id"]?.ToString();

            var updateRequest = new RestRequest($"book/{bookId}", Method.Put);
            updateRequest.AddHeader("Authorization", $"Bearer {token}");

            updateRequest.AddJsonBody(new
            {
                title = "Updated Book Title",
                author = "Updated Author"
            });

            var updateResponse = client.Execute(updateRequest);

            Assert.Multiple(() =>
            {
                Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code not OK");
                Assert.That(updateResponse.Content, Is.Not.Null.Or.Empty, "respon content not ok");

                var updatedBook = JObject.Parse(updateResponse.Content);

                Assert.That(updatedBook["title"]?.ToString(), Is.EqualTo("Updated Book Title"));
                Assert.That(updatedBook["author"]?.ToString(), Is.EqualTo("Updated Author"));
            });
        }

        [Test]
        public void Test_DeleteBook()
        {
            var getRequest = new RestRequest("book", Method.Get);
            var getResponse = client.Execute(getRequest);

            Assert.Multiple(() =>
            {
                Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code not OK");
                Assert.That(getResponse.Content, Is.Not.Null.Or.Empty, "respon content not ok");
            });

            var books = JArray.Parse(getResponse.Content);
            var bookToDelete = books.FirstOrDefault(b => b["title"]?.ToString() == "To Kill a Mockingbird");

            Assert.That(bookToDelete, Is.Not.Null);

            var bookId = bookToDelete["_id"]?.ToString();

            var deleteRequest = new RestRequest($"book/{bookId}", Method.Delete);
            deleteRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteResponse = client.Execute(deleteRequest);

            Assert.Multiple(() =>
            {
                Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Status code not OK");

                var verifyRequset = new RestRequest($"book/{bookId}");
                var verifyResponse = client.Execute(verifyRequset);

                Assert.That(verifyResponse.Content, Is.EqualTo("null"));
            });


        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
