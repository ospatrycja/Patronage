using NUnit.Framework;
using RestSharp;
using RestSharp.Serialization.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Patronage
{
    [TestFixture]
    public class MrBuggyProvidersTest
    {
        RestClient client = new RestClient("http://localhost:8080");

        string username = "PatronageUser";
        string password = "P@ssword1";
        string authHeader;

        [SetUp]
        public void setUp()
        {
            authHeader = createAuthHeader(username, password);
        }

        [TestCase("PatronageUser", "P@ssword1", HttpStatusCode.OK, TestName = "Should return OK HTTP status when user credentials are valid")]
        [TestCase("IncorrectUser", "Incorrect", HttpStatusCode.Unauthorized, TestName = "Should return Unauthorized HTTP status when user credentials are invalid")]
        public void TestGetProvidersListStatusCodes(string username, string password, HttpStatusCode expectedStatusCode)
        {
            string authHeader = createAuthHeader(username, password);
            RestRequest request = createAuthorizedRequest("/provider", Method.GET, authHeader);

            IRestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(expectedStatusCode));
        }

        [TestCase(0, true, TestName = "Should return empty providers list and success status")]
        public void TestGetProvidersList(int expectedListSize, bool expectedStatus)
        {
            RestRequest request = createAuthorizedRequest("/provider", Method.GET, authHeader);

            IRestResponse response = client.Execute(request);
            ProvidersResponse providersResponse = new JsonDeserializer().Deserialize<ProvidersResponse>(response);

            Assert.That(providersResponse.Data.Count, Is.EqualTo(expectedListSize));
            Assert.That(providersResponse.Success, Is.EqualTo(expectedStatus));
        }

        [TestCase("Test Provider", 100.0, TestName = "Should create provider with Test Provider name and price equal to 100")]
        public void TestPostProvider(string providerName, double providerPrice)
        {
            RestRequest request = createAuthorizedRequest("/provider", Method.POST, authHeader);
            request.AddParameter("application/x-www-form-urlencoded", $"name={providerName}&price={providerPrice}", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            ProviderResponse providerResponse = new JsonDeserializer().Deserialize<ProviderResponse>(response);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(providerResponse.Data.Name, Is.EqualTo(providerName));
            Assert.That(providerResponse.Data.Price, Is.EqualTo(providerPrice));
            Assert.That(providerResponse.Success, Is.EqualTo(true));
        }

        [TestCase(1, "New Test Provider", 150.0, TestName = "Should replace Test Provider with New Test Provider with price equal to 150")]
        public void TestPutProvider(int providerId, string providerName, double providerPrice)
        {
            RestRequest request = createAuthorizedRequest($"/provider/{providerId}", Method.PUT, authHeader);
            request.AddParameter("application/x-www-form-urlencoded", $"name={providerName}&price={providerPrice}", ParameterType.RequestBody);

            IRestResponse response = client.Execute(request);
            ProviderResponse providerResponse = new JsonDeserializer().Deserialize<ProviderResponse>(response);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(providerResponse.Data.Name, Is.EqualTo(providerName));
            Assert.That(providerResponse.Data.Price, Is.EqualTo(providerPrice));
            Assert.That(providerResponse.Success, Is.EqualTo(true));
        }

        [TestCase(1, TestName = "Should delete provider with given ID")]
        public void TestDeleteProvider(int providerId)
        {
            RestRequest request = createAuthorizedRequest($"/provider/{providerId}", Method.DELETE, authHeader);

            IRestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        public string createAuthHeader(string username, string password)
        {
            string encodedUsername = base64Encode(username);
            string encodedPassword = base64Encode(password);
            return $"{encodedUsername}:{encodedPassword}";
        }

        public string base64Encode(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }

        public RestRequest createAuthorizedRequest(string resource, Method method, string authHeader)
        {
            RestRequest request = new RestRequest(resource, method);
            request.AddHeader("Authorization", authHeader);
            return request;
        }
    }

    public class ProvidersResponse
    {
        public List<Provider> Data { get; set; }
        public bool Success { get; set; }
    }

    public class ProviderResponse
    {
        public Provider Data { get; set; }
        public bool Success { get; set; }
    }

    public class DeleteProviderResponse
    {
        public bool Success { get; set; }
    }

    public class Provider
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
    }
}
