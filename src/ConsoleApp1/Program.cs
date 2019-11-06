using Hopex.Core;
using Hopex.Core.Domain;
using IQToolkit.Data.Mapping;
using IQToolkit.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    partial class Program
    {
        [Table(Name = "[test].[dbo].[HopexData]")]
        public class Customers
        {
            public string Name { get; set; }
        }


        static async Task Main(string[] args)
        {
            var ctx = new HopexSettings("Data source=.\\sql2017;User Id=sa;Password=Mega!2017", "Test", 810, false);
            var store = new HopexStore(ctx);
            using (var context = await store.NewUnitOfWork(new HopexUserIdentity(), "MySchema", 810))
            {
                var provider = new HopexQueryProvider(context);
                var query = from c in provider.GetTable<Carnet>()
                            where c.Name.Contains("4")
                            select new { Name = c.Name };

                var results = query.ToList();
            }
        }
    }
}
