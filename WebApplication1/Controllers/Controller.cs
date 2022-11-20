using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api")]
    [ApiController]
    public class Controller : ControllerBase
    {
        private List<SearchProfile> _searchProfileList;
        private IMemoryCache _cache;

        public Controller(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
            _searchProfileList = new List<SearchProfile>();
        }
        [HttpPost]
        public void SetKeyWords([FromBody]List<string> keyWords)
        {
            foreach(var word in keyWords)
            {
                _searchProfileList.Add(new SearchProfile(word));
            }
            _cache.Set(CacheKeys.Entry, _searchProfileList);
        }
        [HttpGet]
        public async Task<ActionResult<List<string>>> SearchForWords([FromHeader]List<int> profileIdList, [FromQuery] direction dir, [FromQuery] int maxWordsCount, [FromQuery] bool ignoreCase)
        {
            string? txtFile;
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                txtFile = await reader.ReadToEndAsync();
            }
            _searchProfileList = _cache.Get<List<SearchProfile>>(CacheKeys.Entry);
            if (txtFile == null || _searchProfileList == null) return NotFound();
            List<string> keyPhraseList = new List<string>();
            foreach (var profileId in profileIdList)
            {
                var keyPhrase = _searchProfileList.FirstOrDefault(k => k.Id == profileId);
                if (keyPhrase != null) keyPhraseList.Add(keyPhrase.profile);
            }
            WordFinder wordFinder = new WordFinder(dir, maxWordsCount, ignoreCase, txtFile, keyPhraseList);
            var results = wordFinder.SearchForResults();
            if (results == null) return NotFound();
            return results;
        }
    }
}
