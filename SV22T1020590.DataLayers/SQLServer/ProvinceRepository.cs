using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SV22T1020590.DataLayers.Interfaces;
using SV22T1020590.Models.DataDictionary;

namespace SV22T1020590.DataLayers.SQLServer
{
    public class ProvinceRepository : BaseRepository, IDataDictionaryRepository<Province>
    {
        public ProvinceRepository(string connectionString) : base(connectionString) { }

        public async Task<List<Province>> ListAsync()
        {
            const string sql = "SELECT ProvinceName FROM Provinces";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var items = await cn.QueryAsync<Province>(sql);
            return items.ToList();
        }
    }
}
