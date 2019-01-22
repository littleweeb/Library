using System;
using System.Collections.Generic;
using System.Text;

namespace LittleWeebLibrary.Handlers
{
    public interface IAnimeTitleHandler
    {
        List<string> GenerateSearchQuerys(string animeTitle, int strictness);

    }
    class AnimeTitleHandler
    {
    }
}
