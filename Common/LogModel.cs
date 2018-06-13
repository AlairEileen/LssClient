using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    
    public class ContentModel<T>
    {
        public string Content { get; set; }
        //private static MongoDBTool mongoDBTool = new MongoDBTool();
        //public static IMongoCollection<T> Collection { get => mongoDBTool.GetMongoCollection<T>(); }
        //public static UpdateDefinitionBuilder<T> Update { get => Builders<T>.Update; }
        //public static FilterDefinitionBuilder<T> Filter { get => Builders<T>.Filter; }

    }

    public class LogModel : ContentModel<LogModel>
    {
    }
    public class BugModel : ContentModel<BugModel>
    {
    }
}
