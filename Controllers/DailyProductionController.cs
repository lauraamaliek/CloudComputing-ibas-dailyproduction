using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DailyProduction.Models;
using Azure.Data.Tables;
using Azure;
using Microsoft.Extensions.Configuration;

namespace IbasAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DailyProductionController : ControllerBase
    {
        // Construct a new "TableServiceClient using a TableSharedKeyCredential.
        
        private readonly TableClient _tableClient;
        private List<DailyProductionDTO> _productionRepo;
        private readonly ILogger<DailyProductionController> _logger;

        public DailyProductionController(ILogger<DailyProductionController> logger,IConfiguration configuration)
        {
            string accountName = configuration["AzureStorage:AccountName"];
            string storageAccountKey = configuration["AzureStorage:AccountKey"];
            string storageUri = configuration["AzureStorage:Uri"];
            string tableName = configuration["AzureStorage:TableName"];
            _logger = logger;
            
            _tableClient = new TableClient(
                new Uri(storageUri),
                tableName,
                new TableSharedKeyCredential(accountName, storageAccountKey));
            
            // Create the table in the service.
            _tableClient.CreateIfNotExists();
            
        }
        
        [HttpGet]
        public IEnumerable<DailyProductionDTO> Get()
        {
            Pageable<TableEntity> queryResults = _tableClient.Query<TableEntity>();

            var result = new List<DailyProductionDTO>();

            foreach (TableEntity entity in queryResults)
            {
                result.Add(new DailyProductionDTO
                {
                    Model = (BikeModel)int.Parse(entity.PartitionKey),
                    Date = entity.GetDateTime("ProductionTime") ?? default,
                    ItemsProduced = entity.GetInt32("itemsProduced") ?? 0
                });
            }

            _logger.LogInformation($"Hentede {result.Count} produktionsrækker fra Azure Table.");

            return result;
        }
    }
}
