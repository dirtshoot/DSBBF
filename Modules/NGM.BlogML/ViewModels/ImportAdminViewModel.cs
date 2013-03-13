using System.Collections.Generic;
using NGM.BlogML.Models;

namespace NGM.BlogML.ViewModels {
    public class ImportAdminViewModel {
        public ImportPart Model { get; set; }

        public IEnumerable<KeyValuePair<int, string>> Blogs { get; set; }
    }
}