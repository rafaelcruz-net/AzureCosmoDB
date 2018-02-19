using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureCosmoDBSample.Model.Repository
{
    public class ClienteRepository
    {
        public string Endpoint = @"https://azure-cosmos-sample.documents.azure.com:443/";
        public string Key = "UDAKQLQNvidb5iscTVvG22eRe18bIZJzQKY4fT1vMzzrcv1IRCOt2DLmDcpRb27g5SFNkr4EvgXKjzMSEpzIcg==";
        public string DatabaseId = "AzureSampleDB";
        public string CollectionId = "ClienteCollection";
        public DocumentClient DocumentClient;

        public ClienteRepository()
        {
            this.DocumentClient = new DocumentClient(new Uri(Endpoint), Key);

            DocumentClient.CreateDatabaseIfNotExistsAsync(new Microsoft.Azure.Documents.Database() { Id = DatabaseId }).Wait();
            DocumentClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(DatabaseId),
                                                                    new DocumentCollection() { Id = CollectionId },
                                                                    new RequestOptions() {  OfferThroughput = 1000 }).Wait();
        }

        public async Task Save(Cliente model)
        {
            await this.DocumentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), model);
        }

        public async Task Remove(Guid id)
        {
            await this.DocumentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id.ToString()));
        }

        public async Task Update(Guid id, Cliente model)
        {
            await this.DocumentClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id.ToString()), model);
        }

        public async Task<Cliente> Get(Guid id)
        {
            try
            {
                Document doc = await this.DocumentClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id.ToString()));
                return JsonConvert.DeserializeObject<Cliente>(doc.ToString());
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                else
                    throw;
            }
        }

        public async Task<IEnumerable<Cliente>> GetAll()
        {
            var documents = this.DocumentClient.CreateDocumentQuery<Cliente>(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                                                                             new FeedOptions { MaxItemCount = -1 })
                                                                             .AsDocumentQuery();

            List<Cliente> result = new List<Cliente>();

            while (documents.HasMoreResults)
                result.AddRange(await documents.ExecuteNextAsync<Cliente>());

            return result;

        }

    }
}
