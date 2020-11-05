
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using Utils;
using Common;
using Common.Request;

public class QueryTrack
{

    /// <summary>
    /// QueryTrack.query(new QueryTrackReq()
    /// {
    ///           customer = config.customer,
    ///           sign = SignUtils.GetMD5(queryTrackParam.ToString() + config.key + config.customer),
    ///           param =  queryTrackParam
    /// });
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public static string query(QueryTrackReq query)
    {

        var request = ObjectToDictionaryUtils.ObjectToMap(query);
        if (request == null)
        {
            return null;
        }
        var result = HttpUtils.doPostForm(ApiInfoConstant.QUERY_URL, request);
        return result;
    }
}   