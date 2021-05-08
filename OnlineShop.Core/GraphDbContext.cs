
using Neo4j.Driver.V1;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineShop.Core
{
    public class GraphDbContext
    {
        public GraphDbContext()
        {
            var url = ConfigurationManager.AppSettings["GraphDBUrl"];
            var user = ConfigurationManager.AppSettings["GraphDBUser"];
            var password = ConfigurationManager.AppSettings["GraphDBPassword"];

            var driver = GraphDatabase.Driver(url, AuthTokens.Basic(user, password), Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig());

            //Pass that driver to the BoltGraphClient
            var bgc = new BoltGraphClient(driver);

            //Connect.
            bgc.Connect();
            GraphClient = bgc;
        }
        public IGraphClient GraphClient;
    }
}
