using System.Threading.Tasks;

namespace HDT.Plugins.Advisor.Services
{
	public interface IArchetypeImporter
	{
		Task<int> ImportDecks(bool archive, bool delete, bool removeClass);
	}
}