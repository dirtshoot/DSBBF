using Orchard;

namespace NGM.BlogML.Services {
    public interface IExportService : IDependency {
        void Export(int id);
    }
}
