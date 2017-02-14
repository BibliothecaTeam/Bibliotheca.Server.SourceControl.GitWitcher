using System.Threading.Tasks;
using Bibliotheca.Server.SourceControl.GitWitcher.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheca.Server.Indexer.AzureSearch.Api.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/quests")]
    public class QuestsController : Controller
    {
        private readonly IQuestsService _questsService;

        public QuestsController(IQuestsService questsService)
        {
            _questsService = questsService;
        }

        [HttpPost("{projectId}")]
        public async Task<IActionResult> Post(string projectId)
        {
            await _questsService.CreateQuestAsync(projectId);
            return Ok();
        }
    }
}
