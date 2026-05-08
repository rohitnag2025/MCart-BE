using Microsoft.AspNetCore.Mvc;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly SearchClient _searchClient;
        public SearchController(IConfiguration config)
        {
            var endpoint = config["AzureSearch:Endpoint"];
            var key = config["AzureSearch:ApiKey"];
            var indexName = config["AzureSearch:IndexName"];
            _searchClient = new SearchClient(new System.Uri(endpoint), indexName, new Azure.AzureKeyCredential(key));
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] string? filter = null, [FromQuery] int top = 10)
        {
            var options = new SearchOptions { Size = top };
            if (!string.IsNullOrEmpty(filter))
                options.Filter = filter;
            var results = await _searchClient.SearchAsync<SearchDocument>(query, options);
            var docs = new List<SearchDocument>();
            await foreach (var result in results.Value.GetResultsAsync())
                docs.Add(result.Document);
            return Ok(docs);
        }

        [HttpGet("suggest")] // /api/search/suggest?query=abc
        public async Task<IActionResult> Suggest([FromQuery] string query, [FromQuery] string suggesterName = "sg")
        {
            var options = new SuggestOptions { Size = 5 };
            var results = await _searchClient.SuggestAsync<SearchDocument>(query, suggesterName, options);
            var suggestions = new List<string>();
            foreach (var result in results.Value.Results)
                suggestions.Add(result.Text);
            return Ok(suggestions);
        }
    }
}
