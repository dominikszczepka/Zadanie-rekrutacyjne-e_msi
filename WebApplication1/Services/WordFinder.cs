namespace WebApplication1.Services
{
    public class WordFinder
    {
        private readonly direction _dir;
        private readonly int _maxWords;
        private readonly bool _ignoreCase;
        private string? _txtFile;
        private List<string> _keyPhraseList;
        private List<string>? _listOfLines;
        private List<string>? _listOfLinesToCheck;
        public WordFinder(direction dir, int maxWords, bool ignoreCase, string? txtFile, List<string> keyPhraseList)
        {
            _dir = dir;
            _maxWords = maxWords;
            _ignoreCase = ignoreCase;
            _txtFile = txtFile;
            _keyPhraseList = keyPhraseList;
        }

        public List<string>? SearchForResults()
        {
            _listOfLines = GetListOfLines(_txtFile);
            ShouldIgnoreCase();
            List<string>? results = new List<string>();
            foreach (var keyPhrase in _keyPhraseList)
            {
                if (_txtFile.Contains(keyPhrase))
                    {
                        var result = SearchSingleResult(keyPhrase);
                        if (result != null) results.Add(result); 
                    }
            }
            return results;
        }
        private void ShouldIgnoreCase()
        {
            if (!_ignoreCase)
            {
                _listOfLinesToCheck = _listOfLines;
                return;
            }
            List<string> tempListOfLines = new List<string>();
            List<string> tempKeyPhraseList = new List<string>();
            foreach (var line in _listOfLines)
            {
                tempListOfLines.Add(line.ToLower());
            }
            foreach (var profile in _keyPhraseList)
            {
                tempKeyPhraseList.Add(profile.ToLower());
            }
            _txtFile=_txtFile.ToLower();
            _listOfLinesToCheck = tempListOfLines;
            _keyPhraseList = tempKeyPhraseList;
        }
        private string? SearchSingleResult(string keyPhrase)
        {
            switch (_dir)
            {
                case direction.left:
                    return SearchHorizontal(keyPhrase,_dir,_maxWords);
                case direction.right:
                    return SearchHorizontal(keyPhrase,_dir,_maxWords);
                case direction.top:
                    return SearchVertical(keyPhrase);
                case direction.bottom:
                    return SearchVertical(keyPhrase);
                default: return null;
            }
        }
        private string? SearchHorizontal(string keyPhrase,direction dir,int maxWords)
        {
            var crucialWordInKeyPhrase = 
                dir == direction.left? 
                keyPhrase.Split(new char[] { ' ' }).ToList().First() :
                keyPhrase.Split(new char[] { ' ' }).ToList().Last();
            var lineNumber = FindLine(crucialWordInKeyPhrase, _listOfLinesToCheck);
            var wordsInALine = _listOfLinesToCheck[(int)lineNumber].Split(new char[] { ' ' }).ToList();
            //temporary index points to the curcial word in a list of words in a particular line of text
            var tmp_index = wordsInALine.IndexOf(crucialWordInKeyPhrase);
            var originalWordsInALine = _listOfLines[(int)lineNumber].Split(new char[] { ' ' }).ToList();
            //if the word temporary index points to is at the start or at the end of the line, index switches lines
            if (dir == direction.left && tmp_index == 0)
            {
                lineNumber--;
                originalWordsInALine = _listOfLines[(int)lineNumber].Split(new char[] { ' ' }).ToList();
                tmp_index = originalWordsInALine.Count-1;
            }
            else if (dir == direction.right && tmp_index == originalWordsInALine.Count - 1)
            {
                lineNumber++;
                originalWordsInALine = _listOfLines[(int)lineNumber].Split(new char[] { ' ' }).ToList();
                tmp_index = 0;
            }
            else tmp_index = dir == direction.left ? tmp_index-1 : tmp_index+1;
            string result = originalWordsInALine[tmp_index]; 
            for (int i = 1; i < maxWords; i++)
            {
                try
                {
                    result = dir == direction.left ?
                        originalWordsInALine[tmp_index-1] + " " + result:
                        result + " " + originalWordsInALine[tmp_index+1];
                    tmp_index = dir == direction.left ? tmp_index - 1 : tmp_index + 1;
                }
                catch (ArgumentOutOfRangeException)
                {
                    lineNumber = dir == direction.left ?
                            lineNumber - 1 :
                            lineNumber + 1;
                    if (lineNumber < 0 || lineNumber > _listOfLines.Count - 1) break;
                    originalWordsInALine = _listOfLines[(int)lineNumber].Split(new char[] { ' ' }).ToList();
                    tmp_index = dir == direction.left ?
                        originalWordsInALine.Count - 1 :
                        0;
                    result = dir == direction.left ?
                        originalWordsInALine[tmp_index] + " " + result:
                        result + " " + originalWordsInALine[tmp_index];
                }
            }
            return result;
        }
        private string? SearchVertical(string keyPhrase)
        {
            var fristWordInAKeyPhrase = keyPhrase.Split(new char[] { ' ' }).ToList().First();
            var lineNumber = FindLine(fristWordInAKeyPhrase, _listOfLines);
            string line;
            string originalLine;
            try 
            {
                line = _dir == direction.bottom ?
                    _listOfLinesToCheck[(int)lineNumber + 1] :
                    _listOfLinesToCheck[(int)lineNumber - 1];
                originalLine = _dir == direction.bottom ?
                    _listOfLines[(int)lineNumber + 1] :
                    _listOfLines[(int)lineNumber - 1];
            }
            catch(Exception)
            {
                return null;
            }
            int searchProfileStartingIndex = _listOfLinesToCheck[(int)lineNumber].IndexOf(fristWordInAKeyPhrase);
            int indexOfChar = searchProfileStartingIndex;
            while (line[indexOfChar].Equals(' ') && indexOfChar < keyPhrase.Length + searchProfileStartingIndex)
            {
                indexOfChar++;
            }
            if (line[indexOfChar].Equals(' ')) return null;
            string result = "";
            // indexOfChar is an index of a character form the word at the top or bottom of search profile
            int tmp_i = indexOfChar;
            while (!line[tmp_i].Equals(' ') && tmp_i != 0)
            {
                tmp_i--;
                result=originalLine[tmp_i]+result;
            }
            // result is complete with all characters before the indexOfChar
            tmp_i = indexOfChar;
            while (!line[tmp_i].Equals(' ') && tmp_i < line.Length)
            {
                result+=(originalLine[tmp_i]);
                tmp_i++;
            }
            // result is complete with all characters of the first word
            if (_maxWords > 1)
            {
                string? additionalWords = SearchHorizontal(result, direction.right, _maxWords - 1);
                if (additionalWords != null) result = result + " " + additionalWords;
            }
            return result;
        }
        private int? FindLine(string word, List<string> file)
        {
            int lineCounter = 0;
            foreach (string line in file)
            {
                if (line.Contains(word)) return lineCounter;
                lineCounter++;
            }
            return null;
        }
        private List<string>? GetListOfLines(string? txtFile)
        {
            return txtFile.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
