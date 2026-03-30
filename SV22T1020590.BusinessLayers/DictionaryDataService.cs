using SV22T1020590.DataLayers.Interfaces;
using SV22T1020590.DataLayers.SQLServer;
using SV22T1020590.Models.DataDictionary;
using SV22T1020590.BusinessLayers;

using System.Threading.Tasks;

namespace SV22T1020590.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến từ điển dữ liệu
    /// </summary>
    public static class DictionaryDataService
    {
        private static readonly IDataDictionaryRepository<Province> provinceDB;

        /// <summary>
        /// Constructor
        /// </summary>
        static DictionaryDataService()
        {
            provinceDB = new ProvinceRepository(Configuration.ConnectionString);
        }

        #region Province

        /// <summary>
        /// Lấy danh sách tỉnh thành.
        /// </summary>
        /// <returns>
        /// Danh sách tỉnh thành.
        /// </returns>
        public static async Task<List<Province>> ListProvincesAsync()
        {
            return await provinceDB.ListAsync();
        }

        #endregion
    }
}