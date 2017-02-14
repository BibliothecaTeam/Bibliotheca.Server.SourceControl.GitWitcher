using System.Threading.Tasks;

namespace Bibliotheca.Server.SourceControl.GitWitcher.Core.Services
{
    public interface IQuestsService
    {
        Task CreateQuestAsync(string projectId);
    }
}