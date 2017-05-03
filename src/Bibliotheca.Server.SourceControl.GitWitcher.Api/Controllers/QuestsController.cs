using System.Threading.Tasks;
using Bibliotheca.Server.SourceControl.GitWitcher.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bibliotheca.Server.Indexer.AzureSearch.Api.Controllers
{
    /// <summary>
    /// Controler for webhooks.
    /// </summary>
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/quests")]
    public class QuestsController : Controller
    {
        private readonly IQuestsService _questsService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="questsService">Quests service.</param>
        public QuestsController(IQuestsService questsService)
        {
            _questsService = questsService;
        }

        /// <summary>
        /// Method which is run by webhooks.
        /// </summary>
        /// <param name="projectId">Project id.</param>
        /// <returns>If execute successfully returns 200 (Ok).</returns>
        [HttpPost("{projectId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Post(string projectId)
        {
            await _questsService.CreateQuestAsync(projectId);
            return Ok();
        }
    }
}
