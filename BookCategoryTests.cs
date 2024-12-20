using Newtonsoft.Json.Linq;
using RestSharp;
using System.Net;

namespace ApiTests
{
    [TestFixture]
    public class BookCategoryTests : IDisposable
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
        public void Test_BookCategoryLifecycle()
        {
            // Step 1: Create a new book category
            var createCategoryRequest = new RestRequest("/category", Method.Post);
            createCategoryRequest.AddHeader("Authorization", $"Bearer {token}");

            createCategoryRequest.AddJsonBody(new
            {
                title = "Fictional Literature"
            });

            var createResponse = client.Execute(createCategoryRequest);

            Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var createdCategory = JObject.Parse(createResponse.Content);
            var categoryId = createdCategory["_id"]?.ToString();
            var newTitle = createdCategory["title"]?.ToString();

            Assert.That(categoryId, Is.Not.Null.Or.Empty);
            Assert.That(newTitle, Is.EqualTo("Fictional Literature"));

            // Step 2: Retrieve all book categories and verify the newly created category is present
            var getAllCategories = new RestRequest("/category", Method.Get);
            var getAllCategoriesResponse = client.Execute(getAllCategories);

            Assert.Multiple(() =>
            {
                Assert.That(getAllCategoriesResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                Assert.That(getAllCategoriesResponse.Content, Is.Not.Empty);

                var categories = JArray.Parse(getAllCategoriesResponse.Content);


                Assert.That(categories.Type, Is.EqualTo(JTokenType.Array));

                Assert.That(categories.Count, Is.GreaterThan(0));

                var createdCategory = categories.FirstOrDefault(c => c["title"]?.ToString() == "Fictional Literature");

                Assert.That(createdCategory, Is.Not.Null);

                var verifyId = categories.FirstOrDefault(c => c["_id"]?.ToString() == categoryId);

                Assert.That(verifyId, Is.Not.Null);
                Assert.That(categoryId, Is.EqualTo(verifyId["_id"]?.ToString()));
            });

            // Step 3: Update the category title
            var editReqsuest = new RestRequest($"/category/{categoryId}", Method.Put);
            editReqsuest.AddHeader("Authorization", $"Bearer {token}");
            editReqsuest.AddJsonBody(new
            {
                title = "Updated Fictional Literature"
            });
            var editResponse = client.Execute(editReqsuest);
            Assert.That(editResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            // Step 4: Verify that the category details have been updated
            var getEditedCategoryRequset = new RestRequest($"/category/{categoryId}", Method.Get);
            var getEditedCategoryResponse = client.Execute(getEditedCategoryRequset);

            Assert.Multiple(() =>
            {
                Assert.That(getEditedCategoryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(getEditedCategoryResponse.Content, Is.Not.Empty);

                var updatedCategoryGet = JObject.Parse(getEditedCategoryResponse.Content);
                Assert.That(updatedCategoryGet["title"]?.ToString(), Is.EqualTo("Updated Fictional Literature"));
            });

            // Step 5: Delete the category and validate it's no longer accessible
            var deleteCategory = new RestRequest($"/category/{categoryId}", Method.Delete);
            deleteCategory.AddHeader("Authorization", $"Bearer {token}");
            var deleteResponse = client.Execute(deleteCategory);

            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            // Step 6: Verify that the deleted category cannot be found
            var verifyDeleteReqsuset = new RestRequest($"/category/{categoryId}", Method.Get);
            var verifyDeleteResponse = client.Execute(verifyDeleteReqsuset);

            Assert.That(verifyDeleteResponse.Content, Is.EqualTo("null"));
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}
